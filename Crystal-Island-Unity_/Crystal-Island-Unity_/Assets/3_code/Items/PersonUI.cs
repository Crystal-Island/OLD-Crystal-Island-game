using System;
using UnityEngine.UI;
using KoboldTools;

namespace Polymoney
{
    public class PersonUI : VCBehaviour<Person>
    {
        /// <summary>
        /// Holds the education text for a particular person.
        /// </summary>
        public Text personEducation;
        /// <summary>
        /// Holds the hobby text for a particular person.
        /// </summary>
        public Text personHobbies;

        /// <summary>
        /// Updates the UI text based on the model data.
        /// </summary>
        public override void onModelChanged()
        {
            if (this.model != null)
            {
                this.personEducation.text = this.model.LocalisedEducation;
                this.personHobbies.text = this.model.LocalisedHobbies;
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
