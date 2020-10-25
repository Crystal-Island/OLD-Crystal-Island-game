using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Polymoney
{
    public class PolymoneyDebug : MonoBehaviour
    {
        public Dropdown gameStateDropdown;
        public Button giveButton;
        public Button takeButton;
        public Offer giveOffer;
        public Offer takeOffer;

        // Use this for initialization
        void Start()
        {
            if (NetworkServer.active)
            {
                gameStateDropdown.interactable = true;
                gameStateDropdown.ClearOptions();
                gameStateDropdown.AddOptions(Enum.GetNames(typeof(PolymoneyGameFlow.FlowStates)).ToList());
                gameStateDropdown.onValueChanged.AddListener(stateDropdownValueChanged);
                PolymoneyGameFlow.instance.changeState.AddListener(gameStateChanged);
            }
            else
            {
                gameStateDropdown.interactable = false;
            }

            giveButton.onClick.AddListener(giveButtonClicked);
            takeButton.onClick.AddListener(takeButtonClicked);
        }

        // Update is called once per frame
        void OnDestroy()
        {
            giveButton.onClick.RemoveListener(giveButtonClicked);
            takeButton.onClick.RemoveListener(takeButtonClicked);
            gameStateDropdown.onValueChanged.RemoveListener(stateDropdownValueChanged);
            if(PolymoneyGameFlow.instance != null)
                PolymoneyGameFlow.instance.changeState.RemoveListener(gameStateChanged);
        }

        private void giveButtonClicked()
        {
            Level.instance.authoritativePlayer.ClientApplyOffer(giveOffer, Level.instance.authoritativePlayer, Level.instance.authoritativePlayer);
        }

        private void takeButtonClicked()
        {
            Level.instance.authoritativePlayer.ClientApplyOffer(takeOffer, Level.instance.authoritativePlayer, Level.instance.authoritativePlayer);
        }

        private void stateDropdownValueChanged(int option)
        {
            PolymoneyGameFlow.instance.forceState((int)Enum.Parse(typeof(PolymoneyGameFlow.FlowStates), gameStateDropdown.options[option].text));
        }

        private void gameStateChanged(int oldState, int newState)
        {
            int currentDropdownValue = (int)Enum.Parse(typeof(PolymoneyGameFlow.FlowStates), gameStateDropdown.options[gameStateDropdown.value].text);
            if(newState != currentDropdownValue)
            {
                gameStateDropdown.value = Enum.GetNames(typeof(PolymoneyGameFlow.FlowStates)).ToList().IndexOf(((PolymoneyGameFlow.FlowStates)newState).ToString());
            }
        }
    }
}
