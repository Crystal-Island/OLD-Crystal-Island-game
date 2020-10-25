using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class LevelControlTurns : NetworkBehaviour, IFlowCondition
    {
        public PolymoneyGameFlow.FlowStates[] states;
        [EnumFlag(convertToInt = true)]
        public PolymoneyGameFlow.FlowStates countTurnsForGameoverPlayers;
        private bool allTurnsFinished;

        public IEnumerator Start()
        {
            // Abort on all devices but the server.
            if (!this.isServer)
            {
                yield break;
            }

            // Wait for the level instance to appear.
            while (Level.instance == null)
            {
                yield return null;
            }

            // Wait for the game state machine singleton.
            while (GameFlow.instance == null)
            {
                yield return null;
            }


            Level.instance.onPlayerAdded.AddListener(this.playerAdded);

            //add all players that already exist
            foreach (Player player in Level.instance.allPlayers)
            {
                this.playerAdded(player);
            }

            GameFlow.instance.changeState.AddListener(this.flowStateChanged);
            foreach (PolymoneyGameFlow.FlowStates state in this.states)
            {
                RootLogger.Info(this, "Server: Adding myself as exit condition to the state '{0}'", state);
                GameFlow.instance.addExitCondition((int)state, this);
            }
        }
        public bool conditionMet
        {
            get
            {
                return this.allTurnsFinished;
            }
        }
        public void playerAdded(Player player)
        {
            player.OnWaitingForTurnCompletion.AddListener(this.playerCompletedTurn);
            player.OnGameOver.AddListener(this.playerCompletedTurn);
        }

        public void playerCompletedTurn()
        {
            RootLogger.Info(this, "Server: A player has completed their turn");

            if (isServer && NetworkServer.connections.Count(c => c != null) > Level.instance.allPlayers.Count)
            {
                RootLogger.Info(this, "Server: Not all players are connected. Turn completion check for all players is suspended");
                return;
            }

            if (Level.instance.allPlayers.All(e => e.TurnFinished || (e.GameOver && ((int)countTurnsForGameoverPlayers & GameFlow.instance.currentState) == 0)))
            {
                RootLogger.Info(this, "Server: All players have completed their turn");
                foreach (Player player in Level.instance.allPlayers)
                {
                    player.ServerResetEndTurn();
                }
                this.allTurnsFinished = true;
            }
        }
        public void flowStateChanged(int oldState, int newState)
        {
            RootLogger.Info(this, "Server: Resetting the end-turn flow condition");
            this.allTurnsFinished = false;
        }
    }
}
