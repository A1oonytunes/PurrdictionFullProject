using System;

namespace com.onlineobject.objectnet {
    public class TransportPeer : ITransportPeer {

        ushort id;

        string address;
        
        ushort port;

        bool connected = false;

        // Action to be called when a client is direclty connected with another peer.
        Action<ITransportClient> onClientConnectedOnPeer;

        object connection;

        public TransportPeer(ushort id, string address, ushort port, Action<ITransportClient> onConnected) {
            this.id = id;
            this.address = address;
            this.port = port;
            this.onClientConnectedOnPeer = onConnected;
        }

        public ushort GetId() {
            return this.id;
        }

        public string GetAddress() {
            return this.address;
        }

        public ushort GetPort() {
            return this.port;
        }

        public void SetAddress(string address) {
            this.address = address;
        }

        public void SetPort(ushort port) {
            this.port = port;
        }

        public T GetConnection<T>() {
            return (T)this.connection;
        }

        public void SetConnection<T>(T value) {
            this.connection = value;
        }

        public bool IsConnected() {
            return this.connected;
        }

        public void SetConnected(bool value) {
            this.connected = value;
        }

        /// <summary>
        /// Invokes the client connected on peer action.
        /// </summary>
        /// <param name="client">The client that has connected.</param>
        public void OnClientConnectedOnPeer(ITransportClient client) {
            onClientConnectedOnPeer?.Invoke(client);
        }

        
    }
}