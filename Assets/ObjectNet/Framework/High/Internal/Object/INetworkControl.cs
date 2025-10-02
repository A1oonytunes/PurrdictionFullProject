namespace com.onlineobject.objectnet {
    public interface INetworkControl {

        /// <summary>
        /// Retrieves the current behavior mode.
        /// </summary>
        /// <returns>The <see cref="BehaviorMode"/> that is currently set.</returns>
        void TakeControl();

        /// <summary>
        /// Retrieves the current behavior mode.
        /// </summary>
        /// <returns>The <see cref="BehaviorMode"/> that is currently set.</returns>
        void ReleaseControl();

        /// <summary>
        /// Transfer control of object to another player.
        /// </summary>
        /// <param name="playerElementId">Element id of player that will received object control</param>
        void TransferControl(int playerElementId);

        /// <summary>
        /// Transfer control of object to another player.
        /// </summary>
        /// <param name="targetClient">Client that will received object control</param>
        void TransferControl(IClient target);

        /// <summary>
        /// Transfer control of object to another player.
        /// </summary>
        /// <param name="targetPlayer">Player that will received object control</param>
        void TransferControl(INetworkControl target);

        /// <summary>
        /// Set ownership access level
        /// </summary>
        /// <param name="level">Level to apply</param>
        void SetAccessLevel(OwnerShipAccessLevel level);

        /// <summary>
        /// Return ownership access level
        /// </summary>
        /// <returns>Current ownership access level</returns>
        OwnerShipAccessLevel GetAccessLevel();

        /// <summary>
        /// Sets the state indicating whether this network element represents a player.
        /// </summary>
        /// <param name="value">True if this represents a player, false otherwise.</param>
        void SetIsPlayer(bool value);

        /// <summary>
        /// Checks if this network element represents a player.
        /// </summary>
        /// <returns>True if this represents a player, false otherwise.</returns>
        bool IsPlayer();

        /// <summary>
        /// Return if this object is a respawned instance of this object
        /// </summary>
        /// <returns>true is a respawned object, otherwise false</returns>
        bool IsRespawned();

        /// <summary>
        /// Determines if the network element is active.
        /// </summary>
        /// <returns>True if the network element is active; otherwise, false.</returns>
        bool IsActive();

        /// <summary>
        /// Determines if the network element is passive.
        /// </summary>
        /// <returns>True if the network element is passive; otherwise, false.</returns>
        bool IsPassive();

        /// <summary>
        /// Initialize execution iof internal network behaviors
        /// </summary>
        void InitializeExecutor();
    }
}