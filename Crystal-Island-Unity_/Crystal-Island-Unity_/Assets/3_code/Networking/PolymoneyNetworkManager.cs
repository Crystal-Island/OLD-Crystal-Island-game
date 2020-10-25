using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Polymoney {
    [Serializable]
    public class HostStatus {
        private bool _paused;
        private bool _focused;
        private bool _screenBlocked;
        public bool PauseChanged;
        public bool FocusChanged;
        public bool ScreenBlockChanged;

        public bool Paused {
            get {
                // this.PauseChanged = false;
                return this._paused;
            }
            set {
                if (this._paused != value) {
                    this._paused = value;
                    this.PauseChanged = true;
                }
            }
        }

        public bool Focused {
            get {
                // this.FocusChanged = false;
                return this._focused;
            }
            set {
                if (this._focused != value) {
                    this._focused = value;
                    this.FocusChanged = true;
                }
            }
        }

        public bool ScreenBlocked {
            get {
                // this.ScreenBlockChanged = false;
                return this._screenBlocked;
            }
            set {
                if (this._screenBlocked != value) {
                    this._screenBlocked = value;
                    this.ScreenBlockChanged = true;
                }
            }
        }

        public HostStatus() {
            this._paused = false;
            this._focused = true;
            this._screenBlocked = false;
            this.PauseChanged = true;
            this.FocusChanged = true;
            this.ScreenBlockChanged = true;
        }

        public HostStatus(NetworkStatusMessage msg) {
            this._paused = false;
            this._focused = true;
            this._screenBlocked = false;
            this.PauseChanged = true;
            this.FocusChanged = true;
            this.ScreenBlockChanged = true;

            this.UpdateWith(msg);
        }

        public void UpdateWith(NetworkStatusMessage msg) {
            if (msg.Event == NetworkStatusEvent.PAUSE) {
                this.Paused = msg.Status;
            } else if (msg.Event == NetworkStatusEvent.FOCUS) {
                this.Focused = msg.Status;
            } else if (msg.Event == NetworkStatusEvent.BLOCK_SCREEN) {
                this.ScreenBlocked = msg.Status;
            }
        }

        public void AssumeAvailable() {
            this.Paused = false;
        }

        public void AssumeUnavailable() {
            this.Paused = true;
        }

        public void ClearDirtyFlags() {
            this.PauseChanged = false;
            this.FocusChanged = false;
            this.ScreenBlockChanged = false;
        }

        public override string ToString() {
            return String.Format("HostStatus(paused={0}, focused={1}, screen={2}, dirty={3})", this._paused, this._focused, this._screenBlocked, this.PauseChanged || this.FocusChanged || this.ScreenBlockChanged);
        }
    }

    public class BlockScreenEvent : UnityEvent<bool> { }

    /// <summary>
    /// The purpose of the PolymoneyNetworkManager is to provide a UI with which players may create or join a networked game.
    /// The class inherits from Unity HLAPI NetworkLobbyManager, and requires a NetworkDiscovery component to add network discovery functionality.
    /// </summary>
    [RequireComponent(typeof(NetworkDiscovery))]
    public class PolymoneyNetworkManager : NetworkLobbyManager, IStateManager {
        /// <summary>
        /// Holds a reference to the create-game button.
        /// </summary>
        [Header("Buttons")]
        public Button createGameButton;
        /// <summary>
        /// Holds a reference to the confirm-create-game button. If clicked, causes
        /// the client to create a local network server.
        /// </summary>
        public Button confirmCreateGameButton;
        /// <summary>
        /// Holds a reference to the cancel-pre-lobby button during game
        /// creation. If clicked, causes the network discovery system to stop
        /// searching for available local network games.
        /// </summary>
        public Button cancelCreateGameButton;
        /// <summary>
        /// Holds a reference to the cancel-lobby button. If clicked, aborts the game setup.
        /// </summary>
        public Button cancelLobbyButton;
        /// <summary>
        /// Holds a reference to te reconnect button. If clicket, restarts the game.
        /// </summary>
        public Button reconnectButton;
        /// <summary>
        /// Determines the name of the game, if one is created by the player.
        /// </summary>
        [Header("Inputs")]
        public InputField gamenameInput;

        /// <summary>
        /// If set to <c>true</c>, prevents the device from going into sleep mode.
        /// </summary>
        [Header("Other")]
        public bool preventSleep;
        /// <summary>
        /// Holds a reference to the <see cref="NetworkDiscovery"/> component.
        /// </summary>
        public NetworkDiscovery networkDiscovery;
        /// <summary>
        /// Holds a reference to the <see cref="Canvas"/>.
        /// </summary>
        public Canvas canvas;
        /// <summary>
        /// Issued, when the game should pause or unpause.
        /// </summary>
        public BlockScreenEvent OnBlockScreen = new BlockScreenEvent();
        /// <summary>
        /// Records the status of the local client.
        /// </summary>
        public HostStatus LocalClientStatus = new HostStatus();

        /// <summary>
        /// Records the status of all clients in the network. This is only used on the server.
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="HostStatus"></typeparam>
        private Dictionary<int, HostStatus> HostStates = new Dictionary<int, HostStatus>(); 

        /// <summary>
        /// Adds event listeners for the UI buttons and initializes the Unity network discovery system.
        /// </summary>
        private void Start() {
            // Prevent phone from sleeping to keep network connection up
            if (this.preventSleep) {
                RootLogger.Info(this, "Setting the device to never fall asleep.");
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            // Assign the network discovery reference.
            if (this.networkDiscovery == null) {
                this.networkDiscovery = this.GetComponent<NetworkDiscovery>();
            }

            // Assign the canvas reference.
            if (this.canvas == null) {
                this.canvas = this.GetComponentInChildren<Canvas>();
            }

            // Add UI listeners.
            this.createGameButton.onClick.AddListener(this.onClickCreateGame);
            this.confirmCreateGameButton.onClick.AddListener(this.onClickConfirmCreateGame);
            this.cancelCreateGameButton.onClick.AddListener(this.onClickCancelCreateGame);
            this.cancelLobbyButton.onClick.AddListener(this.onClickCancelLobby);
            this.reconnectButton.onClick.AddListener(this.onClickReconnect);

            // Listen for game discovery broadcasts.
            this.networkDiscovery.Initialize();
            this.networkDiscovery.StartAsClient();
        }

        private void OnApplicationQuit() {
            RootLogger.Debug(this, "OnApplicationQuit() called");
            if (NetworkClient.active) {
                RootLogger.Debug(this, "OnApplicationQuit() called on a client");
                NetworkStatusMessage msg = new NetworkStatusMessage(NetworkRole.CLIENT, NetworkStatusEvent.QUIT, true);
                this.client.SendByChannel(PolymoneyMsgType.NetworkStatus, msg, 1);
                this.client.connection.FlushChannels();
            }
        }

        // This coroutine is only called twice before the game is paused
        // (OnApplicationPause(true)). Once the game resumes, the coroutine is
        // started again (OnApplicationPause(false)), while the previously
        // started coroutine (OnApplicationPause(true)) continues to run.
        private IEnumerator OnApplicationPause(bool pause) {
            RootLogger.Debug(this, "OnApplicationPause({0}) started", pause);
            if (NetworkServer.active) {
                RootLogger.Debug(this, "OnApplicationPause({0}) called on the server", pause);
                if (pause) {
                    NetworkStatusMessage msg = new NetworkStatusMessage(NetworkRole.SERVER, NetworkStatusEvent.BLOCK_SCREEN, pause);
                    bool success = this.SendToAvailable(NetworkServer.connections, PolymoneyMsgType.NetworkStatus, msg, 1, true);
                    yield return null;
                    if (!success) {
                        this.SendToAvailable(NetworkServer.connections, PolymoneyMsgType.NetworkStatus, msg, 1, true);
                    }
                } else {
                    // Send out a call to all clients to see whether they are available.
                    // Assume none of the clients are available.
                    foreach (NetworkConnection conn in NetworkServer.connections) {
                        int id = conn.connectionId;
                        if (this.HostStates.ContainsKey(id)) {
                            this.HostStates[id].AssumeUnavailable();
                        } else {
                            HostStatus h = new HostStatus();
                            h.AssumeUnavailable();
                            this.HostStates.Add(id, h);
                        }
                        conn.SendByChannel(PolymoneyMsgType.ClientAvailable, new ClientAvailableMessage(), 1);
                    }

                    this.UpdateScreenBlockStatus();
                }
            } else if (NetworkClient.active) {
                RootLogger.Debug(this, "OnApplicationPause({0}) called on a client", pause);
                NetworkStatusMessage msg = new NetworkStatusMessage(NetworkRole.CLIENT, NetworkStatusEvent.PAUSE, pause);
                bool success = this.client.SendByChannel(PolymoneyMsgType.NetworkStatus, msg, 1);
                this.client.connection.FlushChannels();
                yield return null;
                if (!success) {
                    this.client.SendByChannel(PolymoneyMsgType.NetworkStatus, msg, 1);
                    this.client.connection.FlushChannels();
                }
            }
        }

        // This coroutine is called only twice before the game is paused
        // (OnApplicationFocus(false)), but only if the game pauses directly
        // instead of losing focus first. If the game loses focus first, the
        // coroutine is executed normally. When the game comes back, the
        // coroutine is started anew (OnApplicationFocus(true)), but if the game
        // is merely unsuspended but not refocused, another coroutine
        // (OnApplicationFocus(false)) is started, and the previous coroutine is
        // continued. This causes calls of three coroutines to be interleaved.
        private IEnumerator OnApplicationFocus(bool focus) {
            RootLogger.Debug(this, "OnApplicationFocus({0}) started", focus);
            if (NetworkClient.active) {
                RootLogger.Debug(this, "OnApplicationFocus({0}) called on a client", focus);
                NetworkStatusMessage msg = new NetworkStatusMessage(NetworkRole.CLIENT, NetworkStatusEvent.FOCUS, focus);
                bool success = this.client.SendByChannel(PolymoneyMsgType.NetworkStatus, msg, 1);
                this.client.connection.FlushChannels();
                yield return null;
                if (!success) {
                    this.client.SendByChannel(PolymoneyMsgType.NetworkStatus, msg, 1);
                    this.client.connection.FlushChannels();
                }
            }
        }

        private void OnServerNetworkStatusMessage(NetworkMessage message) {
            NetworkStatusMessage statusMsg = message.ReadMessage<NetworkStatusMessage>();
            RootLogger.Debug(this, "Received message from client: {0} (from: {1})", statusMsg, message.conn);
            if (this.HostStates.ContainsKey(message.conn.connectionId)) {
                this.HostStates[message.conn.connectionId].UpdateWith(statusMsg);
            } else {
                this.HostStates.Add(message.conn.connectionId, new HostStatus(statusMsg));
            }

            this.UpdateScreenBlockStatus();
        }

        private void OnClientNetworkStatusMessage(NetworkMessage message) {
            NetworkStatusMessage statusMsg = message.ReadMessage<NetworkStatusMessage>();
            RootLogger.Debug(this, "Received message from server: {0}", statusMsg);
            this.LocalClientStatus.UpdateWith(statusMsg);
            if (this.LocalClientStatus.ScreenBlockChanged) {
                RootLogger.Debug(this, "The block-screen status for this client has changed to {0}", this.LocalClientStatus.ScreenBlocked);
                this.OnBlockScreen.Invoke(this.LocalClientStatus.ScreenBlocked);
                this.LocalClientStatus.ClearDirtyFlags();
            }
        }

        private void OnServerClientAvailableMessage(NetworkMessage message) {
            RootLogger.Debug(this, "A client has told the server that it is available");
            int id = message.conn.connectionId;
            if (this.HostStates.ContainsKey(id)) {
                this.HostStates[id].AssumeAvailable();
            } else {
                this.HostStates.Add(id, new HostStatus());
            }

            this.UpdateScreenBlockStatus();
        }

        private void OnClientClientAvailableMessage(NetworkMessage message) {
            RootLogger.Debug(this, "The client has been asked whether it is available, and it will respond with yes");
            this.LocalClientStatus.AssumeAvailable();
            this.client.SendByChannel(PolymoneyMsgType.ClientAvailable, new ClientAvailableMessage(), 0);
        }

        private void UpdateScreenBlockStatus() {
            if (this.HostStates.Values.Any(v => v.PauseChanged)) {
                if (this.HostStates.Values.Any(v => v.Paused)) {
                    RootLogger.Debug(this, "At least one of the clients is unavailable; the game must pause.");
                    NetworkStatusMessage msgb = new NetworkStatusMessage(NetworkRole.SERVER, NetworkStatusEvent.BLOCK_SCREEN, true);
                    this.SendToAvailable(NetworkServer.connections, PolymoneyMsgType.NetworkStatus, msgb, 0, false);
                } else {
                    RootLogger.Debug(this, "All clients are available; the game may unpause.");
                    NetworkStatusMessage msgb = new NetworkStatusMessage(NetworkRole.SERVER, NetworkStatusEvent.BLOCK_SCREEN, false);
                    this.SendToAvailable(NetworkServer.connections, PolymoneyMsgType.NetworkStatus, msgb, 0, false);
                }
                foreach (HostStatus s in this.HostStates.Values) {
                    s.ClearDirtyFlags();
                }
            }
        }

        private bool SendToAvailable(IEnumerable conns, short msgType, MessageBase msg, int channelId, bool immediate) {
            List<bool> successes = new List<bool>();
            foreach (NetworkConnection conn in conns) {
                if (conn != null) {
                    int id = conn.connectionId;
                    if (!this.HostStates.ContainsKey(id)) {
                        this.HostStates.Add(id, new HostStatus());
                    }
                    if (!this.HostStates[id].Paused) {
                        successes.Add(conn.SendByChannel(PolymoneyMsgType.NetworkStatus, msg, channelId));
                        if (immediate) {
                            conn.FlushChannels();
                        }
                    }
                }
            }

            return successes.All(s => s);
        }

        /// <summary>
        /// To be called when a client tries to reconnect to a server.
        /// </summary>
        public void onClickReconnect() {
            RootLogger.Debug(this, "onClickReconnect()");
            /*List<GameObject> result = new List<GameObject>();
            List<GameObject> rootGameObjectsExceptDontDestroyOnLoad = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                rootGameObjectsExceptDontDestroyOnLoad.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());
            }

            List<GameObject> rootGameObjects = new List<GameObject>();
            Transform[] allTransforms = FindObjectsOfType<Transform>(); //FindObjectsOfTypeAll<Transform>() Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform root = allTransforms[i].root;
                if (root.hideFlags == HideFlags.None && !rootGameObjects.Contains(root.gameObject))
                {
                    rootGameObjects.Add(root.gameObject);
                }
            }

            for (int i = 0; i < rootGameObjects.Count; i++)
            {
                if (!rootGameObjectsExceptDontDestroyOnLoad.Contains(rootGameObjects[i]))
                    result.Add(rootGameObjects[i]);
            }

            //foreach( GameObject obj in result )
            //    Debug.Log( obj );
            for(int i = result.Count-1; i>=0; i--)
            {
                Destroy(result[i]);
            }*/

            SceneManager.LoadScene(0);
        }

        public override void OnStartClient(NetworkClient lobbyClient) {
            RootLogger.Debug(this, "OnStartClient(client={0}) for server at {1}", lobbyClient, lobbyClient.serverIp);
            base.OnStartClient(lobbyClient);
            lobbyClient.RegisterHandler(PolymoneyMsgType.NetworkStatus, this.OnClientNetworkStatusMessage);
            lobbyClient.RegisterHandler(PolymoneyMsgType.ClientAvailable, this.OnClientClientAvailableMessage);
        }

        public override void OnLobbyStartClient(NetworkClient lobbyClient) {
            RootLogger.Debug(this, "OnLobbyStartClient(client={0})", lobbyClient);
            base.OnLobbyStartClient(lobbyClient);
        }

        public override void OnStopClient() {
            RootLogger.Debug(this, "OnStopClient()");
            this.client.UnregisterHandler(PolymoneyMsgType.NetworkStatus);
            this.client.UnregisterHandler(PolymoneyMsgType.ClientAvailable);
            base.OnStopClient();
        }

        public override void OnLobbyClientAddPlayerFailed() {
            RootLogger.Debug(this, "OnLobbyStartClient()");
            base.OnLobbyClientAddPlayerFailed();
        }

        public override void OnClientError(NetworkConnection conn, int errorCode) {
            RootLogger.Warning(this, "OnClientError(conn={0}, error={1})", conn, errorCode);
            base.OnClientError(conn, errorCode);
        }

        public override void OnLobbyServerPlayersReady() {
            RootLogger.Debug(this, "OnLobbyServerPlayersReady()");
            base.OnLobbyServerPlayersReady();
        }

        public override void OnLobbyClientConnect(NetworkConnection conn) {
            RootLogger.Debug(this, "OnLobbyClientConnect(conn={0}) with address {1})", conn, conn.address);
            base.OnLobbyClientConnect(conn);
        }

        public override void OnClientConnect(NetworkConnection conn) {
            RootLogger.Debug(this, "OnClientConnect(conn={0})", conn);
            if (!this.HostStates.ContainsKey(conn.connectionId)) {
                this.HostStates.Add(conn.connectionId, new HostStatus());
            }
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn) {
            RootLogger.Warning(this, "OnClientDisconnect(conn={0})", conn);
            base.OnClientDisconnect(conn);
        }

        public override void OnClientNotReady(NetworkConnection conn) {
            RootLogger.Debug(this, "OnClientNotReady(conn={0})", conn);
            base.OnClientNotReady(conn);
        }

        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
            RootLogger.Debug(this, "OnLobbyServerCreateGamePlayer(conn={0}, controllerId={1})", conn, playerControllerId);
            return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
        }

        public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId) {
            RootLogger.Debug(this, "OnLobbyServerCreateLobbyPlayer(conn={0}, controllerId={1})", conn, playerControllerId);
            return base.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
        }

        /// <summary>
        /// To be called when a player creates a new game. Stops looking for
        /// other games and causes the internal state machine to migrate to
        /// state <see cref="UIState.CREATEGAME"/>.
        /// </summary>
        public void onClickCreateGame() {
            if (this.networkDiscovery.running) {
                this.networkDiscovery.StopBroadcast();
            }
            this.stateManager.onChangeState((int) UIState.CREATEGAME);
        }
        /// <summary>
        /// To be called when a player confirms the creation of a new game.
        /// Causes a local game host (server+client in one) to be spun up.
        /// </summary>
        public void onClickConfirmCreateGame() {
            this.confirmCreateGameButton.interactable = false;
            this.StartHost();
        }
        /// <summary>
        /// To be called when backing out of game creation. Results in a state
        /// change to <see cref="UIState.PRELOBBY"/> and restarts network
        /// discovery to look for other games.
        /// </summary>
        public void onClickCancelCreateGame() {
            if (!this.networkDiscovery.running) {
                this.networkDiscovery.Initialize();
                this.networkDiscovery.StartAsClient();
            }
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
        }
        /// <summary>
        /// To be called when backing out of the lobby (game-setup). Stops the
        /// host, restarts the network discovery, and changes to the
        /// <see cref="UIState.PRELOBBY"/> game state.
        /// </summary>
        public void onClickCancelLobby() {
            this.StopHost();
            this.networkDiscovery.StopBroadcast();
            this.networkDiscovery.Initialize();
            this.networkDiscovery.StartAsClient();
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
        }
        /// <summary>
        /// Called when the game client is to enter the lobby. Results in a state change to <see cref="UIState.LOBBY"/>.
        /// </summary>
        public override void OnLobbyClientEnter() {
            this.stateManager.onChangeState((int) UIState.LOBBY);
        }
        /// <summary>
        /// Called when the game client is to exit the lobby. Results in a state change to <see cref="UIState.PRELOBBY"/>.
        /// </summary>
        public override void OnLobbyClientExit() {
            //this.stateManager.onChangeState((int)UIState.PRELOBBY);
        }
        public override void OnLobbyServerConnect(NetworkConnection conn) {
            RootLogger.Info(this, "Server: A new client has connected.");

            if (NetworkServer.connections.Count > this.maxConnections) {
                RootLogger.Warning(this, "Server: I have more connections than allowed.");
                conn.Disconnect();
            }
        }
        public override void OnLobbyServerDisconnect(NetworkConnection conn) {
            RootLogger.Warning(this, "Server: Have lost connection to a client.");
            //this.networkDiscovery.Initialize();
            //this.networkDiscovery.StartAsServer();
        }
        public override void OnLobbyClientDisconnect(NetworkConnection conn) {
            RootLogger.Info(this, "Client: Have lost connection to the server.");
            this.stateManager.onChangeState((int) UIState.RECONNECT);
        }
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
            RootLogger.Info(this, "Server: A player was added.");
            base.OnServerAddPlayer(conn, playerControllerId);
        }
        public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId) {
            RootLogger.Info(this, "Server: A player was removed.");
        }
        /// <summary>
        /// Called when a host should be spun up. Sets the broadcast data and starts a server.
        /// </summary>
        public override void OnStartHost() {
            RootLogger.Debug(this, "OnStartHost()");
            // Set the broadcast for game discovery.
            string broadcastData = String.Format("PolymoneyGame:{0}:{1}:20120", this.gamenameInput.text, Network.player.ipAddress);
            RootLogger.Info(this, "Server: Setting broadcast data to '{0}'", broadcastData);

            this.networkDiscovery.useNetworkManager = false;
            this.networkDiscovery.broadcastData = broadcastData;
            this.networkDiscovery.StartAsServer();
            base.OnStartHost();
        }

        public override void OnLobbyStartHost() {
            RootLogger.Debug(this, "OnLobbyStartHost()");
            base.OnLobbyStartHost();
        }

        public override void OnStartServer() {
            RootLogger.Debug(this, "OnStartServer()");
            base.OnStartServer();
            NetworkServer.RegisterHandler(PolymoneyMsgType.NetworkStatus, this.OnServerNetworkStatusMessage);
            NetworkServer.RegisterHandler(PolymoneyMsgType.ClientAvailable, this.OnServerClientAvailableMessage);
        }

        public override void OnLobbyStartServer() {
            RootLogger.Debug(this, "OnLobbyStartServer()");
            base.OnLobbyStartServer();
        }

        public override void OnLobbyStopHost() {
            RootLogger.Debug(this, "OnLobbyStopHost()");
            KoboldTools.DontDestroyOnLoad.destroyAll();
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
            base.OnLobbyStopHost();
        }

        public override void OnStopServer() {
            RootLogger.Debug(this, "OnStopServer()");
            NetworkServer.UnregisterHandler(PolymoneyMsgType.NetworkStatus);
            NetworkServer.UnregisterHandler(PolymoneyMsgType.ClientAvailable);
            base.OnStopServer();
        }

        public override void OnLobbyStopClient() {
            KoboldTools.DontDestroyOnLoad.destroyAll();
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
            base.OnLobbyStopClient();
        }
        /// <summary>
        /// Called on the server when a networked scene has finished loading. Stops network discovery broadcasting
        /// when the lobby scene is not active. Subsequently calls the base class' event handler of the same name.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        public override void OnLobbyServerSceneChanged(string sceneName) {
            // Stop broadcast when not in lobby scene.
            if (sceneName != this.lobbyScene) {
                this.networkDiscovery.StopBroadcast();
            }
            base.OnLobbyServerSceneChanged(sceneName);
        }
        /// <summary>
        /// Called on the client when a new networked scene has finished loading. Stops network discovery
        /// broadcasting when the lobby scene is not active. Subsequently calls the base class' event handler
        /// of the same name.
        /// </summary>
        /// <param name="conn">The relevant network connection.</param>
        public override void OnLobbyClientSceneChanged(NetworkConnection conn) {
            // Stop broadcast when not in lobby scene
            if (SceneManager.GetActiveScene().name != this.lobbyScene) {
                if (this.networkDiscovery.running) {
                    this.networkDiscovery.StopBroadcast();
                }
                this.stateManager.onChangeState((int) UIState.NONE);
            }
            base.OnLobbyClientSceneChanged(conn);
        }
        /// <summary>
        /// Called on the server when a client has completed switching from the lobby scene to a game player scene.
        /// </summary>
        /// <param name="lobbyPlayer">Lobby player.</param>
        /// <param name="gamePlayer">Game player.</param>
        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
            PolymoneyNetworkManagerSetupPlayer playerSetup = this.GetComponent<PolymoneyNetworkManagerSetupPlayer>();
            if (playerSetup != null) {
                playerSetup.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
            }

            return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
        }

        #region StateManager
        [Flags]
        [StateFlags]
        public enum UIState {
            PRELOBBY = 1 << 0,
            CREATEGAME = 1 << 1,
            GAMELIST = 1 << 2,
            LOBBY = 1 << 4,
            NONE = 1 << 5,
            RECONNECT = 1 << 6
        }

        private StateManager stateManager = new StateManager((int) UIState.PRELOBBY);
        public int currentState {
            get {
                return stateManager.currentState;
            }
        }

        public UnityEvent<int, int> changeState {
            get {
                return stateManager.changeState;
            }
        }

        public void addState(int state) {
            stateManager.addState(state);
        }

        public bool hasState(int state) {
            return stateManager.hasState(state);
        }

        public void onChangeState(int newState) {
            stateManager.onChangeState(newState);
        }

        public void removeAndAddState(int removeState, int addState) {
            stateManager.removeAndAddState(removeState, addState);
        }

        public void removeState(int state) {
            stateManager.removeState(state);
        }
        #endregion
    }
}