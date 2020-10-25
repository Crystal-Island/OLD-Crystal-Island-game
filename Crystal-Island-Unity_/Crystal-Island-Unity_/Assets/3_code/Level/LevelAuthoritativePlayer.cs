using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    public class LevelAuthoritativePlayer : VCBehaviour<Level>
    {
        public GameObject levelUiObject;
        public GameObject playerUiObject;

        public override void onModelChanged()
        {
            VC<Level>.addModelToAllControllers(this.model, this.levelUiObject, true);

            this.model.onAuthoritativePlayerChanged.AddListener(this.authoritativePlayerChanged);

            this.authoritativePlayerChanged();
        }

        public override void onModelRemoved()
        {
            this.model.onAuthoritativePlayerChanged.RemoveListener(this.authoritativePlayerChanged);
        }

        private void authoritativePlayerChanged()
        {
            VC<Player>.addModelToAllControllers(this.model.authoritativePlayer, this.playerUiObject, true);
        }
    }
}
