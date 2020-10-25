using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace KoboldTools
{
    public class AvailabeCanvases
    {
        public GameObject go;
        public bool active;
        public bool activeDefault;
        public AvailabeCanvases(GameObject inGo, bool inActive)
        {
            go = inGo;
            active = inActive;
            activeDefault = inActive;
        }
    }

    /// <summary>
    /// Custom editor tool to export hi-res screenshots
    /// </summary>
    public class kgScreenshotTool : EditorWindow
    {
        private int _resolutionX = 3840;
        private int _resolutionY = 2160;
        private int _superSize = 2;
        private float _timeScale = 1.0f;
        private float _timeScaleStep = 0.1f;
        private bool _autoName = true;
        private string _screenshotName = "";
        private string _directory;
        private List<AvailabeCanvases> _canvases = new List<AvailabeCanvases>();

        [MenuItem("Tools/Screenshot Tool")]
        public static void openScreenshtoTool()
        {
            //show window
#pragma warning disable 0219
            kgScreenshotTool window = (kgScreenshotTool)EditorWindow.GetWindow(typeof(kgScreenshotTool));
#pragma warning restore 0219
        }

        public void OnGUI()
        {
            GetScreenResolution();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== Configuration ===", EditorStyles.boldLabel);
            GUILayout.Space(10);
            DrawResolutionArea();
            GUILayout.Space(15);
            DrawNamingArea();
            GUILayout.Space(15);
            DrawDirectorArea();
            GUILayout.Space(15);
            DrawCanvasesArea();
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Take Screen", GUILayout.Width(200)))
            {
                TakeScreenshot();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawResolutionArea()
        {
            EditorGUILayout.LabelField("  Resolution", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            GUILayout.Label("     (Current: " + _resolutionX + " x " + _resolutionY + ")");
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("    Supersize:", GUILayout.Width(80));
            if (_superSize <= 1)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                _superSize--;
            }
            GUI.enabled = true;
            GUILayout.Label(_superSize.ToString(), GUILayout.Width(20));
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                _superSize++;
            }
            GUI.enabled = false;
            GUILayout.Label(" (Target: " + _resolutionX * _superSize + " x " + _resolutionY * _superSize + ")", GUILayout.Width(200));
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawNamingArea()
        {
            EditorGUILayout.LabelField("  Naming", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("    Autonaming");
            _autoName = GUILayout.Toggle(_autoName, "");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("    Name:", GUILayout.Width(60));
            if (_autoName)
            {
                GUI.enabled = false;
                GUILayout.Label("screen_{WIDTH}x{HEIGHT}_{DATE}_{TIME}.png");
                GUI.enabled = true;
            }
            else
            {
                _screenshotName = GUILayout.TextField(_screenshotName, 30, GUILayout.Width(180));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawDirectorArea()
        {
            EditorGUILayout.LabelField("  Director", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (EditorApplication.isPaused)
            {
                if (GUILayout.Button("Play", GUILayout.Width(80)))
                {
                    EditorApplication.isPaused = false;
                }
                if (GUILayout.Button("Next Step", GUILayout.Width(80)))
                {
                    EditorApplication.Step();
                }
            }
            else
            {
                if (GUILayout.Button("Pause", GUILayout.Width(80)))
                {
                    EditorApplication.isPaused = true;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("    Speed:", GUILayout.Width(60));
            if (_timeScale <= 0.0f)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                _timeScale -= _timeScaleStep;
                Time.timeScale = Mathf.Round(_timeScale * 10f) / 10f;
            }
            GUI.enabled = true;
            GUILayout.Label(Time.timeScale.ToString(), GUILayout.Width(25));
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                _timeScale += _timeScaleStep;
                Time.timeScale = Mathf.Round(_timeScale * 10f) / 10f;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCanvasesArea()
        {
            EditorGUILayout.LabelField("  Active Canvases", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (Application.isPlaying)
            {
                if (_canvases.Count <= 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("Enable All", GUILayout.Width(80)))
                {
                    SetCanvasSettingsAll(true);
                }
                if (GUILayout.Button("Disable All", GUILayout.Width(80)))
                {
                    SetCanvasSettingsAll(false);
                }
                GUI.enabled = true;
                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    RefreshCanvasList();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                if (_canvases.Count > 0)
                {
                    for (int i = 0; i < _canvases.Count; i++)
                    {
                        if (_canvases[i].go != null)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("    " + _canvases[i].go.name);
                            _canvases[i].active = GUILayout.Toggle(_canvases[i].active, "");
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            GUI.enabled = false;
                            GUILayout.Label("    (null)");
                            _canvases[i].active = GUILayout.Toggle(_canvases[i].active, "");
                            GUI.enabled = true;
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    GUILayout.Label("   No canvas found.");
                }

                //update canvases if needed
                for (int i = 0; i < _canvases.Count; i++)
                {
                    if (_canvases[i].go.activeInHierarchy != _canvases[i].active)
                    {
                        _canvases[i].go.SetActive(_canvases[i].active);
                    }
                }
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Label("    (only available in play mode)");
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
            }
        }

        private void TakeScreenshot()
        {
            //prepare
            checkDirectory();
            //save screenshot
            ScreenCapture.CaptureScreenshot(ScreenShotName(_resolutionX * _superSize, _resolutionY * _superSize), _superSize);
        }

        private void RefreshCanvasList()
        {
            //reset canvas settings
            foreach (AvailabeCanvases cvs in _canvases)
            {
                cvs.go.SetActive(cvs.activeDefault);
            }
            //clear list
            _canvases = new List<AvailabeCanvases>();
            //fetch new
            Object[] foundCanvases = GameObject.FindObjectsOfType(typeof(Canvas));
            for (int i = 0; i < foundCanvases.Length; i++)
            {
                _canvases.Add(new AvailabeCanvases(((Canvas)foundCanvases[i]).gameObject, ((Canvas)foundCanvases[i]).gameObject.activeInHierarchy));
            }
        }

        private void SetCanvasSettingsAll(bool enable)
        {
            for (int i = 0; i < _canvases.Count; i++)
            {
                _canvases[i].active = enable;
                _canvases[i].go.SetActive(enable);
            }
        }

        private void checkDirectory()
        {
            //make dir if not exists
            _directory = Application.dataPath + "/screenshots";
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
        }

        private string ScreenShotName(int width, int height)
        {
            if (_autoName)
            {
                return string.Format("{0}/screen_{1}x{2}_{3}.png", _directory, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            }
            else
            {
                return "/" + _screenshotName + ".png";
            }
        }

        private void GetScreenResolution()
        {
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
            _resolutionX = (int)((Vector2)Res).x;
            _resolutionY = (int)((Vector2)Res).y;
        }
    }
}
