using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KoboldTools;
using System;

namespace Polymoney
{
    public class PlayerCreateMarketplace : VCBehaviour<Player>
    {
        public MarketplaceSet runtimeMarketplaces;
        private string marketplaceGuid = "";

        public override void onModelChanged()
        {
            GameFlow.instance.changeState.AddListener(gameStateChanged);
        }

        public override void onModelRemoved()
        {
            runtimeMarketplaces.syncProvider.RemoveMarketplace(marketplaceGuid);
            GameFlow.instance.changeState.AddListener(gameStateChanged);
        }

        private void gameStateChanged(int oldState, int newState)
        {
            if(oldState == (int)PolymoneyGameFlow.FlowStates.CHARACTER_GENERATION)
            {
                //player name is set after character generation
                //create marketplace if it doesn't exist
                if (runtimeMarketplaces.getByGuid(marketplaceGuid) == null)
                {
                    createMarketplace();
                }
            }
        }

        private void createMarketplace()
        {
            //setup marketplace and use syncprovider of the marketplace set to rpc creation over the network
            Marketplace newMarketplace = ScriptableObject.CreateInstance<Marketplace>();
            newMarketplace.init(model.Person.LocalisedTitle, "");
            marketplaceGuid = Guid.NewGuid().ToString();
            //TODO: this will only work on server. Change it to a command on the player!
            runtimeMarketplaces.syncProvider.AddMarketplace(JsonUtility.ToJson(newMarketplace),marketplaceGuid);
            //marketplace was only used to serialize it into json. destroy it now.
            Destroy(newMarketplace);
        }
    }
}
