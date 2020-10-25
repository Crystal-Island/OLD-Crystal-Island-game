using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// Debug Class for skipping game states for testing purposes
    /// </summary>
    public class ButtonSkipGameFlow : VCBehaviour<Button>, IFlowCondition
    {
        private bool _conditionMet = false;

        public override void onModelChanged()
        {
            //add listeners
            model.onClick.AddListener(clickedButton);
            GameFlow.instance.changeState.AddListener(changedState);

            //initialize
            RootLogger.Info(this, "Initialize flow skip");

            //add condition to all by default
            GameFlow.instance.addExitCondition(this);

            //remove condition from all states where handling is already implemented
            GameFlow.instance.removeExitCondition((int)PolymoneyGameFlow.FlowStates.PLAYER_INTRODUCTION, this);
            GameFlow.instance.removeExitCondition((int)PolymoneyGameFlow.FlowStates.PLAYER_TRADE, this);
            GameFlow.instance.removeExitCondition((int)PolymoneyGameFlow.FlowStates.INTRO_WORLD, this);
            GameFlow.instance.removeExitCondition((int)PolymoneyGameFlow.FlowStates.BEGIN_MONTH, this);
            GameFlow.instance.removeExitCondition((int)PolymoneyGameFlow.FlowStates.PLAYER_EVENTS, this);
            GameFlow.instance.removeExitCondition((int)PolymoneyGameFlow.FlowStates.END_MONTH, this);
        }

        public override void onModelRemoved()
        {
            //remove listener
            GameFlow.instance.changeState.RemoveListener(changedState);
        }

        private void changedState(int oldState, int newState)
        {
            RootLogger.Info(this, "Changed state in skip button");
            _conditionMet = false;
        }

        private void clickedButton()
        {
            _conditionMet = true;
        }

        public bool conditionMet
        {
            get
            {
                return _conditionMet;
            }
        }
    }

}
