namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a transport server that can manage communication over a network or communication channel.
    /// </summary>
    /// <remarks>
    /// This interface extends the <see cref="ITransport"/> interface. It is a marker interface and does not
    /// define any additional members beyond those inherited from <see cref="ITransport"/>.
    /// Implementing classes should provide the specific details for how transport server functionality is handled.
    /// </remarks>
    public interface ITransportServer : ITransport {
        
    }
}