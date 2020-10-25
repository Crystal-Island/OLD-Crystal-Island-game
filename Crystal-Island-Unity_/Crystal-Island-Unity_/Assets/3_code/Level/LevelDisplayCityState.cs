using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class LevelDisplayCityState : VCBehaviour<Level>
    {
        public Slider progressSlider;

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
            if (this.progressSlider != null)
            {
                this.progressSlider.value = this.model.CityState;
            }
        }
    }
}
