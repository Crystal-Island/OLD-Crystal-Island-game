using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PolymoneyNetworkManager : MonoBehaviourPunCallbacks
{
    public InputField gameNameInput;
    public Button createGameButton;
    public Button joinGameButton;

    private void Start()
    {
        // Connect to the Photon server.
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // Called when connected to the Photon server.
        Debug.Log("Connected to Photon Master Server");
    }

    public void CreateGame()
    {
        // Create a new room with the given name.
        string roomName = gameNameInput.text;
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinGame()
    {
        // Join an existing room.
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        // Called when successfully joined a room.
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        // Load your game scene or perform other setup here.
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // Called when failed to join a room, usually because no rooms are available.
        Debug.Log("Failed to join a room: " + message);
        // You can create a new room or display an error message.
    }
}
