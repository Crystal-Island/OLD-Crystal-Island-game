using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;

namespace Polymoney
{
    public class ComplIntroSetMarketplace : MonoBehaviour
    {
        public Marketplace introMarket = null;
        private Panel _panel = null;

        private void Awake()
        {
            this._panel = GetComponent<Panel>();
            this._panel.closeComplete.AddListener(this.panelCloseComplete);
        }

        public void panelCloseComplete()
        {
            Level.instance.authoritativePlayer.WatchedMarket = this.introMarket;
        }
    }
}
