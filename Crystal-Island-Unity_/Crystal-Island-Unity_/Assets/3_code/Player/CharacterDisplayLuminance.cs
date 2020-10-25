using UnityEngine;

namespace Polymoney
{
    [RequireComponent(typeof(Character))]
    public class CharacterDisplayLuminance : MonoBehaviour
    {
        public Light lightSource;
        public SpriteRenderer scaleSource;
        public Color fromColor = Color.white;
        public Color toColor = Color.white;
        public float luminanceMultiplier = 25;
        private Character character;

        public void Awake()
        {
            if (this.lightSource == null)
            {
                this.lightSource = GetComponentInChildren<Light>();
            }

            this.character = GetComponent<Character>();
            this.character.OnLuminanceChanged.AddListener(this.onLuminanceChanged);
        }

        private void onLuminanceChanged()
        {
            if(this.lightSource != null)
                this.lightSource.intensity = this.luminanceMultiplier * this.character.Luminance;
            if(this.scaleSource != null)
                this.scaleSource.color = Color.Lerp(fromColor, toColor, this.character.Luminance);
        }
    }
}
