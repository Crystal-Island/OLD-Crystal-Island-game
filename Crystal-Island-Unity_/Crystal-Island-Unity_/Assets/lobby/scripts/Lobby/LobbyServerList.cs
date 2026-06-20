using UnityEngine;
using Mirror.Discovery;
using System.Collections.Generic;

namespace Prototype.NetworkLobby
{
    // Migrated from UNet relay matchmaking (matchMaker.ListMatches) to Mirror's LAN NetworkDiscovery.
    // Servers advertise themselves (LobbyManager.OnRoomStartServer -> discovery.AdvertiseServer);
    // this panel broadcasts discovery requests and lists every server that replies.
    // Discovery is event-driven (no pages); for internet matchmaking use Edgegap lobby / a list-server.
    public class LobbyServerList : MonoBehaviour
    {
        public LobbyManager lobbyManager;

        public RectTransform serverListRect;
        public GameObject serverEntryPrefab;
        public GameObject noServerFound;

        static Color OddServerColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        static Color EvenServerColor = new Color(.94f, .94f, .94f, 1.0f);

        // Keyed by ServerResponse.serverId so the same host (possibly seen on multiple NICs) shows once.
        readonly Dictionary<long, GameObject> _discovered = new Dictionary<long, GameObject>();

        void OnEnable()
        {
            ClearList();
            noServerFound.SetActive(true);

            if (lobbyManager.discovery == null)
            {
                Debug.LogWarning("LobbyServerList: no NetworkDiscovery assigned on the LobbyManager; LAN browsing is disabled.");
                return;
            }

            lobbyManager.discovery.OnServerFound.AddListener(OnDiscoveredServer);
            lobbyManager.discovery.StartDiscovery();
        }

        void OnDisable()
        {
            if (lobbyManager.discovery != null)
            {
                lobbyManager.discovery.OnServerFound.RemoveListener(OnDiscoveredServer);
                lobbyManager.discovery.StopDiscovery();
            }

            ClearList();
        }

        void OnDiscoveredServer(ServerResponse info)
        {
            // Already listed: nothing to do (could refresh a "last seen" timestamp here later).
            if (_discovered.ContainsKey(info.serverId))
                return;

            noServerFound.SetActive(false);

            GameObject o = Instantiate(serverEntryPrefab) as GameObject;
            Color c = (_discovered.Count % 2 == 0) ? OddServerColor : EvenServerColor;
            o.GetComponent<LobbyServerEntry>().Populate(info, lobbyManager, c);
            o.transform.SetParent(serverListRect, false);

            _discovered.Add(info.serverId, o);
        }

        // Re-scan the LAN. Wired to the existing refresh / page buttons (direction is ignored now).
        public void ChangePage(int dir)
        {
            Refresh();
        }

        public void Refresh()
        {
            ClearList();
            noServerFound.SetActive(true);

            if (lobbyManager.discovery != null)
                lobbyManager.discovery.StartDiscovery();
        }

        void ClearList()
        {
            foreach (Transform t in serverListRect)
                Destroy(t.gameObject);

            _discovered.Clear();
        }
    }
}
