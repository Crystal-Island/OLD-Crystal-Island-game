using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KoboldTools
{

    public interface ILocationMap
    {
        Vector2 longLatToUnit { get; set; }
        Vector2 longLatOrigin { get; set; }
        Vector3 getUnitPosition(Vector2 longLat);
        Vector2 getLongLatPosition(Vector3 unitPosition);
    }
}
