using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.Events;

namespace Polymoney
{

    public class PlayerOpenMarket : VCBehaviour<Player>, IInteractive
    {
        [EnumFlag]
        public PolymoneyGameFlow.FlowStates allowMarketInState;
        private UnityEvent _interacted = new UnityEvent();

        public override void onModelChanged()
        {
            if (GameFlow.instance != null) {
                GameFlow.instance.changeState.AddListener(gameStateChanged);
            }
        }

        public override void onModelRemoved()
        {
            if (GameFlow.instance != null) {
                GameFlow.instance.changeState.RemoveListener(gameStateChanged);
            }
        }

        public UnityEvent interacted
        {
            get
            {
                return _interacted;
            }
        }

        public void onPointerDown()
        {
            //do nothing
        }

        public void onPointerUp()
        {
            if (this.enabled)
            {
                if (GameFlow.instance.hasState((int)allowMarketInState))
                {
                    interacted.Invoke();
                    //check for market
                    if (model != null && model.OwnMarketplace != null)
                    {
                        Level.instance.authoritativePlayer.WatchedMarket = model.OwnMarketplace;
                    }
                }
            }
        }

        private void gameStateChanged(int oldState, int newState)
        {
            //force close on gamestate change when market is not allowed in state
            if(model != null && model == Level.instance.authoritativePlayer.WatchedMarket && model.WatchedMarket != null)
            {
                if ((newState & (int)allowMarketInState) == 0)
                {
                    Debug.LogFormat("Forceclose. Newstate: {0} int: {2}, Checkstate: {1} int: {3}", (PolymoneyGameFlow.FlowStates)newState, allowMarketInState, newState, (int)allowMarketInState);
                    Level.instance.authoritativePlayer.WatchedMarket = null;
                }
            }
        }
    }
}
