using System;

namespace com.onlineobject.objectnet.embedded {
    /// <summary>Specifies a method as the message handler for messages with the given ID.</summary>
    /// <remarks>
    ///   <para>
    ///     In order for a method to qualify as a message handler, it <i>must</i> match a valid message handler method signature. <see cref="EmbeddedServer"/>s
    ///     will only use methods marked with this attribute if they match the <see cref="EmbeddedServer.MessageHandler"/> signature, and <see cref="EmbeddedClient"/>s
    ///     will only use methods marked with this attribute if they match the <see cref="EmbeddedClient.MessageHandler"/> signature.
    ///   </para>
    ///   <para>
    ///     Methods marked with this attribute which match neither of the valid message handler signatures will not be used by <see cref="EmbeddedServer"/>s
    ///     or <see cref="EmbeddedClient"/>s and will cause warnings at runtime.
    ///   </para>
    ///   <para>
    ///     If you want a <see cref="EmbeddedServer"/> or <see cref="EmbeddedClient"/> to only use a subset of all message handler methods, you can do so by setting up
    ///     custom message handler groups. Simply set the group ID in the <see cref="EmbeddedMessageHandlerAttribute(ushort, byte)"/> constructor and pass the
    ///     same value to the <see cref="EmbeddedServer.Start(ushort, ushort, byte, bool)"/> or <see cref="EmbeddedClient.Connect(string, int, byte, EmbeddedMessage, bool)"/> method. This
    ///     will make that <see cref="EmbeddedServer"/> or <see cref="EmbeddedClient"/> only use message handlers which have the same group ID.
    ///   </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class EmbeddedMessageHandlerAttribute : Attribute
    {
        /// <summary>The ID of the message type which this method is meant to handle.</summary>
        public readonly ushort MessageId;
        /// <summary>The ID of the group of message handlers which this method belongs to.</summary>
        public readonly byte GroupId;

        /// <summary>Initializes a new instance of the <see cref="EmbeddedMessageHandlerAttribute"/> class with the <paramref name="messageId"/> and <paramref name="groupId"/> values.</summary>
        /// <param name="messageId">The ID of the message type which this method is meant to handle.</param>
        /// <param name="groupId">The ID of the group of message handlers which this method belongs to.</param>
        /// <remarks>
        ///   <see cref="EmbeddedServer"/>s will only use this method if its signature matches the <see cref="EmbeddedServer.MessageHandler"/> signature.
        ///   <see cref="EmbeddedClient"/>s will only use this method if its signature matches the <see cref="EmbeddedClient.MessageHandler"/> signature.
        ///   This method will be ignored if its signature matches neither of the valid message handler signatures.
        /// </remarks>
        public EmbeddedMessageHandlerAttribute(ushort messageId, byte groupId = 0)
        {
            MessageId = messageId;
            GroupId = groupId;
        }
    }
}
