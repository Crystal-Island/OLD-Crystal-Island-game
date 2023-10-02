using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Polymoney {
    public class PolymoneyNetworkDiscoveryUI : MonoBehaviourPunCallbacks {
        public float displayFrequency = 1f;
        public GameObject gameButtonTemplate;

        private List<GameObject> buttons = new List<GameObject>();
        private float sinceLastDisplay = 1f;

        public override void OnEnable() {
            base.OnEnable();

            // Start Photon Network.
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster() {
            // Join a Photon lobby.
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby() {
            Debug.Log("Joined Photon Lobby");
        }

        private void Update() {
            // Limit redraws
            if (sinceLastDisplay < displayFrequency) {
                sinceLastDisplay += Time.deltaTime;
                return;
            }
            sinceLastDisplay = 0f;

            // Clear the previous list of buttons
            foreach (var button in buttons) {
                Destroy(button);
            }
            buttons.Clear();

            // Create new list
            if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby) {
                RoomInfo[] rooms = PhotonNetwork.GetRoomList();
                foreach (var room in rooms) {
                    string[] roomNameParts = room.Name.Split(':');
                    if (roomNameParts.Length == 4 && roomNameParts[0] == "PolymoneyGame") {
                        // Create a button
                        GameObject buttonObject = Instantiate(gameButtonTemplate);
                        buttons.Add(buttonObject);
                        
                        Button buttonComponent = buttonObject.GetComponent<Button>();
                        buttonComponent.onClick.RemoveAllListeners();
                        buttonComponent.onClick.AddListener(() => {
                            StartConnection(roomNameParts[2], Convert.ToInt32(roomNameParts[3]));
                        });

                        // Set button text
                        Text text = buttonComponent.GetComponentInChildren<Text>();
                        text.text = String.Format("{0} ({1}:{2})", roomNameParts[1], roomNameParts[2], roomNameParts[3]);

                        // Activate button
                        buttonObject.SetActive(true);
                    }
                }
            }
        }

        private void StartConnection(string networkAddress, int networkPort) {
            if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby) {
                PhotonNetwork.JoinOrCreateRoom(networkAddress, new RoomOptions { MaxPlayers = 4 }, null);
            }
        }
    }
}
