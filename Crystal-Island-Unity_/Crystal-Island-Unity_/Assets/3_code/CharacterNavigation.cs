using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using KoboldTools;

namespace Polymoney
{
    /// <summary>
    /// Moves the character along a navmesh according to the character's steeringTarget, whenever the stateChanged event on the character is called.
    /// </summary>
    public class CharacterNavigation : VCBehaviour<Character>
    {
        /// <summary>
        /// Holds a reference to the navMeshAgent of the Character
        /// </summary>
        public NavMeshAgent navMeshAgent;
        private Animator animator;
        private Vector3 currentDestination = Vector3.zero;

        public override void onModelChanged()
        {
            //add listeners
            model.stateChanged.AddListener(characterStateChanged);
            animator = GetComponentInChildren<Animator>();
        }

        public override void onModelRemoved()
        {
            //remove listeners
            model.stateChanged.RemoveListener(characterStateChanged);
        }

        /// <summary>
        /// gets called when the character state changes
        /// </summary>
        private void characterStateChanged()
        {
            if (navMeshAgent != null)
            {
                if(model.steeringTarget != currentDestination)
                {
                    StopCoroutine(navigate());
                    //set the navmesh target
                    navMeshAgent.destination = model.steeringTarget;
                    currentDestination = model.steeringTarget;
                    StartCoroutine(navigate());
                }

            }
        }

        private IEnumerator navigate()
        {
            if (animator != null)
                animator.SetBool("walking", true);
            yield return null;
            while(navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                yield return null;
            }

            if (animator != null)
                animator.SetBool("walking", false);
        }
    }
}
