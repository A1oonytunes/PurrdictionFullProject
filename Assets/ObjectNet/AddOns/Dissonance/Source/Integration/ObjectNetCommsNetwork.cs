#if DISSONANCE_ENABLED
using Dissonance;
using Dissonance.Extensions;
using Dissonance.Networking;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
#else
using UnityEngine;
#endif

namespace com.onlineobject.objectnet.voice {
    public class ObjectNetCommsNetwork
#if DISSONANCE_ENABLED
        : BaseCommsNetwork<ObjectNetVoiceServer, ObjectNetVoiceClient, ObjectNetVoicePeer, Unit, Unit> {
#else
        : MonoBehaviour {
#endif

#if DISSONANCE_ENABLED
        private bool initialized = false;

        private readonly Dissonance.Datastructures.ConcurrentPool<byte[]> loopbackBuffers = new Dissonance.Datastructures.ConcurrentPool<byte[]>(8, () => new byte[1024]);
        private readonly List<ArraySegment<byte>> loopbackQueue = new List<ArraySegment<byte>>();

        protected override ObjectNetVoiceClient CreateClient([CanBeNull] Unit connectionParameters) {
            return new ObjectNetVoiceClient(this);
        }

        protected override ObjectNetVoiceServer CreateServer([CanBeNull] Unit connectionParameters) {
            return new ObjectNetVoiceServer(this);
        }

        // Check every frame
        protected override void Update() {
            base.Update();
            // Check if Dissonance is ready
            if (NetworkManager.Instance() != null) {
                if (IsInitialized) {
                    // Check if the HLAPI is ready
                    var networkActive = NetworkManager.Instance().HasConnection() ? NetworkManager.Instance().GetConnection().IsConnected() : false;
                    if (networkActive) {
                        if (this.initialized == false) {
                            // Check what mode the HLAPI is in
                            var server = (NetworkManager.Instance().IsServerConnection() == true);
                            var client = ((NetworkManager.Instance().InRelayMode() == false) &&
                                          (NetworkManager.Instance().InAuthoritativeMode() == false));

                            // Check what mode Dissonance is in and if
                            // they're different then call the correct method
                            if (Mode.IsServerEnabled() != server || Mode.IsClientEnabled() != client) {
                            // if (server || client) {
                                if (server && client)                   // HLAPI is server and client, so run as a non-dedicated host (passing in the correct parameters)
                                    RunAsHost(Unit.None, Unit.None);
                                else if (server)                        // HLAPI is just a server, so run as a dedicated host
                                    RunAsDedicatedServer(Unit.None);
                                else if (client)                        // HLAPI is just a client, so run as a client
                                    RunAsClient(Unit.None);
                                this.initialized = true;
                            }
                        }
                    } else if ((Mode != NetworkMode.None) || ((networkActive == false) && (this.initialized == true))) {
                        Stop(); // Network is not active, make sure Dissonance is not active
                        this.initialized = false;
                        this.loopbackQueue.Clear();
                    }

                    // Send looped back packets
                    for (var i = 0; i < loopbackQueue.Count; i++) {
                        Client?.NetworkReceivedPacket(loopbackQueue[i]);

                        // Recycle the packet into the pool of byte buffers
                        // ReSharper disable once AssignNullToNotNullAttribute (Justification: ArraySegment array is not null)
                        loopbackBuffers.Put(loopbackQueue[i].Array);
                    }
                    loopbackQueue.Clear();
                }
            }            
        }

        internal bool PreprocessPacketToClient(ArraySegment<byte> packet, ObjectNetVoicePeer destination) {
            // No client means this can't be loopback
            if (Client == null)
                return false;

            // HLAPI way to check if this is loopback.
            if (NetworkManager.Instance().GetConnection().GetSocket().GetConnectionID() != destination.clientId)
                return false;

            // This is loopback!

            // check that we have a valid local client,
            // in cases of startup or in-progress shutdowns
            if (Client != null) {
                // Don't immediately deliver the packet, add it to a queue and
                // deliver it next frame. This prevents the local client from
                // executing "within" the local server which can cause
                // confusing stack traces.
                loopbackQueue.Add(packet.CopyToSegment(loopbackBuffers.Get()));
            }

            return true;
        }

        internal bool PreprocessPacketToServer(ArraySegment<byte> packet) {
            // I have no idea if the Mirror handles loopback. Whether it does or does not isn't important though - it's more
            // efficient to handle the loopback special case directly instead of passing through the entire network system!

            // This should never even be called if this peer is not a client!
            if (Client == null)
                throw Log.CreatePossibleBugException("client packet processing running, but this peer is not a client", "dd75dce4-e85c-4bb3-96ec-3a3636cc4fbe");

            // Is this loopback?
            if (Server == null)
                return false;

            // This is loopback!

            // Since this is loopback destination == source (by definition)
            Server.NetworkReceivedPacket(new ObjectNetVoicePeer(NetworkManager.Instance().GetConnection().GetSocket().GetConnectionID()), packet);

            return true;
        }
#endif
    }
}