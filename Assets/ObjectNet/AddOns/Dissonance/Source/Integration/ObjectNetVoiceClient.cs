#if DISSONANCE_ENABLED
using Dissonance.Datastructures;
using Dissonance.Networking;
using System;
#else
using UnityEngine;
#endif


namespace com.onlineobject.objectnet.voice {
    public class ObjectNetVoiceClient
#if DISSONANCE_ENABLED
        : BaseClient<ObjectNetVoiceServer, ObjectNetVoiceClient, ObjectNetVoicePeer> {
#else
        : MonoBehaviour {
#endif

#if DISSONANCE_ENABLED
        private readonly ObjectNetCommsNetwork  network;
        private readonly ConcurrentPool<byte[]> dataBufferPooling = new ConcurrentPool<byte[]>(3, () => new byte[4096]);

        public ObjectNetVoiceClient(ObjectNetCommsNetwork network) : base(network) {
            this.network = network;
            NetworkManager.RegisterEvent(IntegrationEvent.VoiceToClient, this.OnVoicePacketReceived, true);
        }

        private void OnVoicePacketReceived(IDataStream reader) {
            byte[] receivedBuffer = reader.Read<byte[]>();
            NetworkReceivedPacket(new ArraySegment<byte>(receivedBuffer, 0, receivedBuffer.Length));
        }

        public override void Connect() {
            Connected();
        }

        public override void Disconnect() {
            base.Disconnect();
        }

        protected override void ReadMessages() {
            // Messages are received in an event handler, so we don't need to do any work to read events
        }

        protected override void SendReliable(ArraySegment<byte> packet) {
            this.Send(packet, DeliveryMode.Reliable);
        }

        protected override void SendUnreliable(ArraySegment<byte> packet) {
            this.Send(packet, DeliveryMode.Unreliable);
        }
        private void Send(ArraySegment<byte> packet, DeliveryMode mode) {
            if (network.PreprocessPacketToServer(packet))
                return;
            using (DataStream writer = new DataStream()) {
                byte[] result = new byte[packet.Count];
                packet.AsSpan().CopyTo(result);
                writer.Write(result);
                NetworkManager.Instance().Send(IntegrationEvent.VoiceToServer, writer, mode);
            }
        }
#endif
    }
}