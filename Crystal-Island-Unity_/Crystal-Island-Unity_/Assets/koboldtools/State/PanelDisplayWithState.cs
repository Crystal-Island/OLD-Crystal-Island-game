using UnityEngine;
using System.Collections;
namespace KoboldTools
{
    public class PanelDisplayWithState : VCBehaviour<IPanel>
    {

        private IStateManager stateManager;
        public GameObject stateManagerObject;

        [EnumFlag(true)]
        public int show;
        [EnumFlag(true)]
        public int forceHide;

        public override void onModelChanged()
        {
            stateManager = stateManagerObject == null ? null : stateManagerObject.GetComponent<IStateManager>();
            if (stateManager != null)
            {
                stateManager.changeState.AddListener(changedState);
                // Initialize the panel to match the current state. changedState() only *closes* a
                // panel that is already logically open (model.isOpen); but at startup a panel can be
                // logically closed (isOpen == false) yet still visible (CanvasGroup alpha == 1 from
                // the prefab). The game flow used to race through every state at startup, which
                // incidentally opened-then-closed such panels and left them hidden. Now that the flow
                // parks at INTRO until the game actually begins, nothing tells these panels to hide,
                // so we must force each one to its correct visual state here.
                bool shouldShow = stateManager.hasState(show) && !stateManager.hasState(forceHide);
                if (shouldShow)
                {
                    model.onOpen();
                }
                else
                {
                    // onClose() alone is unreliable at startup: the panel may already be logically
                    // closed, and its display controller (PanelDisplayTransitions) closes by animating
                    // position, NOT by zeroing alpha — so the panel (and anything parented under its
                    // CanvasGroup, e.g. leftover game-over objects) stays fully visible over the intro.
                    // Zero the CanvasGroup directly so it is actually hidden.
                    model.onClose();
                    if (model.canvasGroup != null)
                    {
                        model.canvasGroup.alpha = 0f;
                        model.canvasGroup.interactable = false;
                        model.canvasGroup.blocksRaycasts = false;
                    }
                }
            }
        }

        public override void onModelRemoved()
        {
            stateManager.changeState.RemoveListener(changedState);
        }

        private void changedState(int oldState, int newState)
        {
            if (!model.isOpen && stateManager.hasState(show) && !stateManager.hasState(forceHide))
            {
                model.onOpen();
            }
            else if (model.isOpen && (!stateManager.hasState(show) || stateManager.hasState(forceHide)))
            {
                model.onClose();
            }
        }
    }

}
