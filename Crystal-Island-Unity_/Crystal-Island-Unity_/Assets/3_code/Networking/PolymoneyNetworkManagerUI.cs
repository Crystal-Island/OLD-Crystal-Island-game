using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    // Migrated from UNet NetworkLobbyManager to Mirror NetworkRoomManager.
    //  - model type NetworkLobbyManager -> NetworkRoomManager
    //  - model.lobbySlots[] (fixed array, may contain nulls) -> model.roomSlots (HashSet, active players only)
    //  - model.maxPlayers -> model.maxConnections (Mirror's room size is bounded by maxConnections;
    //    set maxConnections on the manager to the intended player count).
    public class PolymoneyNetworkManagerUI : VCBehaviour<NetworkRoomManager>
    {
        public float displayFrequency = 1f;
        public LobbyPlayerUI lobbyPlayerUITemplate;
        public Text maxPlayersDisplay;
        public Button startGameButton;
        public GameObject waitingForPlayers;
        public string maxPlayersTextId = "maxPlayerInfo";
        public GameObject optionsController, optionPanel;

        private KoboldTools.Pool<LobbyPlayerUI> lobbyPlayerUIPool;
        private List<LobbyPlayerUI> lobbyPlayerUIUsed;
        private float sinceLastDisplay = 1f;
        private bool gameStarted = false;

        public override void onModelChanged()
        {
            if (Localisation.instance != null)
            {
                Localisation.instance.eLanguageChanged.AddListener(this.onLanguageChanged);
            }
            this.startGameButton.interactable = false;
            this.startGameButton.onClick.AddListener(this.onClickStartGame);
            this.startGameButton.gameObject.SetActive(false);
            this.waitingForPlayers.SetActive(true);

            this.lobbyPlayerUIPool = new KoboldTools.Pool<LobbyPlayerUI>(this.lobbyPlayerUITemplate);
            for (int i = 0; i < this.model.maxConnections; i++)
            {
                LobbyPlayerUI uiElement = this.lobbyPlayerUIPool.pop();
                uiElement.gameObject.SetActive(true);
            }
            this.lobbyPlayerUIUsed = this.lobbyPlayerUIPool.getUsed().ToList();
            this.lobbyPlayerUIUsed.Reverse();
        }

        private void Update()
        {
            if (this.model == null)
            {
                return;
            }

            //limit redraws
            if (this.sinceLastDisplay < this.displayFrequency)
            {
                this.sinceLastDisplay += Time.deltaTime;
                return;
            }
            this.sinceLastDisplay = 0f;

            // Update the models. roomSlots is a HashSet (was a fixed lobbySlots[] array in UNet), so
            // snapshot it to a list to map active room players into the fixed set of UI rows. Sort by the
            // room player index (a SyncVar Mirror assigns per player) so rows stay stable between frames.
            List<NetworkRoomPlayer> roomList = this.model.roomSlots.OrderBy(p => p.index).ToList();
            for (int i = 0; i < this.lobbyPlayerUIUsed.Count; i++)
            {
                LobbyPlayer slot = (i < roomList.Count) ? roomList[i] as LobbyPlayer : null;
                if (this.lobbyPlayerUIUsed[i].model != slot)
                {
                    this.lobbyPlayerUIUsed[i].onSetModel(slot);
                }
            }

            // Set the player language to the selected game language.
            if (Localisation.instance != null)
            {
                string langName = Localisation.instance.activeLanguage.langNameEnglish;
                foreach (NetworkRoomPlayer roomPlayer in this.model.roomSlots)
                {
                    LobbyPlayer player = roomPlayer as LobbyPlayer;
                    if (player != null && player.languageName != langName)
                    {
                        player.languageName = langName;
                    }
                }
            }

            // Activate the start game button if we're on the server.
            if (!this.startGameButton.gameObject.activeSelf && NetworkServer.active)
            {
                this.startGameButton.gameObject.SetActive(true);
            }

            // Make the start button interactable if all players are ready, and also disable the waiting notification.
            bool allPlayersReady = this.lobbyPlayerUIUsed.Where(e => e.model != null).All(e => e.model.playerReady);
            if (allPlayersReady)
            {
                if (this.waitingForPlayers.activeSelf)
                {
                    this.waitingForPlayers.SetActive(false);
                }
                if (NetworkServer.active && !this.startGameButton.interactable)
                {
                    this.gameStarted = false;
                    this.startGameButton.interactable = true;
                }
            }
            else
            {
                if (!this.waitingForPlayers.activeSelf)
                {
                    this.waitingForPlayers.SetActive(true);
                }
                if (NetworkServer.active && this.startGameButton.interactable)
                {
                    this.startGameButton.interactable = false;
                }
            }
        }

        private void onLanguageChanged()
        {
            this.maxPlayersDisplay.text = Localisation.instance.getLocalisedFormat(this.maxPlayersTextId, this.model.maxConnections);
        }

        private void onClickStartGame()
        {
            if (!this.gameStarted && NetworkServer.active)
            {
                this.gameStarted = true;
                // Host force-starts the game. We can't set NetworkRoomPlayer.readyToBegin from this
                // assembly (Mirror forbids cross-assembly [SyncVar] writes), and forcing every player
                // ready + CheckReadyToBegin() ultimately just calls ServerChangeScene(GameplayScene),
                // so drive the room->game transition directly. The game's own readiness gate uses
                // Polymoney.LobbyPlayer.playerReady (see allPlayersReady above), not Mirror's readyToBegin.
                this.model.ServerChangeScene(this.model.GameplayScene);
                this.startGameButton.gameObject.SetActive(false);


                // TODO: NETWORKING-MIGRATION - host->client lobby-options propagation still disabled.
                // BEHAVIOR LOST: when the server host clicks "Start Game" with one or more remote clients connected,
                // this previously broadcast the host's Options_Controller settings (tax mode, water-crystal turn,
                // disaster severity/frequency, mayor panel visibility, etc.) to every connected client via RpcUpdateSettings().
                // While disabled, host-selected lobby options will NOT be propagated - clients will run with their
                // local defaults, which is incorrect for shared-game gameplay. Re-wire once Options_Controller is migrated:
                //   if (NetworkServer.active && NetworkServer.connections.Count > 1)
                //       optionsController.GetComponent<Options_Controller>().RpcUpdateSettings();
            }
        }
    }
}
