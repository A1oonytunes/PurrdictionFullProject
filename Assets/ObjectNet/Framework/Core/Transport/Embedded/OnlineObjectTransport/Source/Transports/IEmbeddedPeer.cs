using System;

namespace com.onlineobject.objectnet.embedded.Transports
{
    /// <summary>The header type of a <see cref="EmbeddedMessage"/>.</summary>
    public enum MessageHeader : byte
    {
        /// <summary>An unreliable user message.</summary>
        Unreliable,
        /// <summary>An internal unreliable ack message.</summary>
        Ack,
        /// <summary>An internal unreliable connect message.</summary>
        Connect,
        /// <summary>An internal unreliable connection rejection message.</summary>
        Reject,
        /// <summary>An internal unreliable heartbeat message.</summary>
        Heartbeat,
        /// <summary>An internal unreliable disconnect message.</summary>
        Disconnect,

        /// <summary>A notify message.</summary>
        Notify,

        /// <summary>A reliable user message.</summary>
        Reliable,
        /// <summary>An internal reliable welcome message.</summary>
        Welcome,
        /// <summary>An internal reliable client connected message.</summary>
        ClientConnected,
        /// <summary>An internal reliable client disconnected message.</summary>
        ClientDisconnected,
    }

    /// <summary>Defines methods, properties, and events which every transport's server <i>and</i> client must implement.</summary>
    public interface IEmbeddedPeer
    {
        /// <summary>Invoked when data is received by the transport.</summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;
        /// <summary>Invoked when a disconnection is initiated or detected by the transport.</summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>Initiates handling of any received messages.</summary>
        void Poll();
    }
}
