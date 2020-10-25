using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Polymoney
{
    public class PlayerMarketplaceDisplaySymbols : MarketplaceDisplaySymbols
    {
        public override Marketplace getMarketplace()
        {
            Player player = GetComponentInParent<Player>();
            return player.OwnMarketplace;
        }
    }
}
