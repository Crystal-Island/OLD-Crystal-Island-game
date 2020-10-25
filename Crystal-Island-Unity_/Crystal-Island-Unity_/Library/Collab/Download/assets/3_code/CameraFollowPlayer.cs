using System.Collections;
using UnityEngine;
using Cinemachine;

namespace Polymoney
{
    public class CameraFollowPlayer : MonoBehaviour
    {
        public bool follow = true;
        public bool lookAt = true;
        private CinemachineVirtualCamera _camera;
        private Level _level;
        private Player _player;

        public void Awake()
        {
            this._camera = GetComponent<CinemachineVirtualCamera>();
        }
        public IEnumerator Start()
        {
            while (Level.instance == null)
            {
                yield return null;
            }

            this._level = Level.instance;
            this._level.onAuthoritativePlayerChanged.AddListener(this.onAuthoritativePlayerChanged);
        }
        private void onAuthoritativePlayerChanged()
        {
            if (this._player != null)
            {
                this._player.CharacterChanged.RemoveListener(this.characterChanged);
            }
            this._player = this._level.authoritativePlayer;
            this._player.CharacterChanged.AddListener(this.characterChanged);
        }
        private void characterChanged()
        {
            if (this._player.LoadedCharacter != null)
            {
                // Tell the virtual camera to follow the authoritative player.
                if (this.follow)
                {
                    this._camera.m_Follow = this._player.LoadedCharacter.transform;
                }

                // Tell the virtual camera to look at the authoritative player.
                if (this.lookAt)
                {
                    this._camera.m_LookAt = this._player.LoadedCharacter.transform;
                }
            }
        }
    }
}
