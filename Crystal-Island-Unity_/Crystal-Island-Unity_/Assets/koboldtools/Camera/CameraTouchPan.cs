using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace KoboldTools
{
    public class CameraTouchPan : VCBehaviour<Camera>, IInteractiveBlocker
    {
        private bool _panEnabled = true;
        public bool panEnabled
        {
            set { _panEnabled = value; }
        }
        private float _cumulatedMagnitude = 0f;
        private bool _blocked = false;
        public bool blocked
        {
            get
            {
                return _blocked;
            }
        }
        public bool yUp = false;
        public bool inverse = false;
        public float panSpeed = 1f;
        public Rect containRect = new Rect();

        public bool useBorders = true;
        public Transform topLeftBorder;
        public Transform topRightBorder;
        public Transform bottomLeftBorder;
        public Transform bottomRightBorder;

        public bool doubleTapCenterEnabled = true;
        public Transform centerTransform;
        public float doubleTapSeconds = 0.5f;

        public float borderMaxDistance = 1f;
        public float borderMinDistance = 0f;
        public float borderMoveThreshold = 0.5f;

        private Vector3 lastMouse;
        private float sinceLastTap = 0f;

        private void LateUpdate()
        {
            if (model == null)
                return;

            if (_panEnabled)
            {
                if (doubleTapCenterEnabled)
                {

                    //center
                    if (sinceLastTap > 0f)
                    {
                        sinceLastTap -= Time.deltaTime;
                        if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
                        {
                            //double tap
                            centerOnPosition(centerTransform.position);
                            sinceLastTap = 0f;
                        }
                    }
                    else
                    {
                        if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
                        {
                            sinceLastTap = doubleTapSeconds;
                        }
                    }

                }

                //movement
                Vector2 deltaWorld = Vector2.zero;

                //Touch  
                if (Input.touchCount == 1)
                {
                    //get pan
                    Touch touchZero = Input.GetTouch(0);
                    if (touchZero.phase == TouchPhase.Moved)
                    {
                        deltaWorld = new Vector2(
                                2f * model.orthographicSize * model.aspect * (touchZero.deltaPosition.x / Screen.width),
                                2f * model.orthographicSize * (touchZero.deltaPosition.y / Screen.height)
                            );
                     }
                    
                }

                //Mouse
                if (Input.GetMouseButtonDown(0))
                {
                    lastMouse = Input.mousePosition;
                }

                if (Input.GetMouseButton(0))
                {
                    //get pan
                    Vector3 deltaPosition = Input.mousePosition - lastMouse;

                    deltaWorld = new Vector2(
                            2f * model.orthographicSize * model.aspect * (deltaPosition.x / Screen.width),
                            2f * model.orthographicSize * (deltaPosition.y / Screen.height)
                        );
                    lastMouse = Input.mousePosition;

                }

                //Both
                if (deltaWorld != Vector2.zero)
                {
                    Debug.Log("pan");
                    deltaWorld *= panSpeed;
                    _cumulatedMagnitude += deltaWorld.magnitude;
                    //Gamestate.instance.addState((int)Gamestates.ZOOMPANNING);

                    if (inverse)
                        deltaWorld = -deltaWorld;

                    if (yUp)
                    {
                        centerOnPosition(new Vector2(
                            transform.position.x + deltaWorld.x,
                            transform.position.z + deltaWorld.y
                            ));
                    }
                    else
                    {
                        centerOnPosition(new Vector2(
                            transform.position.x + deltaWorld.x,
                            transform.position.y + deltaWorld.y
                            ));
                    }
                }
                else
                {
                    //Gamestate.instance.removeState((int)Gamestates.ZOOMPANNING);
                }

                if(!_blocked && _cumulatedMagnitude > 1f)
                {
                    _blocked = true;
                }

                if (_blocked && Input.touchCount == 0 && !Input.GetMouseButton(0))
                {
                    _cumulatedMagnitude = 0f;
                    _blocked = false;
                }
            }

            if (useBorders)
            {

                //update borders
                bottomRightBorder.position = new Vector3(
                    model.transform.position.x + model.orthographicSize * model.aspect + Mathf.Lerp(borderMinDistance, borderMaxDistance, (containRect.xMax - model.transform.position.x) / borderMoveThreshold),
                    model.transform.position.y - model.orthographicSize - Mathf.Lerp(borderMinDistance, borderMaxDistance, (model.transform.position.y - containRect.yMin) / borderMoveThreshold),
                    model.transform.position.z + 1f
                );

                bottomLeftBorder.position = new Vector3(
                    model.transform.position.x - model.orthographicSize * model.aspect - Mathf.Lerp(borderMinDistance, borderMaxDistance, (model.transform.position.x - containRect.xMin) / borderMoveThreshold),
                    model.transform.position.y - model.orthographicSize - Mathf.Lerp(borderMinDistance, borderMaxDistance, (model.transform.position.y - containRect.yMin) / borderMoveThreshold),
                    model.transform.position.z + 1f
                );

                topRightBorder.position = new Vector3(
                    model.transform.position.x + model.orthographicSize * model.aspect + Mathf.Lerp(borderMinDistance, borderMaxDistance, (containRect.xMax - model.transform.position.x) / borderMoveThreshold),
                    model.transform.position.y + model.orthographicSize + Mathf.Lerp(borderMinDistance, borderMaxDistance, (containRect.yMax - model.transform.position.y) / borderMoveThreshold),
                    model.transform.position.z + 1f
                );

                topLeftBorder.position = new Vector3(
                    model.transform.position.x - model.orthographicSize * model.aspect - Mathf.Lerp(borderMinDistance, borderMaxDistance, (model.transform.position.x - containRect.xMin) / borderMoveThreshold),
                    model.transform.position.y + model.orthographicSize + Mathf.Lerp(borderMinDistance, borderMaxDistance, (containRect.yMax - model.transform.position.y) / borderMoveThreshold),
                    model.transform.position.z + 1f
                );
            }
            
        }

        public void center()
        {
            centerOnPosition(centerTransform.position);
        }

        public void centerOnPosition(Vector2 newPosition)
        {
            if (containRect.size != Vector2.zero)
            {

                newPosition = new Vector2(
                    Mathf.Max(Mathf.Min(containRect.xMax, newPosition.x), containRect.xMin),
                    Mathf.Max(Mathf.Min(containRect.yMax, newPosition.y), containRect.yMin)
                    );


            }

            if (yUp)
            {
                model.transform.position = new Vector3(newPosition.x, model.transform.position.y, newPosition.y);
            }
            else
            {
                model.transform.position = new Vector3(newPosition.x, newPosition.y, model.transform.position.z);
            }
            

        }

        private void OnDrawGizmosSelected()
        {
            if (containRect.size == Vector2.zero)
                return;

            Gizmos.color = Color.green;
            if (yUp)
            {
                Gizmos.DrawWireCube(new Vector3(containRect.center.x,transform.position.y,containRect.center.y),new Vector3(containRect.size.x,0f,containRect.size.y));
            }
            else
            {
                Gizmos.DrawWireCube(containRect.center, containRect.size);
            }
            
        }

    }
}
