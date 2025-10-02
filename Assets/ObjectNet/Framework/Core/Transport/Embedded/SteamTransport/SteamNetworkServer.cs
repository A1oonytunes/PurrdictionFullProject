#if STEAMWORKS_NET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#endif

namespace com.onlineobject.objectnet.steamworks {
    public class SteamNetworkServer
#if STEAMWORKS_NET
        : SteamNetworkTransport, ITransportServer {
#else 
        {
#endif
#if STEAMWORKS_NET
        private BidirectionalDictionary<HSteamNetConnection, int> connectionToInternalId;

        private BidirectionalDictionary<CSteamID, int> steamIdToInternalId;

        private bool serverStarted = false;

        private int nextConnectionID;

        private HSteamListenSocket listenSocket;

        private Callback<SteamNetConnectionStatusChangedCallback_t> onConnectionChange = null;

        /// <summary>
        /// A dictionary mapping embedded connections to their corresponding client objects.
        /// </summary>
        private Dictionary<HSteamNetConnection, SteamNetworkClient> clients = new Dictionary<HSteamNetConnection, SteamNetworkClient>();

        // Data to return to pooling
        private List<byte[]> dequeuedBuffers = new List<byte[]>();

        public SteamNetworkServer() {
            this.connectionToInternalId     = new BidirectionalDictionary<HSteamNetConnection, int>();
            this.steamIdToInternalId        = new BidirectionalDictionary<CSteamID, int>();
            this.nextConnectionID           = 1;
            this.onConnectionChange         = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
        }
        
        /// <summary>
        /// Stablish a p2p server listener socket
        /// </summary>
        private void Host() {
            SteamNetworkingConfigValue_t[] options = new SteamNetworkingConfigValue_t[] { };
#if UNITY_SERVER
            this.listenSocket = SteamGameServerNetworkingSockets.CreateListenSocketP2P(0, options.Length, options);
#else
            this.listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, options.Length, options);
#endif
        }

        /// <summary>
        /// Notify regard soem change on conenction status
        /// </summary>
        /// <param name="param">Changed callback parameters</param>
        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param) {
            ulong clientSteamID = param.m_info.m_identityRemote.GetSteamID64();
            if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting) {
                if ((TransportDefinitions.MaximunOfClients > 0) && (connectionToInternalId.Count >= TransportDefinitions.MaximunOfClients)) {
                    NetworkDebugger.Log($"Incoming connection {clientSteamID} would exceed max connection count. Rejecting.");
#if UNITY_SERVER
                    SteamGameServerNetworkingSockets.CloseConnection(param.m_hConn, 0, "Max Connection Count", false);
#else
                    SteamNetworkingSockets.CloseConnection(param.m_hConn, 0, "Max Connection Count", false);
#endif
                    return;
                }

                EResult res;

#if UNITY_SERVER
                if ((res = SteamGameServerNetworkingSockets.AcceptConnection(param.m_hConn)) == EResult.k_EResultOK)
#else
                if ((res = SteamNetworkingSockets.AcceptConnection(param.m_hConn)) == EResult.k_EResultOK)
#endif
                {
                    NetworkDebugger.Log($"Accepting connection {clientSteamID}");
                } else {
                    NetworkDebugger.Log($"Connection {clientSteamID} could not be accepted: {res}");
                }
            } else if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected) {
                int connectionId = nextConnectionID++;
                connectionToInternalId.Add(param.m_hConn, connectionId);
                steamIdToInternalId.Add(param.m_info.m_identityRemote.GetSteamID(), connectionId);
                this.clients.Add(param.m_hConn, new SteamNetworkClient(param.m_hConn).FlagConnectedOnServer());
                this.OnClientConnected(this.clients[param.m_hConn]);
                NetworkDebugger.Log($"Client with SteamID {clientSteamID} connected. Assigning connection id {connectionId}");
            } else if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || 
                       param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally) {
                if (connectionToInternalId.TryGetValue(param.m_hConn, out int connId)) {
                    InternalDisconnect(connId, param.m_hConn);
                }
            } else {
                NetworkDebugger.Log($"Connection {clientSteamID} state changed: {param.m_info.m_eState}");
            }
        }

        /// <summary>
        /// Execute internal disconect on the socket
        /// </summary>
        /// <param name="connectionId">Connection id to disconnect</param>
        /// <param name="socket">Socket to disconnect</param>
        private void InternalDisconnect(int connectionId, HSteamNetConnection socket) {            
#if UNITY_SERVER
            SteamGameServerNetworkingSockets.CloseConnection(socket, 0, "Graceful disconnect", false);
#else
            SteamNetworkingSockets.CloseConnection(socket, 0, "Graceful disconnect", false);
#endif
            SteamNetworkClient disconnectedClient = this.clients[socket];
            connectionToInternalId.Remove(connectionId);
            steamIdToInternalId.Remove(connectionId);
            this.clients.Remove(socket);
            NetworkDebugger.Log($"Client with ConnectionID {connectionId} disconnected.");
            this.OnClientDisconnected(disconnectedClient);
        }

        /// <summary>
        /// Disconnect server
        /// </summary>
        public override void Disconnect() {
            this.Disconnect(nextConnectionID);
            this.Shutdown();
        }

        /// <summary>
        /// Disconnect some socket
        /// </summary>
        /// <param name="connectionId">Connection id to disconnect</param>
        public void Disconnect(int connectionId) {
            foreach (var connenction in connectionToInternalId.SecondTypes) {
                HSteamNetConnection hsteamNetwork = connectionToInternalId[connenction];

                NetworkDebugger.Log($"Connection id {connenction} disconnected.");
#if UNITY_SERVER
                SteamGameServerNetworkingSockets.CloseConnection(hsteamNetwork, 0, "Disconnected by server", false);
#else
                SteamNetworkingSockets.CloseConnection(hsteamNetwork, 0, "Disconnected by server", false);
#endif
                steamIdToInternalId.Remove(connectionId);
                connectionToInternalId.Remove(connectionId);
                this.OnClientDisconnected(this.clients[hsteamNetwork]);
            }
            // Close listening socket ( just in case )
            SteamNetworkingSockets.CloseListenSocket(this.listenSocket);
        }

        /// <summary>
        /// Flush steam socket data from channel
        /// </summary>
        public void FlushData() {
            foreach (HSteamNetConnection conn in connectionToInternalId.FirstTypes.ToList()) {
#if UNITY_SERVER
                SteamGameServerNetworkingSockets.FlushMessagesOnConnection(conn);
#else
                SteamNetworkingSockets.FlushMessagesOnConnection(conn);
#endif
            }
        }

        /// <summary>
        /// Perform a received data on channel socket
        /// </summary>        
        private void ReceiveData() {
            foreach (HSteamNetConnection conn in connectionToInternalId.FirstTypes.ToList()) {
                if (connectionToInternalId.TryGetValue(conn, out int connId)) {
                    IntPtr[] ptrs = new IntPtr[MAX_MESSAGES];
                    int messageCount;
#if UNITY_SERVER
                    if ((messageCount = SteamGameServerNetworkingSockets.ReceiveMessagesOnConnection(conn, ptrs, MAX_MESSAGES)) > 0)
#else
                    if ((messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(conn, ptrs, MAX_MESSAGES)) > 0)
#endif
                    {
                        for (int i = 0; i < messageCount; i++) {
                            (byte[] data, int ch) = ProcessMessage(ptrs[i]);
                            this.OnMessageReceived(this.clients[conn], data);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Send data over negtwork using steam socket connection channel
        /// </summary>
        /// <param name="connectionId">Connection id to send</param>
        /// <param name="data">Data buffer to send</param>
        /// <param name="deliveryMode">Delivery mode to send</param>
        private void Send(int connectionId, byte[] data, DeliveryMode deliveryMode = DeliveryMode.Unreliable) {
            if (connectionToInternalId.TryGetValue(connectionId, out HSteamNetConnection conn)) {
                EResult res = this.SendSocket(conn, data, deliveryMode);
                if (res == EResult.k_EResultNoConnection || res == EResult.k_EResultInvalidParam) {
                    NetworkDebugger.Log($"Connection to {connectionId} was lost.");
                    InternalDisconnect(connectionId, conn);
                } else if (res != EResult.k_EResultOK) {
                    NetworkDebugger.LogError($"Could not send: {res}");
                }
            } else {
                NetworkDebugger.LogError("Trying to send on an unknown connection: " + connectionId);                
            }
        }

        /// <summary>
        /// Return the client address associated with a connection id
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>CLient address</returns>
        public string GetClientAddress(int connectionId) {
            if (steamIdToInternalId.TryGetValue(connectionId, out CSteamID steamId)) {
                return steamId.ToString();
            } else {
                NetworkDebugger.LogError("Trying to get info on an unknown connection: " + connectionId);
                return string.Empty;
            }
        }

        /// <summary>
        /// Shutdown serter socket
        /// </summary>
        public void Shutdown() {
#if UNITY_SERVER
            SteamGameServerNetworkingSockets.CloseListenSocket(listenSocket);
#else
            SteamNetworkingSockets.CloseListenSocket(listenSocket);
#endif

            this.onConnectionChange?.Dispose();
            this.onConnectionChange = null;
            // Invalidate socket
            this.listenSocket = default(HSteamListenSocket);
            this.serverStarted = false;
        }

        /// <summary>
        /// Initialize swerver socket
        /// </summary>
        public override void Initialize() {
            try {
                this.serverStarted = false;
                base.Initialize();
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Destiory server socket connection
        /// </summary>
        public override void Destroy() {
            this.Shutdown();
        }

        /// <summary>
        /// Processes any pending operations such as sending messages and updating client states.
        /// </summary>
        public override List<byte[]> Process() {
            if (this.IsConnected()) {
                try {
                    this.dequeuedBuffers.Clear();

                    this.ReceiveData();
                    // Send pending messages (Reliable)
                    while (this.DequeueMessage(DeliveryMode.Reliable, out var dataBytes)) {
                        this.InternalSendMessage(dataBytes, DeliveryMode.Reliable);
                        this.dequeuedBuffers.Add(dataBytes);
                    }
                    // Send pending messages (Unreliable)
                    while (this.DequeueMessage(DeliveryMode.Unreliable, out var dataBytes)) {
                        this.InternalSendMessage(dataBytes, DeliveryMode.Unreliable);
                        this.dequeuedBuffers.Add(dataBytes);
                    }
                } finally {
                    // Process all clients
                    foreach (SteamNetworkClient connection in this.clients.Values) {
                        this.dequeuedBuffers.AddRange(connection.Process());
                    }
                }
            }
            return this.dequeuedBuffers;
        }

        /// <summary>
        /// Starts the server and begins listening for incoming connections.
        /// </summary>
        /// <returns>True if the server successfully starts, false otherwise.</returns>
        public override bool Connect() {
            this.serverStarted = false;
            try {
                this.Host();
                this.serverStarted = true;
            } catch (Exception err) {
                NetworkDebugger.LogError("Error trying to connect: " + err.Message);
            }
            return this.serverStarted;
        }

        /// <summary>
        /// Checks if the server is currently connected and running.
        /// </summary>
        /// <returns>True if the server is running, false otherwise.</returns>
        public override bool IsConnected() {
            return (this.listenSocket != null) && (this.serverStarted == true);
        }

        /// <summary>
        /// Sends a message to all connected clients.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="mode">The delivery mode (reliable or unreliable).</param>
        private void InternalSendMessage(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            foreach (SteamNetworkClient connection in clients.Values) {
                this.InternalSendMessageToClient(connection, data, mode);
            }
        }
#endif
    }
}
