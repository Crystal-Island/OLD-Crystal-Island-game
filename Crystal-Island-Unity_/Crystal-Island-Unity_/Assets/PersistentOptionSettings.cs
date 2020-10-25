using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymoney;
using UnityEngine.SceneManagement;

public class PersistentOptionSettings : MonoBehaviour {

    public static PersistentOptionSettings instance;
    public int MinimumUpkeep = 1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Persistent Option settings can only have one instance");

        }
    }
}
