#if DISSONANCE_ENABLED
using Dissonance.Networking;
using System;
#else
using UnityEngine;
#endif

namespace com.onlineobject.objectnet.voice {
    public class ObjectNetVoiceServer
#if DISSONANCE_ENABLED
        : BaseServer<ObjectNetVoiceServer, ObjectNetVoiceClient, ObjectNetVoicePeer> {
#else
        : MonoBehaviour {
#endif

#if DISSONANCE_ENABLED
        private readonly ObjectNetCommsNetwork network;

        public ObjectNetVoiceServer(ObjectNetCommsNetwork network) {
            this.network = network;
            NetworkManager.RegisterEvent(IntegrationEvent.VoiceToServer, this.OnVoicePacketReceived, true);
        }

        private void OnVoicePacketReceived(IDataStream reader) {
            IClient originClient = (reader as INetworkStream).GetClient();
            byte[] receivedBuffer = reader.Read<byte[]>();
            NetworkReceivedPacket(new ObjectNetVoicePeer((originClient as NetworkClient).GetConnectionId()), new ArraySegment<byte>(receivedBuffer, 0, receivedBuffer.Length));
        }

        protected override void ReadMessages() {
            // Messages are delivered in the callback set in `RegisterNamedMessageHandler`. Nothing to do here.
        }

        protected override void SendReliable(ObjectNetVoicePeer connection, ArraySegment<byte> packet) {
            this.Send(connection, packet, DeliveryMode.Reliable);
        }

        protected override void SendUnreliable(ObjectNetVoicePeer connection, ArraySegment<byte> packet) {
            this.Send(connection, packet, DeliveryMode.Unreliable);
        }

        private void Send(ObjectNetVoicePeer connection, ArraySegment<byte> packet, DeliveryMode mode) {
            if (network.PreprocessPacketToClient(packet, connection))
                return;
            foreach (IClient client in NetworkManager.Instance().GetConnection(ConnectionType.Server).GetSocket().GetConnectedClients()) {
                if ((client as NetworkClient).GetConnectionId().Equals(connection.clientId)) {
                    using (DataStream writer = new DataStream()) {
                        byte[] result = new byte[packet.Count];
                        packet.AsSpan().CopyTo(result);
                        writer.Write(result);
                        client.Send(IntegrationEvent.VoiceToClient, writer, mode);
                    }
                    break;
                }
            }
        }
#endif
    }
}