using System;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class JobUI : VCBehaviour<Job>
    {
        /// <summary>
        /// Holds the cost of the player's job.
        /// </summary>
        public Text jobTimeCost;
        /// <summary>
        /// Holds the job earnings.
        /// </summary>
        public Text earningsAmount;
        public string timeCostTextId = "jobTime";
        public string salaryTextId = "salary";

        /// <summary>
        /// Updates the UI text based on the model data.
        /// </summary>
        public override void onModelChanged()
        {
            // Update the UI data on the player's job from the model data.
            if (this.model != null)
            {
                this.jobTimeCost.text = Localisation.instance.getLocalisedFormat(this.timeCostTextId, this.model.TimeCost);
                this.earningsAmount.text = Localisation.instance.getLocalisedFormat(this.salaryTextId, this.model.Salary);
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
