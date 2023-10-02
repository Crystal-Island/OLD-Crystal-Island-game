using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{

    public class PolymoneyNetworkManagerUI : VCBehaviour<NetworkLobbyManager>
    {
        NetworkIdentity m_Identity;

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
            for (int i = 0; i < this.model.maxPlayers; i++)
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

            // Update the models
            for (int i = 0; i < this.model.lobbySlots.Length; i++)
            {
                if (this.lobbyPlayerUIUsed[i].model != this.model.lobbySlots[i])
                {
                    this.lobbyPlayerUIUsed[i].onSetModel(this.model.lobbySlots[i]);
                }
            }

            // Set the player language to the selected game language.
            if (Localisation.instance != null)
            {
                string langName = Localisation.instance.activeLanguage.langNameEnglish;
                foreach (LobbyPlayer player in this.model.lobbySlots)
                {
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
            this.maxPlayersDisplay.text = Localisation.instance.getLocalisedFormat(this.maxPlayersTextId, this.model.maxPlayers);
        }

        private void onClickStartGame()
        {
            if (!this.gameStarted && NetworkServer.active)
            {
                this.gameStarted = true;
                foreach (LobbyPlayer player in this.model.lobbySlots)
                {
                    if (player != null)
                    {
                        player.readyToBegin = true;
                    }
                }
                this.model.CheckReadyToBegin();
                this.startGameButton.gameObject.SetActive(false);
                
                
                if(GetComponent<NetworkDiscovery>().isServer && m_Identity.connectionToClient.connectionId > 1)
                {
                    //Apply options selected
                    print("Apply Option Menus");               
                    optionsController.GetComponent<Options_Controller>().RpcUpdateSettings();
                }
                
            }
        }
    }
}
