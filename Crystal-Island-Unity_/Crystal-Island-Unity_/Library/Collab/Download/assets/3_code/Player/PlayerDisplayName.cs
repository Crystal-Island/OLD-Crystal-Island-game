using System;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class PlayerDisplayName : VCBehaviour<Player>
    {
        public Text nameText;

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
            if (this.model.Person != null)
            {
                this.nameText.text = this.model.Person.LocalisedTitle;
            }
        }
    }
}
