using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class LevelStartGame : VCBehaviour<Level>
    {
        public Camera Camera;
        private PlayableDirector Director;

        public void Awake()
        {
            this.Director = this.Camera.GetComponent<PlayableDirector>();
        }

        public override void onModelChanged()
        {
            this.model.onAllPlayersReady.AddListener(this.onAllPlayersReady);
        }

        public override void onModelRemoved()
        {
            this.model.onAllPlayersReady.RemoveListener(this.onAllPlayersReady);
        }

        private void onAllPlayersReady()
        {
            this.Director.Play();
        }
    }
}
