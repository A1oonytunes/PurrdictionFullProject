#if STEAMWORKS_NET
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
#endif

namespace com.onlineobject.objectnet.steamworks {

    [TransportType(ConnectionDelaySupport: true, MinimunConnectionDelay: 100, MaximumConnectionDelay: 1000, DefaultClientConnectionDelay: 250)]
    public class SteamNetworkClient
#if STEAMWORKS_NET
        : SteamNetworkTransport, ITransportClient {
#else 
        {
#endif

#if STEAMWORKS_NET
        public bool Connected { get; private set; }
        public bool Error { get; private set; }

        // private event Action<byte[], int> OnReceivedData;
        
        private Callback<SteamNetConnectionStatusChangedCallback_t> onConnectionChange = null;

        private CSteamID hostSteamID = CSteamID.Nil;
        
        private HSteamNetConnection HostConnection;

        private List<Action> BufferedData;

        // Data to return to pooling
        private List<byte[]> dequeuedBuffers = new List<byte[]>();

        public SteamNetworkClient() {
            this.BufferedData = new List<Action>();
        }

        public SteamNetworkClient(HSteamNetConnection transport) {
            this.HostConnection = transport;
            this.BufferedData   = new List<Action>();
            this.Connected      = false;
        }

        /// <summary>
        /// Starts the server and begins listening for incoming connections.
        /// </summary>
        /// <returns>True if the server successfully starts, false otherwise.</returns>
        public override bool Connect() {
            this.onConnectionChange = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
            this.Connected          = false;
            try {
                hostSteamID = new CSteamID(UInt64.Parse(this.GetIp()));
                SteamNetworkingIdentity smi = new SteamNetworkingIdentity();
                smi.SetSteamID(hostSteamID);
                SteamNetworkingConfigValue_t[] options = new SteamNetworkingConfigValue_t[] { };
                HostConnection = SteamNetworkingSockets.ConnectP2P(ref smi, 0, options.Length, options);
                this.Connected = true;
                while (BufferedData.Count > 0) {
                    var pending = this.BufferedData[0];
                    this.BufferedData.RemoveAt(0);
                    pending();                    
                }
            } catch (FormatException) {
                NetworkDebugger.LogError($"Connection string was not in the right format. Did you enter a SteamId?");
                Error = true;
            } catch (Exception ex) {
                NetworkDebugger.LogError($"Unexpected exception: {ex.Message}");
                Error = true;
            } finally {
                if (Error) {
                    NetworkDebugger.LogError("Connection failed.");
                }
            }

            return this.IsConnected();
        }

        /// <summary>
        /// Notify regard soem change on conenction status
        /// </summary>
        /// <param name="param">Changed callback parameters</param>
        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param) {
            ulong clientSteamID = param.m_info.m_identityRemote.GetSteamID64();
            if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected) {
                Connected = true;
                this.OnClientConnected(this);
                NetworkDebugger.Log("Connection established.");                
            } else if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || 
                       param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally) {
                NetworkDebugger.Log($"Connection was closed by peer, {param.m_info.m_szEndDebug}");
                this.Disconnect();
                this.OnClientDisconnected(this);
            } else {
                NetworkDebugger.Log($"Connection state changed: {param.m_info.m_eState.ToString()} - {param.m_info.m_szEndDebug}");
            }
        }
        /// <summary>
        /// Disconnect client socket
        /// </summary>
        public override void Disconnect() {
            Dispose();
            if (Connected) {
                InternalDisconnect();
            }
            if (HostConnection.m_HSteamNetConnection != 0) {
                NetworkDebugger.Log("Sending Disconnect message");
                SteamNetworkingSockets.CloseConnection(HostConnection, 0, "Graceful disconnect", false);
                HostConnection.m_HSteamNetConnection = 0;
            }
        }

        /// <summary>
        /// Dispose client socket from memory
        /// </summary>
        protected void Dispose() {
            if (onConnectionChange != null) {
                onConnectionChange.Dispose();
                onConnectionChange = null;
            }
        }

        /// <summary>
        /// Execute teh internal process to disconnect socket
        /// </summary>
        private void InternalDisconnect() {
            Connected = false;
            NetworkDebugger.Log("Disconnected.");
            SteamNetworkingSockets.CloseConnection(HostConnection, 0, "Disconnected", false);
            this.OnClientDisconnected(this);
        }

        /// <summary>
        /// Perform a received data on channel socket
        /// </summary>
        public void ReceiveData() {
            IntPtr[] ptrs = new IntPtr[MAX_MESSAGES];
            int messageCount;
            if ((messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(HostConnection, ptrs, MAX_MESSAGES)) > 0) {
                for (int i = 0; i < messageCount; i++) {
                    (byte[] data, int ch) = ProcessMessage(ptrs[i]);
                    if (this.IsConnected()) {
                        this.OnMessageReceived(this, data);
                    } else {
                        this.BufferedData.Add(() => this.OnMessageReceived(this, data));
                    }
                }
            }
        }

        /// <summary>
        /// Send data over negtwork using steam socket connection channel
        /// </summary>
        /// <param name="data">Data buffer to send</param>
        /// <param name="deliveryMode">Delivery mode to send</param>
        public override void Send(byte[] data, DeliveryMode deliveryMode = DeliveryMode.Unreliable) {
            try {
                EResult res = SendSocket(HostConnection, data, deliveryMode);
                if (res == EResult.k_EResultNoConnection || res == EResult.k_EResultInvalidParam) {
                    NetworkDebugger.Log($"Connection to server was lost.");
                    this.InternalDisconnect();
                } else if (res != EResult.k_EResultOK) {
                    NetworkDebugger.LogError($"Could not send: {res.ToString()}");
                }
            } catch (Exception ex) {
                NetworkDebugger.LogError($"SteamNetworking exception during Send: {ex.Message}");
                this.InternalDisconnect();
            }
        }

        /// <summary>
        /// Flag that this client is connected at server
        /// Note: This is required only on the server side
        /// </summary>
        public SteamNetworkClient FlagConnectedOnServer() {
            this.Connected = true;
            return this;
        }

        /// <summary>
        /// Flag that this client is disconnected on server
        /// Note: This is required only on the server side
        /// </summary>
        public SteamNetworkClient FlagDisconnectedOnServer() {
            this.Connected = false;
            return this;
        }

        /// <summary>
        /// Flush any incoming data on socket channel
        /// </summary>
        public void FlushData() {
            SteamNetworkingSockets.FlushMessagesOnConnection(HostConnection);
        }

        /// <summary>
        /// Disposes of the network driver if it has been created.
        /// </summary>
        public override void Destroy() {
            this.Disconnect();
        }

        /// <summary>
        /// Initializes the client, calling the base class initialization method.
        /// </summary>
        public override void Initialize() {
            try {
                base.Initialize();                
            } catch (FormatException) {
                NetworkDebugger.LogError($"Connection string was not in the right format. Did you enter a SteamId?");
                this.Error = true;
            } catch (Exception ex) {
                NetworkDebugger.LogError($"Unexpected exception: {ex.Message}");
                this.Error = true;
            }
        }

        /// <summary>
        /// Checks if the client is currently connected to the server.
        /// </summary>
        /// <returns>True if the client is connected, false otherwise.</returns>
        public override bool IsConnected() {
            return ((HostConnection != null) && (Mathf.Abs(HostConnection.m_HSteamNetConnection) > 0) && (this.Connected == true));
        }

        /// <summary>
        /// Determines if the connection was lost since the last check.
        /// </summary>
        /// <returns>True if the connection was previously connected and is now disconnected, false otherwise.</returns>
        public bool IsConnectionLost() {
            return (this.IsConnected() == false);
        }

        /// <summary>
        /// Processes network events such as sending and receiving messages, and handles connection state changes.
        /// </summary>
        public override List<byte[]> Process() {
            if (this.IsConnected()) {
                this.dequeuedBuffers.Clear();

                this.ReceiveData();
                // Send pending messages (Reliable)
                while (this.DequeueMessage(DeliveryMode.Reliable, out var dataBytes)) {
                    this.Send(dataBytes, DeliveryMode.Reliable);
                    this.dequeuedBuffers.Add(dataBytes);
                }
                // Send pending messages (Unreliable)
                while (this.DequeueMessage(DeliveryMode.Unreliable, out var dataBytes)) {
                    this.Send(dataBytes, DeliveryMode.Unreliable);
                    this.dequeuedBuffers.Add(dataBytes);
                }
            }
            return this.dequeuedBuffers;
        }

        /// <summary>
        /// Registers a peer with the transport client.
        /// </summary>
        /// <param name="peer">The peer to register.</param>
        public void RegisterPeer(ITransportPeer peer) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Unregisters the specified peer from the collection of peers.
        /// </summary>
        /// <param name="peer">The peer to unregister.</param>
        public void UnregisterPeer(ITransportPeer peer) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Unregisters the peer with the specified ID from the collection of peers.
        /// </summary>
        /// <param name="id">The ID of the peer to unregister.</param>
        public void UnregisterPeer(ushort id) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Retrieves the peer with the specified ID from the collection of peers.
        /// </summary>
        /// <param name="id">The ID of the peer to retrieve.</param>
        /// <returns>The peer with the specified ID.</returns>
        public ITransportPeer GetPeer(ushort id) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }

        /// <summary>
        /// Initializes the peer to peer server
        /// </summary>
        /// <param name="peerToPeerPort">Port where peer to peer data will be received</param>
        public void InitializePeerToPeerServer(ushort peerToPeerPort) {
            throw new System.Exception("Peer to Peer is not supported by this tranport system");
        }
#endif
    }
}
