using System;
using UnityEngine;
using UnityEngine.UI;

namespace KoboldTools {
    public class DisplayVersion : MonoBehaviour {
        private Text debugTextField;

        void Awake() {
            this.debugTextField = GetComponent<Text>();
        }

        void Start() {
            this.debugTextField.text = String.Format("{0} v{1} (Unity {2})", Application.productName, Application.version, Application.unityVersion);
        }
    }
}