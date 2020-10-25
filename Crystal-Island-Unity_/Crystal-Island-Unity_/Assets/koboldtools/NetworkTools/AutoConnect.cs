using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace KoboldTools
{
    public class AutoConnect : ViewController<NetworkManager>
    {
        public override void onModelChanged()
        {
            if (System.Environment.GetCommandLineArgs().Contains("autostartClient"))
            {
                model.StartClient();
            }
            else if (System.Environment.GetCommandLineArgs().Contains("autostartHost"))
            {
                model.StartHost();
            }
            else if (System.Environment.GetCommandLineArgs().Contains("autostartServer"))
            {
                model.StartServer();
            }
        }

        public override void onModelRemoved()
        {
            throw new NotImplementedException();
        }


        void Update()
        {
            if (model == null)
                return;

            if (Input.GetKeyDown(KeyCode.C))
            {
                model.StartClient();
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                model.StartHost();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                model.StartServer();
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                model.StopClient();
                model.StopServer();
            }


        }

    }
}
