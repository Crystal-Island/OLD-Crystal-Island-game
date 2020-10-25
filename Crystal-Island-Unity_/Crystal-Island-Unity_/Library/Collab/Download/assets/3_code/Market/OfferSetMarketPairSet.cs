using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Polymoney
{
    [CreateAssetMenu(fileName = "OfferSetMarketPairSet", menuName = "New OfferSetMarketPairSet")]
    public class OfferSetMarketPairSet : ScriptableObject
    {
        public OfferMarketPair[] offerMarketPairs;
    }
}
