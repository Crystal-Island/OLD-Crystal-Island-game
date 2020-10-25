using System.Collections.Generic;
using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    public class LevelDisplayPlayers : VCBehaviour<Level>
    {
        public bool DisplayAuthoritativePlayer;
        public GameObject ShowOnNoPlayers;
        public Transform PlayerUiTemplate;
        private Pool<Transform> playerPool;

        public void Awake()
        {
            this.playerPool = new Pool<Transform>(this.PlayerUiTemplate);
        }

        public override void onModelChanged()
        {
            this.model.onLevelStateChanged.AddListener(this.onLevelStateChanged);
        }

        public override void onModelRemoved()
        {
            this.model.onLevelStateChanged.RemoveListener(this.onLevelStateChanged);
        }

        private void onLevelStateChanged()
        {
            this.playerPool.releaseAll();
            int displayedPlayers = 0;
            foreach (Player player in this.model.allPlayers)
            {
                if (!player.isLocalPlayer || (player.isLocalPlayer && this.DisplayAuthoritativePlayer))
                {
                    Transform playerUi = this.playerPool.pop();
                    playerUi.gameObject.SetActive(true);
                    VC<Player>.addModelToAllControllers(player, playerUi.gameObject);
                    displayedPlayers += 1;
                }
            }

            if (displayedPlayers == 0)
            {
                this.ShowOnNoPlayers.SetActive(true);
            }
            else
            {
                this.ShowOnNoPlayers.SetActive(false);
            }
        }
    }
}
