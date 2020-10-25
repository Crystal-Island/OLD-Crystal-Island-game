using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using UnityEngine.EventSystems;

namespace Polymoney
{
    public class LevelInput : VCBehaviour<Level>
    {
        public float cancelDelta = 0.2f;
        public ParticleSystem walkIndicator = null;
        private RaycastHit hit;
        private Vector3 pointerDown = Vector3.zero;
        private bool blockedByUI = false;

        public override void onModelChanged()
        {
            base.onModelChanged();
        }

        public override void onModelRemoved()
        {
            base.onModelRemoved();
        }

        private void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                blockedByUI = EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject();
                if (!blockedByUI/* && EventSystem.current.currentSelectedGameObject == null*/)
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        //if we hit an object tagged with ground, set the loaded characters steering target to that point
                        if (hit.collider.tag == "Ground")
                        {
                            pointerDown = hit.point;
                        }
                    }
                }
            }

            //check for input
            if (Input.GetMouseButtonUp(0))
            {
                blockedByUI = EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject();
                //check if the finger is over any ui element
                if (!blockedByUI/* && EventSystem.current.currentSelectedGameObject == null*/)
                {
                    //send raycast to check for object and position
                    if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        if (Vector3.Distance(hit.point, pointerDown) < cancelDelta)
                        {

                            //if we hit an object tagged with ground, set the loaded characters steering target to that point
                            if (hit.collider.tag == "Ground")
                            {
                                if (model.authoritativePlayer.LoadedCharacter != null)
                                {
                                    model.authoritativePlayer.ClientSetSteeringTarget(hit.point);
                                    walkIndicator.transform.position = hit.point + Vector3.up;
                                    walkIndicator.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
                                    walkIndicator.Play();
                                    AudioController.Play("tab_ground");
                                }
                            }
                        }
                    }
                }
                else
                {
                    //do nothing, touch is handled by unity ui
                }
            }
        }
    }
}
