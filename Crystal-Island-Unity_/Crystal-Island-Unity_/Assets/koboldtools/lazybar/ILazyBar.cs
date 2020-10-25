using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
namespace KoboldTools
{
    public interface ILazyBar
    {

        float sliderValue { get; }
        RectTransform fillRect { get; }
        RectTransform lazyRect { get; }

        UnityEvent<bool> changeValue { get; }
        void onChangeValue(float value, bool lazy);

    }
}
