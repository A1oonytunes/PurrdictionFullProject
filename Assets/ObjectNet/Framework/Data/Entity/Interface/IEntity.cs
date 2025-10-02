namespace com.onlineobject.objectnet {
    /// <summary>
    /// Defines a contract for an entity that can perform computations, synchronization, and data consumption.
    /// </summary>
    public interface IEntity {
        /// <summary>
        /// Determines whether the entity is currently active.
        /// </summary>
        /// <returns>True if the entity is active; otherwise, false.</returns>
        bool IsActive();

        /// <summary>
        /// Pause this behaviour
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume this behaviour
        /// </summary>
        void Resume();

        /// <summary>
        /// Determine if synchonization is paused
        /// </summary>
        /// <returns>True if is paused; otherwise, false.</returns>
        bool IsPaused();

        /// <summary>
        /// Performs computation or processing logic associated with the entity.
        /// </summary>
        void Compute();

        /// <summary>
        /// Synchronizes the entity's state with an external system or data source.
        /// </summary>
        void Synchronize();

        /// <summary>
        /// Consumes data from the provided data stream.
        /// </summary>
        /// <param name="reader">The data stream to consume data from.</param>
        void Consume(IDataStream reader);

        /// <summary>
        /// Flags the instance as updated based on the provided value.
        /// </summary>
        /// <param name="updated">The updated state to set.</param>
        void FlagUpdated(bool updated);

        /// <summary>
        /// Checks if the entity has been updated since the last synchronization or computation.
        /// </summary>
        /// <returns>True if the entity was updated; otherwise, false.</returns>
        bool WasUpdated();

        /// <summary>
        /// Invalidates the entity's current state, typically requiring a refresh or re-computation.
        /// </summary>
        void Invalidate();
    }

}