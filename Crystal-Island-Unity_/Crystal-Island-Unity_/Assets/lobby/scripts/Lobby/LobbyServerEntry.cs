using UnityEngine;
using UnityEngine.UI;
using Mirror.Discovery;

namespace Prototype.NetworkLobby
{
    // Migrated from UNet relay matchmaking (MatchInfoSnapshot / matchMaker.JoinMatch) to Mirror
    // NetworkDiscovery. A discovered server is described by a ServerResponse (its reachable Uri).
    // NOTE: the built-in ServerResponse only carries the server's address. Server name and player
    // count need a custom NetworkDiscovery subclass with an extended response message (follow-up).
    public class LobbyServerEntry : MonoBehaviour
    {
        public Text serverInfoText;
        public Text slotInfo;
        public Button joinButton;

        public void Populate(ServerResponse response, LobbyManager lobbyManager, Color c)
        {
            serverInfoText.text = response.EndPoint != null
                ? response.EndPoint.Address.ToString()
                : response.uri.Host;

            slotInfo.text = "LAN";

            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() => { JoinServer(response, lobbyManager); });

            GetComponent<Image>().color = c;
        }

        void JoinServer(ServerResponse response, LobbyManager lobbyManager)
        {
            lobbyManager.networkAddress = response.uri.Host;
            lobbyManager.StartClient(response.uri);

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();
            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        }
    }
}
