#if UNITY_TRANSPORT_ENABLED
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
#endif

namespace com.onlineobject.objectnet {
    public sealed class UnityServer
#if UNITY_TRANSPORT_ENABLED
        : UnityTransport, ITransportServer {
#else
        {
#endif

#if UNITY_TRANSPORT_ENABLED

        List<UnityClient> connections;

        // Data to return to pooling
        private List<byte[]> dequeuedBuffers = new List<byte[]>();

        public override bool Connect() {
            return ( this.GetDriver().Listen() > -1 );
        }

        public override void Disconnect() {
            foreach (UnityClient client in this.connections) {
                client.Disconnect();
            }
            this.GetDriver().Dispose();
            this.connections.Clear();
        }

        public override bool IsConnected() {
            return ((this.GetDriver().IsCreated) && 
                    (this.GetDriver().Listening));
        }

        public override void Initialize() {
            base.Initialize();
            this.connections    = new List<UnityClient>();
#if UNITY_2022_1_OR_NEWER
            var endpoint        = TransportServerBind.UseAnyAddress.Equals(TransportDefinitions.ServerBindingType) ?
                                          ((TransportAddressFamily.Ipv4.Equals(TransportDefinitions.AddressFamily) ? NetworkEndpoint.AnyIpv4 : 
                                                                                                                     NetworkEndpoint.AnyIpv6)).WithPort(this.GetPort()) :
                                          NetworkEndpoint.Parse(this.GetIp(), this.GetPort(), (TransportAddressFamily.Ipv4.Equals(TransportDefinitions.AddressFamily) ? NetworkFamily.Ipv4 : NetworkFamily.Ipv6));
#else
            var endpoint        = TransportServerBind.UseAnyAddress.Equals(TransportDefinitions.ServerBindingType) ?
                                          ((TransportAddressFamily.Ipv4.Equals(TransportDefinitions.AddressFamily) ? NetworkEndPoint.AnyIpv4 : 
                                                                                                                     NetworkEndPoint.AnyIpv6)).WithPort(this.GetPort()) :
                                          NetworkEndPoint.Parse(this.GetIp(), this.GetPort(), (TransportAddressFamily.Ipv4.Equals(TransportDefinitions.AddressFamily) ? NetworkFamily.Ipv4 : NetworkFamily.Ipv6));
#endif
            if (this.GetDriver().Bind(endpoint) != 0) {
                return;
            }
        }

        public override void Destroy() {
            if (this.GetDriver().IsCreated) {
                this.GetDriver().Dispose();
                this.connections.Clear();
            }
        }

        // Update is called once per frame
        public override List<byte[]> Process() {
            this.GetDriver().ScheduleUpdate().Complete();
            try {
                this.dequeuedBuffers.Clear();
                // Clean up connections.
                this.CleanupConnections();
                // Accept new connectons
                this.AcceptConnections();
                // Send pending messages ( Reliable )
                while (this.DequeueMessage(DeliveryMode.Reliable, out var dataBytes)) {
                    if (!this.InternalSendMessage(dataBytes, DeliveryMode.Reliable)) break;
                    this.dequeuedBuffers.Add(dataBytes);
                }
                // Send pending messages ( Unreliable )
                while (this.DequeueMessage(DeliveryMode.Unreliable, out var dataBytes)) {
                    if (!this.InternalSendMessage(dataBytes, DeliveryMode.Unreliable)) break;
                    this.dequeuedBuffers.Add(dataBytes);
                }
            } finally {
                // Now process all client's
                foreach (UnityClient connection in this.connections) {
                    this.dequeuedBuffers.AddRange(connection.Process());
                }
            }
            return this.dequeuedBuffers;
        }

        public override void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            base.Send(data, mode);
        }

        private bool InternalSendMessage(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            bool result = false;
            foreach (UnityClient connection in connections) {
                result |= this.InternalSendMessageToClient(connection, data, mode);
            }
            return result;
        }

        private void CleanupConnections() {
            // Clean up connections.
            for (int i = this.connections.Count - 1; i >= 0; i--) {
                if (this.connections[i] != null) {
                    if (!this.connections[i].IsConnected()) {
                        UnityClient disconnectedClient = this.connections[i];
                        this.connections.RemoveAtSwapBack(i);
                        this.OnClientDisconnected(disconnectedClient);                        
                        i--;
                    }
                } else {
                    this.connections.RemoveAtSwapBack(i);
                }
            }
        }

        private void AcceptConnections() {
            // Accept client connections
            NetworkConnection clientConnection;
            while ((clientConnection = this.GetDriver().Accept()) != default(NetworkConnection)) {
                UnityClient newClient = new UnityClient(this.GetDriver(), clientConnection, this.GetRealiablePipeline());
                newClient.SetIp(this.GetDriver().GetRemoteEndpoint(clientConnection).Address.ToString().Split(":").GetValue(0).ToString());
                newClient.SetPort(this.GetDriver().GetRemoteEndpoint(clientConnection).Port);
                newClient.Configure(base.onClientConnected, base.onClientDisconnected, base.onMessageReceived);
                this.connections.Add(newClient);
                this.OnClientConnected(newClient);
            }
        }
#endif
    }
}