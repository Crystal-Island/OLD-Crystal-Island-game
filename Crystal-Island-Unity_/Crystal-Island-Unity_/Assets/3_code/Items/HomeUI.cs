using System;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class HomeUI : VCBehaviour<Home>
    {
        /// <summary>
        /// Holds the cost of the player's home.
        /// </summary>
        public Text homeCost;
        public string homeCostTextId = "homeCost";

        /// <summary>
        /// Updates the UI text based on the model data.
        /// </summary>
        public override void onModelChanged()
        {
            // Update the UI data on the player's home from the model data.
            if (this.model != null)
            {
                this.homeCost.text = Localisation.instance.getLocalisedFormat(this.homeCostTextId, this.model.Rent);
            }
        }
        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void onModelRemoved()
        {
        }
    }
}
