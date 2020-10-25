using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    /// <summary>
    /// Handles the UI of the LobbyPlayer.
    /// </summary>
    public class LobbyPlayerUI : VCBehaviour<LobbyPlayer>
    {
        public Toggle readyToggle;
        public Toggle mayorToggle;
        public InputField nicknameInput;
        public GameObject empty;
        public GameObject used;

        private new void Start()
        {
            //initialize without model
            this.empty.gameObject.SetActive(true);
            this.used.gameObject.SetActive(false);

            //call base start to initialize vc
            base.Start();
        }

        public override void onModelChanged()
        {
            //show used state
            this.empty.gameObject.SetActive(false);
            this.used.gameObject.SetActive(true);

            //add listeners
            this.readyToggle.onValueChanged.AddListener(this.readyChanged);
            this.mayorToggle.onValueChanged.AddListener(this.mayorChanged);
            this.nicknameInput.onEndEdit.AddListener(this.nicknameChanged);
            this.model.stateChanged.AddListener(this.modelStateChanged);

            //initialize state
            this.modelStateChanged();
        }

        public override void onModelRemoved()
        {
            //remove listeners
            this.readyToggle.onValueChanged.RemoveListener(this.readyChanged);
            this.mayorToggle.onValueChanged.RemoveListener(this.mayorChanged);
            this.nicknameInput.onEndEdit.RemoveListener(this.nicknameChanged);
            this.model.stateChanged.RemoveListener(this.modelStateChanged);

            //set empty state
            this.empty.gameObject.SetActive(true);
            this.used.gameObject.SetActive(false);
        }

        private void modelStateChanged()
        {
            this.nicknameInput.text = this.model.name + " "  + this.model.netId;
            this.readyToggle.isOn = this.model.playerReady;
            this.mayorToggle.isOn = this.model.runsForMayor;

            if (!this.model.isLocalPlayer)
            {
                this.nicknameInput.interactable = false;
                this.readyToggle.interactable = false;
                this.mayorToggle.interactable = false;
            }
            else
            {
                this.readyToggle.interactable = true;
                this.nicknameInput.interactable = !this.model.playerReady;
                this.mayorToggle.interactable = !this.model.playerReady;
            }
        }

        private void readyChanged(bool newValue)
        {
            if (this.model.isLocalPlayer)
            {
                this.model.ClientPlayerReady(newValue);
                this.mayorToggle.interactable = !newValue;
                this.nicknameInput.interactable = !newValue;
            }
        }

        private void mayorChanged(bool newValue)
        {
            if (this.model.isLocalPlayer)
            {
                this.model.ClientChangeMayor(newValue);
            }
        }

        private void nicknameChanged(string newValue)
        {
            if (this.model.isLocalPlayer)
            {
                this.model.ClientChangeName(newValue);
            }
        }

    }
}
