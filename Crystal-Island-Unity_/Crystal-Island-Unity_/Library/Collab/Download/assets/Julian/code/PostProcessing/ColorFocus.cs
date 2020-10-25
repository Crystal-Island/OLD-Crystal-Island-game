namespace JulianSchoenbaechler.PostProcessing
{
    using UnityEngine;

    public class ColorFocus : PostProcessingBase, ISerializationCallbackReceiver
    {
        public RenderTexture debugRenderTexture = null;

        #region Fields

        [Tooltip("The camera that renders the focus mask layer.\n" +
            "The rendered image will be interpreted as greyscale data. The camera must not have " +
            "a render texture assigned!")]
        [SerializeField]
        protected Camera maskCamera;

        [Tooltip("The resolution of the rendered mask.\n- Lower value = smaller resolution / better performance\n" +
            "- Higher value = greater resolution / worse performance")]
        [SerializeField, Range(1, 10)]
        private int resolution = 4;

        [Tooltip("Darkness amount of the effect (not in focus).")]
        [SerializeField, Range(0f, 1f)]
        private float effectDarkness = 0.5f;

        [Tooltip("Color saturation of the effect (not in focus).")]
        [SerializeField, Range(0f, 1f)]
        private float effectColorSaturation = 0f;

        [Tooltip("Desaturated B/W Balance (R)")]
        [SerializeField, Range(0f, 2f)]
        private float balanceR = 1f;

        [Tooltip("Desaturated B/W Balance (G)")]
        [SerializeField, Range(0f, 2f)]
        private float balanceG = 1f;

        [Tooltip("Desaturated B/W Balance (B)")]
        [SerializeField, Range(0f, 2f)]
        private float balanceB = 1f;

        protected Material mat;
        private RenderTexture renderMask;
        private bool updateProperties = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the mask camera that is assigned to this image effect.
        /// </summary>
        /// <value>The camera that renders the used mask for this effect.</value>
        public Camera MaskCamera
        {
            get { return maskCamera; }
        }

        /// <summary>
        /// Gets the mask rendering resolution of this effect.
        /// </summary>
        /// <value>The resolution value.</value>
        public int Resolution
        {
            get { return resolution; }
        }

        /// <summary>
        /// Gets or sets the effects darkness amount (on out of focus).
        /// Value between [0...1].
        /// </summary>
        /// <value>The value that darkens the image.</value>
        public float EffectDarkness
        {
            get { return effectDarkness; }

            set
            {
                effectDarkness = Mathf.Clamp(value, 0f, 1f);
                updateProperties = true;
            }
        }

        /// <summary>
        /// Gets or sets the effects color saturation amount (on out of focus).
        /// Value between [0...1].
        /// </summary>
        /// <value>The value that saturates/desaturates the image.</value>
        public float EffectColorSaturation
        {
            get { return effectColorSaturation; }

            set
            {
                effectColorSaturation = Mathf.Clamp(value, 0f, 1f);
                updateProperties = true;
            }
        }

        /// <summary>
        /// Gets or sets a multiplier for the red color channel used when desaturating an the image.
        /// Value between [0...2].
        /// </summary>
        /// <value>The desaturation collor balance (red).</value>
        public float SaturationBalanceRed
        {
            get { return balanceR; }

            set
            {
                balanceR = Mathf.Clamp(value, 0f, 2f);
                updateProperties = true;
            }
        }

        /// <summary>
        /// Gets or sets a multiplier for the green color channel used when desaturating an the image.
        /// Value between [0...2].
        /// </summary>
        /// <value>The desaturation collor balance (green).</value>
        public float SaturationBalanceGreen
        {
            get { return balanceG; }

            set
            {
                balanceG = Mathf.Clamp(value, 0f, 2f);
                updateProperties = true;
            }
        }

        /// <summary>
        /// Gets or sets a multiplier for the blue color channel used when desaturating an the image.
        /// Value between [0...2].
        /// </summary>
        /// <value>The desaturation collor balance (blue).</value>
        public float SaturationBalanceBlue
        {
            get { return balanceR; }

            set
            {
                balanceB = Mathf.Clamp(value, 0f, 2f);
                updateProperties = true;
            }
        }

        #endregion

        #region Post Process

        /// <summary>
        /// Apply shader property changes.
        /// </summary>
        public void ApplyProperties()
        {
            mat.SetTexture("_Mask", renderMask);
            mat.SetFloat("_Darkness", effectDarkness);
            mat.SetFloat("_Saturation", effectColorSaturation);
            mat.SetFloat("_BalanceR", balanceR);
            mat.SetFloat("_BalanceG", balanceG);
            mat.SetFloat("_BalanceB", balanceB);
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            if(CheckSupport())
            {
                mat = FromShader("Hidden/Color Focus");
                maskCamera.enabled = true;
                ApplyProperties();
            }
        }

        /// <summary>
        /// OnRenderImage is called after all rendering is complete to render image.
        /// </summary>
        /// <param name="src">The source RenderTexture.</param>
        /// <param name="dest">The destination RenderTexture.</param>
        protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if(updateProperties)
            {
                ApplyProperties();
                updateProperties = false;
            }

            if(isSupported)
                Graphics.Blit(src, dest, mat);
            else
                Graphics.Blit(src, dest);
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        private void Start()
        {
            if(maskCamera == null)
            {
                Debug.LogError("[PostProcessing] ColorFocus: no mask camera assigned.");
                return;
            }

            if (debugRenderTexture == null)
            {

                renderMask = new RenderTexture(
                    Screen.width / (11 - resolution),
                    Screen.height / (11 - resolution),
                    0,
                    RenderTextureFormat.ARGB32
                );

                renderMask.Create();
                renderMask.name = "ColorFocusMaskTexture";
                

            }
            else
            {
                renderMask = debugRenderTexture;
            }
            maskCamera.targetTexture = renderMask;
            
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (renderMask != null)
            {
                renderMask.DiscardContents();
                renderMask.Release();
            }
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            maskCamera.enabled = false;
        }

        #endregion

        #region Editor

//#if UNITY_EDITOR
        /// <summary>
        /// For receiving a callback before Unity serializes this object.
        /// </summary>
        public void OnBeforeSerialize() { }

        /// <summary>
        /// For receiving a callback after Unity serializes this object.
        /// </summary>
        public void OnAfterDeserialize()
        {
            updateProperties = true;
        }
//#endif

        #endregion
    }
}
