using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Polymoney
{
    public class PolymoneyNetworkManagerUI : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public float displayFrequency = 1f;
        public LobbyPlayerUI lobbyPlayerUITemplate;
        public Text maxPlayersDisplay;
        public Button startGameButton;
        public GameObject waitingForPlayers;
        public string maxPlayersTextId = "maxPlayerInfo";
        public GameObject optionsController, optionPanel;

        private Pool<LobbyPlayerUI> lobbyPlayerUIPool;
        private List<LobbyPlayerUI> lobbyPlayerUIUsed;
        private float sinceLastDisplay = 1f;
        private bool gameStarted = false;

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            // Perform any initialization logic when a player joins a room.
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            // Handle logic when a new player enters the room.
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            // Handle logic when a player leaves the room.
        }

        public override void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == 1) // Event code for game start
            {
                // Handle the game start event.
            }
        }

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

            this.lobbyPlayerUIPool = new Pool<LobbyPlayerUI>(this.lobbyPlayerUITemplate);
            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                LobbyPlayerUI uiElement = this.lobbyPlayerUIPool.pop();
                uiElement.gameObject.SetActive(true);
            }
            this.lobbyPlayerUIUsed = this.lobbyPlayerUIPool.getUsed().ToList();
            this.lobbyPlayerUIUsed.Reverse();
        }

        private void Update()
        {
            if (PhotonNetwork.InRoom)
            {
                // Handle your UI updates and game logic here.
                if (this.sinceLastDisplay < this.displayFrequency)
                {
                    this.sinceLastDisplay += Time.deltaTime;
                    return;
                }
                this.sinceLastDisplay = 0f;

                for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    if (this.lobbyPlayerUIUsed[i].model != PhotonNetwork.PlayerList[i])
                    {
                        // Update UI elements with player information.
                        this.lobbyPlayerUIUsed[i].onSetModel(PhotonNetwork.PlayerList[i]);
                    }
                }

                // Handle other UI updates and game logic here.

                // Activate the start game button if we're the master client.
                if (!this.startGameButton.gameObject.activeSelf && PhotonNetwork.IsMasterClient)
                {
                    this.startGameButton.gameObject.SetActive(true);
                }

                bool allPlayersReady = true;
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    object readyObj;
                    if (player.CustomProperties.TryGetValue("PlayerReady", out readyObj))
                    {
                        bool playerReady = (bool)readyObj;
                        if (!playerReady)
                        {
                            allPlayersReady = false;
                            break;
                        }
                    }
                    else
                    {
                        allPlayersReady = false;
                        break;
                    }
                }

                if (allPlayersReady)
                {
                    if (this.waitingForPlayers.activeSelf)
                    {
                        this.waitingForPlayers.SetActive(false);
                    }
                    if (PhotonNetwork.IsMasterClient && !this.startGameButton.interactable)
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
                    if (PhotonNetwork.IsMasterClient && this.startGameButton.interactable)
                    {
                        this.startGameButton.interactable = false;
                    }
                }
            }
        }

        private void onLanguageChanged()
        {
            this.maxPlayersDisplay.text = Localisation.instance.getLocalisedFormat(this.maxPlayersTextId, PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        private void onClickStartGame()
        {
            if (!this.gameStarted && PhotonNetwork.IsMasterClient)
            {
                this.gameStarted = true;

                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    Hashtable customProperties = new Hashtable();
                    customProperties["PlayerReady"] = true;
                    player.SetCustomProperties(customProperties);
                }

                // Continue with your game start logic.

                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    // Apply options selected.
                    print("Apply Option Menus");
                    optionsController.GetComponent<Options_Controller>().RpcUpdateSettings();
                }

                // Set a custom room property to indicate that the game has started.
                Hashtable roomProps = new Hashtable();
                roomProps["GameStarted"] = true;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                // Trigger an event to notify other players that the game has started.
                RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(1, null, options, SendOptions.SendReliable);
            }
        }
    }
}
