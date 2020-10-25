using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.UI;

namespace Polymoney
{
    public class BenefitDisplay : VCBehaviour<Benefit>
    {
        public Text textField;

        public override void onModelChanged()
        {
            bool first = true;
            textField.text = "You get: ";

            if (model.FairyDust > 0f)
            {
                textField.text += string.Format("{1}{0} Fairydust.", model.FairyDust, first ? "" : "\n"); //loca currencyFairydust
                first = false;
            }

            foreach (CurrencyValue cv in model.Income)
            {
                if (cv.value > 0f)
                {
                    textField.text += string.Format("{2}{0}.- {1}.", cv.value, cv.currency, first ? "" : "\n"); //loca currencyMoney
                    first = false;
                }
            }

            if(model.RemoveIncident.Count > 0)
            {
                textField.text += string.Format("{1}Help with {0}", model.RemoveIncident.ToVerboseString(), first ? "" : "\n"); //loca removeIncident
                first = false;
            }

        }

        public override void onModelRemoved()
        {
            base.onModelRemoved();
        }
    }
}
