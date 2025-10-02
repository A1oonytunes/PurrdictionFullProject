using System;
using System.Collections.Generic;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines the interface for a transport layer used in network communications.
    /// </summary>
    public interface ITransport {

        /// <summary>
        /// Sets the IP address for the transport.
        /// </summary>
        /// <param name="ip">The IP address to set.</param>
        void SetIp(string ip);

        /// <summary>
        /// Sets the port for the transport.
        /// </summary>
        /// <param name="port">The port number to set.</param>
        /// <param name="type">The type of port to set (TCP/UDP/Both).</param>
        void SetPort(ushort port, TransportPortType type = TransportPortType.Both);

        /// <summary>
        /// Retrieves the currently set IP address.
        /// </summary>
        /// <returns>The IP address as a string.</returns>
        string GetIp();

        /// <summary>
        /// Retrieves the currently set port number.
        /// </summary>
        /// <param name="type">The type of port to retrieve (TCP/UDP/Both).</param>
        /// <returns>The port number.</returns>
        ushort GetPort(TransportPortType type = TransportPortType.Both);

        /// <summary>
        /// Initializes the transport layer.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Destroys the transport layer, performing any necessary cleanup.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Processes any pending transport layer actions.
        /// </summary>
        /// <returns>Data buffers used udring the process</returns>
        List<byte[]> Process();

        /// <summary>
        /// Attempts to establish a connection using the transport layer.
        /// </summary>
        /// <returns>True if the connection was successful, false otherwise.</returns>
        bool Connect();

        /// <summary>
        /// Checks if the transport is currently connected.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        bool IsConnected();

        /// <summary>
        /// Disconnect transport system
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sets the idle timeout for the transport.
        /// </summary>
        /// <param name="time">The timeout duration in seconds.</param>
        void SetIdleTimeout(int time);

        /// <summary>
        /// Retrieves the currently set idle timeout for the transport.
        /// </summary>
        /// <returns>The idle timeout in seconds.</returns>
        int GetIdleTimeout();

        /// <summary>
        /// Return if this transport will disconnect client by timeout
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        bool IsTimeoutEnabled();

        /// <summary>
        /// Set if this transport will disconnect client by timeout
        /// </summary>
        /// <param name="value">Time out enabled</param>
        void SetTimeoutEnabled(bool value);

        /// <summary>
        /// Return if this transport will disconnect
        /// </summary>
        /// <returns>True if enabled, otherwise false</returns>
        bool IsDisconnectionEnabled();

        /// <summary>
        /// Set if this transport will disconnect
        /// </summary>
        /// <param name="value">Time out enabled</param>
        void SetDisconnectionEnabled(bool value);
            
        /// <summary>
        /// Sends data using the transport.
        /// </summary>
        /// <param name="data">The byte array containing the data to send.</param>
        /// <param name="mode">The delivery mode for sending the data (Reliable/Unreliable).</param>
        void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable);

        /// <summary>
        /// Called when a client has successfully connected.
        /// </summary>
        /// <param name="client">The connected client.</param>
        void OnClientConnected(ITransportClient client);

        /// <summary>
        /// Called when a client has disconnected.
        /// </summary>
        /// <param name="client">The disconnected client.</param>
        void OnClientDisconnected(ITransportClient client);

        /// <summary>
        /// Called when a message is received from a client.
        /// </summary>
        /// <param name="client">The client that sent the message.</param>
        /// <param name="data">The received message data.</param>
        void OnMessageReceived(ITransportClient client, byte[] data);

        /// <summary>
        /// Configures the transport layer with callback functions for client connection events and message receiving.
        /// </summary>
        /// <param name="onClientConnected">Callback invoked when a client connects.</param>
        /// <param name="onClientDisconnected">Callback invoked when a client disconnects.</param>
        /// <param name="onMessageReceived">Callback invoked when a message is received from a client.</param>
        void Configure(Action<ITransportClient> onClientConnected,
                       Action<ITransportClient> onClientDisconnected,
                       Action<ITransportClient, byte[]> onMessageReceived);
    }

}