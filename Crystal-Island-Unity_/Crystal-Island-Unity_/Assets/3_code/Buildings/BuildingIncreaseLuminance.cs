using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class BuildingIncreaseLuminance : VCBehaviour<Building>
    {
        public float LuminanceIncrement = 0.1f;

        public override void onModelChanged()
        {
            this.model.interacted.AddListener(this.interacted);
        }

        public override void onModelRemoved()
        {
            this.model.interacted.RemoveListener(this.interacted);
        }

        private void interacted()
        {
            RootLogger.Info(this, "Increasing the luminance of the building");
            float newLuminance = this.model.Luminance + this.LuminanceIncrement;
            Level.instance.authoritativePlayer.ClientSetBuildingLuminance(this.model.netId, newLuminance);
        }
    }
}
