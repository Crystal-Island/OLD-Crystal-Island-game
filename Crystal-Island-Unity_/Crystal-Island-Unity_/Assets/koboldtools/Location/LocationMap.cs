using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace KoboldTools
{
    public class LocationMap : MonoBehaviour, ILocationMap
    {
        [SerializeField]
        private Vector2 _longLatOrigin = Vector2.zero;
        public Vector2 longLatOrigin
        {
            get
            {
                return _longLatOrigin;
            }

            set
            {
                _longLatOrigin = value;
            }
        }
        [SerializeField]
        private Vector2 _longLatToUnit = Vector2.one;
        public Vector2 longLatToUnit
        {
            get
            {
                return _longLatToUnit;
            }

            set
            {
                _longLatToUnit = value;
            }
        }

        public Vector3 getUnitPosition(Vector2 longLat)
        {
            /*
            x = (total width of image in px) *(180 + latitude) / 360
            y = (total height of image in px) *(90 - longitude) / 180
            */

            Vector3 longLatOriginPosition = convertLongLat(longLatOrigin);
           

            return transform.position + convertLongLat(longLat) - longLatOriginPosition;

           /* return new Vector3(
                transform.position.x + (longLat.x - longLatOrigin.x) * longLatToUnit.x,
                transform.position.y + (longLat.y - longLatOrigin.y) * longLatToUnit.y,
                0f
                );*/
        }

        private Vector3 convertLongLat(Vector2 longLat)
        {
            return new Vector3(
                
                -longLatToUnit.y * (90f - longLat.y) / 180f,
                longLatToUnit.x * (180f + longLat.x) / 360f,
                0f
                );
        }

        private Vector2 convertUnitPosition(Vector3 unitPosition)
        {
            return new Vector2(
                (-unitPosition.x * 180f / -longLatToUnit.y) + 90f,
                (unitPosition.y * 360f / longLatToUnit.x) - 180f
                );
        }

        public Vector2 getLongLatPosition(Vector3 unitPosition)
        {
            return (convertUnitPosition(unitPosition+convertLongLat(longLatOrigin)));
        }
    }
}