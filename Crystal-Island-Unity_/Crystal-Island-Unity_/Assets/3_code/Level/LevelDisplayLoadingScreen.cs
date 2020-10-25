using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    [RequireComponent(typeof(Panel))]
    public class LevelDisplayLoadingScreen : VCBehaviour<Level>
    {
        private Panel Panel;

        public void Awake()
        {
            this.Panel = GetComponent<Panel>();
        }

        public new void Start()
        {
            base.Start();
            this.Panel.onOpen();
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
            this.Panel.onClose();
        }
    }
}
