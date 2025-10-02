using System;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// This class represents a prefab entry to store events on database prefabs
    /// </summary>
    [Serializable]
    public class NetworkPrefabsEventsEntry : System.Object {

        // Event triggered when the object is spawned on the client side.
        [EventInformations(EventName = "OnPrefabSpawn", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(NetworkObject) }, EventDescriptiom = "Trigger when this object was spawned")]
        [SerializeField]
        public EventReferencePrefab onSpawnPrefab;

        [EventInformations(EventName = "OnPrefabDespawn", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(NetworkObject) }, EventDescriptiom = "Trigger when this object was despawned")]
        [SerializeField]
        public EventReferencePrefab onDespawnPrefab;

        [EventInformations(EventName = "AcceptReceiveOwnerShip", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(NetworkObject) }, ReturnType = typeof(bool), EventDescriptiom = "Validate if player can accept be the owner os some object")]
        [SerializeField]
        public EventReferencePrefab onAcceptOwnerShip;

        [EventInformations(EventName = "AcceptReleaseOwnerShip", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(NetworkObject) }, ReturnType = typeof(bool), EventDescriptiom = "Validate if player can accept be the release of some object ownership")]
        [SerializeField]
        public EventReferencePrefab onAcceptReleaseOwnerShip;

        [EventInformations(EventName = "OnTakeOwnerShip", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(NetworkObject) }, EventDescriptiom = "Trigger when player receive ownership of some object")]
        [SerializeField]
        public EventReferencePrefab onTakeObjectOwnerShip;

        [EventInformations(EventName = "OnReleaseOwnerShip", ExecutionSide = EventReferenceSide.BothSides, ParametersType = new Type[] { typeof(NetworkObject) }, EventDescriptiom = "Trigger when player has his ownership of some object released")]
        [SerializeField]
        public EventReferencePrefab onReleaseObjectOwnerShip;

    }
}
