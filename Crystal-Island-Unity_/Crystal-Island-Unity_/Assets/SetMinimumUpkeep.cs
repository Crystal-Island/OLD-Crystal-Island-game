using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymoney;
using UnityEngine.UI;
public class SetMinimumUpkeep : MonoBehaviour {

    Slider slider;
	// Use this for initialization
	void Start () {
        slider = this.GetComponent<Slider>();

        slider.value = PersistentOptionSettings.instance.MinimumUpkeep;
        
	}

    public void OnValueChange()
    {
        PersistentOptionSettings.instance.MinimumUpkeep = (int)slider.value;
    }
}
