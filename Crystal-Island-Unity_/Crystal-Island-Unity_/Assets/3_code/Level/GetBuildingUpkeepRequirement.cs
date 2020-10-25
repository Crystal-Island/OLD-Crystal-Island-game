using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using Polymoney;

public class GetBuildingUpkeepRequirement : MonoBehaviour {


    LocaliseElementUIText localizationComponent;
    // Use this for initialization
    void Start()
    {

        localizationComponent = this.gameObject.GetComponent<LocaliseElementUIText>();
        Level.instance.onLevelStateChanged.AddListener(listenForStateChange);
    }



    void listenForStateChange()
    {
        localizationComponent.SetValue(Level.instance.MinimumUpkeep);
        localizationComponent.updateTextWithInts();

    }
}
