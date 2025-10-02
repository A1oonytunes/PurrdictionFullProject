namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the basic functionalities of a network client.
    /// </summary>
    public interface IClient {
        /// <summary>
        /// Retrieves the IP address and port number as a string.
        /// </summary>
        /// <returns>A string representing the IP address and port number.</returns>
        string GetIpPort();

        /// <summary>
        /// Retrieves the IP address of the client.
        /// </summary>
        /// <returns>A string representing the IP address.</returns>
        string GetIp();

        /// <summary>
        /// Retrieves the port number of the client.
        /// </summary>
        /// <returns>An unsigned short representing the port number.</returns>
        int GetPort();

        /// <summary>
        /// Sets the communication channel for the client.
        /// </summary>
        /// <param name="transport">The channel to be used for communication.</param>
        void SetChannel(IChannel transport);

        /// <summary>
        /// Retrieves the communication channel associated with the client.
        /// </summary>
        /// <returns>The communication channel.</returns>
        IChannel GetChannel();

        /// <summary>
        /// Sets the transport mechanism for the client.
        /// </summary>
        /// <param name="transport">The transport mechanism to be used.</param>
        void SetTransport(ITransport transport);

        /// <summary>
        /// Retrieves the transport mechanism associated with the client.
        /// </summary>
        /// <returns>The transport mechanism.</returns>
        ITransport GetTransport();

        /// <summary>
        /// Sends data to the server using the specified delivery mode.
        /// </summary>
        /// <param name="data">The byte array containing the data to send.</param>
        /// <param name="mode">The delivery mode (default is Unreliable).</param>
        void Send(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable);

        /// <summary>
        /// Transmits data to the server using the specified delivery mode.
        /// This method may be similar to Send, but could have different underlying implementations or purposes.
        /// </summary>
        /// <param name="data">The byte array containing the data to transmit.</param>
        /// <param name="mode">The delivery mode (default is Unreliable).</param>
        void Transmit(byte[] data, DeliveryMode mode = DeliveryMode.Unreliable);

        /// <summary>
        /// Sends an event with a code and data to the server using the specified delivery mode.
        /// </summary>
        /// <param name="eventCode">The event code to identify the type of event.</param>
        /// <param name="writer">The DataStream containing the data to send.</param>
        /// <param name="mode">The delivery mode (default is Unreliable).</param>
        void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable);

        /// <summary>
        /// Sends data using a DataStream to the server with the specified delivery mode.
        /// </summary>
        /// <param name="writer">The DataStream containing the data to send.</param>
        /// <param name="mode">The delivery mode (default is Unreliable).</param>
        void Send(DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable);

        /// <summary>
        /// Register element under this client control
        /// </summary>
        /// <param name="control">Element under client control</param>
        void RegisterControl(INetworkControl control);

        /// <summary>
        /// Register element under this client control
        /// </summary>
        /// <param name="control">Element under client control</param>
        void ReleaseControl(INetworkControl control);

        /// <summary>
        /// Get all elements under control of this client
        /// </summary>
        /// <returns>Arrays with all controls</returns>
        INetworkControl[] GetControls();
    }

}