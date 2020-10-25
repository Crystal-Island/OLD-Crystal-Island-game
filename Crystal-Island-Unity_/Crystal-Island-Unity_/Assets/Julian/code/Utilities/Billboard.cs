using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] protected Transform lookAt;
    [SerializeField] protected Vector3 up = Vector3.up;

    /// <summary>
	/// LateUpdate is called every frame, if the Behaviour is enabled.
	/// It is called after all Update functions have been called.
	/// </summary>
	private void LateUpdate()
    {
        transform.LookAt(lookAt, up);
    }
}
