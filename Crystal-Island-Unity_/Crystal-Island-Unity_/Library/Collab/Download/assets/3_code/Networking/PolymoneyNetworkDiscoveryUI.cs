using System;
using System.Collections;
using System.Collections.Generic;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Polymoney {
    public class PolymoneyNetworkDiscoveryUI : VCBehaviour<NetworkDiscovery> {
        public float displayFrequency = 1f;
        public Button gameButtonTemplate;

        private Pool<Button> buttonPool;
        private float sinceLastDisplay = 1f;

        public override void onModelChanged() {
            buttonPool = new Pool<Button>(gameButtonTemplate);
        }

        private void Update() {
            //limit redraws
            if (sinceLastDisplay < displayFrequency) {
                sinceLastDisplay += Time.deltaTime;
                return;
            }
            sinceLastDisplay = 0f;

            //release current list
            buttonPool.releaseAll();

            //create new list
            if (model.broadcastsReceived != null) {
                foreach (string addr in model.broadcastsReceived.Keys) {
                    //get data
                    NetworkBroadcastResult res = model.broadcastsReceived[addr];
                    string dataString = BytesToString(res.broadcastData);
                    string[] items = dataString.Split(':');

                    //add button callback
                    if (items.Length == 4 && items[0] == "PolymoneyGame") {
                        //create button from pool
                        Button buttonObject = buttonPool.pop();
                        buttonObject.onClick.RemoveAllListeners();
                        buttonObject.onClick.AddListener(() => {
                            this.StartConnection(items[2], Convert.ToInt32(items[3]));
                        });

                        //add button values
                        Text text = buttonObject.GetComponentInChildren<Text>();
                        text.text = String.Format("{0} ({1}:{2})", items[1], items[2], items[3]);

                        //display button
                        buttonObject.gameObject.SetActive(true);
                    }

                }
            }
        }

        private void StartConnection(string networkAddress, int networkPort) {
            if (NetworkManager.singleton != null) {
                if (NetworkManager.singleton.client == null) {
                    NetworkManager.singleton.networkAddress = networkAddress;
                    NetworkManager.singleton.networkPort = networkPort;
                    RootLogger.Info(this, "Trying to connect to {0}:{1}", networkAddress, networkPort);
                    NetworkManager.singleton.StartClient();
                } else {
                    RootLogger.Exception(this, "Could not establish a connection to the server, a client is already present.");
                }
            } else {
                RootLogger.Exception(this, "Could not find a network manager instance!");
            }
        }

        private string BytesToString(byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
}