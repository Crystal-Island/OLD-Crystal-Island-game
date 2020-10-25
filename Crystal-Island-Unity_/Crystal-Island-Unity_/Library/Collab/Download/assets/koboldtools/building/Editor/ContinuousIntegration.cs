using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KoboldTools;
using KoboldTools.Helpers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KoboldTools.Building {
    public enum BuildType {
        Debug = 1,
        Release = 2,
    }

    public class ContinuousIntegration {
        private static string BuildDirectory = "build";
        private static string ProjectConfigPath = "Assets/StreamingAssets/project.json";
        private static string AssetBundlePath = "bundles";

        /// <summary>
        /// This method executes the build and is to be called via command line using unity.
        ///
        /// It accepts command line arguments (must be supplied after Unity command line arguments and "--".)
        /// <param name="buildNumber">Optional: An unsigned integer that indicates the current build sequence number</param>
        /// <param name="buildType">Optional: A variant of KoboldTools.Building.BuildType, either "Release" or "Debug"</param>
        /// <param name="productPath">Optional: The output path of the build step</param>
        /// <param name="target">The build target platform (a variant of BuildTarget)</param>
        /// </summary>
        /// <example>
        /// <c>Unity.exe -quit -batchmode -logFile unity.log -projectPath PROJECT_PATH -executeMethod "KoboldTools.Building.ContinuousIntegration.Build" -- [-buildType BUILD_TYPE] [-buildNumber N] [-productPath PRODUCT_PATH] TARGET</c>
        /// </example>
        public static void Build() {
            // Obtain the build parameters from the command line arguments.
            Debug.Log("Parsing the command line arguments");
            uint buildNumber;
            BuildType buildType;
            List<BuildTarget> targets;
            string outputPath;
            string keystorePath;
            string keystorePassword;
            string keyaliasName;
            string keyaliasPassword;
            string productName;
            ParseArguments(out buildNumber, out buildType, out targets, out outputPath, out keystorePath, out keystorePassword, out keyaliasName, out keyaliasPassword, out productName);

            BuildInternal(buildNumber, buildType, targets, outputPath, keystorePath, keystorePassword, keyaliasName, keyaliasPassword, productName);
        }

        [MenuItem("Build/CI: Build project (current platform, debug)")]
        public static void EditorBuildCurrentDebug() {
            BuildInternal(0, BuildType.Debug, new List<BuildTarget> { EditorUserBuildSettings.activeBuildTarget });
        }

        [MenuItem("Build/CI: Build project (current platform, release)")]
        public static void EditorBuildCurrentRelease() {
            BuildInternal(0, BuildType.Release, new List<BuildTarget> { EditorUserBuildSettings.activeBuildTarget });
        }

        [MenuItem("Build/CI: Build asset bundles (current platform, uncompressed)")]
        public static void EditorAssetBuildCurrentUncompressed() {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget, false);
        }

        [MenuItem("Build/CI: Build asset bundles (current platform, compressed)")]
        public static void EditorAssetBuildCurrentCompressed() {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget, true);
        }

        private static void BuildInternal(uint buildNumber, BuildType buildType, List<BuildTarget> targets, string outputPath = "", string keystorePath = "", string keystorePassword = "", string keyaliasName = "", string keyaliasPassword = "", string productName = "") {
            Debug.Log("Initiating a project build");
            string projectRoot = GetProjectRoot();
            if (!String.IsNullOrEmpty(projectRoot)) {
                // Obtain project parameters from a config file.
                Debug.Log("Loading the project configuration");
                Project config = LoadConfig(projectRoot);
                if (config == null) {
                    throw new Exception(String.Format("Could not find a configuration file at '{0}'", Path.Combine(projectRoot, ProjectConfigPath)));
                }

                // Set the build number.
                Debug.Log("Setting the build number");
                if (buildNumber > 0) {
                    config.Version.BuildMetadata = buildNumber.ToString();
                } else {
                    uint tmp;
                    if (uint.TryParse(config.Version.BuildMetadata, out tmp)) {
                        config.Version.BuildMetadata = (tmp + 1).ToString();
                    } else {
                        config.Version.BuildMetadata = "0";
                    }
                }

                // Set the unity editor version.
                if (!String.IsNullOrEmpty(config.UnityVersion) && config.UnityVersion != Application.unityVersion) {
                    Debug.LogWarningFormat("The project was previously built with {0}, but now with {1}. This could lead to errors.", config.UnityVersion, Application.unityVersion);
                }
                config.UnityVersion = Application.unityVersion;

                // Save the project configuration.
                SaveConfig(projectRoot, config);

                // Build each product individually.
                if (String.IsNullOrEmpty(outputPath)) {
                    string repoPath = GetRepositoryRoot();
                    if (!String.IsNullOrEmpty(repoPath)) {
                        outputPath = Path.Combine(repoPath, BuildDirectory);
                    } else {
                        throw new Exception("Could not determine the repository root");
                    }
                }

                foreach (BuildTarget target in targets) {
                    foreach (Product product in config.Products)
                    {
                        //skip if product is not marked for build
                        if(!product.Build)
                            continue;
                        //skip if wrong product
                        if(product.InternalIdentifier != productName && productName != "")
                            continue;

                        // Obtain the app name
                        string appName = GetApplicationName(product.Identifier);

                        // Obtain the app product path.
                        string productPath = Path35.Combine(outputPath, GetOutputDirectory(target, config.Name, config.Version));
                        productPath = Path35.Combine(productPath, product.InternalIdentifier, GetProductName(target, appName, config.Version));

                        // Apply the player and build settings.
                        Debug.LogFormat("Setting PlayerSettings: productName={0}, applicationIdentifier={1}, bundleVersion={2}", product.Title, product.Identifier, config.Version.Public());
                        PlayerSettings.productName = product.Title;
                        PlayerSettings.applicationIdentifier = product.Identifier;
                        PlayerSettings.bundleVersion = config.Version.Public();
                        if (target == BuildTarget.iOS) {
                            PlayerSettings.iOS.buildNumber = config.Version.Ios();
                        } else if (target == BuildTarget.Android) {
                            PlayerSettings.Android.bundleVersionCode = config.Version.Android();
                            PlayerSettings.Android.keystoreName = keystorePath;
                            PlayerSettings.Android.keystorePass = keystorePassword;
                            PlayerSettings.Android.keyaliasName = keyaliasName;
                            PlayerSettings.Android.keyaliasPass = keyaliasPassword;
                            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
                        }

                        // Apply Splashscreen Settings
                        ApplySplashScreenSettings(product);

                        Debug.LogFormat("Building application '{0}' ({1}) against target: {2} ('{3}')", appName, config.Version.Internal(), target, productPath);
                        BuildSingleProduct(target, buildType, productPath, product.Scenes);

                        Debug.LogFormat("Applying the streaming assets folder settings of application '{0}'", appName);
                        ManageStreamingAssets(target, productPath, product.StreamingAssetsWhitelist);
                    }
                }
            } else {
                throw new Exception("Could not determine the project root directory");
            }
        }

        /// <summary>
        /// Apply product-specific splash screen settings
        /// </summary>
        /// <param name="product">Product definition</param>
        public static void ApplySplashScreenSettings(Product product)
        {
            PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[product.SplashDefinition.SplashLogos.Count];
            var tLogos = new List<PlayerSettings.SplashScreenLogo>();
            for(int i = 0; i < product.SplashDefinition.SplashLogos.Count; i++)
            {
                tLogos.Add(product.SplashDefinition.SplashLogos[i].CreateLogo());
            }
            PlayerSettings.SplashScreen.logos = tLogos.ToArray();
            PlayerSettings.SplashScreen.show = product.SplashDefinition.Show;
            PlayerSettings.SplashScreen.UnityLogoStyle splashStyle;
            if(Enum35.TryParse(product.SplashDefinition.LogoStyle, out splashStyle))
            {
                PlayerSettings.SplashScreen.unityLogoStyle = splashStyle;
            }
            PlayerSettings.SplashScreen.AnimationMode splashAnimationMode;
            if(Enum35.TryParse(product.SplashDefinition.AnimationMode, out splashAnimationMode))
            {
                if(splashAnimationMode == PlayerSettings.SplashScreen.AnimationMode.Custom)
                {
                    if(product.SplashDefinition.Zoom <= 1f && product.SplashDefinition.Zoom >= 0f)
                    {
                        PlayerSettings.SplashScreen.animationLogoZoom = product.SplashDefinition.Zoom;
                    }
                    if(product.SplashDefinition.BGZoom <= 1f && product.SplashDefinition.BGZoom >= 0f)
                    {
                        PlayerSettings.SplashScreen.animationLogoZoom = product.SplashDefinition.BGZoom;
                    }
                }
                else
                {
                    PlayerSettings.SplashScreen.animationMode = splashAnimationMode;
                }
            }
            PlayerSettings.SplashScreen.showUnityLogo = product.SplashDefinition.ShowUnity;
            if(product.SplashDefinition.ShowUnity)
            {
                PlayerSettings.SplashScreen.DrawMode splashDrawMode;
                if(Enum35.TryParse(product.SplashDefinition.DrawMode, out splashDrawMode))
                {
                    PlayerSettings.SplashScreen.drawMode = splashDrawMode;
                }
            }
            PlayerSettings.SplashScreen.overlayOpacity = product.SplashDefinition.OverlayOpacity;
            PlayerSettings.SplashScreen.backgroundColor = product.SplashDefinition.BackgroundColor;
            PlayerSettings.SplashScreen.background = AssetDatabase.LoadAssetAtPath<Sprite>(product.SplashDefinition.BackgroundPath);
        }

        private static void BuildSingleProduct(BuildTarget target, BuildType buildType, string productPath, List<string> scenes) {
            // Create the output directory and overwrite any existing products of the same name.
            string parent = Path.GetDirectoryName(productPath);
            if (!Directory.Exists(parent)) {
                Directory.CreateDirectory(parent);
            }
            if (File.Exists(productPath)) {
                Debug.LogWarningFormat("The file at '{0}' already exists. It will be deleted now.", productPath);
                File.Delete(productPath);
            }
            if (Directory.Exists(productPath)) {
                Debug.LogWarningFormat("The directory at '{0}' already exists. It will be deleted now.", productPath);
                Directory.Delete(productPath, true);
            }

            // Set the build options.
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.target = target;
            if (buildType == BuildType.Debug) {
                options.options = BuildOptions.Development;
            } else if (buildType == BuildType.Release) {
                options.options = BuildOptions.None;
            }
            options.scenes = scenes.ToArray();
            options.locationPathName = productPath;

            // Save the all open scenes.
            EditorSceneManager.SaveOpenScenes();

            // Build all asset bundles.
            Debug.LogFormat("Building asset bundles for target {0}", target);
            BuildAssetBundles(target, target != BuildTarget.WebGL);

            // Build the player.
            Debug.LogFormat("Building the player for target {0}", target);
            BuildPipeline.BuildPlayer(options);
        }

        private static void ManageStreamingAssets(BuildTarget target, string productPath, List<string> whitelist) {
            if (whitelist.Count > 0) {
                string basePath = GetProductStreamingAssets(target, productPath);

                if (!String.IsNullOrEmpty(basePath) && Directory.Exists(basePath)) {
                    Directory35.EnumerateFiles(basePath)
                        .Where(p => {
                            string name = Path.GetFileName(p);
                            return whitelist.Count(n => n == name) == 0;
                        })
                        .ForEach(p => {
                            Debug.LogFormat("Deleting the file {0} in the streaming assets path of product at '{1}'", p, productPath);
                            File.Delete(p);
                        });

                    Directory35.EnumerateDirectories(basePath)
                        .Where(p => {
                            string name = Path.GetFileName(p);
                            return whitelist.Count(n => n == name) == 0;
                        })
                        .ForEach(p => {
                            Debug.LogFormat("Deleting the directory {0} in the streaming assets path of product at '{1}'", p, productPath);
                            Directory.Delete(p, true);
                        });
                } else {
                    Debug.LogWarningFormat("Could not find the streaming assets directory of the product at '{0}'", productPath);
                }
            } else {
                Debug.Log("The StreamingAssets whitelist is empty; the content of the folder will be kept as-is");
            }
        }

        /// <summary>
        /// Builds this project's asset bundles.
        /// </summary>
        /// <param name="target">The target platform</param>
        /// <param name="enableCompression">Enable or disable compression</param>
        private static void BuildAssetBundles(BuildTarget target, bool enableCompression) {
            if (!HasDuplicateAssetBundleNames()) {
                string outputPath = Path.Combine(Application.streamingAssetsPath, AssetBundlePath);

                // Ensure the presence of the streaming assets path.
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }

                // Build the asset bundles with or without compression.
                if (enableCompression) {
                    Debug.Log("Building asset bundles with compression");
                    BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, target);
                } else {
                    Debug.Log("Building asset bundles without compression");
                    BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle, target);
                }
            } else {
                throw new Exception("Found duplicate asset bundle names");
            }
        }

        /// <summary>
        /// Ensures that the product name contains the correct extension, given
        /// the target platform.
        /// </summary>
        /// <param name="target">The target platform</param>
        /// <param name="productName">The name of the build product</param>
        /// <returns>A product name</returns>
        private static string EnsureExtension(BuildTarget target, string productName) {
            string ext = "";
            switch (target) {
                case BuildTarget.Android:
                    ext = ".apk";
                    break;

                case BuildTarget.StandaloneOSX:
                    ext = ".app";
                    break;

                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    ext = ".exe";
                    break;

                default:
                    break;
            }
            return String.Format("{0}{1}", productName, ext);
        }

        /// <summary>
        /// Given a application identifier, returns its name, e.g. the last part of the application identifier.
        /// </summary>
        /// <param name="applicationIdentifier">A string of the form tld.organization.project (ex. com.exampleinc.supergame)</param>
        /// <returns></returns>
        private static string GetApplicationName(string applicationIdentifier) {
            string[] parts = applicationIdentifier.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Last();
        }

        /// <summary>
        /// Constructs a product output directory.
        /// </summary>
        /// <param name="target">The target platform</param>
        /// <param name="name">The name of the product</param>
        /// <param name="version">The product version</param>
        /// <returns></returns>
        private static string GetOutputDirectory(BuildTarget target, string name, SemVer version) {
            string targetName = target.ToString().ToLower();
            name = name.ToLower().Replace(' ', '_');

            return String.Format("{1}_{0}_v{2}", targetName, name, version.Public());
        }

        /// <summary>
        /// Constructs the name of the product.
        /// </summary>
        /// <param name="target">The target platform</param>
        /// <param name="name">The name of the product</param>
        /// <param name="version">The product version</param>
        /// <returns></returns>
        private static string GetProductName(BuildTarget target, string name, SemVer version) {
            string productName = name.ToLower().Replace(' ', '_');

            return EnsureExtension(target, productName);
        }

        /// <summary>
        /// Obtains a list of scenes selected for builds in the unity editor.
        /// </summary>
        /// <returns>A list of scene paths</returns>
        private static List<string> GetBuildScenePaths() {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            List<string> scenes = new List<string>();
            for (int i = 0; i < sceneCount; i++) {
                scenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
            }

            return scenes;
        }

        private static string GetProductStreamingAssets(BuildTarget target, string productPath) {
            switch (target) {
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    string parent = Path.GetDirectoryName(productPath);
                    string productName = Path.GetFileNameWithoutExtension(productPath);
                    return Path35.Combine(parent, String.Format("{0}_Data", productName), "StreamingAssets");

                case BuildTarget.StandaloneOSX:
                    return Path35.Combine(productPath, "Contents", "Resources", "Data", "StreamingAssets");

                default:
                    return null;
            }
        }

        /// <summary>
        /// Obtains the unity project root.
        /// </summary>
        /// <returns>A path string or <c>null</c></returns>
        public static string GetProjectRoot() {
            if (Path.GetFileName(Application.dataPath) == "Assets") {
                return Path.GetDirectoryName(Application.dataPath);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Obtains the project's repository root.
        /// </summary>
        /// <returns>A path string or <c>null</c></returns>
        private static string GetRepositoryRoot() {
            string tokenFile = "ignore.conf";
            string projectPath = GetProjectRoot();
            if (projectPath != null) {
                string tmp = projectPath;
                string[] candidates = Directory.GetFiles(tmp, tokenFile, SearchOption.TopDirectoryOnly);
                if (candidates.Length == 0) {
                    tmp = Path.GetDirectoryName(tmp);
                    candidates = Directory.GetFiles(tmp, tokenFile, SearchOption.TopDirectoryOnly);
                    if (candidates.Length == 0) {
                        tmp = Path.GetDirectoryName(tmp);
                        candidates = Directory.GetFiles(tmp, tokenFile, SearchOption.TopDirectoryOnly);
                        if (candidates.Length == 0) {
                            return null;
                        } else {
                            return tmp;
                        }
                    } else {
                        return tmp;
                    }
                } else {
                    return tmp;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the project's build configuration.
        /// </summary>
        /// <param name="projectRoot">The path to the unity project root</param>
        /// <returns>An instance of <c>ProjectConfiguration</c> or <c>null<c/></returns>
        public static Project LoadConfig(string projectRoot) {
            // Obtain the current project's config file.
            string projectConfigPath = Path.Combine(projectRoot, ProjectConfigPath);
            if (File.Exists(projectConfigPath)) {
                using(StreamReader r = new StreamReader(projectConfigPath, Encoding.UTF8)) {
                    return JsonUtility.FromJson<Project>(r.ReadToEnd());
                }
            } else {
                return null;
            }
        }

        /// <summary>
        /// Saves the project's build configuration.
        /// </summary>
        /// <param name="projectRoot">The path to the unity project root</param>
        /// <param name="config">An instance of <c>ProjectConfiguration</c></param>
        public static void SaveConfig(string projectRoot, Project config) {
            // Save the current project config.
            string configPath = Path.Combine(projectRoot, ProjectConfigPath);
            string configDir = Path.GetDirectoryName(configPath);
            if (!Directory.Exists(configDir)) {
                Directory.CreateDirectory(configDir);
            }
            using(StreamWriter w = new StreamWriter(configPath)) {
                w.Write(JsonUtility.ToJson(config, true));
            }
        }

        /// <summary>
        /// Verifies that no asset bundles share the same name.
        /// </summary>
        /// <returns><c>true</c> if there are duplicate asset bundle names, <c>false</c> otherwise.</returns>
        private static bool HasDuplicateAssetBundleNames() {
            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            return assetBundleNames.Distinct().Count() != assetBundleNames.Length;
        }

        /// <summary>
        /// Parses command line arguments to this build script and fills in the supplied variables.
        /// </summary>
        /// <param name="buildNumber"></param>
        /// <param name="buildType"></param>
        /// <param name="targets"></param>
        /// <param name="outputPath"></param>
        /// <param name="keystorePath"></param>
        /// <param name="keystorePassword"></param>
        /// <param name="keyaliasName"></param>
        /// <param name="keyaliasPassword"></param>
        private static void ParseArguments(out uint buildNumber, out BuildType buildType, out List<BuildTarget> targets, out string outputPath, out string keystorePath, out string keystorePassword, out string keyaliasName, out string keyaliasPassword, out string productName) {
            string[] args = Environment.GetCommandLineArgs();
            List<string> buildArgs = args.SkipWhile(arg => arg != "--").Skip(1).ToList();

            // Did the user even supply arguments?
            if (buildArgs.Count == 0) {
                throw new Exception("No build script command line arguments found. Pass them after a double hyphen '--' at the end of the unity command line.");
            }

            // Did the user supply a buildNumber argument?
            TryParseOption(
                buildArgs,
                "-buildNumber",
                e => {
                    uint tmp;
                    return uint.TryParse(e, out tmp);
                },
                e => {
                    uint tmp;
                    uint.TryParse(e, out tmp);
                    return tmp;
                },
                "You must supply an unsigned integer",
                out buildNumber
            );

            // Did the user supply a buildType argument?
            bool r = TryParseOption(
                buildArgs,
                "-buildType",
                e => Enum.IsDefined(typeof(BuildType), e),
                e => (BuildType) Enum.Parse(typeof(BuildType), e),
                String.Format("You must supply one of {0}", String.Join(", ", Enum.GetNames(typeof(BuildType)))),
                out buildType
            );
            if (!r) {
                buildType = BuildType.Release;
            }

            // Did the user supply a outputPath argument?
            TryParseOption(
                buildArgs,
                "-outputPath",
                e => true,
                e => e,
                "You must supply a path string",
                out outputPath
            );

            // Did the user supply keystore information?
            TryParseOption(
                buildArgs,
                "-keystorePath",
                e => true,
                e => e,
                "You must supply a path string",
                out keystorePath
            );
            TryParseOption(
                buildArgs,
                "-keystorePassword",
                e => true,
                e => e,
                "You must supply a string",
                out keystorePassword
            );
            TryParseOption(
                buildArgs,
                "-keyaliasName",
                e => true,
                e => e,
                "You must supply a string",
                out keyaliasName
            );
            TryParseOption(
                buildArgs,
                "-keyaliasPassword",
                e => true,
                e => e,
                "You must supply a string",
                out keyaliasPassword
            );
            TryParseOption(
                buildArgs,
                "-product",
                e => true,
                e => e,
                "You must supply a string,",
                out productName
                );

            // Did the user supply valid targets?
            if (buildArgs.Count == 0) {
                throw new Exception("No build target(s) found. You must specify at least one variant of UnityEditor.BuildTarget");
            } else {
                targets = buildArgs
                    .Select(userTarget => {
                        if (!Enum.IsDefined(typeof(BuildTarget), userTarget)) {
                            throw new Exception(String.Format("Invalid build target '{0}': You must supply one of {1}", userTarget, String.Join(", ", Enum.GetNames(typeof(BuildTarget)))));
                        } else {
                            return (BuildTarget) Enum.Parse(typeof(BuildTarget), userTarget);
                        }
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Given a list of arguments, parse the specified option (a named,
        /// optional argument). Removes the specified argument and it's value(s)
        /// from the list.
        /// </summary>
        /// <param name="args">A list of command line arguments</param>
        /// <param name="name">The option's name</param>
        /// <param name="validator">A function that takes the option's string value and validates it</param>
        /// <param name="converter">A function that takes the option's string value and converts it to the output type</param>
        /// <param name="error">An error string thrown if the option is specified, but invalid</param>
        /// <param name="value">The output value of the specified option or the default value</param>
        /// <typeparam name="T">The output type</typeparam>
        /// <returns><c>true</c> if the argument was parsed successfully, <c>false<c/> otherwise</returns>
        private static bool TryParseOption<T>(List<string> args, string name, Func<string, bool> validator, Func<string, T> converter, string error, out T value) {
            int idx = args.LastIndexOf(name);
            if (idx >= 0) {
                if (idx > args.Count - 2 || !validator(args[idx + 1])) {
                    throw new Exception(String.Format("Invalid '{0}' argument: {1}", name, error));
                }
                value = converter(args[idx + 1]);
                args.RemoveRange(idx, 2);
                return true;
            }

            value = default(T);
            return false;
        }
    }
}