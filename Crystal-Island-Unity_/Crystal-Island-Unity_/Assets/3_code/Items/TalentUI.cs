using System;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class TalentUI : VCBehaviour<Talent>
    {
        public UiResource Resource;
        /// <summary>
        /// Holds the text that describes the player's talent category.
        /// </summary>
        public Text talentType;
        public Image talentTypeIcon;

        private string defaultText = "";
        private Sprite defaultSprite = null;

        /// <summary>
        /// Updates the UI text based on the model data.
        /// </summary>
        public override void onModelChanged()
        {
            //set defaults
            if(this.defaultSprite == null && this.talentTypeIcon != null)
            {
                this.defaultSprite = this.talentTypeIcon.sprite;
            }
            if(this.defaultText == "" && this.talentType != null)
            {
                this.defaultText = this.talentType.text;
            }

            //set new
            if (this.talentType != null)
            {
                this.talentType.text = this.model.Type;
            }
            if (this.talentTypeIcon != null)
            {
                this.talentTypeIcon.sprite = this.Resource.GetSpriteByTags(this.model.Tags);
            }
        }
        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void onModelRemoved()
        {
            //set to default
            if (this.talentType != null)
            {
                this.talentType.text = this.defaultText;
            }
            if (this.talentTypeIcon != null)
            {
                this.talentTypeIcon.sprite = this.defaultSprite;
            }
        }
    }
}
