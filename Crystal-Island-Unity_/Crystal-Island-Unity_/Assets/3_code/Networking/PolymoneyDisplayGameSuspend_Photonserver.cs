using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Polymoney {
    public class PolymoneyDisplayGameSuspend : MonoBehaviourPunCallbacks {
        private Panel Panel;

        private void Awake() {
            if (this.Panel == null) {
                this.Panel = this.GetComponent<Panel>();
            }
        }

        private void Start() {
            // Ensure we are connected to the Photon network
            if (!PhotonNetwork.IsConnected) {
                Debug.LogError("Not connected to Photon Network. Make sure to set up Photon correctly.");
                return;
            }

            // Register to the custom event triggered by PolymoneyNetworkManager
            PolymoneyNetworkManager manager = FindObjectOfType<PolymoneyNetworkManager>();
            if (manager != null) {
                manager.OnBlockScreen.AddListener(this.OnBlockScreen);
            } else {
                Debug.LogError("PolymoneyNetworkManager not found in the scene.");
            }
        }

        // This method will be called when the custom event is raised
        private void OnBlockScreen(bool status) {
            if (status) {
                Debug.Log("Game paused");
                this.Panel.onOpen();
            } else {
                Debug.Log("Game unpaused");
                this.Panel.onClose();
            }
        }
    }
}
