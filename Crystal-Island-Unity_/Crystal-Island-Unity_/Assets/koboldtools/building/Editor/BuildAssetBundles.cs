using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
namespace KoboldTools
{
    public class BuildAssetBundles
    {

        [MenuItem("Build/Build Asset Bundles (Current, uncompressed) %#&b")]
        public static void build()
        {
            buildAssetBundles(EditorUserBuildSettings.activeBuildTarget, false, false);
        }

        [MenuItem("Build/Build Asset Bundles (Windows)")]
        public static void buildWindows()
        {
            buildAssetBundles(BuildTarget.StandaloneWindows, true, true);
        }

        [MenuItem("Build/Build Asset Bundles (OSX)")]
        public static void buildOSX()
        {
            buildAssetBundles(BuildTarget.StandaloneOSX, true, false);
        }

        [MenuItem("Build/Build Asset Bundles (Android)")]
        public static void buildAndroid()
        {
            buildAssetBundles(BuildTarget.Android, true, false);
        }

        [MenuItem("Build/Build Asset Bundles (iOS)")]
        public static void buildIOS()
        {
            buildAssetBundles(BuildTarget.iOS, true, false);
        }

        public static void buildAssetBundles(BuildTarget inTargetPlatform, bool enableCompression = true, bool doTestBuildCopy = false)
        {
            string outputPath = Path.Combine(Application.streamingAssetsPath, "bundles");

            //re-create folder
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (!hasDuplicateAssetBundleNames())
            {
                //build
                if (enableCompression)
                {
                    BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, inTargetPlatform);
                }
                else
                {
                    BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle, inTargetPlatform);
                }

                if (doTestBuildCopy)
                {
                    //make copy top test build path
                    string testBuildPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "/DevBuild/DevBuild_Data/StreamingAssets");
                    try
                    {
                        Directory.Delete(testBuildPath, true);
                        FileAccessHelpers.directoryCopy(outputPath, testBuildPath, true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Copying of assetbundles to editor build failed. - " + e.ToString());
                    }
                }
            }
            else
            {
                Debug.LogError("Building aborted. Duplicate assetbundle names.");
            }
        }

        private static bool hasDuplicateAssetBundleNames()
        {
            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            //check
            return !(assetBundleNames.Distinct().Count() == assetBundleNames.Length);
        }
    }
}