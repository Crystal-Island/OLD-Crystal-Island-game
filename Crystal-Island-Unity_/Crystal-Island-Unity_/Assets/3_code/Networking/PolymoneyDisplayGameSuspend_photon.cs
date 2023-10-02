using Photon.Pun;
using UnityEngine;

namespace Polymoney
{
    public class PolymoneyDisplayGameSuspend : MonoBehaviourPunCallbacks
    {
        private Panel Panel;

        private void Awake()
        {
            if (this.Panel == null)
            {
                this.Panel = this.GetComponent<Panel>();
            }
        }

        private void Start()
        {
            // Join or create a Photon room when the game starts
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            // Once connected to the Photon Master Server, join or create a room
            PhotonNetwork.JoinOrCreateRoom("YourRoomName", new RoomOptions(), TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            // This method is called when you successfully join a room
            Debug.Log("Joined a Photon room");
            Panel.onClose();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            // Handle disconnection from the Photon server
            Debug.Log("Disconnected from Photon: " + cause.ToString());
            Panel.onOpen();
        }
    }
}
