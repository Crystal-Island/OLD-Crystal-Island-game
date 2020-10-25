using System.Collections.Generic;
using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    /// <summary>
    /// Based on the player's status (mayor or not), enable or disable
    /// the appropriate UI elements.
    /// </summary>
    public class PlayerUIFilter : VCBehaviour<Player>
    {
        public List<GameObject> cityPlayerObjects = new List<GameObject>();
        public List<GameObject> regularPlayerObjects = new List<GameObject>();

        /// <summary>
        /// Called, when the model (a <see cref="Player"/>) has changed or
        /// when a model was created.
        /// </summary>
        public override void onModelChanged()
        {
            this.model.PlayerStateChanged.AddListener(this.playerStateChanged);
        }
        /// <summary>
        /// Called, when the model (a <see cref="Player"/>) will be destroyed.
        /// </summary>
        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.playerStateChanged);
        }
        /// <summary>
        /// Called, when the player state has changed. If the <see cref="Player.Mayor"/>
        /// flag is set to <c>true</c>, enable all city player UI elements and disable
        /// all regular player UI elements, and the other way around.
        /// </summary>
        private void playerStateChanged()
        {
            foreach (GameObject cityPlayerObject in this.cityPlayerObjects)
            {
                if (cityPlayerObject != null)
                {
                    cityPlayerObject.SetActive(this.model.Mayor);
                }
            }
            foreach (GameObject regularPlayerObject in this.regularPlayerObjects)
            {
                if (regularPlayerObject != null)
                {
                    regularPlayerObject.SetActive(!this.model.Mayor);
                }
            }
        }
    }
}
