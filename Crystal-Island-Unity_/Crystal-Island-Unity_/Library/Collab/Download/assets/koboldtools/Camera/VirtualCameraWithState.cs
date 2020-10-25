using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace KoboldTools
{
    public class VirtualCameraWithState : VCBehaviour<CinemachineVirtualCamera>
    {
        public GameObject stateManagerObject;

        [EnumFlag(true)]
        public int state;
        public int setToPriority = 1000;
        private int defaultPriority = 50;
        private IStateManager stateManager = null;

        public override void onModelChanged()
        {
            stateManager = stateManagerObject.GetComponent<IStateManager>();
            stateManager.changeState.AddListener(changedState);
            defaultPriority = model.Priority;
        }

        public override void onModelRemoved()
        {
            stateManager.changeState.RemoveListener(changedState);
        }

        private void changedState(int oldState, int newState)
        {
            if (stateManager.hasState(state))
            {
                model.Priority = setToPriority;
            }
            else
            {
                model.Priority = defaultPriority;
            }
        }
    }
}
