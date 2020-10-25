using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
namespace KoboldTools
{
    public class LazyBar : MonoBehaviour, ILazyBar
    {
        private BoolEvent _changeValue = new BoolEvent();
        public UnityEvent<bool> changeValue
        {
            get
            {
                return _changeValue;
            }
        }
        [SerializeField]
        private RectTransform _fillRect;
        public RectTransform fillRect
        {
            get
            {
                return _fillRect;
            }
        }
        [SerializeField]
        private RectTransform _lazyRect;
        public RectTransform lazyRect
        {
            get
            {
                return _lazyRect;
            }
        }
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float _sliderValue = 0f;
        public float sliderValue
        {
            get
            {
                return _sliderValue;
            }
        }

        public void onChangeValue(float value, bool lazy)
        {
            _sliderValue = Mathf.Clamp01(value);
            changeValue.Invoke(lazy);
        }
    }
}
