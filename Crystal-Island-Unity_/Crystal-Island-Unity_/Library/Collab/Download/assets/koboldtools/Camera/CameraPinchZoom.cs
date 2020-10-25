using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KoboldTools
{
    public class CameraPinchZoom : ViewController<Camera>, IInteractiveBlocker
    {
        public float orthoZoomSpeed = 0.5f;
        public float maxOrthoSize = 9.6f;
        public float minOrthoSize = 4.8f;
        public float baseOrthoSize = 9.6f;

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
            baseOrthoSize = model.orthographicSize;
            base.onModelChanged();
        }

        private void Update()
        {
            if (model == null)
                return;

            if(Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                deltaMagnitude = Vector2.Distance(touchZero.position - touchZero.deltaPosition, touchOne.position - touchOne.deltaPosition) - Vector2.Distance(touchZero.position, touchOne.position);

                _blocked = true;

            }else
            {
                deltaMagnitude = Input.mouseScrollDelta.y * 10f;
            }
            if (model.orthographic && Mathf.Abs(deltaMagnitude) > 0f)
            {
                model.orthographicSize += deltaMagnitude * orthoZoomSpeed;
                model.orthographicSize = Mathf.Max(Mathf.Min(model.orthographicSize, maxOrthoSize), minOrthoSize);
                untilStop = pinchDelay;
                if (!delaying)
                {
                    startPinch.Invoke();
                    delaying = true;
                    StartCoroutine(delayPinchStop());
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

        private void OnDisable()
        {
            model.orthographicSize = baseOrthoSize;
        }
    }
}
