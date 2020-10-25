using System;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class LevelDisplayLuminance : VCBehaviour<Level>
    {
        public Text luminanceText;
        public string luminanceTextId = "luminance";

        public override void onModelChanged()
        {
            this.model.onLevelStateChanged.AddListener(this.onLevelStateChanged);
            this.onLevelStateChanged();
        }

        public override void onModelRemoved()
        {
            this.model.onLevelStateChanged.RemoveListener(this.onLevelStateChanged);
        }

        private void onLevelStateChanged()
        {
            this.luminanceText.text = Localisation.instance.getLocalisedFormat(this.luminanceTextId, this.model.TotalLuminance);
        }
    }
}
