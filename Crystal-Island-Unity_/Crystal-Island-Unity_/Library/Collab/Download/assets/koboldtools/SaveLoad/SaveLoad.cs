using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KoboldTools
{

    public class SaveLoad : MonoBehaviour, ISaveLoad
    {
        #region Singleton

        //Here is a private reference only this class can access
        [SerializeField]
        private static ISaveLoad _instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ISaveLoad instance
        {
            get
            {
                //If _instance hasn't been set yet, we grab it from the scene!
                //This will only happen the first time this reference is used.
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<SaveLoad>();
                }
                return _instance;
            }
        }

        #endregion

        public float loadFloat(string savedKey)
        {
            return checkKey(savedKey) ? PlayerPrefs.GetFloat(savedKey) : 0f;
        }
        public int loadInt(string savedKey)
        {
            return checkKey(savedKey) ? PlayerPrefs.GetInt(savedKey) : -1;
        }
        public string loadString(string savedKey)
        {
            return checkKey(savedKey) ? PlayerPrefs.GetString(savedKey) : "";
        }

        public void save(string savedKey, float savedValue)
        {
            PlayerPrefs.SetFloat(savedKey, savedValue);
        }
        public void save(string savedKey, int savedValue)
        {
            PlayerPrefs.SetInt(savedKey, savedValue);
        }
        public void save(string savedKey, string savedValue)
        {
            PlayerPrefs.SetString(savedKey, savedValue);
        }

        public bool hasKey(string savedKey)
        {
            return checkKey(savedKey, true);
        }

        private bool checkKey(string savedKey, bool noError = false)
        {
            if (!PlayerPrefs.HasKey(savedKey))
            {
                if (!noError)
                    Debug.LogError("[SAVELOAD] key " + savedKey + " does not exist!");
                return false;
            }
            else
            {
                return true;
            }
        }


    }
}