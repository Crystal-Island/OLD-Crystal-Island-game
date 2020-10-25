using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System;
using System.Collections.Generic;
namespace KoboldTools
{
    public class BuildWindow : EditorWindow
    {
        private string _buildName = "KG";
        private string _companyName = "Koboldgames GmbH";
        private string[] _buildScenes = new string[] { "Assets/0_scenes/main.unity" };

        private string _configPath = "";
        private string _configFileName = "build.cfg";
        private string _versionFilePath = "";
        private string _versionFileName = "version";

        private string _buildPath = "";
        private int _buildVersionInt = 0;
        private string _buildVersionString = "";
        private string _buildVersionUIInput = "";
        private int _gameVersionInt = 0;
        private string _gameVersionString = "0";
        private string _gameVersionUIInput = "";
        private int _gameSubVersionInt = 1;
        private string _gameSubVersionString = "1";
        private string _gameSubVersionUIInput = "";


        private bool _developmentBuild = false;
        private bool _buildAndRun = false;

        private BuildTarget[] _buildPlatformList;

        private bool _win32 = false;
        private bool _win64 = false;
        private bool _osx = false;
        private bool _linux32 = false;
        private bool _linux64 = false;
        private bool _linuxuniversal = false;
        private bool _android = false;
        private bool _ios = false;
        private bool _webgl = false;

        [MenuItem("Build/Build Game", false, 10)]
        public static void openBuildWindow()
        {
            //show window
#pragma warning disable 0219
            BuildWindow window = (BuildWindow)EditorWindow.GetWindow(typeof(BuildWindow));
            //window.Show();
#pragma warning restore 0219
        }

        public void OnEnable()
        {
            _configPath = Application.dataPath + "/building/";
            _versionFilePath = Application.dataPath + "/building/";
        }

        public void OnFocus()
        {
            fetchBuildSettings();
        }

