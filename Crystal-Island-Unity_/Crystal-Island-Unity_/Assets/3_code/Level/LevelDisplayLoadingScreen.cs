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
        // TODO: NETWORKING-MIGRATION - temporary fallback added during Unity 2017->2019 migration.
        // Solo Editor play cannot complete the PlayerControlStartup readiness handshake because LAN
        // discovery / broadcast was disabled when we commented out legacy UnityEngine.Network calls.
        // Remove _closed flag and soloPlayTimeoutFallback() coroutine when networking is properly
        // migrated to Mirror/Photon and the readiness handshake works again.
        private bool _closed = false;
        private const float kLoadingScreenSoloTimeoutSeconds = 5f;

        public void Awake()
        {
            this.Panel = GetComponent<Panel>();
        }

        public new void Start()
        {
            base.Start();
            this.Panel.onOpen();
            StartCoroutine(soloPlayTimeoutFallback());
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
            if (_closed) return;
            _closed = true;
            this.Panel.onClose();
        }

        // TODO: NETWORKING-MIGRATION - remove when readiness handshake is restored
        private IEnumerator soloPlayTimeoutFallback()
        {
            yield return new WaitForSeconds(kLoadingScreenSoloTimeoutSeconds);
            if (_closed) yield break;
            Debug.LogWarning("[Polymoney] LevelDisplayLoadingScreen: readiness handshake timeout (" + kLoadingScreenSoloTimeoutSeconds + "s). Closing loading screen via migration-baseline fallback. Remove when networking is restored.");
            _closed = true;
            this.Panel.onClose();
        }
    }
}
