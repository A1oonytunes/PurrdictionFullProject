using System.Collections;
using UnityEngine;

namespace com.onlineobject.objectnet.integration {
    /// <summary>
    /// Handles the UI display for network disconnection events within a Unity application.
    /// </summary>
    public class UINetworkDisconnection : MonoBehaviour {

        /// <summary>
        /// The delay in seconds before the network disconnection detection routine checks the connection status.
        /// </summary>
        public float DetectionDelay;

        /// <summary>
        /// The UI GameObject that should be activated when a disconnection is detected.
        /// </summary>
        public GameObject DisconnectionRoot;

        /// <summary>
        /// A reference to the coroutine that is responsible for detecting network disconnections.
        /// </summary>
        private Coroutine detectionRoutine;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the detection routine.
        /// </summary>
        private void OnEnable() {
            // Initially, make sure the disconnection UI is not visible.
            this.DisconnectionRoot.SetActive(false);
            // Start the coroutine that detects network disconnections.
            this.detectionRoutine = StartCoroutine(DetectDisconnection(this.DetectionDelay));
        }

        /// <summary>
        /// Called when the script instance is being disabled or destroyed.
        /// Stops the detection routine if it is running.
        /// </summary>
        private void OnDisable() {
            // If the detection coroutine is running, stop it.
            if (this.detectionRoutine != null) {
                StopCoroutine(this.detectionRoutine);
                this.detectionRoutine = null;
            }
        }

        /// <summary>
        /// A coroutine that periodically checks for network disconnections.
        /// </summary>
        /// <param name="interval">The time in seconds to wait between each check.</param>
        /// <returns>An IEnumerator needed for coroutine execution.</returns>
        private IEnumerator DetectDisconnection(float interval) {
            // Loop indefinitely to check for disconnection status.
            while (true) {
                // Check if the NetworkManager instance exists and has a client connection.
                if (NetworkManager.Instance() != null) {
                    if (NetworkManager.Instance().HasConnection(ConnectionType.Client)) {
                        if (NetworkManager.Instance().IsClientConnection()) {
                            // Activate the disconnection UI if the client is disconnected and a disconnection has been detected.
                            this.DisconnectionRoot.SetActive(NetworkManager.Instance().IsDisconnectionDetected() &&
                                                             !NetworkManager.Instance().IsConnected());
                        }
                    }
                }
                // Wait for the specified interval before checking again.
                yield return new WaitForSeconds(interval);
            }
        }
    }

}
