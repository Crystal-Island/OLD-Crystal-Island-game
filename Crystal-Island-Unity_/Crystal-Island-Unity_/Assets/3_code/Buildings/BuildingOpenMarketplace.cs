using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class BuildingOpenMarketplace : VCBehaviour<Building>
    {
        [Tooltip("If set to true, the marketplace will only be accessible if polymoney has been introduced.")]
        public bool RequirePolymoney = false;

        public override void onModelChanged()
        {
            model.interacted.AddListener(interacted);
        }

        public override void onModelRemoved()
        {
            model.interacted.RemoveListener(interacted);
        }

        private void interacted()
        {
            if (model.Marketplace.seller != null)
            {
                if (
                    !this.RequirePolymoney || (this.RequirePolymoney && Level.instance.PolymoneyIntroduced)
                    && (model.Marketplace.offers.Count > 0 || model.Marketplace.seller == Level.instance.authoritativePlayer)             
                    ) {
                    Level.instance.authoritativePlayer.WatchedMarket = model.Marketplace;
                }
            }
            else
            {
                RootLogger.Warning(this, "Cannot open the marketplace, because no seller is set");
            }
        }
    }
}
