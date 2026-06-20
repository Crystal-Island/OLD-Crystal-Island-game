using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// The Player representation in the lobby. Handles all state, that the player can change in the lobby.
    /// </summary>
    // Migrated from UNet NetworkLobbyPlayer to Mirror NetworkRoomPlayer.
    // Mirror SyncVar hooks must take (oldValue, newValue); the backing field is already set before
    // the hook fires. Hook names use nameof for compile-time safety.
    public class LobbyPlayer : NetworkRoomPlayer
    {
        [SyncVar(hook = nameof(playerReadyChanged))]
        public bool playerReady = false;
        [SyncVar(hook = nameof(languageNameChanged))]
        public string languageName = "English";
        [SyncVar(hook = nameof(mayorChanged))]
        public bool runsForMayor = false;
        [SyncVar(hook = nameof(nameChanged))]
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

        private void playerReadyChanged(bool oldValue, bool newValue)
        {
            this.playerReady = newValue;
            this.stateChanged.Invoke();
        }

        private void languageNameChanged(string oldValue, string newValue)
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

        private void mayorChanged(bool oldValue, bool newValue)
        {
            this.runsForMayor = newValue;
            this.stateChanged.Invoke();
        }

        private void nameChanged(string oldValue, string newValue)
        {
            this.name = newValue;
            if (!this.isClient)
            {
                this.stateChanged.Invoke();
            }
        }
    }
}
