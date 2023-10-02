using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Polymoney
{
    public class PolymoneyNetworkDiscoveryUI : MonoBehaviourPunCallbacks
    {
        public float displayFrequency = 1f;
        public Button gameButtonTemplate;

        private GameObject[] gameButtons;
        private float sinceLastDisplay = 1f;

        private void Start()
        {
            // Initialize Photon
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            // Create a Photon room or join an existing one
            PhotonNetwork.JoinOrCreateRoom("YourRoomName", new RoomOptions(), TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            // Initialize the game buttons here, if needed
        }

        private void Update()
        {
            // Limit redraws
            if (sinceLastDisplay < displayFrequency)
            {
                sinceLastDisplay += Time.deltaTime;
                return;
            }
            sinceLastDisplay = 0f;

            // Release current list (if using object pooling, clear old buttons)
            if (gameButtons != null)
            {
                foreach (var button in gameButtons)
                {
                    Destroy(button);
                }
            }

            // Create new list
            if (PhotonNetwork.InRoom)
            {
                // Use Photon's room list
                Room[] rooms = PhotonNetwork.GetRoomList();
                gameButtons = new GameObject[rooms.Length];

                for (int i = 0; i < rooms.Length; i++)
                {
                    Room room = rooms[i];

                    // Check room properties, adjust criteria as needed
                    if (room.CustomProperties.ContainsKey("GameType") &&
                        room.CustomProperties.ContainsKey("IPAddress") &&
                        room.CustomProperties.ContainsKey("Port"))
                    {
                        string gameType = (string)room.CustomProperties["GameType"];
                        string ipAddress = (string)room.CustomProperties["IPAddress"];
                        int port = (int)room.CustomProperties["Port"];

                        // Create a button for the room
                        GameObject buttonObject = Instantiate(gameButtonTemplate.gameObject, transform);
                        Button button = buttonObject.GetComponent<Button>();
                        Text buttonText = buttonObject.GetComponentInChildren<Text>();

                        buttonText.text = $"{gameType} ({ipAddress}:{port})";

                        button.onClick.AddListener(() => {
                            StartConnection(ipAddress, port);
                        });

                        gameButtons[i] = buttonObject;
                    }
                }
            }
        }

        private void StartConnection(string networkAddress, int networkPort)
        {
            // Perform connection logic here using Photon PUN
            // Example: PhotonNetwork.ConnectToMaster(networkAddress, networkPort, PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
        }
    }
}
