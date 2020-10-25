using System.Collections;
using System.Collections.Generic;
using KoboldTools;
using KoboldTools.Logging;
using UnityEngine;

namespace Polymoney {
	public class PolymoneyDisplayGameSuspend : MonoBehaviour {
		private Panel Panel;

		private void Awake() {
			if (this.Panel == null) {
				this.Panel = this.GetComponent<Panel>();
			}
		}

		private IEnumerator Start() {
			while (PolymoneyNetworkManager.singleton == null) {
				yield return null;
			}
			PolymoneyNetworkManager manager = (PolymoneyNetworkManager) PolymoneyNetworkManager.singleton;
			manager.OnBlockScreen.AddListener(this.OnBlockScreen);
			this.OnBlockScreen(manager.LocalClientStatus.ScreenBlocked);
		}

		private void OnBlockScreen(bool status) {
			if (status) {
				RootLogger.Info(this, "Game paused");
				this.Panel.onOpen();
			} else {
				RootLogger.Info(this, "Game unpaused");
				this.Panel.onClose();
			}
		}
	}
}