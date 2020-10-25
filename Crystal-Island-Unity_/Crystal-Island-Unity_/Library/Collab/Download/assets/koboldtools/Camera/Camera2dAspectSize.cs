using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2dAspectSize : MonoBehaviour {

    public Camera camera2d;
    public Vector2 baseScreenSize;

    void Awake ()
    {

        float aspect = (float)Screen.width / (float)Screen.height;
        float shouldHeight = baseScreenSize.x / aspect;

        camera2d.orthographicSize = shouldHeight / 2f / 100f;//(shouldHeight / baseScreenSize.y) * camera2d.orthographicSize;
	}
	
}
