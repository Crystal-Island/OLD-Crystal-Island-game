using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace KoboldTools
{
    public class CameraPinchZoomVirtual : VCBehaviour<CinemachineVirtualCamera>, IInteractiveBlocker
    {

        public float orthoZoomSpeed = 0.5f;
        public float maxHeight = 2f;
        public float minHeight = 1f;

        public float pinchDelay = 0.2f;
        public UnityEvent startPinch = new UnityEvent();
        public UnityEvent stopPinch = new UnityEvent();

        bool delaying = false;
        float untilStop = 0.2f;
        float deltaMagnitude = 0f;
        bool _blocked = false;

        public bool blocked
        {
            get
            {
                return _blocked;
            }
        }

        public override void onModelChanged()
        {
            base.onModelChanged();
        }

        private void Update()
        {
            if (model == null)
                return;

            if (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.IsPointerOverGameObject(0))
                return;

            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                deltaMagnitude = Vector2.Distance(touchZero.position - touchZero.deltaPosition, touchOne.position - touchOne.deltaPosition) - Vector2.Distance(touchZero.position, touchOne.position);

                _blocked = true;

            }else
            {
                deltaMagnitude = Input.mouseScrollDelta.y * 10f;
            }

            if (Mathf.Abs(deltaMagnitude) > 0f)
            {
                Vector3 pos = model.transform.position;
                pos += deltaMagnitude * orthoZoomSpeed * -model.transform.forward;
                if (pos.y > minHeight && pos.y < maxHeight)
                {
                    model.transform.position = pos;
                    untilStop = pinchDelay;
                    if (!delaying)
                    {
                        startPinch.Invoke();
                        delaying = true;
                        StartCoroutine(delayPinchStop());
                    }
                }
            }

            if(_blocked && Input.touchCount == 0)
            {
                _blocked = false;
            }

        }

        private IEnumerator delayPinchStop()
        {
            while(untilStop > 0f)
            {
                untilStop -= Time.deltaTime;
                yield return null;
            }
            delaying = false;
            stopPinch.Invoke();
        }

    }
}
