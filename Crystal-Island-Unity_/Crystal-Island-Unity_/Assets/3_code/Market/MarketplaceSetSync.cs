using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using KoboldTools.Logging;

namespace Polymoney
{
    public interface IMarketplaceSetSyncProvider
    {
        void AddMarketplace(string marketplaceData, string marketplaceGuid);
        void RemoveMarketplace(string marketplaceGuid);
        void AddOffer(string marketplaceGuid, string offerData, string offerGuid);
        void RemoveOffer(string marketplaceguid, string offerGuid);
        void ClearOffers(string marketplaceGuid, bool persistent);
        void SetMarketSeller(string marketplaceGuid, NetworkIdentity sellerIdentity);
    }

    public class MarketplaceSetSync : NetworkBehaviour, IMarketplaceSetSyncProvider
    {
        public MarketplaceSet runtimeMarketplaces = null;

        private void OnEnable()
        {
            //set sync provider
            runtimeMarketplaces.syncProvider = this;
        }

        private void OnDisable()
        {
            runtimeMarketplaces.syncProvider = null;
        }

        public override void OnStartServer()
        {
            if (isServer)
                StartCoroutine(init());
        }

        /* public override on()
         {
             if (isServer)
                 StartCoroutine(init());
         }*/

        private IEnumerator init()
        {
            while(NetworkServer.connections.Any(c => !c.isReady)){
                yield return null;
            }

            //distribute Guids for initial marketplaces (depends on list order at initialization)
            int offerCount = 0;
            string[] newMarketGuids = new string[runtimeMarketplaces.marketplaces.Count];
            for (int i = 0; i < newMarketGuids.Length; i++)
            {
                newMarketGuids[i] = Guid.NewGuid().ToString();
                offerCount += runtimeMarketplaces.marketplaces[i].offers.Count;
            }
            string[] newOfferGuids = new string[offerCount];
            for (int i = 0; i < newOfferGuids.Length; i++)
            {
                newOfferGuids[i] = Guid.NewGuid().ToString();
            }
            RpcSetInitialGuids(newMarketGuids, newOfferGuids);
        }

        [ClientRpc]
        private void RpcSetInitialGuids(string[] marketGuids, string[] offerGuids)
        {
            int offerIndex = 0;
            int marketIndex = 0;
            foreach(Marketplace marketplace in runtimeMarketplaces.marketplaces)
            {
                if (marketIndex >= marketGuids.Length)
                {
                    RootLogger.Exception(this, "Not enough Guids provided for initial Marketplaces.");
                }
                else
                {
                    marketplace.guid = new Guid(marketGuids[marketIndex]);
                    marketIndex++;
                    foreach (Offer offer in marketplace.offers)
                    {
                        if (offerIndex >= offerGuids.Length)
                        {
                            RootLogger.Exception(this, "Not enough Guids provided for initial Offers.");
                        }
                        else
                        {
                            RootLogger.Debug(this, "Rpc: Set Guid of Offer '{0}' to '{1}'", offer.name, offerGuids[offerIndex]);
                            offer.guid = new Guid(offerGuids[offerIndex]);
                            offerIndex++;
                        }
                    }
                }
            }
        }
        public void AddMarketplace(string marketplaceData, string marketplaceGuid)
        {
            RpcAddMarketplace(marketplaceData, marketplaceGuid);
        }
        [ClientRpc]
        private void RpcAddMarketplace(string marketplaceData, string marketplaceGuid)
        {
            //create instance
            Marketplace newMarketplace = ScriptableObject.CreateInstance<Marketplace>();
            //overwrite with serialized data
            JsonUtility.FromJsonOverwrite(marketplaceData, newMarketplace);
            //create guid
            newMarketplace.guid = new Guid(marketplaceGuid);
            //add to marketplaces
            runtimeMarketplaces.addMarketplace(newMarketplace);
        }
        public void AddOffer(string marketplaceGuid, string offerData, string offerGuid)
        {
            RpcAddOffer(marketplaceGuid, offerData, offerGuid);
        }
        [ClientRpc]
        private void RpcAddOffer(string marketplaceGuid, string offerData, string offerGuid)
        {
            //create instance
            Offer newOffer = ScriptableObject.CreateInstance<Offer>();
            //overwrite with serialized data
            JsonUtility.FromJsonOverwrite(offerData, newOffer);
            //create guid
            newOffer.guid = new Guid(offerGuid);
            //add to marketplaces
            runtimeMarketplaces.getByGuid(marketplaceGuid).addOffer(newOffer);
        }
        public void RemoveMarketplace(string marketplaceGuid)
        {
            RpcRemoveMarketplace(marketplaceGuid);
        }
        [ClientRpc]
        private void RpcRemoveMarketplace(string marketplaceGuid)
        {
            runtimeMarketplaces.removeMarketplace(runtimeMarketplaces.getByGuid(marketplaceGuid));
        }
        public void RemoveOffer(string marketplaceGuid, string offerGuid)
        {
            RpcRemoveOffer(marketplaceGuid, offerGuid);
        }
        [ClientRpc]
        private void RpcRemoveOffer(string marketplaceGuid, string offerGuid)
        {
            Marketplace marketplace = runtimeMarketplaces.getByGuid(marketplaceGuid);
            Offer offer = marketplace.getOfferByGuid(offerGuid);
            marketplace.removeOffer(offer);
        }

        public void ClearOffers(string marketplaceGuid, bool persistent)
        {
            RpcClearOffers(marketplaceGuid, persistent);
        }

        [ClientRpc]
        private void RpcClearOffers(string marketplaceGuid, bool persistent)
        {
            Marketplace marketplace = runtimeMarketplaces.getByGuid(marketplaceGuid);
            marketplace.clearOffers(persistent);
        }

        public void SetMarketSeller(string marketplaceGuid, NetworkIdentity sellerIdentity)
        {
            RpcSetMarketSeller(marketplaceGuid, sellerIdentity);
        }
        [ClientRpc]
        private void RpcSetMarketSeller(string marketplaceGuid, NetworkIdentity sellerIdentity)
        {
            Player player = sellerIdentity.GetComponent<Player>();
            RootLogger.Debug(this, "Rpc: Market {0} has a new seller: {1}.", marketplaceGuid, player.name);
            runtimeMarketplaces.getByGuid(marketplaceGuid).seller = player;
        }
    }
}
