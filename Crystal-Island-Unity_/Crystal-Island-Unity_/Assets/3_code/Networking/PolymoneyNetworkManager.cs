using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Mirror.Discovery;
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
    /// Migrated from UNet NetworkLobbyManager to Mirror NetworkRoomManager. LAN game discovery is provided by the
    /// <see cref="PolymoneyNetworkDiscovery"/> component (Mirror replacement for UNet NetworkDiscovery).
    /// </summary>
    [RequireComponent(typeof(PolymoneyNetworkDiscovery))]
    public class PolymoneyNetworkManager : NetworkRoomManager, IStateManager {
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
        /// Holds a reference to the <see cref="PolymoneyNetworkDiscovery"/> component.
        /// </summary>
        public PolymoneyNetworkDiscovery networkDiscovery;
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
        /// Adds event listeners for the UI buttons and initializes the network discovery system.
        /// </summary>
        public override void Start() {
            base.Start();

            // Prevent phone from sleeping to keep network connection up
            if (this.preventSleep) {
                RootLogger.Info(this, "Setting the device to never fall asleep.");
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            // Assign the network discovery reference.
            if (this.networkDiscovery == null) {
                this.networkDiscovery = this.GetComponent<PolymoneyNetworkDiscovery>();
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

            // Listen for game discovery broadcasts (browse for available LAN games).
            this.BrowseForGames();
        }

        // ----------------- Discovery helpers (Mirror NetworkDiscovery) -----------------

        // Start broadcasting discovery requests to find LAN games. Safe to call repeatedly.
        private void BrowseForGames() {
            if (this.networkDiscovery == null) {
                return;
            }
            this.networkDiscovery.StopDiscovery();
            this.networkDiscovery.StartDiscovery();
        }

        // Start replying to discovery requests so this host appears in clients' game lists.
        private void AdvertiseGame() {
            if (this.networkDiscovery == null) {
                return;
            }
            this.networkDiscovery.StopDiscovery();
            this.networkDiscovery.AdvertiseServer();
        }

        private void StopDiscovery() {
            if (this.networkDiscovery != null) {
                this.networkDiscovery.StopDiscovery();
            }
        }

        public override void OnApplicationQuit() {
            RootLogger.Debug(this, "OnApplicationQuit() called");
            if (NetworkClient.active) {
                RootLogger.Debug(this, "OnApplicationQuit() called on a client");
                NetworkStatusMessage msg = new NetworkStatusMessage(NetworkRole.CLIENT, NetworkStatusEvent.QUIT, true);
                NetworkClient.Send(msg, Channels.Reliable);
            }
            base.OnApplicationQuit();
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
                    this.SendToAvailable(NetworkServer.connections.Values, msg, Channels.Reliable);
                    yield return null;
                } else {
                    // Send out a call to all clients to see whether they are available.
                    // Assume none of the clients are available.
                    foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values) {
                        int id = conn.connectionId;
                        if (this.HostStates.ContainsKey(id)) {
                            this.HostStates[id].AssumeUnavailable();
                        } else {
                            HostStatus h = new HostStatus();
                            h.AssumeUnavailable();
                            this.HostStates.Add(id, h);
                        }
                        conn.Send(new ClientAvailableMessage(), Channels.Reliable);
                    }

                    this.UpdateScreenBlockStatus();
                }
            } else if (NetworkClient.active) {
                RootLogger.Debug(this, "OnApplicationPause({0}) called on a client", pause);
                NetworkStatusMessage msg = new NetworkStatusMessage(NetworkRole.CLIENT, NetworkStatusEvent.PAUSE, pause);
                NetworkClient.Send(msg, Channels.Reliable);
                yield return null;
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
                NetworkClient.Send(msg, Channels.Reliable);
                yield return null;
            }
        }

        // ----------------- Message handlers (Mirror typed handlers) -----------------

        private void OnServerNetworkStatusMessage(NetworkConnectionToClient conn, NetworkStatusMessage statusMsg) {
            RootLogger.Debug(this, "Received message from client: {0} (from: {1})", statusMsg, conn);
            if (this.HostStates.ContainsKey(conn.connectionId)) {
                this.HostStates[conn.connectionId].UpdateWith(statusMsg);
            } else {
                this.HostStates.Add(conn.connectionId, new HostStatus(statusMsg));
            }

            this.UpdateScreenBlockStatus();
        }

        private void OnClientNetworkStatusMessage(NetworkStatusMessage statusMsg) {
            RootLogger.Debug(this, "Received message from server: {0}", statusMsg);
            this.LocalClientStatus.UpdateWith(statusMsg);
            if (this.LocalClientStatus.ScreenBlockChanged) {
                RootLogger.Debug(this, "The block-screen status for this client has changed to {0}", this.LocalClientStatus.ScreenBlocked);
                this.OnBlockScreen.Invoke(this.LocalClientStatus.ScreenBlocked);
                this.LocalClientStatus.ClearDirtyFlags();
            }
        }

        private void OnServerClientAvailableMessage(NetworkConnectionToClient conn, ClientAvailableMessage message) {
            RootLogger.Debug(this, "A client has told the server that it is available");
            int id = conn.connectionId;
            if (this.HostStates.ContainsKey(id)) {
                this.HostStates[id].AssumeAvailable();
            } else {
                this.HostStates.Add(id, new HostStatus());
            }

            this.UpdateScreenBlockStatus();
        }

        private void OnClientClientAvailableMessage(ClientAvailableMessage message) {
            RootLogger.Debug(this, "The client has been asked whether it is available, and it will respond with yes");
            this.LocalClientStatus.AssumeAvailable();
            NetworkClient.Send(new ClientAvailableMessage(), Channels.Reliable);
        }

        private void UpdateScreenBlockStatus() {
            if (this.HostStates.Values.Any(v => v.PauseChanged)) {
                if (this.HostStates.Values.Any(v => v.Paused)) {
                    RootLogger.Debug(this, "At least one of the clients is unavailable; the game must pause.");
                    NetworkStatusMessage msgb = new NetworkStatusMessage(NetworkRole.SERVER, NetworkStatusEvent.BLOCK_SCREEN, true);
                    this.SendToAvailable(NetworkServer.connections.Values, msgb, Channels.Reliable);
                } else {
                    RootLogger.Debug(this, "All clients are available; the game may unpause.");
                    NetworkStatusMessage msgb = new NetworkStatusMessage(NetworkRole.SERVER, NetworkStatusEvent.BLOCK_SCREEN, false);
                    this.SendToAvailable(NetworkServer.connections.Values, msgb, Channels.Reliable);
                }
                foreach (HostStatus s in this.HostStates.Values) {
                    s.ClearDirtyFlags();
                }
            }
        }

        // Mirror's conn.Send returns void (reliable channel is guaranteed), so the UNet per-send
        // success/retry bookkeeping is gone. Only sends to clients we believe are available.
        private void SendToAvailable(IEnumerable<NetworkConnectionToClient> conns, NetworkStatusMessage msg, int channelId) {
            foreach (NetworkConnectionToClient conn in conns) {
                if (conn != null) {
                    int id = conn.connectionId;
                    if (!this.HostStates.ContainsKey(id)) {
                        this.HostStates.Add(id, new HostStatus());
                    }
                    if (!this.HostStates[id].Paused) {
                        conn.Send(msg, channelId);
                    }
                }
            }
        }

        /// <summary>
        /// To be called when a client tries to reconnect to a server.
        /// </summary>
        public void onClickReconnect() {
            RootLogger.Debug(this, "onClickReconnect()");

            SceneManager.LoadScene(0);
        }

        public override void OnRoomStartClient() {
            RootLogger.Debug(this, "OnRoomStartClient()");
            base.OnRoomStartClient();
            NetworkClient.RegisterHandler<NetworkStatusMessage>(this.OnClientNetworkStatusMessage, false);
            NetworkClient.RegisterHandler<ClientAvailableMessage>(this.OnClientClientAvailableMessage, false);
        }

        public override void OnRoomStopClient() {
            RootLogger.Debug(this, "OnRoomStopClient()");
            NetworkClient.UnregisterHandler<NetworkStatusMessage>();
            NetworkClient.UnregisterHandler<ClientAvailableMessage>();
            KoboldTools.DontDestroyOnLoad.destroyAll();
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
            base.OnRoomStopClient();
        }

        public override void OnClientError(TransportError error, string reason) {
            RootLogger.Warning(this, "OnClientError(error={0}, reason={1})", error, reason);
            base.OnClientError(error, reason);
        }

        public override void OnClientTransportException(System.Exception exception) {
            base.OnClientTransportException(exception);
        }

        public override void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason) {
            base.OnServerError(conn, error, reason);
        }

        public override void OnRoomServerPlayersReady() {
            RootLogger.Debug(this, "OnRoomServerPlayersReady()");
            base.OnRoomServerPlayersReady();
        }

        public override void OnRoomClientConnect() {
            RootLogger.Debug(this, "OnRoomClientConnect()");
            base.OnRoomClientConnect();
        }

        public override void OnClientNotReady() {
            RootLogger.Debug(this, "OnClientNotReady()");
            base.OnClientNotReady();
        }

        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer) {
            RootLogger.Debug(this, "OnRoomServerCreateGamePlayer(conn={0})", conn);
            return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
        }

        public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn) {
            RootLogger.Debug(this, "OnRoomServerCreateRoomPlayer(conn={0})", conn);
            return base.OnRoomServerCreateRoomPlayer(conn);
        }

        /// <summary>
        /// To be called when a player creates a new game. Stops looking for
        /// other games and causes the internal state machine to migrate to
        /// state <see cref="UIState.CREATEGAME"/>.
        /// </summary>
        public void onClickCreateGame() {
            this.StopDiscovery();
            this.stateManager.onChangeState((int) UIState.CREATEGAME);
        }
        /// <summary>
        /// To be called when a player confirms the creation of a new game.
        /// Causes a local game host (server+client in one) to be spun up.
        /// </summary>
        public void onClickConfirmCreateGame() {
            this.confirmCreateGameButton.interactable = false;
            // Make the chosen game name available to the discovery responder before hosting.
            if (this.networkDiscovery != null && this.gamenameInput != null) {
                this.networkDiscovery.gameName = this.gamenameInput.text;
            }
            this.StartHost();
        }
        /// <summary>
        /// To be called when backing out of game creation. Results in a state
        /// change to <see cref="UIState.PRELOBBY"/> and restarts network
        /// discovery to look for other games.
        /// </summary>
        public void onClickCancelCreateGame() {
            this.BrowseForGames();
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
        }
        /// <summary>
        /// To be called when backing out of the lobby (game-setup). Stops the
        /// host, restarts the network discovery, and changes to the
        /// <see cref="UIState.PRELOBBY"/> game state.
        /// </summary>
        public void onClickCancelLobby() {
            this.StopHost();
            this.BrowseForGames();
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
        }
        /// <summary>
        /// Called when the game client is to enter the room. Results in a state change to <see cref="UIState.LOBBY"/>.
        /// </summary>
        public override void OnRoomClientEnter() {
            this.stateManager.onChangeState((int) UIState.LOBBY);
        }
        /// <summary>
        /// Called when the game client is to exit the room. Results in a state change to <see cref="UIState.PRELOBBY"/>.
        /// </summary>
        public override void OnRoomClientExit() {
            //this.stateManager.onChangeState((int)UIState.PRELOBBY);
        }
        public override void OnRoomServerConnect(NetworkConnectionToClient conn) {
            RootLogger.Info(this, "Server: A new client has connected.");

            if (NetworkServer.connections.Count > this.maxConnections) {
                RootLogger.Warning(this, "Server: I have more connections than allowed.");
                conn.Disconnect();
                return;
            }

            if (!this.HostStates.ContainsKey(conn.connectionId)) {
                this.HostStates.Add(conn.connectionId, new HostStatus());
            }
        }
        public override void OnRoomServerDisconnect(NetworkConnectionToClient conn) {
            RootLogger.Warning(this, "Server: Have lost connection to a client.");
            this.HostStates.Remove(conn.connectionId);
            base.OnRoomServerDisconnect(conn);
        }
        public override void OnRoomClientDisconnect() {
            RootLogger.Info(this, "Client: Have lost connection to the server.");
            this.stateManager.onChangeState((int) UIState.RECONNECT);
            base.OnRoomClientDisconnect();
        }
        public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
            RootLogger.Info(this, "Server: A player was added.");
            base.OnServerAddPlayer(conn);
        }
        /// <summary>
        /// Called when a host should be spun up. Advertises the game on the LAN and starts a server.
        /// </summary>
        public override void OnRoomStartHost() {
            RootLogger.Debug(this, "OnRoomStartHost()");
            base.OnRoomStartHost();
        }

        public override void OnRoomStartServer() {
            RootLogger.Debug(this, "OnRoomStartServer()");
            base.OnRoomStartServer();
            // requireAuthentication: false because this project has no NetworkAuthenticator; otherwise
            // these app-level status messages could be silently dropped.
            NetworkServer.RegisterHandler<NetworkStatusMessage>(this.OnServerNetworkStatusMessage, false);
            NetworkServer.RegisterHandler<ClientAvailableMessage>(this.OnServerClientAvailableMessage, false);

            // Restores the UNet "broadcast PolymoneyGame:<name>:<ip>:<port>" behaviour: any Crystal Island
            // client browsing the LAN will now see this host's game in its pre-lobby game list and can join.
            this.AdvertiseGame();
        }

        public override void OnRoomStopServer() {
            RootLogger.Debug(this, "OnRoomStopServer()");
            NetworkServer.UnregisterHandler<NetworkStatusMessage>();
            NetworkServer.UnregisterHandler<ClientAvailableMessage>();
            this.StopDiscovery();
            base.OnRoomStopServer();
        }

        public override void OnRoomStopHost() {
            RootLogger.Debug(this, "OnRoomStopHost()");
            KoboldTools.DontDestroyOnLoad.destroyAll();
            this.stateManager.onChangeState((int) UIState.PRELOBBY);
            base.OnRoomStopHost();
        }
        /// <summary>
        /// Called on the server when a networked scene has finished loading. Stops network discovery broadcasting
        /// when the room scene is not active. Subsequently calls the base class' event handler of the same name.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        public override void OnRoomServerSceneChanged(string sceneName) {
            // Stop advertising when not in the room scene.
            if (sceneName != this.RoomScene) {
                this.StopDiscovery();
            }
            base.OnRoomServerSceneChanged(sceneName);
        }
        /// <summary>
        /// Called on the client when a new networked scene has finished loading. Stops network discovery
        /// broadcasting when the room scene is not active. Subsequently calls the base class' event handler
        /// of the same name.
        /// </summary>
        public override void OnRoomClientSceneChanged() {
            // Stop discovery when not in room scene
            if (SceneManager.GetActiveScene().name != this.RoomScene) {
                this.StopDiscovery();
                this.stateManager.onChangeState((int) UIState.NONE);
            }
            base.OnRoomClientSceneChanged();
        }
        /// <summary>
        /// Called on the server when a client has completed switching from the room scene to a game player scene.
        /// </summary>
        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer) {
            PolymoneyNetworkManagerSetupPlayer playerSetup = this.GetComponent<PolymoneyNetworkManagerSetupPlayer>();
            if (playerSetup != null) {
                playerSetup.OnLobbyServerSceneLoadedForPlayer(roomPlayer, gamePlayer);
            }

            return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
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
