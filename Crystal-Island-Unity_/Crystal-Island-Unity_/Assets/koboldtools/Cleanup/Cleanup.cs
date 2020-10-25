using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace KoboldTools
{
    public class Cleanup : MonoBehaviour
    {

#if UNITY_EDITOR
        public void Awake()
        {
            //delete all except this gameobject
            List<GameObject> objToClean = new List<GameObject>();
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (obj.transform.parent == null && obj != gameObject)
                {
                    objToClean.Add(obj);
                }
            }
            //do cleanup
            for (int i = 0; i < objToClean.Count; i++)
            {
                DestroyImmediate(objToClean[i]);
            }
            objToClean.Clear();
        }
#endif
    }
}
