using System.Linq;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class BuildingDisplaySymbols : MarketplaceDisplaySymbols
    {
        public override Marketplace getMarketplace()
        {
            Building building = GetComponentInParent<Building>();
            return building.Marketplace;
        }
    }
}
