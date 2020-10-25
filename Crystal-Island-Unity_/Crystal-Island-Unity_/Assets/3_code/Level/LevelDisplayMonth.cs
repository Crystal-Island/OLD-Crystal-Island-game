using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class LevelDisplayMonth : VCBehaviour<Level>
    {
        public Text monthText;
        public Text progressText;
        public Slider progressSlider;
        public string progressTextId = "progressText";

        public override void onModelChanged()
        {
            this.model.onLevelStateChanged.AddListener(this.levelStateChanged);
            this.levelStateChanged();
        }

        public override void onModelRemoved()
        {
            this.model.onLevelStateChanged.RemoveListener(this.levelStateChanged);
        }

        private void levelStateChanged()
        {
            if (this.monthText != null)
            {
                this.monthText.text = Mathf.Round(this.model.months).ToString();
            }

            if (this.progressText != null)
            {
                this.progressText.text = Localisation.instance.getLocalisedFormat(this.progressTextId, Mathf.Round(this.model.months), this.model.maximumMonths);
            }

            if (this.progressSlider != null)
            {
                this.progressSlider.value = (this.model.months / this.model.maximumMonths);
            }
        }
    }
}
