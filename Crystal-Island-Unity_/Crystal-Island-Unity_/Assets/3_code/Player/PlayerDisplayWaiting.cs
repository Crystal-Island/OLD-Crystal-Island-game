using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    [RequireComponent(typeof(Panel))]
    public class PlayerDisplayWaiting : VCBehaviour<Player>
    {
        private Panel _panel;

        public void Awake()
        {
            this._panel = GetComponent<Panel>();
        }

        public override void onModelChanged()
        {
            this.model.OnWaitingForTurnCompletion.AddListener(this.onWaitingForTurnCompletion);
            this.model.OnTurnCompleted.AddListener(this.onTurnCompleted);
            this._panel.onClose();
        }

        public override void onModelRemoved()
        {
            this.model.OnWaitingForTurnCompletion.RemoveListener(this.onWaitingForTurnCompletion);
            this.model.OnTurnCompleted.RemoveListener(this.onTurnCompleted);
        }

        private void onWaitingForTurnCompletion()
        {
            this._panel.onOpen();
        }

        private void onTurnCompleted()
        {
            this._panel.onClose();
        }
    }
}
