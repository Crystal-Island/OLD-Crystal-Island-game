using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;
using System.Collections;

namespace Prototype.NetworkLobby
{
    // Migrated from UNet NetworkLobbyManager to Mirror NetworkRoomManager.
    // Notes on the migration:
    //  - UNet relay matchmaking (matchMaker / MatchInfo / OnMatchCreate / OnDestroyMatch /
    //    StopMatchMaker) has no Mirror equivalent and was removed. Use direct connect or
    //    NetworkDiscovery instead (see LobbyServerList / LobbyMainMenu migration).
    //  - Mirror supports a single player per connection, so the UNet "add local player"
    //    flow (maxPlayersPerConnection / ClientScene.localPlayers / addPlayerButton) is gone.
    //  - lobbySlots[] (fixed array) -> roomSlots (HashSet<NetworkRoomPlayer>).
    //  - The custom kick message is now a Mirror struct message instead of a MsgType + MessageBase.
    public class LobbyManager : NetworkRoomManager
    {
        // Mirror struct message replacing the UNet "MsgType.Highest + 1" / MessageBase kick message.
        public struct KickMsg : NetworkMessage { }

        static public LobbyManager s_Singleton;


        [Header("Unity UI Lobby")]
        [Tooltip("Time in second between all players ready & match start")]
        public float prematchCountdown = 5.0f;

        [Space]
        [Header("UI Reference")]
        public LobbyTopPanel topPanel;

        public RectTransform mainMenuPanel;
        public RectTransform lobbyPanel;

        public LobbyInfoPanel infoPanel;
        public LobbyCountdownPanel countdownPanel;
        public GameObject addPlayerButton;

        protected RectTransform currentPanel;

        public Button backButton;

        public Text statusInfo;
        public Text hostInfo;

        //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
        //of players, so that even client know how many player there is.
        [HideInInspector]
        public int _playerNumber = 0;

        protected LobbyHook _lobbyHooks;

        protected Coroutine _countdownCoroutine;

        // Replaces UNet relay matchmaking with Mirror's LAN NetworkDiscovery.
        // Assign the NetworkDiscovery component (on the LobbyManager object) in the inspector.
        // For internet matchmaking, swap this for Edgegap lobby or a custom list-server later.
        [Header("LAN Discovery (replaces UNet matchmaking)")]
        public NetworkDiscovery discovery;

        public override void Start()
        {
            base.Start();

            s_Singleton = this;
            _lobbyHooks = GetComponent<Prototype.NetworkLobby.LobbyHook>();
            currentPanel = mainMenuPanel;

            backButton.gameObject.SetActive(false);
            GetComponent<Canvas>().enabled = true;

            DontDestroyOnLoad(gameObject);

            SetServerInfo("Offline", "None");
        }

