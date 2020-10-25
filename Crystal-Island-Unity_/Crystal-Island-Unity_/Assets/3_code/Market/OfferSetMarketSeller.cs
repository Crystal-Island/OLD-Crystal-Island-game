using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine.Networking;
using System.Linq;

namespace Polymoney{

    [System.Serializable]
    public class OfferMarketPair
    {
        public Offer offer;
        public Marketplace marketplace;
    }

    public class OfferSetMarketSeller : NetworkBehaviour
    {
        public MarketplaceSet runtimeMarketplaces;
        public OfferSetMarketPairSet offerMarketPairSet;

        private void Start()
        {
            if (!isServer)
                return;

            foreach(OfferMarketPair omp in offerMarketPairSet.offerMarketPairs)
            {
                omp.offer.offerApplied.AddListener(offerApplied);
            }
        }

        private void offerApplied(Offer offer, Player buyer)
        {
            OfferMarketPair[] pairs = offerMarketPairSet.offerMarketPairs.Where(p => p.offer.guid.Equals(offer.guid)).ToArray();
            foreach(OfferMarketPair pair in pairs)
            {
                runtimeMarketplaces.syncProvider.SetMarketSeller(pair.marketplace.guid.ToString(), buyer.GetComponent<NetworkIdentity>());
            }
        }

    }

}
