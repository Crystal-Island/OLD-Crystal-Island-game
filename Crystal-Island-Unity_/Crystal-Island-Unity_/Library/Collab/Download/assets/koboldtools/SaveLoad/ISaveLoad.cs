using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KoboldTools
{

    public interface ISaveLoad
    {
        void save(string savedKey, float savedValue);
        void save(string savedKey, int savedValue);
        void save(string savedKey, string savedValue);
        float loadFloat(string savedKey);
        int loadInt(string savedKey);
        string loadString(string savedKey);
        bool hasKey(string savedKey);
    }

}
