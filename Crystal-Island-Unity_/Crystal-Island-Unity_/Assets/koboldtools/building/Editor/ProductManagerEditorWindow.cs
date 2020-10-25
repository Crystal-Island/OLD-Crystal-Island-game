using KoboldTools.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace KoboldTools
{
    public class ProductManagerEditorWindow : EditorWindow
    {
        private const string MENU_PATH = "Build/Product Manager...";
        private const string WINDOW_TITLE = "Build Product Manager";
        private const string PRODUCTS_HEADER = "Products & Buildqueue";
        private const string PRODUCTS_LIST_HEADER = "Products";
        private const string BUILDTARGET_LIST_HEADER = "Build Target Settings";
        private const string ASSET_LIST_HEADER = "Asset Whitelist";
        private const string SCENE_LIST_HEADER = "Scenes";
        private const string SPLASH_HEADER = "Splash Screen";
        private const string SPLASH_LIST_HEADER = "Splash Logos";
        private const string DEFAULT_SETTINGS_NAME = "project.json";

        private const float PRODUCTS_WIDTH = 250f;
        private const float COMMON_WIDTH = 250f;
        private const float SPLASH_MIN_WIDTH = 250f;
        private const float SPLASH_MAX_WIDTH = 350f;
        
        /// <summary>
        /// Shorthand Property for direct product access;
        /// </summary>
        private Building.Product Product
        {
            get
            {
                return productList.list[productList.index] as Building.Product;
            }
        }

        private Vector2 scrollPosScenes = Vector2.zero;
        private Vector2 scrollPosLogos = Vector2.zero;
        private Vector2 scrollPosAssets = Vector2.zero;
        private Vector2 scrollPosBuildTargets = Vector2.zero;

        private static ReorderableList productList;
        private ReorderableList sceneList;
        private ReorderableList splashList;
        private ReorderableList assetList;
        private ReorderableList buildTargetList;

        private static bool hasChanges;
        private static Building.Project project;

        [MenuItem(MENU_PATH)]
        public static void OpenProductWindow()
        {
            var wnd = GetWindow<ProductManagerEditorWindow>(WINDOW_TITLE);
            wnd.Show();
        }

        /// <summary>
        /// Load settings file
        /// </summary>
        /// <param name="forceLoad">enforce load even if already loaded</param>
        private void LoadSettings(bool forceLoad = false)
        {
            if(project == null || forceLoad)
            {
                hasChanges = false;
                var root = Building.ContinuousIntegration.GetProjectRoot();
                if(String.IsNullOrEmpty(root))
                    throw new Exception("Could not determine the project root directory");

                project = Building.ContinuousIntegration.LoadConfig(root);
                
                // Create new config if no config file could get loaded
                if(project == null)
                {
                    project = new Building.Project();
                    project.Products.Add(new Building.Product());
                    hasChanges = true;
                }
            }
            productList.list = project.Products;
            productList.index = 0;
            productList.displayRemove = productList.list.Count > 1;
            OnProductSelect(productList);
        }

        private void OnEnable()
        {
            if(productList == null)
                productList = new ReorderableList(null, typeof(Building.Product), false, true, true, true);

            productList.drawHeaderCallback += DrawProductHeader;
            productList.drawElementCallback += DrawProductElement;
            productList.onSelectCallback += OnProductSelect;
            productList.onAddCallback += AddProduct;
            productList.onRemoveCallback += RemoveProduct;

            LoadSettings();

            if(buildTargetList != null)
            {
                buildTargetList.drawElementCallback += DrawBuildTargetElement;
                buildTargetList.onAddCallback += AddBuildTarget;
                buildTargetList.onRemoveCallback += RemoveBuildTarget;

                buildTargetList.elementHeight = EditorGUIUtility.singleLineHeight * 2;
            }
            
            if(sceneList != null)
            {
                sceneList.drawElementCallback += DrawSceneElement;
                sceneList.onAddCallback += AddScene;
                sceneList.onRemoveCallback += RemoveScene;
            }

            if(assetList != null)
            {
                assetList.drawElementCallback += DrawAssetElement;
                assetList.onAddCallback += AddAsset;
                assetList.onRemoveCallback += RemoveAsset;
            }

            if(splashList != null)
            {
                splashList.drawElementCallback += DrawSplashElement;
                splashList.onAddCallback += AddSplash;
                splashList.onRemoveCallback += RemoveSplash;

                splashList.elementHeight = EditorGUIUtility.singleLineHeight * 4;
            }
        }

        private void OnDisable()
        {
            productList.drawHeaderCallback -= DrawProductHeader;
            productList.drawElementCallback -= DrawProductElement;
            productList.onSelectCallback -= OnProductSelect;
            productList.onAddCallback -= AddProduct;
            productList.onRemoveCallback -= RemoveProduct;
            
            sceneList.drawElementCallback -= DrawSceneElement;
            sceneList.onAddCallback -= AddScene;
            sceneList.onRemoveCallback -= RemoveScene;
            
            assetList.drawElementCallback -= DrawAssetElement;
            assetList.onAddCallback -= AddAsset;
            assetList.onRemoveCallback -= RemoveAsset;
            
            splashList.drawElementCallback -= DrawSplashElement;
            splashList.onAddCallback -= AddSplash;
            splashList.onRemoveCallback -= RemoveSplash;
            
            buildTargetList.drawElementCallback -= DrawBuildTargetElement;
            buildTargetList.onAddCallback -= AddBuildTarget;
            buildTargetList.onRemoveCallback -= RemoveBuildTarget;
        }

        private static bool showProjectInformation = true;

        private void OnGUI()
        {
            object oldVal;
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawToolStrip();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            // TODO: WIP section
            #region ProjectInfo

            GUILayout.BeginVertical();
            DrawHeaderLabel("ProjectSettings", Color.gray, ref showProjectInformation);
            if(showProjectInformation)
            {

                GUILayout.BeginHorizontal();

                GUILayout.Label("Name", GUILayout.Width(90f));
                oldVal = project.Name;
                project.Name = GUILayout.TextField(project.Name, GUILayout.Width(145f));
                hasChanges = hasChanges || oldVal.ToString() != project.Name;

                GUILayout.EndHorizontal();

                GUIContent gcVersion = new GUIContent();
                gcVersion.image = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
                gcVersion.text = "Version";

                var rect = GUILayoutUtility.GetRect(200f, gcVersion.image.height);
                GUI.Box(rect, "");
                GUI.Label(rect, gcVersion, EditorStyles.boldLabel);
                rect = new Rect(rect.x + gcVersion.image.width + 70f, rect.y + gcVersion.image.height / 3f - 3f,
                rect.width - gcVersion.image.width, rect.height - gcVersion.image.height / 3f);
                GUI.Label(rect, $"{project.Version.Internal()} / {project.UnityVersion}");

            }
            GUILayout.EndVertical();

            #endregion
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            #region GUI - Products
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(PRODUCTS_WIDTH));
            GUILayout.Space(5f);
            DrawHeaderLabel(PRODUCTS_HEADER);
            productList.DoLayoutList();
            GUILayout.EndVertical();
            #endregion

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            #region GUI - <PRODUCT>/Common
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(COMMON_WIDTH));
            DrawHeaderLabel(Product.Title);
            GUILayout.Label("Common", EditorStyles.boldLabel);
            GUILayout.Label("Title (ProductName):");
            oldVal = Product.Title;
            Product.Title = GUILayout.TextField(Product.Title);
            hasChanges = hasChanges || oldVal.ToString() != Product.Title;
            GUILayout.Label("Internal Identifier:");
            oldVal = Product.InternalIdentifier;

            //check for duplicate internal identifier value in other products
            var style = new GUIStyle(EditorStyles.textField);
            if(Product.InvalidIdentifier)
            {
                style.normal.textColor = Color.red;
                style.focused.textColor = Color.red;
            }
            Product.InternalIdentifier = GUILayout.TextField(Product.InternalIdentifier, style);
            if(Product.InvalidIdentifier)
            {
                EditorGUILayout.HelpBox("Internal Product Identifier must be unique per project and cannot contain Whitepsaces nor a '\"'. This value will be overwritten with a generic GUID if saved!", MessageType.Error);
                Product.InvalidIdentifier = true;
            }
            else
            {
                Product.InvalidIdentifier = false;
            }
            hasChanges = hasChanges || oldVal.ToString() != Product.InternalIdentifier;
            GUILayout.Label("Bundle Identifier:");
            oldVal = Product.Identifier;
            Product.Identifier = GUILayout.TextField(Product.Identifier);
            hasChanges = hasChanges || oldVal.ToString() != Product.Identifier;
            
            GUILayout.Label(BUILDTARGET_LIST_HEADER, EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Note: This is a preview feature. Multiplatform building is not implemented yet. It's mainly for Mobile builds at this moment.\r\nIf iOS/Android is not selected on these, full pipeline will be used as by default.", MessageType.Info);
            scrollPosBuildTargets = GUILayout.BeginScrollView(scrollPosBuildTargets, false, false, GUILayout.MaxHeight(187.5f));
            if(buildTargetList != null)
                buildTargetList.DoLayoutList();
            GUILayout.EndScrollView();

            GUILayout.Space(40f);

            GUILayout.Label(ASSET_LIST_HEADER, EditorStyles.boldLabel);
            scrollPosAssets = GUILayout.BeginScrollView(scrollPosAssets, false, false, GUILayout.MaxHeight(187.5f));
            if(assetList != null)
                assetList.DoLayoutList();
            GUILayout.EndScrollView();

            GUILayout.Label(SCENE_LIST_HEADER, EditorStyles.boldLabel);
            scrollPosScenes = GUILayout.BeginScrollView(scrollPosScenes, false, false, GUILayout.MaxHeight(187.5f));
            if(sceneList != null)
                sceneList.DoLayoutList();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            #endregion

            #region GUI - Splash Screen

            GUILayout.BeginVertical(EditorStyles.helpBox, LayoutAutoMaxMinWidth(SPLASH_MIN_WIDTH, SPLASH_MAX_WIDTH));

            DrawHeaderLabel(SPLASH_HEADER);
            GUILayout.Label("Common", EditorStyles.boldLabel);
            oldVal = Product.SplashDefinition.Show;
            Product.SplashDefinition.Show = GUILayout.Toggle(Product.SplashDefinition.Show, "Show Splashscreen");
            hasChanges = hasChanges || (bool)oldVal != Product.SplashDefinition.Show;

            GUILayout.Label("Splash Style");
            PlayerSettings.SplashScreen.UnityLogoStyle splashStyle;
            Enum35.TryParse(Product.SplashDefinition.LogoStyle, out splashStyle);
            oldVal = Product.SplashDefinition.LogoStyle;
            Product.SplashDefinition.LogoStyle = EditorGUILayout.EnumPopup(splashStyle).ToString();
            hasChanges = hasChanges || oldVal.ToString() != Product.SplashDefinition.LogoStyle;

            GUILayout.Label("Animation");
            PlayerSettings.SplashScreen.AnimationMode animMode;
            Enum35.TryParse(Product.SplashDefinition.AnimationMode, out animMode);
            oldVal = Product.SplashDefinition.AnimationMode;
            Product.SplashDefinition.AnimationMode = EditorGUILayout.EnumPopup(animMode).ToString();
            hasChanges = hasChanges || oldVal.ToString() != Product.SplashDefinition.AnimationMode;

            if(animMode == PlayerSettings.SplashScreen.AnimationMode.Custom)
            {
                GUILayout.Label("Logo Zoom");
                oldVal = Product.SplashDefinition.Zoom;
                Product.SplashDefinition.Zoom = EditorGUILayout.Slider(Product.SplashDefinition.Zoom, 0f, 1f);
                hasChanges = hasChanges || (float)oldVal != Product.SplashDefinition.Zoom;
                GUILayout.Label("Background Zoom");
                oldVal = Product.SplashDefinition.BGZoom;
                Product.SplashDefinition.BGZoom = EditorGUILayout.Slider(Product.SplashDefinition.BGZoom, 0f, 1f);
                hasChanges = hasChanges || (float)oldVal != Product.SplashDefinition.BGZoom;
            }

            GUILayout.Label("Unity Logo & Draw Mode");

            GUILayout.BeginHorizontal();
            oldVal = Product.SplashDefinition.ShowUnity;
            Product.SplashDefinition.ShowUnity = GUILayout.Toggle(Product.SplashDefinition.ShowUnity, "");
            hasChanges = hasChanges || (bool)oldVal != Product.SplashDefinition.ShowUnity;

            GUI.enabled = Product.SplashDefinition.ShowUnity;
            PlayerSettings.SplashScreen.DrawMode drawMode;
            Enum35.TryParse(Product.SplashDefinition.DrawMode, out drawMode);
            oldVal = Product.SplashDefinition.DrawMode;
            Product.SplashDefinition.DrawMode = EditorGUILayout.EnumPopup(drawMode).ToString();
            hasChanges = hasChanges || oldVal.ToString() != Product.SplashDefinition.DrawMode;
            GUI.enabled = true;

            GUILayout.EndHorizontal();
            
            GUILayout.Label(SPLASH_LIST_HEADER,EditorStyles.boldLabel);
            scrollPosLogos = GUILayout.BeginScrollView(scrollPosLogos, false, false);
            if(splashList != null)
                splashList.DoLayoutList();

            GUILayout.EndScrollView();

            GUILayout.Space(5f);
            GUILayout.Label("Background", EditorStyles.boldLabel);
            GUILayout.Label("Overlay Opacity");
            oldVal = Product.SplashDefinition.OverlayOpacity;
            Product.SplashDefinition.OverlayOpacity = EditorGUILayout.Slider(Product.SplashDefinition.OverlayOpacity, 0f, 1f);
            hasChanges = hasChanges || (float)oldVal != Product.SplashDefinition.OverlayOpacity;
            oldVal = Product.SplashDefinition.BackgroundColor;
            Product.SplashDefinition.BackgroundColor = EditorGUILayout.ColorField("Background Color", Product.SplashDefinition.BackgroundColor);
            hasChanges = hasChanges || (Color)oldVal != Product.SplashDefinition.BackgroundColor;

            oldVal = Product.SplashDefinition.BackgroundPath;
            Product.SplashDefinition.BackgroundPath =
                AssetDatabase.GetAssetPath(
                    (EditorGUILayout.ObjectField("Background Image",
                        AssetDatabase.LoadAssetAtPath<Sprite>(Product.SplashDefinition.BackgroundPath),
                        typeof(Sprite), false) as Sprite)
            );
            hasChanges = hasChanges || oldVal.ToString() != Product.SplashDefinition.BackgroundPath;

            GUILayout.EndVertical();

            #endregion

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        private void DrawToolStrip()
        {
            var menu = new GenericMenu();
            Rect pos;
            if(GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(100f)))
            {
                menu.AddItem(new GUIContent("Save"), false, SaveSettings);
                menu.AddItem(new GUIContent("Reload Settings"), false, ForceLoadSettings);
                menu.AddSeparator(String.Empty);
                menu.AddItem(new GUIContent("Import..."), false, ImportSettings);
                menu.AddItem(new GUIContent("Export..."), false, ExportSettings);
                //TODO:Get last rect buggy?
                pos = GUILayoutUtility.GetLastRect();
                pos.position += Vector2.up * EditorGUIUtility.singleLineHeight;
                menu.DropDown(pos);
            }
            if(GUILayout.Button("Product", EditorStyles.toolbarDropDown, GUILayout.Width(100f)))
            {
                menu.AddItem(new GUIContent("Apply"), false, ApplySettings);
                menu.AddItem(new GUIContent("Read from Project"), false, ReadFromProjectSettings);
                //TODO:Get last rect buggy?
                pos = GUILayoutUtility.GetLastRect();
                pos.position += Vector2.up * EditorGUIUtility.singleLineHeight;
                menu.DropDown(pos);
            }
            var changeText = "";
            if(hasChanges)
                changeText = "Unsaved changes";

            GUIStyle saveIndicatorStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            saveIndicatorStyle.margin.top = 2;
            GUILayout.Label(changeText, saveIndicatorStyle, GUILayout.Width(150f) );
            GUILayout.FlexibleSpace();
        }

        #region Menus
        /// <summary>
        /// Save all product to config path
        /// </summary>
        private void SaveSettings()
        {
            project.Products = productList.list as List<Building.Product>;
            Building.ContinuousIntegration.SaveConfig(Building.ContinuousIntegration.GetProjectRoot(), project);
            hasChanges = false;
        }

        private void ForceLoadSettings()
        {
            LoadSettings(true);
            hasChanges = false;
        }

        /// <summary>
        /// Reads current project settings into new Product definition
        /// </summary>
        private void ReadFromProjectSettings()
        {
            AddProduct(productList);
            Product.Identifier = PlayerSettings.applicationIdentifier;
            Product.Title = PlayerSettings.productName;
            for(int i = 0; i < EditorBuildSettings.scenes.Length;i++)
            {
                Product.Scenes.Add(EditorBuildSettings.scenes[i].path);
            }
            Product.SplashDefinition.AnimationMode = PlayerSettings.SplashScreen.animationMode.ToString();
            Product.SplashDefinition.BackgroundColor = PlayerSettings.SplashScreen.backgroundColor;
            Product.SplashDefinition.BackgroundPath = AssetDatabase.GetAssetPath(PlayerSettings.SplashScreen.background);
            Product.SplashDefinition.BGZoom = PlayerSettings.SplashScreen.animationBackgroundZoom;
            Product.SplashDefinition.DrawMode = PlayerSettings.SplashScreen.drawMode.ToString();
            Product.SplashDefinition.LogoStyle = PlayerSettings.SplashScreen.unityLogoStyle.ToString();
            Product.SplashDefinition.OverlayOpacity = PlayerSettings.SplashScreen.overlayOpacity;
            Product.SplashDefinition.Show = PlayerSettings.SplashScreen.show;
            Product.SplashDefinition.ShowUnity = PlayerSettings.SplashScreen.showUnityLogo;
            Product.SplashDefinition.Zoom = PlayerSettings.SplashScreen.animationLogoZoom;
            for(int i = 0; i < PlayerSettings.SplashScreen.logos.Length;i++)
            {
                Product.SplashDefinition.SplashLogos.Add(
                    new Building.SplashLogo()
                    {
                        Duration = PlayerSettings.SplashScreen.logos[i].duration,
                        SpritePath = AssetDatabase.GetAssetPath(PlayerSettings.SplashScreen.logos[i].logo)
                    });
            }
            OnProductSelect(productList);
        }

        /// <summary>
        /// Applies selected Product definition to current project
        /// </summary>
        private void ApplySettings()
        {
            PlayerSettings.applicationIdentifier = Product.Identifier;
            PlayerSettings.productName = Product.Title;
            var scenes = new List<EditorBuildSettingsScene>();
            for(int i = 0; i < Product.Scenes.Count;i++)
            {
                scenes.Add(new EditorBuildSettingsScene(Product.Scenes[i],true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            Building.ContinuousIntegration.ApplySplashScreenSettings(Product);
        }

        /// <summary>
        /// Import config from file
        /// </summary>
        private void ImportSettings()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Import settings...", "",
                new string[] { "Project Settings", "json" });
            if(String.IsNullOrEmpty(path))
                return;

            if(System.IO.File.Exists(path))
            {
                using(var r = new System.IO.StreamReader(path, System.Text.Encoding.UTF8))
                {
                    project = JsonUtility.FromJson<Building.Project>(r.ReadToEnd());
                }
                productList.index = 0;
                productList.list = project.Products;
                OnProductSelect(productList);
                hasChanges = true;
            }
        }

        /// <summary>
        /// Epxort config to target file
        /// </summary>
        private void ExportSettings()
        {
            project.Products = productList.list as List<Building.Product>;
            var path = EditorUtility.SaveFilePanel("Export settings to...", "", DEFAULT_SETTINGS_NAME, "json");
            if(String.IsNullOrEmpty(path))
                return;
                
            using(var w = new System.IO.StreamWriter(path))
            {
                w.Write(JsonUtility.ToJson(project, true));
            }
        }
        #endregion

        #region Product Callbacks
        private void DrawProductHeader(Rect rect)
        {
            GUI.Label(rect, PRODUCTS_LIST_HEADER, EditorStyles.boldLabel);
        }

        private void DrawProductElement(Rect rect, int index, bool active, bool focused)
        {
            var style = new GUIStyle(EditorStyles.label);
            var product = (productList.list[index] as Building.Product);
            if(String.IsNullOrEmpty(product.InternalIdentifier)
                || CheckForDuplicateInternalIdentifier(product.InternalIdentifier)
                || product.InternalIdentifier.Any(c => Char.IsWhiteSpace(c) || c == '"'))
            {
                style.normal.textColor = Color.red;
                style.focused.textColor = Color.red;
                product.InvalidIdentifier = true;
            }
            else
            {
                product.InvalidIdentifier = false;
            }
            product.Build = EditorGUI.Toggle(
                new Rect(rect.position.x,rect.position.y,20f,EditorGUIUtility.singleLineHeight),
                product.Build);
            EditorGUI.LabelField(new Rect(rect.position.x+20f,rect.position.y, rect.width-25f,rect.height),
                $"{product.Title} [{product.InternalIdentifier}]",
                style);

            if(CheckForDuplicateIdentifier(product.Identifier))
            {
                GUIContent gc = new GUIContent();
                gc.image = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
                gc.tooltip = "Product uses duplicate BundleIdentifier. This configuration may fail the Jenkins pipeline on deployment if multiple identical Identifiers are selected.";
                GUI.Box(
                    new Rect(rect.position.x + rect.width - 25f,
                        rect.position.y,
                        50f, rect.height), gc, EditorStyles.label);
            }
        }

        private void OnProductSelect(ReorderableList rlist)
        {
            // Update Scene list
            if(sceneList == null)
            {
                sceneList = new ReorderableList(AssetDatabaseExtension.LoadAssetsFromMultiplePaths<SceneAsset>(Product.Scenes), typeof(SceneAsset), false, false, true, true);
            }
            else
            {
                sceneList.list = AssetDatabaseExtension.LoadAssetsFromMultiplePaths<SceneAsset>(Product.Scenes);
            }

            //  Update BuildTargetList
            if(buildTargetList == null)
            {
                buildTargetList = new ReorderableList(Product.BuildTargets, typeof(Building.BuildTargetDefinition),false,false,true,true);
            }
            else
            {
                buildTargetList.list = Product.BuildTargets;
            }
            buildTargetList.displayRemove = buildTargetList.list.Count > 1;

            //  Update AssetList
            if(assetList == null)
            {
                assetList = new ReorderableList(AssetDatabaseExtension.LoadAssetsFromMultiplePaths<DefaultAsset>(Product.StreamingAssetsWhitelist), typeof(DefaultAsset),true,false,true,true);
            }
            else
            {
                assetList.list = AssetDatabaseExtension.LoadAssetsFromMultiplePaths<DefaultAsset>(Product.StreamingAssetsWhitelist);
            }

            // Update Splash Screen list
            if(splashList == null)
            {
                splashList = new ReorderableList(Product.SplashDefinition.SplashLogos, typeof(Building.SplashLogo),true,false,true,true);
            }
            else
            {
                splashList.list = Product.SplashDefinition.SplashLogos;
            }
        }
        
        private void AddProduct(ReorderableList rlist)
        {
            productList.list.Add(new Building.Product());
            productList.index = productList.list.Count - 1;
            OnProductSelect(productList);
            productList.displayRemove = productList.list.Count > 1;
            hasChanges = true;
        }

        private void RemoveProduct(ReorderableList rlist)
        {
            productList.list.RemoveAt(productList.index);
            if(--productList.index < 0)
                productList.index = 0;
            OnProductSelect(productList);

            productList.displayRemove = productList.list.Count > 1;
            hasChanges = true;
        }

        #endregion

        #region Asset Callbacks

        private void DrawAssetElement(Rect rect, int index, bool active, bool focused)
        {
            var oldVal = assetList.list[index];
            assetList.list[index] = EditorGUI.ObjectField(rect, "", (DefaultAsset)assetList.list[index], typeof(DefaultAsset), true);
            Product.StreamingAssetsWhitelist[index] = AssetDatabase.GetAssetPath(assetList.list[index] as DefaultAsset);
            hasChanges = hasChanges || oldVal != assetList.list[index];
        }

        private void AddAsset(ReorderableList list)
        {
            assetList.list.Add(null);
            Product.StreamingAssetsWhitelist.Add("");
            hasChanges = true;
        }

        private void RemoveAsset(ReorderableList list)
        {
            assetList.list.RemoveAt(list.index);
            Product.StreamingAssetsWhitelist.RemoveAt(list.index);
            hasChanges = true;
        }
        #endregion

        #region Scene Callbacks

        private void DrawSceneElement(Rect rect, int index, bool active, bool focused)
        {
            var oldVal = sceneList.list[index];
            sceneList.list[index] = EditorGUI.ObjectField(rect, "", (SceneAsset)sceneList.list[index], typeof(SceneAsset), true);
            Product.Scenes[index] = AssetDatabase.GetAssetPath(sceneList.list[index] as SceneAsset);
            hasChanges = hasChanges || oldVal != sceneList.list[index];
        }

        private void AddScene(ReorderableList list)
        {
            sceneList.list.Add(null);
            Product.Scenes.Add("");
            hasChanges = true;
        }

        private void RemoveScene(ReorderableList list)
        {
            sceneList.list.RemoveAt(list.index);
            Product.Scenes.RemoveAt(list.index);
            hasChanges = true;
        }

        #endregion

        #region Splash Callbacks
        
        private void DrawSplashElement(Rect rect, int index, bool active, bool focused)
        {
            GUILayout.BeginHorizontal();

            splashList.list[index] = Product.SplashDefinition.SplashLogos[index];
            var oldVal = Product.SplashDefinition.SplashLogos[index].SpritePath;
            Product.SplashDefinition.SplashLogos[index].SpritePath =
                AssetDatabase.GetAssetPath(
                    (EditorGUI.ObjectField(
                    new Rect(rect.x, rect.y + 1.5f, rect.width, rect.height - 5f),
                    "", ((Building.SplashLogo)splashList.list[index]).GetSprite(), typeof(Sprite), false) as Sprite)
            );
            hasChanges = hasChanges || oldVal != Product.SplashDefinition.SplashLogos[index].SpritePath;

            GUI.Label(rect, "Logo Duration");
            var oldVal2 = Product.SplashDefinition.SplashLogos[index].Duration;
            Product.SplashDefinition.SplashLogos[index].Duration = EditorGUI.Slider(
                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight,
                rect.width - EditorGUIUtility.singleLineHeight * 4 - 2.5f,
                EditorGUIUtility.singleLineHeight),
                Product.SplashDefinition.SplashLogos[index].Duration, 2f, 10f);

            hasChanges = hasChanges || oldVal2 != Product.SplashDefinition.SplashLogos[index].Duration;
            GUILayout.EndHorizontal();

        }

        private void AddSplash(ReorderableList list)
        {
            Product.SplashDefinition.SplashLogos.Add(new Building.SplashLogo());
            hasChanges = true;
        }

        private void RemoveSplash(ReorderableList list)
        {
            Product.SplashDefinition.SplashLogos.RemoveAt(list.index);
            hasChanges = true;
        }
        #endregion

        #region BuildTarget Callbacks

        private void DrawBuildTargetElement(Rect rect, int index, bool active, bool focused)
        {
            BuildTarget target;
            Enum35.TryParse(Product.BuildTargets[index].Target, out target);

            EditorGUI.LabelField(rect, "Platform");
            Product.BuildTargets[index].Target = EditorGUI.EnumPopup(
            new Rect(rect.position.x + 60f, rect.position.y, rect.width -60f, rect.height/2f),
            target).ToString();

            var rect2 = new Rect(rect.x, rect.y + rect.height/2f, rect.width, rect.height / 2f);
            EditorGUI.LabelField(rect2, "Pipeline");

            //  TODO: Refactor to generic version?
            //  Different enums required for simple restriction on enum popup.
            if(target != BuildTarget.iOS)
            {
                Building.BuildStepsAndroid pipeline;
                Enum35.TryParse(Product.BuildTargets[index].BuildSteps, out pipeline);
                Product.BuildTargets[index].BuildSteps = EditorGUI.EnumPopup(
                new Rect(rect2.position.x + 60f, rect2.position.y, rect2.width - 60f, rect2.height),
                pipeline).ToString();
            }
            else
            {
                Building.BuildSteps pipeline;
                Enum35.TryParse(Product.BuildTargets[index].BuildSteps, out pipeline);
                Product.BuildTargets[index].BuildSteps = EditorGUI.EnumPopup(
                new Rect(rect2.position.x + 60f, rect2.position.y, rect2.width - 60f, rect2.height),
                pipeline).ToString();
            }
        }

        private void AddBuildTarget(ReorderableList list)
        {
            buildTargetList.list.Add(new Building.BuildTargetDefinition());
            buildTargetList.index = buildTargetList.list.Count - 1;
            buildTargetList.displayRemove = buildTargetList.list.Count > 1;
            hasChanges = true;
        }

        private void RemoveBuildTarget(ReorderableList list)
        {
            buildTargetList.list.RemoveAt(buildTargetList.index);
            if(--buildTargetList.index < 0)
                buildTargetList.index = 0;

            buildTargetList.displayRemove = buildTargetList.list.Count > 1;
            hasChanges = true;
        }
        #endregion

        //TODO: Write as extension method
        private bool CheckForDuplicateInternalIdentifier(string value)
        {
            var count = 0;
            for(int i = 0; i < productList.list.Count; i++)
            {
                if((productList.list[i] as Building.Product).InternalIdentifier == value
                    && ++count > 1)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CheckForDuplicateIdentifier(string value)
        {
            var count = 0;
            for(int i = 0; i < productList.list.Count; i++)
            {
                if((productList.list[i] as Building.Product).Identifier == value
                    && ++count > 1)
                {
                    return true;
                }
            }
            return false;
        }

        //TODO: Move external
        #region GUI helpers
        /// <summary>
        /// Predefines Layout options for auto width within a range
        /// </summary>
        /// <param name="min">Min width</param>
        /// <param name="max">Max width</param>
        /// <returns>Layout options</returns>
        private GUILayoutOption[] LayoutAutoMaxMinWidth(float min, float max)
        {
            return new GUILayoutOption[]
            {
                GUILayout.MinWidth(min),
                GUILayout.MaxWidth(max),
                GUILayout.ExpandWidth(true)
            };
        }
        
        /// <summary>
        /// Draw Header Label with text on default color (grey)
        /// </summary>
        /// <param name="title">Text to draw</param>
        private void DrawHeaderLabel(string title)
        {
            DrawHeaderLabel(title, Color.grey);
        }
        /// <summary>
        /// Draw Header Label with text on custom color
        /// </summary>
        /// <param name="title">Text to draw</param>
        /// <param name="backgroundColor">Background color</param>
        private void DrawHeaderLabel(string title, Color backgroundColor)
        {
            GUILayout.BeginHorizontal();

            var style = new GUIStyle(EditorStyles.largeLabel);
            style.fontStyle = FontStyle.Bold;
            style.normal.background = new Texture2D(1, 1);
            style.normal.background.SetPixel(0, 0, backgroundColor);
            style.normal.background.Apply();
            
            GUILayout.Label(title, style);
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
        #endregion

        private void DrawHeaderLabel(string title, Color backgroundColor, ref bool foldoutState)
        {
            var bgStyle = new GUIStyle();
            bgStyle.normal.background = new Texture2D(1,1);
            bgStyle.normal.background.SetPixel(0, 0, backgroundColor);
            bgStyle.normal.background.Apply();
            GUILayout.BeginHorizontal(bgStyle);

            var style = new GUIStyle(EditorStyles.foldout);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = EditorStyles.boldLabel.fontSize;
            style.onNormal = style.normal;
            style.focused = style.normal;
            style.hover = style.normal;
            style.active = style.normal;
            style.onActive = style.onNormal;
            style.onHover = style.onNormal;
            style.onFocused = style.onNormal;
            foldoutState = EditorGUILayout.Foldout(foldoutState, title, true, style);
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
    }
}