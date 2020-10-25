using System;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class PlayerDisplayFairydust : VCBehaviour<Player>
    {
        public Text fairydustText;

        public override void onModelChanged()
        {
            this.model.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
            this.onPlayerStateChanged();
        }

        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
        }

        private void onPlayerStateChanged()
        {
            this.fairydustText.text = String.Format("{0:D}", this.model.Pocket.FairyDust);
        }
    }
}
