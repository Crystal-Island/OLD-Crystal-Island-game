using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using KoboldTools.Logging;

namespace Polymoney
{
    public class PlayerControlStartup : NetworkBehaviour
    {
        [SyncVar]
        private int clientCount = 0;
        [SyncVar]
        private int playerCount = 0;
        [SyncVar]
        private bool allPlayersReady = false;

        private static int globallyReadyClients = 0;
        private static bool localPlayersReady = false;
        private Level level = null;

        public IEnumerator Start()
        {
            PlayerControlStartup.localPlayersReady = false;
            PlayerControlStartup.globallyReadyClients = 0;

            while (Level.instance == null)
            {
                yield return null;
            }

            this.level = Level.instance;
        }

        public void Update()
        {
            if (!this.allPlayersReady)
            {
                // On the server, make sure to get the correct values for the
                // number of clients and players.
                if (this.isServer)
                {
                    if (this.clientCount != NetworkServer.connections.Count(c => c != null))
                    {
                        this.clientCount = NetworkServer.connections.Count(c => c != null);
                    }
                    if (this.playerCount != PolymoneyNetworkManager.singleton.numPlayers)
                    {
                        this.playerCount = PolymoneyNetworkManager.singleton.numPlayers;
                    }
                }

                // On the local player, check whether
                if (this.isLocalPlayer && this.level != null)
                {
                    // All players have loaded successfully,
                    if (!PlayerControlStartup.localPlayersReady && this.playerCount > 0 && this.level.allPlayers.Count == this.playerCount)
                    {
                        // And if the level data has been loaded locally,
                        if (this.level.levelData != null)
                        {
                            RootLogger.Info(this, "All player objects are initialized on this client and the level data is available");

                            // Then the server that you are ready.
                            PlayerControlStartup.localPlayersReady = true;
                            this.CmdAllPlayersReady();
                        }
                    }
                }
            }
        }

        [Command]
        private void CmdAllPlayersReady()
        {
            RootLogger.Info(this, "Cmd: A client has completed loading the player objects.");
            PlayerControlStartup.globallyReadyClients += 1;

            if (this.clientCount > 0 && PlayerControlStartup.globallyReadyClients == this.clientCount)
            {
                this.allPlayersReady = true;
                this.RpcAllPlayersReady();
            }
        }

        [ClientRpc]
        private void RpcAllPlayersReady()
        {
            RootLogger.Info(this, "Rpc: All clients have completed loading their player objects.");
            this.level.onAllPlayersReady.Invoke();

            if (RootLogger.LogLevel == KoboldTools.Logging.Level.DEBUG)
            {
                NetworkIdentity[] tmp = (NetworkIdentity[]) GameObject.FindObjectsOfType(typeof(NetworkIdentity));
                IOrderedEnumerable<NetworkIdentity> netIds = tmp.OrderBy(e => e.netId.Value);
                foreach (NetworkIdentity id in netIds)
                {
                    RootLogger.Debug(this, "Found NetworkIdentity {0} - {1}", id.gameObject.name, id.netId);
                }
            }
        }
    }
}
