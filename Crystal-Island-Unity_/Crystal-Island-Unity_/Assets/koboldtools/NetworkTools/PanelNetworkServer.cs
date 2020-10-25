using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace KoboldTools
{
    public class PanelNetworkServer : ViewController<IPanel>
    {
        public override void onModelChanged()
        {
            //
        }

        public override void onModelRemoved()
        {
            //
        }


        // Update is called once per frame
        void Update()
        {
            if (model == null)
                return;

            if (NetworkServer.active && !model.isOpen)
            {
                model.onOpen();
            }else if(!NetworkServer.active && model.isOpen)
            {
                model.onClose();
            }
        }
    }
}
