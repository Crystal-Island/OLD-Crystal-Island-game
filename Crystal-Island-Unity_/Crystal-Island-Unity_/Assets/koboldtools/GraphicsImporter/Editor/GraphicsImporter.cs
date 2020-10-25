using UnityEngine;
using UnityEditor;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

public class GraphicsImporter : EditorWindow
{
    private int _srcRes = 3840;
    private int _targetRes = 1920;
    private float _camSize = 9.6f;
    private bool _forceImport = false;

    private string _targetSubFolder = "graphics";

    private string _srcPath = "";
    private string _targetPath = "";

    private int _dirCount = 0;
    private int _fileCount = 0;
    private int _copiedFileCount = 0;
    private int _errorCount = 0;

    [MenuItem("Tools/Graphics Importer")]
	public static void openImageImporter() {
        //show window
        #pragma warning disable 0219
        GraphicsImporter window = (GraphicsImporter)EditorWindow.GetWindow (typeof (GraphicsImporter));
        #pragma warning restore 0219
    }

    public void OnEnable()
    {
        _srcRes = EditorPrefs.GetInt("custom_importer_srcRes", 3840);
        _targetRes = EditorPrefs.GetInt("custom_importer_targetRes", 1920);
        _camSize = EditorPrefs.GetFloat("custom_importer_camSize", 9.6f);
    }

    public void OnGUI() 
	{
        GUILayout.Space(10);
		EditorGUILayout.LabelField(" === Graphics Importer ===", EditorStyles.boldLabel);
		GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("    Source resolution:", GUILayout.Width(130));
        _srcRes = EditorGUILayout.IntField("", _srcRes, GUILayout.Width(80));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("    Target resolution:", GUILayout.Width(130));
        _targetRes = EditorGUILayout.IntField("", _targetRes, GUILayout.Width(80));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("    Cam size:", GUILayout.Width(130));
        _camSize = EditorGUILayout.FloatField("", _camSize, GUILayout.Width(80));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Label("  ");
        _forceImport = GUILayout.Toggle(_forceImport, " Force reimport of all images");
        //GUILayout.Label("Force reimport of all images");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(40);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        if (GUILayout.Button("Import", GUILayout.Width(200)))
		{
            EditorPrefs.SetInt("custom_importer_srcRes", _srcRes);
            EditorPrefs.SetInt("custom_importer_targetRes", _targetRes);
            EditorPrefs.SetFloat("custom_importer_camSize", _camSize);
            EditorPrefs.SetInt("custom_importer_pixelperunit", Mathf.RoundToInt(_targetRes/_camSize/2f));
            importImages();
            AssetDatabase.Refresh();
            EditorPrefs.DeleteKey("custom_importer_pixelperunit");
        }
		GUILayout.EndHorizontal();
	}

    private void importImages()
    {
        //copy & resize files
        _srcPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/SourceGraphics";
        _targetPath = Application.dataPath + "/" + _targetSubFolder;
        _dirCount = 0;
        _fileCount = 0;
        _copiedFileCount = 0;
        _errorCount = 0;
        Debug.Log("[GraphicsImporter] start resampling...");
        copyImagesRecursively(new DirectoryInfo(@_srcPath), new DirectoryInfo(@_targetPath));
        Debug.Log("[GraphicsImporter] import done. " + _copiedFileCount + " of " + _fileCount + " images in " + _dirCount + " directories were processed.");
        if (_errorCount > 0)
        {
            Debug.LogWarning("[GraphicsImporter] " + _errorCount + " error(s) occured during import. please check log for further details.");
        }
    }

    private void copyImagesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (DirectoryInfo dir in source.GetDirectories())
        {
            //Debug.Log("[GraphicsImporter] copy recursively directory '" + dir.Name);
            copyImagesRecursively(dir, target.CreateSubdirectory(dir.Name));
            _dirCount++;
        }
        foreach (FileInfo file in source.GetFiles())
        {
            bool doCopy = true;
            _fileCount++;
            if (File.Exists(Path.Combine(target.FullName, file.Name)))
            {
                FileInfo targetFile = new FileInfo(Path.Combine(target.FullName, file.Name));
                if (file.LastWriteTime < targetFile.LastWriteTime)
                {
                    doCopy = false;
                }
            }

            //copy & resize
            if (doCopy || _forceImport)
            {
                _copiedFileCount++;
                Debug.Log("[GraphicsImporter] resample '" + file.Name + "' with scale factor " + (1.0f * _targetRes / _srcRes));
                resizeImage(file.FullName, Path.Combine(target.FullName, file.Name), 1.0f * _targetRes / _srcRes);
            }
        }
    }

    private void resizeImage(string imageFile, string outputFile, double scaleFactor)
    {
        try
        {
            using (var srcImage = Image.FromFile(imageFile))
            {
                if (srcImage == null)
                {
                    Debug.LogError("[GraphicsImporter] '" + imageFile + "' is invaild image. skipping file.");
                    _errorCount++;
                    return;
                }

                var newWidth = Mathf.CeilToInt((float)(srcImage.Width * scaleFactor));
                var newHeight = Mathf.CeilToInt((float)(srcImage.Height * scaleFactor));

                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = System.Drawing.Graphics.FromImage(newImage))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));
                    newImage.Save(outputFile);
                }
            }
        }
        catch (System.Exception e)
        {
            _errorCount++;
            Debug.LogError("[GraphicsImporter] " + e);
            Debug.LogError("[GraphicsImporter] '" + imageFile + "' is invaild image type. skipping file.");
        }
    }
}