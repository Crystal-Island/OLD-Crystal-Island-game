using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KoboldTools
{
    public class SkipController : MonoBehaviour
    {
        private bool _controlActive = false;
        public bool controlActive { get { return _controlActive; } }
        public UnityEvent skip = new UnityEvent();
        public UnityEvent activated = new UnityEvent();
        public UnityEvent deactivated = new UnityEvent();
        public void onSkip()
        {
            skip.Invoke();
        }
        public void onDeactivate()
        {
            if (_controlActive)
            {
                _controlActive = false;
                deactivated.Invoke();
            }
        }
        public void onActivate()
        {
            if (!_controlActive)
            {
                _controlActive = true;
                activated.Invoke();
            }
        }

    }

}
