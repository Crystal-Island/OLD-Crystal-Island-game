using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KoboldTools
{
    public class NamedUIText : ViewController<INamed>
    {
        public Text textField;
        public override void onModelChanged()
        {
            textField.text = model.name;
        }

    }
}
