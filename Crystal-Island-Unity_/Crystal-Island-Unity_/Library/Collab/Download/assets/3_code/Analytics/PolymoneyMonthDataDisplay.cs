using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.UI;
using System.Linq;

namespace Polymoney
{
    public class PolymoneyMonthDataDisplay : VCBehaviour<PolymoneyMonthData>
    {
        public RectTransform container;
        public UiResource uiResource;
        public Image cityFinaceTransform;
        public Image playerFTransform;
        public Image playerQTransform;
        private int graphHeight = 400;

        public override void onModelChanged()
        {
            //get current transform height
            graphHeight = Mathf.FloorToInt(container.rect.height);

            //colors
            cityFinaceTransform.color = uiResource.currencyColors[2];
            playerFTransform.color = uiResource.currencyColors[0];
            playerQTransform.color = uiResource.currencyColors[1];

            //balances
            float maxBalance = (float)RootAnalytics.GetMaxBalance();
            float playerFiat = 0f;
            float playerQ = 0f;
            int count = 0;

            foreach(PolymoneyPlayerData p in model.Players)
            {
                Player current = Level.instance.allPlayers.FirstOrDefault(c => c.netId.Value == p.PlayerId);
                if (!current.Mayor)
                {
                    playerFiat += (float)p.FiatAccountBalance;
                    playerQ += (float)p.QAccountBalance;
                    count++;
                }
            }

            playerFiat = playerFiat / count;
            playerQ = playerQ / count;
            
            //visual sizes
            cityFinaceTransform.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)graphHeight * model.Game.CityAccountBalance / maxBalance);
            playerFTransform.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((float)graphHeight * (playerFiat + playerQ) / maxBalance)); //add q balance for display
            playerQTransform.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)graphHeight * playerQ / maxBalance);
        }
    }
}
