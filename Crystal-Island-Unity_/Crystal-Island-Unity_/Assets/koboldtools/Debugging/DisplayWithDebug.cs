using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace KoboldTools.DebugTool
{
    public class DisplayWithDebug : MonoBehaviour
    {
        void Start()
        {
            gameObject.SetActive(Application.isEditor || Debug.isDebugBuild);
        }
    }
}
