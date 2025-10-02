namespace com.onlineobject.objectnet.embedded {
    /// <summary>Represents a type that can be added to and retrieved from messages using the <see cref="EmbeddedMessage.AddSerializable{T}(T)"/> and <see cref="EmbeddedMessage.GetSerializable{T}"/> methods.</summary>
    public interface IEmbeddedMessageSerializable
    {
        /// <summary>Adds the type to the message.</summary>
        /// <param name="message">The message to add the type to.</param>
        void Serialize(EmbeddedMessage message);
        /// <summary>Retrieves the type from the message.</summary>
        /// <param name="message">The message to retrieve the type from.</param>
        void Deserialize(EmbeddedMessage message);
    }
}
