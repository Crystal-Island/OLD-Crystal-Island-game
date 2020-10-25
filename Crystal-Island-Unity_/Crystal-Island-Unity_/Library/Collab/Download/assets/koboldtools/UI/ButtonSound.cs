using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KoboldTools
{
    public class ButtonSound : VCBehaviour<Button>
    {
        public string clickId = "button_click";

        public override void onModelChanged()
        {
            model.onClick.AddListener(onClick);
        }

        public override void onModelRemoved()
        {
            model.onClick.RemoveListener(onClick);
        }

        private void onClick()
        {
            AudioController.Play(clickId);
        }
    }
}
