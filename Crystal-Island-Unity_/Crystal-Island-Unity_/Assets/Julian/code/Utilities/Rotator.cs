using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] protected Vector3 axis = Vector3.up;
    [SerializeField] protected float speed = 2f;

    /// <summary>
	/// LateUpdate is called every frame, if the Behaviour is enabled.
	/// It is called after all Update functions have been called.
	/// </summary>
	private void LateUpdate()
    {
        transform.Rotate(axis * speed * Time.deltaTime);
    }
}
