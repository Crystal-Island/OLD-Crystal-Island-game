using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    public class LevelFilterVisibility : VCBehaviour<Level>
    {
        public GameObject[] onlyVisibleWithPolymoney;
        public GameObject[] onlyVisibleForPlayers;

        public override void onModelChanged()
        {
            model.onLevelStateChanged.AddListener(stateChanged);
            model.onAuthoritativePlayerChanged.AddListener(authoritativePlayerChanged);

            //initialize
            stateChanged();
            authoritativePlayerChanged();
        }

        public override void onModelRemoved()
        {
            model.onLevelStateChanged.RemoveListener(stateChanged);
            model.onAuthoritativePlayerChanged.RemoveListener(authoritativePlayerChanged);
        }

        private void stateChanged()
        {
            foreach(GameObject go in onlyVisibleWithPolymoney)
            {
                go.SetActive(model.PolymoneyIntroduced);
            }

        }
        private void authoritativePlayerChanged()
        {
            if(model.authoritativePlayer != null)
                model.authoritativePlayer.PlayerStateChanged.AddListener(authoritativePlayerStateChanged);
        }

        private void authoritativePlayerStateChanged()
        {
            foreach (GameObject go in onlyVisibleForPlayers)
            {
                go.SetActive(model.authoritativePlayer != null && !model.authoritativePlayer.Mayor);
            }
        }
    }
}
