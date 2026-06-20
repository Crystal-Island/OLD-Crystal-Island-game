using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Mirror;
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

            // Do NOT start advancing the flow yet. The phase controllers (LevelControlTurns /
            // LevelControlTime) register their exit conditions asynchronously — their Start coroutines
            // wait for Level/GameFlow to exist. If the server let the flow advance immediately it would
            // run past every phase before those conditions are registered and go straight to Game Over.
            // Start the flow only once the game truly begins (Level.onAllPlayersReady), by which point
            // every controller has registered its exit conditions.
            this.flow.running = false;

            //add listener for changes on server
            if (isServer)
            {
                this.flow.changeState.AddListener(flowStateChanged);
                StartCoroutine(StartFlowWhenReady());
            }
        }

        private IEnumerator StartFlowWhenReady()
        {
            while (Level.instance == null)
            {
                yield return null;
            }
            Level.instance.onAllPlayersReady.AddListener(this.OnGameStarted);
        }

        private void OnGameStarted()
        {
            //only run flow on the server; clients receive forced states via RpcStateChange
            this.flow.running = this.isServer;

            // Re-broadcast the current gamestate to every listener now that the game has truly begun.
            // FlowBehaviour.Awake() emits the very first state (INTRO_WORLD) before the scene's
            // controllers — the virtual cameras, the intro/montage sequence, the gamestate-driven
            // panels — have registered their changeState listeners, so they all miss it and never
            // initialize (camera never frames the world, intro never plays, panels stay visible).
            // StateManager.onChangeState won't re-fire an unchanged state, so we invoke the event
            // directly. oldState = -1 matches no state, so this reads as a clean "enter current state".
            this.flow.changeState.Invoke(-1, this.flow.currentState);

            //initial sync to clients
            this.flowStateChanged(flow.currentState, flow.currentState);
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