        public void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== Build '" + PlayerSettings.productName + "' ===", EditorStyles.boldLabel);
            GUILayout.Space(10);
            drawPlatformArea();
            GUILayout.Space(15);
            drawAddSettingsArea();
            GUILayout.Space(15);
            drawVersionArea();
            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("Save", GUILayout.Width(50)))
            {
                saveBuildSettings();
            }
            GUILayout.Space(20);
            if (!(_win32 || _win64 || _osx || _linux32 || _linux64 || _linuxuniversal || _android || _ios || _webgl))
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Save & Build", GUILayout.Width(200)))
            {
                prepareBuilding();

                //start build all versions
                for (int i = 0; i < _buildPlatformList.Length; i++)
                {
                    Debug.Log("Building... " + _buildPlatformList[i]);
                    build(_buildPlatformList[i]);
                }

                concludeBuilding();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void drawPlatformArea()
        {
            EditorGUILayout.LabelField("  Platform", EditorStyles.boldLabel);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    Windows:", GUILayout.Width(80));
            GUILayout.Space(10);
            _win32 = EditorGUILayout.ToggleLeft("x32", _win32, GUILayout.Width(80));
            GUILayout.Space(10);
            _win64 = EditorGUILayout.ToggleLeft("x64", _win64, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    OSX:", GUILayout.Width(80));
            GUILayout.Space(10);
            _osx = EditorGUILayout.ToggleLeft("osx", _osx, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    Linux:", GUILayout.Width(80));
            GUILayout.Space(10);
            _linux32 = EditorGUILayout.ToggleLeft("x32", _linux32, GUILayout.Width(80));
            GUILayout.Space(10);
            _linux64 = EditorGUILayout.ToggleLeft("x64", _linux64, GUILayout.Width(80));
            GUILayout.Space(10);
            _linuxuniversal = EditorGUILayout.ToggleLeft("x32 & x64", _linuxuniversal, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    Mobile:", GUILayout.Width(80));
            GUILayout.Space(10);
            _android = EditorGUILayout.ToggleLeft("android", _android, GUILayout.Width(80));
            GUILayout.Space(10);
            _ios = EditorGUILayout.ToggleLeft("ios", _ios, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("    Web:", GUILayout.Width(80));
            GUILayout.Space(10);
            _webgl = EditorGUILayout.ToggleLeft("wegl", _webgl, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void drawAddSettingsArea()
        {
            EditorGUILayout.LabelField("  Additional Settings", EditorStyles.boldLabel);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            _developmentBuild = EditorGUILayout.ToggleLeft("Dev. Build", _developmentBuild, GUILayout.Width(100));
            _buildAndRun = EditorGUILayout.ToggleLeft("Build and run", _buildAndRun, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void drawVersionArea()
        {
            EditorGUILayout.LabelField("  Version", EditorStyles.boldLabel);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("    Build version:", GUILayout.Width(100));
            _buildName = EditorGUILayout.TextField("", _buildName, GUILayout.Width(80));
            GUILayout.Space(10);
            GUILayout.Label("v", GUILayout.Width(10));
            _gameVersionUIInput = EditorGUILayout.TextField("", _gameVersionUIInput, GUILayout.Width(20));
            GUILayout.Label(".", GUILayout.Width(10));
            _gameSubVersionUIInput = EditorGUILayout.TextField("", _gameSubVersionUIInput, GUILayout.Width(20));
            GUILayout.Label(".", GUILayout.Width(10));
            _buildVersionUIInput = EditorGUILayout.TextField("", _buildVersionUIInput, GUILayout.Width(40));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        // Prepares the building.
        private void prepareBuilding()
        {
            /*Debug.Log(PlayerSettings.Android.keystoreName);
            PlayerSettings.Android.keystoreName = PlayerSettings.Android.keystoreName.Replace('\\', '/');
            Debug.Log(PlayerSettings.Android.keystoreName);

            Debug.Log(PlayerSettings.Android.keystorePass);
            PlayerSettings.Android.keystorePass = PlayerSettings.Android.keystorePass.Replace('\\', '/');
            Debug.Log(PlayerSettings.Android.keystorePass);

            Debug.Log(PlayerSettings.Android.keyaliasName);
            PlayerSettings.Android.keyaliasName = PlayerSettings.Android.keyaliasName.Replace('\\', '/');
            Debug.Log(PlayerSettings.Android.keyaliasName);

            Debug.Log(PlayerSettings.Android.keyaliasPass);
            PlayerSettings.Android.keyaliasPass = PlayerSettings.Android.keyaliasPass.Replace('\\', '/');
            Debug.Log(PlayerSettings.Android.keyaliasPass);*/

            //save scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            string platformConfigString = "";

            //update platform list
            List<BuildTarget> tempBuildPlatformList = new List<BuildTarget>();
            if (_win32)
            {
                tempBuildPlatformList.Add(BuildTarget.StandaloneWindows);
                platformConfigString += "StandaloneWindows;";
            }
            if (_win64)
            {
                tempBuildPlatformList.Add(BuildTarget.StandaloneWindows64);
                platformConfigString += "StandaloneWindows64;";
            }
            if (_osx)
            {
                tempBuildPlatformList.Add(BuildTarget.StandaloneOSX);
                platformConfigString += "StandaloneOSX;";
            }
            if (_linux32)
            {
                tempBuildPlatformList.Add(BuildTarget.StandaloneLinux);
                platformConfigString += "StandaloneLinux;";
            }
            if (_linux64)
            {
                tempBuildPlatformList.Add(BuildTarget.StandaloneLinux64);
                platformConfigString += "StandaloneLinux64;";
            }
            if (_linuxuniversal)
            {
                tempBuildPlatformList.Add(BuildTarget.StandaloneLinuxUniversal);
                platformConfigString += "StandaloneLinuxUniversal;";
            }
            if (_android)
            {
                tempBuildPlatformList.Add(BuildTarget.Android);
                platformConfigString += "Android;";
            }
            if (_ios)
            {
                tempBuildPlatformList.Add(BuildTarget.iOS);
                platformConfigString += "iOS;";
            }
            if (_webgl)
            {
                tempBuildPlatformList.Add(BuildTarget.WebGL);
                platformConfigString += "WebGL;";
            }
            platformConfigString = platformConfigString.Remove(platformConfigString.Length - 1);
            FileAccessHelpers.writeToConfigFile(_configPath + _configFileName, "buildPlatformList", platformConfigString);
            _buildPlatformList = tempBuildPlatformList.ToArray();

            fetchBuildSettings();

            //generate version file
            List<string> versionFile = FileAccessHelpers.readFile(_versionFilePath + _versionFileName);
            for (int i = 0; i < versionFile.Count; i++)
            {
                string[] commands = versionFile[i].Split('=');
                if (commands[0].Equals("buildVersion"))
                {
                    _buildVersionInt = Int32.Parse(commands[1]);
                    versionFile[i] = "buildVersion=" + _buildVersionInt;
                    _buildVersionString = "" + _buildVersionInt;
                    while (_buildVersionString.Length < 3)
                    {
                        _buildVersionString = "0" + _buildVersionString;
                    }
                }
                else if (commands[0].Equals("gameVersion"))
                {
                    _gameVersionInt = Int32.Parse(commands[1]);
                    versionFile[i] = "gameVersion=" + _gameVersionInt;
                    _gameVersionString = "" + _gameVersionInt;
                }
                else if (commands[0].Equals("gameSubVersion"))
                {
                    _gameSubVersionInt = Int32.Parse(commands[1]);
                    versionFile[i] = "gameSubVersion=" + _gameSubVersionInt;
                    _gameSubVersionString = "" + _gameSubVersionInt;
                }
                else if (commands[0].Equals("unityVersion"))
                {
                    versionFile[i] = "unityVersion=" + Application.unityVersion;
                }
                else if (commands[0].Equals("builddate"))
                {
                    versionFile[i] = "builddate=" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                }
            }
            //write file
            FileAccessHelpers.writeFile(versionFile, _versionFilePath + _versionFileName);
            //write copy to streaming asset folder
            FileAccessHelpers.writeFile(versionFile, Application.streamingAssetsPath + "/version");
            //set mobile publishing version numbers
            PlayerSettings.companyName = _companyName;
            PlayerSettings.bundleVersion = _gameVersionString + "." + _gameSubVersionString + "." + _buildVersionString;
            PlayerSettings.Android.bundleVersionCode = Int32.Parse((_gameVersionString + _gameSubVersionString + _buildVersionString));
            PlayerSettings.iOS.buildNumber = _gameVersionString + _gameSubVersionString + _buildVersionString;
        }

        // Builds the game to the specified targetPlatform.
        private void build(BuildTarget targetPlatform)
        {
            string extention = "";
            string buildPlatform = "";
            string buildName = _buildName;
            //build
            switch (targetPlatform)
            {
                case BuildTarget.StandaloneWindows:
                    extention = ".exe";
                    buildPlatform = "WIN";
                    break;
                case BuildTarget.StandaloneWindows64:
                    extention = ".exe";
                    buildPlatform = "WIN64";
                    break;
                case BuildTarget.StandaloneOSX:
                    extention = ".app";
                    buildPlatform = "OSX";
                    break;
                case BuildTarget.StandaloneLinux:
                    buildPlatform = "LINUX";
                    break;
                case BuildTarget.StandaloneLinux64:
                    buildPlatform = "LINUX64";
                    break;
                case BuildTarget.StandaloneLinuxUniversal:
                    buildPlatform = "LINUXUNIVERSAL";
                    break;
                case BuildTarget.WebGL:
                    buildPlatform = "WEBGL";
                    break;
                case BuildTarget.Android:
                    extention = ".apk";
                    buildPlatform = "ANDROID";
                    break;
                case BuildTarget.iOS:
                    buildPlatform = "IOS";
                    break;
                default:
                    Debug.LogError("PLATFORM NOT SUPPORTED. BUILD MAY NOT WORK.");
                    break;
            }

            //set buildpath
            _buildPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/Builds";
            string newBuildDirectory = _buildPath + "/" + buildName + "_" + buildPlatform + "_v" + _gameVersionString + "." + _gameSubVersionString + "." + _buildVersionString;

            //re-create folder
            if (Directory.Exists(newBuildDirectory))
            {
                Directory.Delete(newBuildDirectory, true);
            }
            Directory.CreateDirectory(newBuildDirectory);

            //build asset bundles
            BuildAssetBundles.buildAssetBundles(targetPlatform, true, false);

            //execute build
            if (_developmentBuild)
            {
                BuildPipeline.BuildPlayer(_buildScenes, newBuildDirectory + "/" + buildName + extention, targetPlatform, BuildOptions.Development | (_buildAndRun ? BuildOptions.AutoRunPlayer : BuildOptions.None));
            }
            else
            {
                BuildPipeline.BuildPlayer(_buildScenes, newBuildDirectory + "/" + buildName + extention, targetPlatform, _buildAndRun ? BuildOptions.AutoRunPlayer : BuildOptions.None);
            }
        }

        private void saveBuildSettings()
        {
            FileAccessHelpers.writeToConfigFile(_configPath + _configFileName, "buildName", _buildName);
            FileAccessHelpers.writeToConfigFile(_versionFilePath + _versionFileName, "gameVersion", (Int32.Parse(_gameVersionUIInput)).ToString());
            FileAccessHelpers.writeToConfigFile(_versionFilePath + _versionFileName, "gameSubVersion", (Int32.Parse(_gameSubVersionUIInput)).ToString());
            FileAccessHelpers.writeToConfigFile(_versionFilePath + _versionFileName, "buildVersion", (Int32.Parse(_buildVersionUIInput)).ToString());
            prepareBuilding();
            fetchBuildSettings();
        }

        private void fetchBuildSettings()
        {

            //create files if they don't exist
            if (!Directory.Exists(_versionFilePath))
            {
                Directory.CreateDirectory(_versionFilePath);
            }
            if (!File.Exists(_versionFilePath + _versionFileName))
            {
                List<string> versionLines = new List<string>();
                versionLines.Add("gameVersion=" + _gameVersionString);
                versionLines.Add("gameSubVersion=" + _gameSubVersionString);
                versionLines.Add("buildVersion=0");
                versionLines.Add("unityVersion=");
                versionLines.Add("builddate=");
                FileAccessHelpers.writeFile(versionLines, _versionFilePath + _versionFileName);
            }
            if (!Directory.Exists(_configPath))
            {
                Directory.CreateDirectory(_configPath);
            }
            if (!File.Exists(_configPath + _configFileName))
            {
                //create & write config file
                List<string> buildSettingsLines = new List<string>();
                buildSettingsLines.Add("buildName=NONAME");
                buildSettingsLines.Add("mainScene=Assets/0_scenes/main.unity");
                buildSettingsLines.Add("buildPlatformList=StandaloneWindows;StandaloneOSXIntel");
                FileAccessHelpers.writeFile(buildSettingsLines, _configPath + _configFileName);
            }

            List<string> tempBuildSceneList = new List<string>();
            List<BuildTarget> tempBuildPlatformList = new List<BuildTarget>();

            //fetch version for display
            _gameVersionUIInput = FileAccessHelpers.readFromConfigFile(_versionFilePath + _versionFileName, "gameVersion");
            _gameSubVersionUIInput = FileAccessHelpers.readFromConfigFile(_versionFilePath + _versionFileName, "gameSubVersion");
            _buildVersionUIInput = FileAccessHelpers.readFromConfigFile(_versionFilePath + _versionFileName, "buildVersion");
            while (_buildVersionUIInput.Length < 3)
            {
                _buildVersionUIInput = "0" + _buildVersionUIInput;
            }

            string tempBuildName = FileAccessHelpers.readFromConfigFile(_configPath + _configFileName, "buildName");
            string tempMainPathString = FileAccessHelpers.readFromConfigFile(_configPath + _configFileName, "mainScene");
            string tempBuildPlatformListString = FileAccessHelpers.readFromConfigFile(_configPath + _configFileName, "buildPlatformList");

            if (tempBuildName != "")
            {
                _buildName = tempBuildName;
            }
            else
            {
                FileAccessHelpers.writeToConfigFile(_configPath + _configFileName, "buildName", "NONAME");
                _buildName = "NONAME";
            }

            if (tempMainPathString != "")
            {
                string[] scenes = tempMainPathString.Split(';');
                for (int i = 0; i < scenes.Length; i++)
                {
                    tempBuildSceneList.Add(scenes[i]);
                }
            }
            else
            {
                FileAccessHelpers.writeToConfigFile(_configPath + _configFileName, "mainScene", "Assets/0_Scenes/main.unity");
                tempBuildSceneList.Add("Assets/0_Scenes/main.unity");
            }

            if (tempBuildPlatformListString != "")
            {
                string[] platforms = tempBuildPlatformListString.Split(';');
                for (int i = 0; i < platforms.Length; i++)
                {
                    try
                    {
                        tempBuildPlatformList.Add((BuildTarget)Enum.Parse(typeof(BuildTarget), platforms[i], true));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Build config parse error. - " + e);
                    }
                }
            }
            else
            {
                FileAccessHelpers.writeToConfigFile(_configPath + _configFileName, "buildPlatformList", "StandaloneWindows");
                tempBuildPlatformList.Add(BuildTarget.StandaloneWindows);
            }

            _buildScenes = tempBuildSceneList.ToArray();
            _buildPlatformList = tempBuildPlatformList.ToArray();

            //update ui
            for (int i = 0; i < _buildPlatformList.Length; i++)
            {
                switch (_buildPlatformList[i])
                {
                    case BuildTarget.StandaloneWindows:
                        _win32 = true;
                        break;
                    case BuildTarget.StandaloneWindows64:
                        _win64 = true;
                        break;
                    case BuildTarget.StandaloneOSX:
                        _osx = true;
                        break;
                    case BuildTarget.StandaloneLinux:
                        _linux32 = true;
                        break;
                    case BuildTarget.StandaloneLinux64:
                        _linux64 = true;
                        break;
                    case BuildTarget.StandaloneLinuxUniversal:
                        _linuxuniversal = true;
                        break;
                    case BuildTarget.WebGL:
                        _webgl = true;
                        break;
                    case BuildTarget.Android:
                        _android = true;
                        break;
                    case BuildTarget.iOS:
                        _ios = true;
                        break;
                }
            }
        }

        private void concludeBuilding()
        {
            _buildVersionUIInput = (Int32.Parse(_buildVersionUIInput) + 1).ToString();
            saveBuildSettings();
        }
    }
}
