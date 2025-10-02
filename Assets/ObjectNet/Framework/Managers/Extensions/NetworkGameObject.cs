using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A static class for handling network instantiation of GameObjects.
    /// </summary>
    public static class NetworkGameObject {

        static volatile uint uuidGenerator = 0;

        static volatile object transactionLock = new object();

        static volatile Dictionary<uint, GameObject> waitingOperations = new Dictionary<uint, GameObject>();

        static float instantiateTimeout = NetworkGameObject.WAIT_ANSWER_TIME;

        const float WAIT_ANSWER_TIME = 1.0f;

        public static void ConfigureAsyncTimeout(float timeout) {
            NetworkGameObject.instantiateTimeout = timeout;
        }

        public static void RegisterAsyncResult(uint transactionId, GameObject result) {
            lock (NetworkGameObject.transactionLock) {
                NetworkGameObject.waitingOperations[transactionId] = result;
            }
        }

        public static bool IsWaitingAsyncResultWait(uint transactionId) {
            lock (NetworkGameObject.transactionLock) {
                return NetworkGameObject.waitingOperations.ContainsKey(transactionId);
            }
        }

        public static bool AsyncResultReceived(uint transactionId) {
            lock (NetworkGameObject.transactionLock) {
                return NetworkGameObject.waitingOperations.ContainsKey(transactionId) &&
                       NetworkGameObject.waitingOperations[transactionId] != null;
            }
        }

        private static uint RegisterAsyncOperation() {
            lock (NetworkGameObject.transactionLock) {
                uint transaction = ++NetworkGameObject.uuidGenerator;
                NetworkGameObject.waitingOperations.Add(transaction, null);
                return transaction;
            }
        }

        private static GameObject GetAsyncResultReceived(uint transactionId) {
            lock (NetworkGameObject.transactionLock) {
                return NetworkGameObject.waitingOperations.ContainsKey(transactionId) ? NetworkGameObject.waitingOperations[transactionId] : null;
            }
        }

        private static void CancelAsyncResultWait(uint transactionId) {
            lock (NetworkGameObject.transactionLock) {
                NetworkGameObject.waitingOperations.Remove(transactionId);
            }
        }

        /// <summary>
        /// Instantiates a GameObject over the network and sets its position, rotation, and scale.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <param name="position">The position at which to instantiate the GameObject.</param>
        /// <param name="rotation">The rotation at which to instantiate the GameObject.</param>
        /// <param name="scale">The scale at which to instantiate the GameObject.</param>
        /// <param name="newInstance">The instantiated GameObject.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        private static bool NetworkInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, out GameObject newInstance, uint transactionId = 0) {
            bool result = false;
            newInstance = null;

            // Check if the NetworkManager is not available or if the logic is running locally
            if ((NetworkManager.Instance() == null) ||
                (NetworkManager.Instance().IsRunningLogic() == true)) {
                // Instantiate the GameObject locally
                newInstance = GameObject.Instantiate(prefab, position, rotation);
                newInstance.transform.localScale = scale;
                result = true;
            } else {
                // Instantiate the GameObject over the network
                NetworkInstantiateDetection prefabDetector = prefab.GetComponent<NetworkInstantiateDetection>();
                if (prefabDetector != null) {
                    using (DataStream writer = new DataStream()) {
                        // Write connection ID of client that spawner this object
                        writer.Write(NetworkManager.Instance().GetConnection().GetSocket().GetConnectionID());
                        // Send transaction ID in case of need to answer when this object was created on the client side
                        writer.Write(transactionId);
                        // Write prefab signature, position, rotation, and scale to the data stream
                        writer.Write(prefabDetector.GetPrefabSignature()); // Prefab ID
                        writer.Write(position); // Spawn position
                        writer.Write(rotation.eulerAngles); // Spawn rotation
                        writer.Write(scale); // Spawn scale
                        // Send the network spawn event with the data stream
                        NetworkManager.Instance().Send(CoreGameEvents.NetworkSpawn, writer, DeliveryMode.Reliable);
                        result = true;
                    }
                }
            }
            return result;
        }
        
        /// <summary>
        /// Instantiates a GameObject over the network and sets its position, rotation, and scale.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <param name="position">The position at which to instantiate the GameObject.</param>
        /// <param name="rotation">The rotation at which to instantiate the GameObject.</param>
        /// <param name="scale">The scale at which to instantiate the GameObject.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        public static bool NetworkInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale) {
            return NetworkGameObject.NetworkInstantiate(prefab, position, rotation, scale, out GameObject nullResult);
        }

        /// <summary>
        /// Instantiates a GameObject over the network and sets its position and rotation.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <param name="position">The position at which to instantiate the GameObject.</param>
        /// <param name="rotation">The rotation at which to instantiate the GameObject.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        public static bool NetworkInstantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
            return NetworkGameObject.NetworkInstantiate(prefab, position, rotation, prefab.transform.transform.localScale, out GameObject nullResult);
        }

        /// <summary>
        /// Instantiates a GameObject over the network and sets its position.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <param name="position">The position at which to instantiate the GameObject.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        public static bool NetworkInstantiate(GameObject prefab, Vector3 position) {
            return NetworkGameObject.NetworkInstantiate(prefab, position, prefab.transform.transform.rotation, prefab.transform.transform.localScale, out GameObject nullResult);
        }

        /// <summary>
        /// Instantiates a GameObject over the network using its default position and rotation.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        public static bool NetworkInstantiate(GameObject prefab) {
            return NetworkGameObject.NetworkInstantiate(prefab, prefab.transform.transform.position, prefab.transform.transform.rotation, prefab.transform.transform.localScale, out GameObject nullResult);
        }


        /// <summary>
        /// Instantiates a GameObject over the network and sets its position, rotation, and scale.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <param name="position">The position at which to instantiate the GameObject.</param>
        /// <param name="rotation">The rotation at which to instantiate the GameObject.</param>
        /// <param name="scale">The scale at which to instantiate the GameObject.</param>
        /// <returns>Instance of remote created game object, false null.</returns>
        public static async Task<GameObject> Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale) {
            GameObject  resultObject        = null;
            uint        waitingTransaction  = NetworkGameObject.RegisterAsyncOperation();
            bool        result              = NetworkGameObject.NetworkInstantiate(prefab, position, rotation, scale, out resultObject, waitingTransaction);
            float       timeout             = Time.time + NetworkGameObject.instantiateTimeout;
            if (resultObject == null) {
                if (result) {
                    try {
                        do {
                            await Task.Delay(10);
                        } while ((NetworkGameObject.AsyncResultReceived(waitingTransaction) == false) && (timeout > Time.time));
                        // Update result
                        resultObject = NetworkGameObject.GetAsyncResultReceived(waitingTransaction);
                    } finally {
                        NetworkGameObject.CancelAsyncResultWait(waitingTransaction);
                    }
                }
            }
            return resultObject;
        }

        /// <summary>
        /// Instantiates a GameObject over the network and sets its position and rotation.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <param name="position">The position at which to instantiate the GameObject.</param>
        /// <param name="rotation">The rotation at which to instantiate the GameObject.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        public static async Task<GameObject> Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
            return await NetworkGameObject.Instantiate(prefab, position, rotation, prefab.transform.transform.localScale);
        }

        /// <summary>
        /// Instantiates a GameObject over the network using its default position and rotation.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        public static async Task<GameObject> Instantiate(GameObject prefab) {
            return await NetworkGameObject.Instantiate(prefab, prefab.transform.transform.position, prefab.transform.transform.rotation, prefab.transform.transform.localScale);
        }

        /// <summary>
        /// Instantiates a GameObject over the network and sets its position.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to instantiate.</param>
        /// <param name="position">The position at which to instantiate the GameObject.</param>
        /// <returns>True if the instantiation was successful, false otherwise.</returns>
        public static async Task<GameObject> Instantiate(GameObject prefab, Vector3 position) {
            return await NetworkGameObject.Instantiate(prefab, position, prefab.transform.transform.rotation, prefab.transform.transform.localScale);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static void NetworkDestroy(GameObject obj) {
            // Check if the NetworkManager is not available or if the logic is running locally
            if ((NetworkManager.Instance() == null) ||
                (NetworkManager.Instance().IsRunningLogic() == true)) {
                // Destroy the GameObject locally
                GameObject.Destroy(obj);
            } else {
                // Get object to destroy
                NetworkObject networkObjectToDestroy = obj.GetComponent<NetworkObject>();
                if (networkObjectToDestroy != null) {
                    using (DataStream writer = new DataStream()) {
                        // Write connection ID of client that spawner this object
                        writer.Write(NetworkManager.Instance().GetConnection().GetSocket().GetConnectionID());
                        writer.Write(networkObjectToDestroy.GetNetworkId()); // Send network id to be destroyed
                        // Send the network spawn event with the data stream
                        NetworkManager.Instance().Send(CoreGameEvents.NetworkDestroy, writer, DeliveryMode.Reliable);
                        
                    }
                } else {
                    // If isn't a network obejct i'm going to destroy anyway
                    GameObject.Destroy(obj);
                }
            }
        }
    }
}
