using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// networkbehaviour that syncs gameflow state over the network
    /// </summary>
    public class GameFlowSync : NetworkBehaviour
    {
        /// <summary>
        /// Holds a reference to the <see cref="GameFlow"/>.
        /// </summary>
        private IFlow flow = null;

        /// <summary>
        /// This start operation waits for the <see cref="GameFlow"/> instance to surface.
        /// Subsequently, if running on a server it sets the <see cref="GameFlow.running"/> to true, otherwise false.
        /// Then it registers a listener for the <see cref="GameFlow.changeState"/> event if running on a server.
        /// </summary>
        private void Start()
        {
            //grab flow from children
            this.flow = GameFlow.instance;


            //only run flow on server
            this.flow.running = this.isServer;
            //add listener for changes on server
            if (isServer)
            {
                this.flow.changeState.AddListener(flowStateChanged);
                //initial sync
                this.flowStateChanged(flow.currentState, flow.currentState);
            }

        }

        private void flowStateChanged(int oldState, int newState)
        {
            //sync state to clients
            if (this.isServer)
            {
                this.RpcStateChange(newState);
            }
        }

        /// <summary>
        /// Called when an RPC call from the server requests a state change.
        /// </summary>
        /// <param name="newState">state identifier</param>
        [ClientRpc]
        private void RpcStateChange(int newState)
        {
            if (!this.isServer)
            {
                //client flow is not running, we have to force change
                if (this.flow != null)
                {
                    this.flow.forceState(newState);
                }
            }
            else
            {
                //this is a host, and runs as client and server simultaneously. No forcing needed
            }

        }
    }
}
