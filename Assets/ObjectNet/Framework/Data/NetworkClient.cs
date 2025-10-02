using System.Collections.Generic;
using System.Linq;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network client with functionality to manage connections and data transmission.
    /// </summary>
    public sealed class NetworkClient : IClient {
        // Unique identifier for the connection
        private int connectionId = 0;

        // IP address of the client
        private string ip = "";

        // Port number of the client
        private int port = 0;

        // Sequence number for the last received UDP packet
        private uint udpReceivedSequence = 0;

        // Identifier for the network object associated with this client
        private int networkObjectId;

        // Communication channel for sending and receiving data
        private IChannel channel;

        // Transport layer used by the client for network communication
        private ITransport transport;

        private List<INetworkControl> ownedControls = new List<INetworkControl>();

        /// <summary>
        /// Initializes a new instance of the NetworkClient class with a specified connection ID.
        /// </summary>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        public NetworkClient(int connectionId) {
            this.connectionId = connectionId;
        }

        /// <summary>
        /// Initializes a new instance of the NetworkClient class with specified IP, port, and connection ID.
        /// </summary>
        /// <param name="ip">The IP address of the client.</param>
        /// <param name="port">The port number of the client.</param>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        public NetworkClient(string ip, ushort port, int connectionId) {
            this.ip = ip;
            this.port = port;
            this.connectionId = connectionId;
        }

        /// <summary>
        /// Sets the communication channel for the client.
        /// </summary>
        /// <param name="channel">The channel to be used for communication.</param>
        public void SetChannel(IChannel channel) {
            this.channel = channel;
        }

        /// <summary>
        /// Gets the communication channel associated with the client.
        /// </summary>
        /// <returns>The communication channel.</returns>
        public IChannel GetChannel() {
            return this.channel;
        }

        /// <summary>
        /// Sets the transport layer for the client.
        /// </summary>
        /// <param name="transport">The transport layer to be used for network communication.</param>
        public void SetTransport(ITransport transport) {
            this.transport = transport;
        }

        /// <summary>
        /// Gets the transport layer associated with the client.
        /// </summary>
        /// <returns>The transport layer.</returns>
        public ITransport GetTransport() {
            return this.transport;
        }

        /// <summary>
        /// Sets the connection ID for the client.
        /// </summary>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        public void SetConnectionId(int connectionId) {
            this.connectionId = connectionId;
        }

        /// <summary>
        /// Gets the connection ID of the client.
        /// </summary>
        /// <returns>The connection ID.</returns>
        public int GetConnectionId() {
            return this.connectionId;
        }

        /// <summary>
        /// Gets the IP address and port as a formatted string.
        /// </summary>
        /// <returns>A string in the format "IP:Port".</returns>
        public string GetIpPort() {
            return string.Format("{0}:{1}", this.ip, this.port);
        }

        /// <summary>
        /// Gets the IP address of the client.
        /// </summary>
        /// <returns>The IP address.</returns>
        public string GetIp() {
            return this.ip;
        }

        /// <summary>
        /// Gets the port number of the client.
        /// </summary>
        /// <returns>The port number.</returns>
        public int GetPort() {
            return this.port;
        }

        /// <summary>
        /// Sets the network object ID for the client.
        /// </summary>
        /// <param name="id">The identifier for the network object.</param>
        public void SetNetworkObjectId(int id) {
            this.networkObjectId = id;
        }

        /// <summary>
        /// Gets the network object ID associated with the client.
        /// </summary>
        /// <returns>The network object ID.</returns>
        public int GetNetworkObjectId() {
            return this.networkObjectId;
        }

        /// <summary>
        /// Gets the sequence number for the last received UDP packet.
        /// </summary>
        /// <returns>The sequence number.</returns>
        public uint GetReceivedSequence() {
            return this.udpReceivedSequence;
        }

        /// <summary>
        /// Sets the sequence number for the last received UDP packet.
        /// </summary>
        /// <param name="value">The sequence number.</param>
        public void SetReceivedSequence(uint value) {
            this.udpReceivedSequence = value;
        }

        /// <summary>
        /// Sends data using the assigned channel and transport layer with the specified delivery mode.
        /// </summary>
        /// <param name="writer">The data stream to send.</param>
        /// <param name="mode">The delivery mode for the data transmission.</param>
        public void Send(DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable) {
            this.GetChannel().Send(writer.GetBuffer(), mode, this.GetTransport());
        }

        /// <summary>
        /// Sends a byte array using the assigned channel and transport layer with the specified delivery mode.
        /// </summary>
        /// <param name="data">The byte array to send.</param>
        /// <param name="mode">The delivery mode for the data transmission.</param>
        public void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            this.GetChannel().Send(data, mode, this.GetTransport());
        }

        /// <summary>
        /// Transmits data using the assigned channel and transport layer with the specified delivery mode.
        /// </summary>
        /// <param name="data">The byte array to transmit.</param>
        /// <param name="mode">The delivery mode for the data transmission.</param>
        public void Transmit(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable) {
            this.GetChannel().Transmit(data, mode, this.GetTransport());
        }

        /// <summary>
        /// Sends an event with a code and data using the assigned channel and transport layer with the specified delivery mode.
        /// </summary>
        /// <param name="eventCode">The event code to send.</param>
        /// <param name="writer">The data stream containing the event data.</param>
        /// <param name="mode">The delivery mode for the event transmission.</param>
        public void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable) {
            // Open space to write event code
            writer.ShiftRight(0, sizeof(int));
            writer.Write(eventCode, 0); // Write event code

            // Send buffer
            this.Send(writer.GetBuffer(), mode);
        }

        /// <summary>
        /// Register element under this client control
        /// </summary>
        /// <param name="control">Element under client control</param>
        public void RegisterControl(INetworkControl control) {
            if ( !this.ownedControls.Contains(control) ) {
                this.ownedControls.Add(control);
            }
        }

        /// <summary>
        /// Register element under this client control
        /// </summary>
        /// <param name="control">Element under client control</param>
        public void ReleaseControl(INetworkControl control) {
            if (this.ownedControls.Contains(control)) {
                this.ownedControls.Add(control);
            }
        }

        /// <summary>
        /// Get all elements under control of this client
        /// </summary>
        /// <returns>Arrays with all controls</returns>
        public INetworkControl[] GetControls() {
            return this.ownedControls.ToArray<INetworkControl>();
        }
    }

}