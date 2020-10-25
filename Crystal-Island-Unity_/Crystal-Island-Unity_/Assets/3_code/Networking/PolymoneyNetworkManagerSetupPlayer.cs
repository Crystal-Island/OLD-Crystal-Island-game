using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;
using KoboldTools.Logging;

namespace Polymoney
{

    public class PolymoneyNetworkManagerSetupPlayer : MonoBehaviour
    {
        public void OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayerObj, GameObject gamePlayerObj)
        {
            LobbyPlayer playerLobby = lobbyPlayerObj.GetComponent<LobbyPlayer>();
            Player playerGame = gamePlayerObj.GetComponentInChildren<Player>();

            playerGame.name = playerLobby.name;
            if (playerLobby.runsForMayor)
            {
                RootLogger.Info(this, "The player {0} would like to run for mayor.", playerLobby.name);
                playerGame.RunsForMayor = true;
            }

        }
    }
}
