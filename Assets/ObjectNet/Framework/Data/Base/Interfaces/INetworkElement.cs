using System;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Interface defining the contract for a network element within a networking system.
    /// </summary>
    public interface INetworkElement {
        /// <summary>
        /// Gets the network identifier.
        /// </summary>
        /// <returns>The network identifier as an integer.</returns>
        int GetNetworkId();

        /// <summary>
        /// Sets the network identifier.
        /// </summary>
        /// <param name="id">The network identifier to set.</param>
        void SetNetworkId(int id);

        /// <summary>
        /// Gets the network session identifier.
        /// </summary>
        /// <returns>The network session identifier as a string.</returns>
        string GetNetworkSessionId();

        /// <summary>
        /// Sets the network session identifier.
        /// </summary>
        /// <param name="id">The network session identifier to set.</param>
        void SetNetworkSessionId(string id);

        /// <summary>
        /// Determines if the current instance is the owner of the network element.
        /// </summary>
        /// <returns>True if the current instance is the owner; otherwise, false.</returns>
        bool IsOwner();

        /// <summary>
        /// Detects if the current instance should be the owner based on the network identifier.
        /// </summary>
        /// <param name="networkId">The network identifier to check for ownership.</param>
        void DetectOwner(int networkId);

        /// <summary>
        /// Sets the ownership status of the network element.
        /// </summary>
        /// <param name="value">The ownership status to set.</param>
        void SetOwner(bool value);

        /// <summary>
        /// Define network object associated with Network Element
        /// </summary>
        /// <param name="networkObject">Network object</param>
        void SetNetworkObject(INetworkControl networkObject);

        /// <summary>
        /// Return network onbject associated with Network Element
        /// </summary>
        /// <returns>Network object</returns>
        INetworkControl GetNetworkObject();


        /// <summary>
        /// Return network onbject associated with Network Element
        /// </summary>
        /// <typeparam name="T">Generic type to be returned</typeparam>
        /// <returns>Network object</returns>
        T GetNetworkObject<T>() where T : INetworkControl;

        /// <summary>
        /// Gets the transport channel associated with the network element.
        /// </summary>
        /// <returns>The transport channel as an IChannel.</returns>
        IChannel GetTransport();

        /// <summary>
        /// Sets the transport channel for the network element.
        /// </summary>
        /// <param name="transport">The transport channel to set.</param>
        void SetTransport(IChannel transport);

        /// <summary>
        /// Configures the method used for sending data.
        /// </summary>
        /// <param name="method">The action delegate representing the send method.</param>
        void ConfigureSendMethod(Action<int, DataStream, DeliveryMode, IClient> method);

        /// <summary>
        /// Sends data over the network.
        /// </summary>
        /// <param name="eventCode">The event code associated with the data.</param>
        /// <param name="writer">The data stream to send.</param>
        /// <param name="mode">The delivery mode for the data (default is Unreliable).</param>
        /// <param name="client">target client.</param>
        void Send(int eventCode, DataStream writer, DeliveryMode mode = DeliveryMode.Unreliable, IClient client = null);

        /// <summary>
        /// Processes any pending network operations.
        /// </summary>
        void Process();

        /// <summary>
        /// Gets the network container associated with the network element.
        /// </summary>
        /// <returns>The network container as an INetworkContainer.</returns>
        INetworkContainer GetContainer();

        /// <summary>
        /// Gets the data stream writer associated with the network element.
        /// </summary>
        /// <returns>The data stream writer as an IDataStream.</returns>
        IDataStream GetWritterStream();

        /// <summary>
        /// Sets the data stream writer for the network element.
        /// </summary>
        /// <param name="writter">The data stream writer to set.</param>
        void SetWritterStream(IDataStream writter);

        /// <summary>
        /// Registers a network packet for processing.
        /// </summary>
        /// <param name="packet">The data stream representing the network packet.</param>
        void RegisterNetworkPacket(IDataStream packet);

        /// <summary>
        /// Registers an input packet for processing.
        /// </summary>
        /// <param name="packet">The data stream representing the input packet.</param>
        void RegisterInputPacket(IDataStream packet);

        /// <summary>
        /// Gets the GameObject associated with the network element.
        /// </summary>
        /// <returns>The GameObject.</returns>
        GameObject GetGameObject();

        /// <summary>
        /// Sets the GameObject for the network element.
        /// </summary>
        /// <param name="gameObject">The GameObject to set.</param>
        void SetGameObject(GameObject gameObject);

        /// <summary>
        /// Sets the behavior mode of the network element.
        /// </summary>
        /// <param name="mode">The behavior mode to set.</param>
        void SetMode(BehaviorMode mode);

        /// <summary>
        /// Gets the behavior mode of the network element.
        /// </summary>
        /// <returns>The behavior mode as a BehaviorMode.</returns>
        BehaviorMode GetMode();

        /// <summary>
        /// Sets the delivery mode for network communication.
        /// </summary>
        /// <param name="deliveryMode">The delivery mode to set.</param>
        void SetDeliveryMode(DeliveryMode deliveryMode);

        /// <summary>
        /// Gets the delivery mode for network communication.
        /// </summary>
        /// <returns>The delivery mode as a DeliveryMode.</returns>
        DeliveryMode GetDeliveryMode();

        /// <summary>
        /// Set the current ownership access level of an object
        /// </summary>
        /// <param name="accessLevel">Level of ownership access</param>
        void SetOwnershipAccessLevel(OwnerShipAccessLevel accessLevel);

        /// <summary>
        /// Gets the current ownership access level of an object
        /// </summary>
        /// <returns>The current current ownership access level.</returns>
        OwnerShipAccessLevel GetOwnershipAccessLevel();

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
        /// Sets the player status of the network element.
        /// </summary>
        /// <param name="value">The player status to set.</param>
        void SetIsPlayer(bool value);

        /// <summary>
        /// Sets the player identifier for the network element.
        /// </summary>
        /// <param name="id">The player identifier to set.</param>
        void SetPlayerId(ushort id);

        /// <summary>
        /// Gets the player identifier of the network element.
        /// </summary>
        /// <returns>The player identifier as a ushort.</returns>
        ushort GetPlayerId();

        /// <summary>
        /// Sets the player index for the network element.
        /// </summary>
        /// <param name="index">The player index to set.</param>
        void SetPlayerIndex(ushort index);

        /// <summary>
        /// Gets the player index of the network element.
        /// </summary>
        /// <returns>The player index as a ushort.</returns>
        ushort GetPlayerIndex();

        /// <summary>
        /// Gets the active rate of the network element.
        /// </summary>
        /// <returns>The active rate as an integer.</returns>
        int GetActiveRate(bool normalizeTimeScale = true);

        /// <summary>
        /// Sets the active rate of the network element.
        /// </summary>
        /// <param name="value">The active rate to set.</param>
        void SetActiveRate(int value);

        /// <summary>
        /// Gets the passive rate of the network element.
        /// </summary>
        /// <returns>The passive rate as an integer.</returns>
        int GetPassiveRate(bool normalizeTimeScale = true);

        /// <summary>
        /// Sets the passive rate of the network element.
        /// </summary>
        /// <param name="value">The passive rate to set.</param>
        void SetPassiveRate(int value);

        /// <summary>
        /// Determines if the network element is a player.
        /// </summary>
        /// <returns>True if the network element is a player; otherwise, false.</returns>
        bool IsPlayer();

        /// <summary>
        /// Enable the minimum send rate
        /// </summary>
        /// <param name="enabled">Minimun rate is enabled</param>
        void SetEnableMinimunRate(bool enabled);

        /// <summary>
        /// Return gi minimum rate is enabled
        /// </summary>
        /// <returns>True if is enabled, otherwise false</returns>
        bool IsToEnableMinimunRate();

        /// <summary>
        /// Configure minimum rate os sending messages
        /// </summary>
        /// <param name="minRateValue">Minimum rate value</param>
        public void SetMinimunRateValue(int minRateValue);
        
        /// <summary>
        /// Sets the computation target to the local device.
        /// </summary>
        /// <param name="value">The value indicating whether to compute on the local device.</param>
        void SetToComputeLocalDevice(bool value);

        /// <summary>
        /// Sets the computation target to a remote device.
        /// </summary>
        /// <param name="value">The value indicating whether to compute on a remote device.</param>
        void SetToComputeRemoteDevice(bool value);

        /// <summary>
        /// Registers a behavior with the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="E">The type of the data stream.</typeparam>
        /// <param name="behavior">The behavior to register.</param>
        void RegisterBehavior<T, E>(INetworkEntity<T, E> behavior) where E : IDataStream;

        /// <summary>
        /// Unregisters a behavior from the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity to unregister.</typeparam>
        void UnregisterBehavior<T>() where T : IEntity;

        /// <summary>
        /// Checks if a behavior is registered with the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity to check.</typeparam>
        /// <returns>True if the behavior is registered; otherwise, false.</returns>
        bool HasBehavior<T>() where T : IEntity;

        /// <summary>
        /// Gets a registered behavior from the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity to get.</typeparam>
        /// <returns>The behavior as an entity of type T.</returns>
        T GetBehavior<T>() where T : IEntity;

        /// <summary>
        /// Flag all behaviours as updated, this will ensure that client's will receive this object updated
        /// </summary>
        void UpdateBehaviors();

        /// <summary>
        /// Registers a device behavior with the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="E">The type of the data stream.</typeparam>
        /// <param name="behavior">The device behavior to register.</param>
        void RegisterDeviceBehavior<T, E>(INetworkEntity<T, E> behavior) where E : IDataStream;

        /// <summary>
        /// Unregisters a device behavior from the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity to unregister.</typeparam>
        void UnregisterDeviceBehavior<T>() where T : IEntity;

        /// <summary>
        /// Checks if a device behavior is registered with the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity to check.</typeparam>
        /// <returns>True if the device behavior is registered; otherwise, false.</returns>
        bool HasDeviceBehavior<T>() where T : IEntity;

        /// <summary>
        /// Gets a registered device behavior from the network element.
        /// </summary>
        /// <typeparam name="T">The type of the entity to get.</typeparam>
        /// <returns>The device behavior as an entity of type T.</returns>
        T GetDeviceBehavior<T>() where T : IEntity;

        /// <summary>
        /// Checks if an event is registered with the network element.
        /// </summary>
        /// <param name="eventCode">The event code to check.</param>
        /// <returns>True if the event is registered; otherwise, false.</returns>
        bool HasEvent(int eventCode);

        /// <summary>
        /// Registers an event with a callback action to the network element.
        /// </summary>
        /// <param name="eventCode">The event code to register.</param>
        /// <param name="callBack">The callback action to invoke when the event occurs.</param>
        void RegisterEvent(int eventCode, Action<IDataStream> callBack);

        /// <summary>
        /// Unregisters an event from the network element.
        /// </summary>
        /// <param name="eventCode">The event code to unregister.</param>
        void UnregisterEvent(int eventCode);

        /// <summary>
        /// Invokes a registered event with the provided data stream.
        /// </summary>
        /// <param name="eventCode">The event code to invoke.</param>
        /// <param name="reader">The data stream associated with the event.</param>
        void InvokeEvent(int eventCode, IDataStream reader);
    }

}