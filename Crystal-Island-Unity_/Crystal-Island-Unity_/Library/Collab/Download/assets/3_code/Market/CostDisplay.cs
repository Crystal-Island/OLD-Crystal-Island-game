using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.UI;

namespace Polymoney
{
    public class CostDisplay : VCBehaviour<Cost>
    {
        public Text textField;

        public override void onModelChanged()
        {
            textField.text = "Cost: ";

            if (model.Time > 0f)
                textField.text += Localisation.instance.getLocalisedFormat("timeCost", model.Time); //loca timeCost

            foreach(CurrencyValue cv in model.Expenses)
            {
                if (cv.value > 0f)
                    textField.text += string.Format("{0}"+Localisation.instance.getLocalisedText("currencyMoney")+" {1}", cv.value, cv.currency); //loca currencyMoney
            }
        }

        public override void onModelRemoved()
        {
            base.onModelRemoved();
        }
    }
}
