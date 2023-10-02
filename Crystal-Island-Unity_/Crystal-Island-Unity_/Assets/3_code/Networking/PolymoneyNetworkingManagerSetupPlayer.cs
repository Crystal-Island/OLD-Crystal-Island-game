using Photon.Pun;
using UnityEngine;

namespace Polymoney
{
    public class PolymoneyNetworkManagerSetupPlayer : MonoBehaviour
    {
        public void OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayerObj, GameObject gamePlayerObj)
        {
            LobbyPlayer playerLobby = lobbyPlayerObj.GetComponent<LobbyPlayer>();
            Player playerGame = gamePlayerObj.GetComponentInChildren<Player>();

            playerGame.name = playerLobby.name;

            // Use Photon's networking to sync the RunsForMayor property.
            playerGame.photonView.RPC("SyncRunsForMayor", RpcTarget.AllBuffered, playerLobby.runsForMayor);
        }
    }
}
