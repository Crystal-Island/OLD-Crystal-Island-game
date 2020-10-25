using System;
using System.Collections.Generic;
using System.IO;
using KoboldTools.Helpers;
using UnityEditor;
using UnityEngine;

namespace KoboldTools.Building
{
    #region BuildStep Enums
    /// <summary>
    /// Enum Values to be stored or shown for fully customizable pipeline (includes iOS)
    /// </summary>
    public enum BuildSteps
    {
        FullPipeline            = 0,
        BuildOnly               = 1,
        BuildAndVerify          = 2
    }
    /// <summary>
    /// Enum Values for Android
    /// </summary>
    public enum BuildStepsAndroid
    {
        FullPipeline            = 0,
        BuildOnly               = 1
    }
   
    #endregion

    [Serializable]
    public class Product : ISerializationCallbackReceiver
    {
        public string InternalIdentifier;
        public string Title;
        public string Identifier;
        public List<string> Scenes;
        public List<string> StreamingAssetsWhitelist;
        public SplashSettings SplashDefinition;
        public List<BuildTargetDefinition> BuildTargets;

        [NonSerialized]
        public bool InvalidIdentifier;
        [NonSerialized]
        public bool Build;

        public Product()
        {
            this.InternalIdentifier = String.Empty;
            this.Title = String.Empty;
            this.Identifier = String.Empty;
            this.Scenes = new List<string>();
            this.StreamingAssetsWhitelist = new List<string>();
            this.SplashDefinition = new SplashSettings();
            this.InvalidIdentifier = false;
            this.Build = true;
            this.BuildTargets = new List<BuildTargetDefinition>();
            this.BuildTargets.Add(new BuildTargetDefinition());
        }

        public void OnAfterDeserialize() { }

        /// <summary>
        /// If marked with invalid identifier use GUID on serialization.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if(InvalidIdentifier)
                this.InternalIdentifier = GUID.Generate().ToString();
        }
    }

    [Serializable]
    public class Project : ISerializationCallbackReceiver
    {
        public string Name;
        public string UnityVersion;
        public SemVer Version;
        public List<Product> Products;
        public List<string> BuildQueue;

        public Project()
        {
            this.Name = String.Empty;
            this.Version = new SemVer();
            this.Products = new List<Product>();
            this.BuildQueue = new List<string>();
        }

        /// <summary>
        /// Fills buildqueue with marked product identifiers
        /// </summary>
        public void OnBeforeSerialize()
        {
            BuildQueue.Clear();
            for(int i = 0; i < Products.Count; i++)
            {
                if(Products[i].Build)
                    BuildQueue.Add(Products[i].InternalIdentifier);
            }
        }

        /// <summary>
        /// Sets build marks for products within buildqueue
        /// </summary>
        public void OnAfterDeserialize()
        {
            for(int i = 0; i < Products.Count; i++)
            {
                if(BuildQueue.Contains(Products[i].InternalIdentifier))
                {
                    Products[i].Build = true;
                }
                else
                {
                    Products[i].Build = false;
                }
            }
        }
    }

    /// <summary>
    /// SplashLogo Wrapper class for serialization
    /// </summary>
    [Serializable]
    public class SplashLogo
    {
        public string SpritePath;
        public float Duration;

        public SplashLogo()
        {
            this.SpritePath = String.Empty;
            this.Duration = 2f;
        }

        /// <summary>
        /// Creates and returns a Unity SplashScreenLogo
        /// </summary>
        /// <returns>SplashScreenLogo</returns>
        public PlayerSettings.SplashScreenLogo CreateLogo()
        {
            var logo = new PlayerSettings.SplashScreenLogo() { duration = this.Duration, logo = GetSprite() };
            return logo;
        }

        /// <summary>
        /// Returns Sprite Asset in path
        /// </summary>
        /// <returns>Sprite in path or null if not found</returns>
        public Sprite GetSprite()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }
    }

    [Serializable]
    public class SplashSettings
    {
        public List<SplashLogo> SplashLogos;
        public bool Show;
        public bool ShowUnity;
        public string DrawMode;
        public string LogoStyle;
        public string AnimationMode;
        public float Zoom;
        public float BGZoom;

        public float OverlayOpacity;
        public Color BackgroundColor;
        public string BackgroundPath;

        public SplashSettings()
        {
            this.SplashLogos = new List<SplashLogo>();
            this.Show = true;
            this.ShowUnity = true;
            this.DrawMode = PlayerSettings.SplashScreen.DrawMode.UnityLogoBelow.ToString();
            this.LogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark.ToString();
            this.AnimationMode = PlayerSettings.SplashScreen.AnimationMode.Dolly.ToString();
            this.Zoom = 1f;
            this.BGZoom = 1f;
            this.OverlayOpacity = 1f;
            this.BackgroundColor = new Color(0.1372549f, 0.1215686f, 0.1254902f, 1f);
            this.BackgroundPath = String.Empty;
        }
    }

    [Serializable]
    public class BuildTargetDefinition
    {
        public string Target;
        public string BuildSteps;

        public BuildTargetDefinition()
        {
            this.Target = EditorUserBuildSettings.activeBuildTarget.ToString();
            this.BuildSteps = Building.BuildSteps.FullPipeline.ToString();
        }
    }
}