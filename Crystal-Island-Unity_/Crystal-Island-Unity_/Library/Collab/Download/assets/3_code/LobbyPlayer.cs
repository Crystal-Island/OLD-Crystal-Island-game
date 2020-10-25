using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// The Player representation in the lobby. Handles all state, that the player can change in the lobby.
    /// </summary>
    public class LobbyPlayer : NetworkLobbyPlayer
    {
        [SyncVar(hook = "playerReadyChanged")]
        public bool playerReady = false;
        [SyncVar(hook = "languageNameChanged")]
        public string languageName = "English";
        [SyncVar(hook = "mayorChanged")]
        public bool runsForMayor = false;
        [SyncVar(hook = "nameChanged")]
        public new string name = "Player"; //loca defaultPlayerName
        public UnityEvent stateChanged = new UnityEvent();

        public void ClientPlayerReady(bool newValue)
        {
            this.CmdPlayerReady(newValue);
        }

        [Command]
        private void CmdPlayerReady(bool newValue)
        {
            if (this.playerReady != newValue)
            {
                this.playerReady = newValue;
                if (!this.isClient)
                {
                    this.stateChanged.Invoke();
                }
            }
        }

        public void ClientChangeName(string newValue)
        {
            this.CmdChangeName(newValue);
        }

        //rpc namechange method that is invoked by a local client on the network
        [Command]
        private void CmdChangeName(string newValue)
        {
            this.name = newValue;
            if (!this.isClient)
            {
                this.stateChanged.Invoke();
            }
        }

        public void ClientChangeMayor(bool newValue)
        {
            this.CmdChangeMayor(newValue);
        }

        [Command]
        private void CmdChangeMayor(bool newValue)
        {
            this.runsForMayor = newValue;
            if (!this.isClient)
            {
                this.stateChanged.Invoke();
            }
        }

        private void playerReadyChanged(bool newValue)
        {
            this.playerReady = newValue;
            this.stateChanged.Invoke();
        }

        private void languageNameChanged(string newValue)
        {
            this.languageName = newValue;
            if (!String.IsNullOrEmpty(newValue))
            {
                var lang = Localisation.instance.languages.Values.ToList().Find(e => e.langNameEnglish == newValue);
                if (lang != null)
                {
                    RootLogger.Info(this, "Setting the language to: {0}", newValue);
                    Localisation.instance.activeLanguage = lang;
                }
                else
                {
                    RootLogger.Exception(this, "The selected language '{0}' was not found!", newValue);
                }
            }
            this.stateChanged.Invoke();
        }

        private void mayorChanged(bool newValue)
        {
            this.runsForMayor = newValue;
            this.stateChanged.Invoke();
        }

        private void nameChanged(string newValue)
        {
            this.name = newValue;
            if (!this.isClient)
            {
                this.stateChanged.Invoke();
            }
        }
    }
}
