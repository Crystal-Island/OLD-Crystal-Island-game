namespace JulianSchoenbaechler.PostProcessing
{
    using UnityEngine;

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public abstract class PostProcessingBase : MonoBehaviour
    {
        protected new Camera camera;
        protected bool supportHDRTextures = true;
        protected bool isSupported = true;


        /// <summary>
        /// Check if this post processing effect is supported on this platform.
        /// </summary>
        /// <returns><c>true</c> if effect is supported; otherwise <c>false</c>.</returns>
        public bool CheckSupport()
        {
            return CheckSupport(RenderTextureFormat.Default);
        }

        /// <summary>
        /// Check if this post processing effect is supported on this platform.
        /// </summary>
        /// <param name="format">A render texture format that should be checked.</param>
        /// <returns><c>true</c> if effect is supported; otherwise <c>false</c>.</returns>
        public bool CheckSupport(RenderTextureFormat format)
        {
            supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);

#if UNITY_5_5_OR_NEWER
            if(!SystemInfo.supportsImageEffects)
            {
                return false;
            }
#else
            if(!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
            {
                NotSupported();
                return false;
            }
#endif

            if(!SystemInfo.SupportsRenderTextureFormat(format))
            {
                NotSupported();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create a material from a shader used for post processing.
        /// </summary>
        /// <param name="path">Shader path.</param>
        /// <returns>A new post processing material.</returns>
        protected Material FromShader(string path)
        {
            Shader s = Shader.Find(path);

            if(s == null)
            {
                Debug.LogError("[PostProcessing] Missing shader in " + this.ToString());
                return null;
            }
            else
            {
                Material mat = new Material(s);
                mat.hideFlags = HideFlags.DontSave;

                return mat;
            }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected abstract void OnEnable();

        /// <summary>
        /// OnRenderImage is called after all rendering is complete to render image.
        /// </summary>
        /// <param name="src">The source RenderTexture.</param>
        /// <param name="dest">The destination RenderTexture.</param>
        protected abstract void OnRenderImage(RenderTexture src, RenderTexture dest);

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Called when image effect is not supported.
        /// </summary>
        private void NotSupported()
        {
            isSupported = false;
            enabled = false;

            Debug.LogWarning("[PostProcessing] This image effect is not supported on this platform!");
        }
    }
}
