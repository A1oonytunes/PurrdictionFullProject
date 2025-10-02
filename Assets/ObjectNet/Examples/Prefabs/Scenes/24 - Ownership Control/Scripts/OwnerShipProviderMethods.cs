using UnityEngine;

namespace com.onlineobject.objectnet {
    public class OwnerShipProviderMethods : MonoBehaviour, IInformationProvider {

        /// <summary>
        /// test fi this object can be accepted
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns>True if can be accepted</returns>
        public bool CanAcceptTakeOwnerShip(NetworkObject obj) {
            NetworkDebugger.Log("CanAcceptTakeOwnerShip called");
            return true;
        }

        /// <summary>
        /// test fi this object can be accepted
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns>True if can be accepted</returns>
        public bool CanAcceptReleaseOwnerShip(NetworkObject obj) {
            NetworkDebugger.Log("CanAcceptReleaseOwnerShip called");
            return true;
        }
    }
}