using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class PlayerGiveFairydust : VCBehaviour<Player>
    {
        public Button GiveFairydust;
        public CurrencyValue GiftCost;
        public int GiftBenefit;
        public float PlayerLuminanceIncrement = 0.1f;
        public float BuildingLuminanceIncrement = 0.1f;
        public bool EndTurn = true;

        public new void Start()
        {
            this.GiveFairydust.onClick.AddListener(this.onClickGiveFairydust);
            base.Start();
        }

        public override void onModelChanged()
        {
            this.model.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
        }

        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
        }

        private void onPlayerStateChanged()
        {
        }

        private void onClickGiveFairydust()
        {
            if (Level.instance != null)
            {
                Building building = Level.instance.Buildings.FindAll(e => e.DisplaysLuminance).SelectRandom(1).FirstOrDefault();
                if (building != null)
                {
                    Player aPlayer = Level.instance.authoritativePlayer;
                    aPlayer.ClientGiveFairydust(this.model.netId, building.netId, this.GiftCost, this.GiftBenefit, this.PlayerLuminanceIncrement, this.BuildingLuminanceIncrement);
                    if (this.EndTurn)
                    {
                        aPlayer.ClientEndTurn();
                    }
                }
                else
                {
                    RootLogger.Exception(this, "The Level contains no buildings.");
                }
            }
            else
            {
                RootLogger.Exception(this, "The Level instance is null.");
            }
        }
    }
}
