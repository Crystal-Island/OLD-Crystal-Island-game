using System;
using System.Net;
using UnityEngine;
using Mirror;
using Mirror.Discovery;

namespace Polymoney {
    // Discovery request broadcast by browsing clients. No payload needed.
    public struct PolymoneyDiscoveryRequest : NetworkMessage { }

    // Discovery reply from a host. Carries the game name so the pre-lobby list can show
    // "<gameName> (<ip>:<port>)" — replacing the old UNet broadcast string
    // "PolymoneyGame:<gameName>:<ip>:<port>".
    public struct PolymoneyDiscoveryResponse : NetworkMessage {
        // Filled in by the client from the received packet; not serialized (property, not field).
        public IPEndPoint EndPoint { get; set; }
        public Uri uri;
        public long serverId;
        public string gameName;
    }

    /// <summary>
    /// Mirror replacement for the UNet NetworkDiscovery used by <see cref="PolymoneyNetworkManager"/>.
    /// A host advertises itself (AdvertiseServer) with its chosen <see cref="gameName"/>; browsing
    /// clients call StartDiscovery and receive a <see cref="PolymoneyDiscoveryResponse"/> per host via
    /// the OnServerFound event (consumed by <see cref="PolymoneyNetworkDiscoveryUI"/>).
    /// </summary>
    public class PolymoneyNetworkDiscovery : NetworkDiscoveryBase<PolymoneyDiscoveryRequest, PolymoneyDiscoveryResponse> {
        // Set by the host (PolymoneyNetworkManager) before AdvertiseServer() so clients see the name.
        public string gameName = "Polymoney Game";

        #region Server

        protected override PolymoneyDiscoveryResponse ProcessRequest(PolymoneyDiscoveryRequest request, IPEndPoint endpoint) {
            try {
                return new PolymoneyDiscoveryResponse {
                    serverId = ServerId,
                    uri = transport.ServerUri(),
                    gameName = this.gameName,
                };
            } catch (NotImplementedException) {
                Debug.LogError($"Transport {transport} does not support network discovery");
                throw;
            }
        }

        #endregion

        #region Client

        protected override PolymoneyDiscoveryRequest GetRequest() => new PolymoneyDiscoveryRequest();

        protected override void ProcessResponse(PolymoneyDiscoveryResponse response, IPEndPoint endpoint) {
            // We received the packet from endpoint, so resolve the real host from there rather than
            // trusting the advertised URI (which may not resolve across NICs).
            response.EndPoint = endpoint;
            UriBuilder realUri = new UriBuilder(response.uri) {
                Host = response.EndPoint.Address.ToString()
            };
            response.uri = realUri.Uri;

            OnServerFound.Invoke(response);
        }

        #endregion
    }
}
