using UnityEngine;
using System.Collections;

namespace KoboldTools
{

    public class Singleton<Instance> : MonoBehaviour where Instance : Singleton<Instance>
    {
        public static Instance instance;
        public bool isPersistant;

        public virtual void Awake()
        {
            if (isPersistant)
            {
                if (!instance)
                {
                    //Debug.Log("set instance of " + name);
                    instance = this as Instance;
                }
                else
                {
                    //Debug.Log("destroyed " + name);
                    DestroyObject(gameObject);
                }
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                //Debug.Log("set instance of " + name);
                instance = this as Instance;
            }
        }
    }
}