using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PolymoneyNetworkManagerSetupPlayer : MonoBehaviourPunCallbacks
{
    public void OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayerObj, GameObject gamePlayerObj)
    {
        LobbyPlayer playerLobby = lobbyPlayerObj.GetComponent<LobbyPlayer>();
        Player playerGame = gamePlayerObj.GetComponentInChildren<Player>();

        playerGame.name = playerLobby.name;

        // Use Photon's custom properties to set RunsForMayor
        PhotonHashtable customProperties = new PhotonHashtable();
        customProperties["RunsForMayor"] = playerLobby.runsForMayor;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }
}
