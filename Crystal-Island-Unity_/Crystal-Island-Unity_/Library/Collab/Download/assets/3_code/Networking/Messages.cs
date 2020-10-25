using System;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Polymoney {
    public class PolymoneyMsgType {
        public static short NetworkStatus = MsgType.Highest + 1;
        public static short ClientAvailable = MsgType.Highest + 2;
    }

    [Serializable]
    public enum NetworkRole : ushort {
        SERVER = 1,
        CLIENT = 2,
    }

    [Serializable]
    public enum NetworkStatusEvent : ushort {
        QUIT = 1,
        PAUSE = 2,
        FOCUS = 3,
        BLOCK_SCREEN = 4,
    }

    public class ClientAvailableMessage : EmptyMessage {}

    /// <summary>
    /// This message relays client and server status messages.
    /// </summary>
    public class NetworkStatusMessage : MessageBase {
        public NetworkRole Source;
        public NetworkStatusEvent Event;
        public bool Status;

        public NetworkStatusMessage() {
            this.Source = NetworkRole.CLIENT;
            this.Event = NetworkStatusEvent.PAUSE;
            this.Status = false;
        }

        public NetworkStatusMessage(NetworkRole source, NetworkStatusEvent signal, bool status) {
            this.Source = source;
            this.Event = signal;
            this.Status = status;
        }

        public string ToHumanReadableString() {
            if (this.Source == NetworkRole.SERVER) {
                if (this.Event == NetworkStatusEvent.PAUSE) {
                    if (this.Status) {
                        return "The server was suspended by its operating system";
                    } else {
                        return "The server has come back from suspension";
                    }
                } else if (this.Event == NetworkStatusEvent.FOCUS) {
                    if (this.Status) {
                        return "The server is in focus";
                    } else {
                        return "The server has lost focus";
                    }
                } else if (this.Event == NetworkStatusEvent.BLOCK_SCREEN) {
                    if (this.Status) {
                        return "The server requests clients to block user input (i.e. to pause the game)";
                    } else {
                        return "The server allows clients to unblock user input (i.e. to unpause the game)";
                    }
                }
            } else if (this.Source == NetworkRole.CLIENT) {
                if (this.Event == NetworkStatusEvent.PAUSE) {
                    if (this.Status) {
                        return "The client was suspended by its operating system";
                    } else {
                        return "The client has come back from suspension";
                    }
                } else if (this.Event == NetworkStatusEvent.FOCUS) {
                    if (this.Status) {
                        return "The client is in focus";
                    } else {
                        return "The client has lost focus";
                    }
                }
            }
            
            return this.ToString();
        }

        public override string ToString() {
            return String.Format("NSM(source={0}, event={1}, status={2})", this.Source, this.Event, this.Status);
        }
    }
}