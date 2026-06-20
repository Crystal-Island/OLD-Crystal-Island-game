using System;
using System.Collections.Generic;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace Polymoney {
    // Migrated from UNet NetworkDiscovery (broadcastsReceived / NetworkBroadcastResult polling) to
    // Mirror's event-driven PolymoneyNetworkDiscovery. Hosts found on the LAN arrive via the
    // OnServerFound event, each carrying its game name + reachable URI.
    public class PolymoneyNetworkDiscoveryUI : VCBehaviour<PolymoneyNetworkDiscovery> {
        public float displayFrequency = 1f;
        public Button gameButtonTemplate;

        private KoboldTools.Pool<Button> buttonPool;
        private float sinceLastDisplay = 1f;

        // Discovered hosts keyed by serverId so a host seen on multiple NICs is listed once.
        private readonly Dictionary<long, PolymoneyDiscoveryResponse> discovered = new Dictionary<long, PolymoneyDiscoveryResponse>();

        public override void onModelChanged() {
            buttonPool = new KoboldTools.Pool<Button>(gameButtonTemplate);

            if (model != null) {
                model.OnServerFound.RemoveListener(OnDiscoveredServer);
                model.OnServerFound.AddListener(OnDiscoveredServer);
            }
        }

        private void OnDestroy() {
            if (model != null) {
                model.OnServerFound.RemoveListener(OnDiscoveredServer);
            }
        }

        private void OnDiscoveredServer(PolymoneyDiscoveryResponse info) {
            discovered[info.serverId] = info;
        }

        private void Update() {
            //limit redraws
            if (sinceLastDisplay < displayFrequency) {
                sinceLastDisplay += Time.deltaTime;
                return;
            }
            sinceLastDisplay = 0f;

            if (buttonPool == null) {
                return;
            }

            //release current list
            buttonPool.releaseAll();

            //create new list
            foreach (PolymoneyDiscoveryResponse res in discovered.Values) {
                Uri uri = res.uri;
                if (uri == null) {
                    continue;
                }

                //create button from pool
                Button buttonObject = buttonPool.pop();
                buttonObject.onClick.RemoveAllListeners();
                buttonObject.onClick.AddListener(() => {
                    this.StartConnection(uri);
                });

                //add button values
                Text text = buttonObject.GetComponentInChildren<Text>();
                text.text = String.Format("{0} ({1}:{2})", res.gameName, uri.Host, uri.Port);

                //display button
                buttonObject.gameObject.SetActive(true);
            }
        }

        private void StartConnection(Uri uri) {
            if (NetworkManager.singleton != null) {
                if (!NetworkClient.active) {
                    RootLogger.Info(this, "Trying to connect to {0}", uri);
                    NetworkManager.singleton.StartClient(uri);
                } else {
                    RootLogger.Exception(this, "Could not establish a connection to the server, a client is already present.");
                }
            } else {
                RootLogger.Exception(this, "Could not find a network manager instance!");
            }
        }
    }
}