        public override void OnRoomClientSceneChanged()
        {
            if (Utils.IsSceneActive(RoomScene))
            {
                if (topPanel.isInGame)
                {
                    ChangeTo(lobbyPanel);

                    // Mirror dropped UNet matchmaking, so the host is simply whoever is also running the server.
                    if (NetworkServer.active)
                        backDelegate = StopHostClbk;
                    else
                        backDelegate = StopClientClbk;
                }
                else
                {
                    ChangeTo(mainMenuPanel);
                }

                topPanel.ToggleVisibility(true);
                topPanel.isInGame = false;
            }
            else
            {
                ChangeTo(null);

                Destroy(GameObject.Find("MainMenuUI(Clone)"));

                //backDelegate = StopGameClbk;
                topPanel.isInGame = true;
                topPanel.ToggleVisibility(false);
            }
        }

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }

            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }

            currentPanel = newPanel;

            if (currentPanel != mainMenuPanel)
            {
                backButton.gameObject.SetActive(true);
            }
            else
            {
                backButton.gameObject.SetActive(false);
                SetServerInfo("Offline", "None");
            }
        }

        public void DisplayIsConnecting()
        {
            var _this = this;
            infoPanel.Display("Connecting...", "Cancel", () => { _this.backDelegate(); });
        }

        public void SetServerInfo(string status, string host)
        {
            statusInfo.text = status;
            hostInfo.text = host;
        }


        public delegate void BackButtonDelegate();
        public BackButtonDelegate backDelegate;
        public void GoBackButton()
        {
            backDelegate();
			topPanel.isInGame = false;
        }

        // ----------------- Server management

        public void AddLocalPlayer()
        {
            // Mirror supports only a single player per connection; multiple local players
            // are no longer available, so this is intentionally a no-op.
        }

        public void RemovePlayer(LobbyPlayer player)
        {
            player.RemovePlayer();
        }

        public void SimpleBackClbk()
        {
            ChangeTo(mainMenuPanel);
        }

        public void StopHostClbk()
        {
            StopHost();
            ChangeTo(mainMenuPanel);
        }

        public void StopClientClbk()
        {
            StopClient();
            ChangeTo(mainMenuPanel);
        }

        public void StopServerClbk()
        {
            StopServer();
            ChangeTo(mainMenuPanel);
        }

        public void KickPlayer(NetworkConnectionToClient conn)
        {
            conn.Send(new KickMsg());
        }

        public void KickedMessageHandler(KickMsg msg)
        {
            infoPanel.Display("Kicked by Server", "Close", null);
            NetworkClient.Disconnect();
        }

        //===================

        public override void OnRoomStartHost()
        {
            base.OnRoomStartHost();

            ChangeTo(lobbyPanel);
            backDelegate = StopHostClbk;
            SetServerInfo("Hosting", networkAddress);
        }

        // Start advertising this server to LAN clients (covers both Host and dedicated Server,
        // since StartHost internally starts the server).
        public override void OnRoomStartServer()
        {
            base.OnRoomStartServer();

            if (discovery != null)
                discovery.AdvertiseServer();
        }

        public override void OnRoomStopServer()
        {
            base.OnRoomStopServer();

            if (discovery != null)
                discovery.StopDiscovery();
        }

        //allow to handle the (+) button to add/remove player
        public void OnPlayersNumberModified(int count)
        {
            _playerNumber += count;

            // Mirror supports a single player per connection, so the UNet "add local player"
            // button is obsolete. Keep it hidden.
            if (addPlayerButton != null)
                addPlayerButton.SetActive(false);
        }

        // ----------------- Server callbacks ------------------

        //we want to disable the button JOIN if we don't have enough player
        //But OnRoomClientConnect isn't called on hosting player. So we override the roomPlayer creation
        public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
        {
            GameObject obj = Instantiate(roomPlayerPrefab.gameObject) as GameObject;

            LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>();
            newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);

            foreach (NetworkRoomPlayer roomSlot in roomSlots)
            {
                LobbyPlayer p = roomSlot as LobbyPlayer;

                if (p != null)
                {
                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
                }
            }

            return obj;
        }

        public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnRoomServerDisconnect(conn);

            foreach (NetworkRoomPlayer roomSlot in roomSlots)
            {
                LobbyPlayer p = roomSlot as LobbyPlayer;

                if (p != null)
                {
                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers >= minPlayers);
                }
            }
        }

        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
        {
            //This hook allows you to apply state data from the room-player to the game-player
            //just subclass "LobbyHook" and add it to the lobby object.

            if (_lobbyHooks)
                _lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, roomPlayer, gamePlayer);

            return true;
        }

        // --- Countdown management

        public override void OnRoomServerPlayersReady()
        {
            // Base Mirror behaviour immediately changes to the GameplayScene; instead we run a
            // pre-match countdown and only then start the game (see ServerCountdownCoroutine).
            if (_countdownCoroutine == null)
                _countdownCoroutine = StartCoroutine(ServerCountdownCoroutine());
        }

        public override void OnRoomServerPlayersNotReady()
        {
            base.OnRoomServerPlayersNotReady();

            // A player un-readied while the countdown was running: abort the launch.
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;

                foreach (NetworkRoomPlayer roomSlot in roomSlots)
                {
                    LobbyPlayer p = roomSlot as LobbyPlayer;
                    if (p != null)
                        p.RpcUpdateCountdown(0);
                }
            }
        }

        public IEnumerator ServerCountdownCoroutine()
        {
            float remainingTime = prematchCountdown;
            int floorTime = Mathf.FloorToInt(remainingTime);

            while (remainingTime > 0)
            {
                yield return null;

                remainingTime -= Time.deltaTime;
                int newFloorTime = Mathf.FloorToInt(remainingTime);

                if (newFloorTime != floorTime)
                {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                    floorTime = newFloorTime;

                    foreach (NetworkRoomPlayer roomSlot in roomSlots)
                    {
                        LobbyPlayer p = roomSlot as LobbyPlayer;
                        if (p != null)
                            p.RpcUpdateCountdown(floorTime);
                    }
                }
            }

            foreach (NetworkRoomPlayer roomSlot in roomSlots)
            {
                LobbyPlayer p = roomSlot as LobbyPlayer;
                if (p != null)
                    p.RpcUpdateCountdown(0);
            }

            _countdownCoroutine = null;

            ServerChangeScene(GameplayScene);
        }

        // ----------------- Client callbacks ------------------

        public override void OnRoomStartClient()
        {
            base.OnRoomStartClient();

            // Mirror replaces UNet's per-connection RegisterHandler with typed client handlers.
            NetworkClient.RegisterHandler<KickMsg>(KickedMessageHandler, false);
        }

        public override void OnRoomClientConnect()
        {
            base.OnRoomClientConnect();

            infoPanel.gameObject.SetActive(false);

            if (!NetworkServer.active)
            {//only to do on pure client (not self hosting client)
                ChangeTo(lobbyPanel);
                backDelegate = StopClientClbk;
                SetServerInfo("Client", networkAddress);
            }
        }

        public override void OnRoomClientDisconnect()
        {
            base.OnRoomClientDisconnect();
            ChangeTo(mainMenuPanel);
        }

        public override void OnClientError(TransportError error, string reason)
        {
            ChangeTo(mainMenuPanel);
            infoPanel.Display("Client error : " + error + " (" + reason + ")", "Close", null);
        }
    }
}
