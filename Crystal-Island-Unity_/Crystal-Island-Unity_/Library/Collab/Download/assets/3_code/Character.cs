using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polymoney
{
    /// <summary>
    /// Representation of a character.
    /// </summary>
    public interface ICharacter
    {
        float Luminance { get; set; }
        Vector3 steeringTarget { get; set; }
        UnityEvent stateChanged { get; }
        UnityEvent OnLuminanceChanged { get; }
        UnityEvent OnLuminanceHalf { get; }
        UnityEvent OnLuminanceFull { get; }
    }

    public class Character : MonoBehaviour, ICharacter
    {
        private float _luminance = 0.0f;
        /// <summary>
        /// The target point for the navigation. Character moves towards this point when set.
        /// </summary>
        private Vector3 _steeringTarget = Vector3.zero;
        /// <summary>
        /// State change event of the character. Is called when state of the character changes.
        /// </summary>
        private UnityEvent _stateChanged = new UnityEvent();
        private UnityEvent _onLuminanceChanged = new UnityEvent();
        private UnityEvent _onLuminanceHalf = new UnityEvent();
        private UnityEvent _onLuminanceFull = new UnityEvent();

        public float Luminance
        {
            get
            {
                return this._luminance;
            }

            set
            {
                if (Math.Abs(this._luminance - value) > float.Epsilon)
                {
                    if (value > 1.0f)
                    {
                        this._luminance = 1.0f;
                    }
                    else if (value < 0.0f)
                    {
                        this._luminance = 0.0f;
                    }
                    else
                    {
                        this._luminance = value;
                    }

                    // Fire the appropriate events.
                    if (Math.Abs(this._luminance - 0.5f) <= float.Epsilon)
                    {
                        this._onLuminanceHalf.Invoke();
                    }
                    else if (Math.Abs(this._luminance - 1.0f) <= float.Epsilon)
                    {
                        this._onLuminanceFull.Invoke();
                    }
                    this._onLuminanceChanged.Invoke();
                }
            }
        }

        public Vector3 steeringTarget
        {
            get
            {
                return this._steeringTarget;
            }

            set
            {
                if (this._steeringTarget != value)
                {
                    //set new target and invoke state changed event
                    this._steeringTarget = value;
                    this.stateChanged.Invoke();
                }
            }
        }

        public UnityEvent stateChanged
        {
            get
            {
                return this._stateChanged;
            }
        }

        public UnityEvent OnLuminanceChanged
        {
            get
            {
                return this._onLuminanceChanged;
            }
        }

        public UnityEvent OnLuminanceHalf
        {
            get
            {
                return this._onLuminanceHalf;
            }
        }

        public UnityEvent OnLuminanceFull
        {
            get
            {
                return this._onLuminanceFull;
            }
        }

    }
}
