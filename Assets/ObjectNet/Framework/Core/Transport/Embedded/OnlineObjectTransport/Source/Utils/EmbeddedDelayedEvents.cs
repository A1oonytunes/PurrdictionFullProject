using com.onlineobject.objectnet.embedded.Transports;

namespace com.onlineobject.objectnet.embedded.Utils
{
    /// <summary>Executes an action when invoked.</summary>
    internal abstract class DelayedEvent
    {
        /// <summary>Executes the action.</summary>
        public abstract void Invoke();
    }

    /// <summary>Resends a <see cref="EmbeddedPendingMessage"/> when invoked.</summary>
    internal class ResendEvent : DelayedEvent
    {
        /// <summary>The message to resend.</summary>
        private readonly EmbeddedPendingMessage message;
        /// <summary>The time at which the resend event was queued.</summary>
        private readonly long initiatedAtTime;

        /// <summary>Initializes the event.</summary>
        /// <param name="message">The message to resend.</param>
        /// <param name="initiatedAtTime">The time at which the resend event was queued.</param>
        public ResendEvent(EmbeddedPendingMessage message, long initiatedAtTime)
        {
            this.message = message;
            this.initiatedAtTime = initiatedAtTime;
        }

        /// <inheritdoc/>
        public override void Invoke()
        {
            if (initiatedAtTime == message.LastSendTime) // If this isn't the case then the message has been resent already
                message.RetrySend();
        }
    }

    /// <summary>Executes a heartbeat when invoked.</summary>
    internal class HeartbeatEvent : DelayedEvent
    {
        /// <summary>The peer whose heart to beat.</summary>
        private readonly EmbeddedPeer peer;

        /// <summary>Initializes the event.</summary>
        /// <param name="peer">The peer whose heart to beat.</param>
        public HeartbeatEvent(EmbeddedPeer peer)
        {
            this.peer = peer;
        }

        /// <inheritdoc/>
        public override void Invoke()
        {
            peer.Heartbeat();
        }
    }
}
