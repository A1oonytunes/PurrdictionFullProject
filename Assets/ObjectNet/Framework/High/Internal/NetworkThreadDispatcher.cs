using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Abstract class that handles network thread dispatching in a Unity MonoBehaviour context.
    /// </summary>
    public abstract class NetworkThreadDispatcher : MonoBehaviour {

        /// <summary>
        /// Property to get or set the network clock instance.
        /// </summary>
        protected INetworkClock NetworkClock { get { return this.internalNetworkClock; } set { this.internalNetworkClock = value; } }

        /// <summary>
        /// Property to get or set whether to use Unity coroutines for action execution.
        /// </summary>
        protected bool UseCoroutine { get { return internalUseCoroutine; } set { internalUseCoroutine = value; } }

        private INetworkClock internalNetworkClock; // Internal reference to the network clock.

        private bool internalUseCoroutine = false; // Internal flag to determine if coroutines should be used.

        // Queue to hold actions that need to be executed on the main thread.
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        // Sorted queue to hold actions that need to be executed after a delay.
        private static readonly SortedDictionary<float, Action> _delayedExecutionQueue = new SortedDictionary<float, Action>();

        /// <summary>
        /// Abstract method to check if there is a connection of a specific type.
        /// </summary>
        /// <param name="connectionType">The type of connection to check.</param>
        /// <returns>True if there is a connection, false otherwise.</returns>
        public abstract bool HasConnection(ConnectionType connectionType);

        /// <summary>
        /// Abstract method to get the network transport for a specific connection type.
        /// </summary>
        /// <param name="connectionType">The type of connection to get the transport for.</param>
        /// <returns>The network transport for the specified connection type.</returns>
        public abstract NetworkTransport GetConnection(ConnectionType connectionType);

        /// <summary>
        /// Return if has connection obejct
        /// </summary>
        /// <returns>True if has connection, otherwise false</returns>
        public abstract bool HasConnection();

        /// <summary>
        /// Abstract method to get the network transport for a specific connection type.
        /// </summary>
        /// <returns>The network transport for the specified connection type.</returns>
        public abstract NetworkTransport GetConnection();

        /// <summary>
        /// Abstract method to get the send rate for network synchronization.
        /// </summary>
        /// <returns>The send rate as an integer.</returns>
        public abstract int GetSendRate(bool normalizeTimeScale = true);

        /// <summary>
        /// Abstract method to get the minimum send rate for network synchronization.
        /// </summary>
        /// <returns>The send rate as an integer.</returns>
        public abstract int GetMinimumSendRate(bool normalizeTimeScale = true); 

        /// <summary>
        /// Abstract method to get the networking mode.
        /// </summary>
        /// <returns>The current network connection type.</returns>
        public abstract NetworkConnectionType GetNetworkingMode();

        /// <summary>
        /// Unity's Update method called every frame. Handles immediate execution of actions and delayed actions.
        /// </summary>
        public virtual void Update() {
            // Update network clock with internal Unity processing cycle
            this.internalNetworkClock.UpdateFramesCount(Time.frameCount);

            // Execute immediate actions
            lock (_executionQueue) {
                while (_executionQueue.Count > 0) {
                    _executionQueue.Dequeue().Invoke();
                }
            }

            // Execute delayed actions
            lock (_delayedExecutionQueue) {
                while ((_delayedExecutionQueue.Count > 0) &&
                       (_delayedExecutionQueue.Keys.First() < Time.time)) {
                    Action actionToExecute = _delayedExecutionQueue[_delayedExecutionQueue.Keys.First()];
                    _delayedExecutionQueue.Remove(_delayedExecutionQueue.Keys.First());
                    actionToExecute.Invoke();
                }
            }
        }

        /// <summary>
        /// Unity's FixedUpdate method called at a fixed time interval. Handles network clock updates and synchronization.
        /// </summary>
        public virtual void FixedUpdate() {
            // Update network clock ticks and fixed frame counts
            this.internalNetworkClock.UpdateTick();
            this.internalNetworkClock.UpdateFixedFramesCount();

            // On server, send tick synchronize to clients
            if (NetworkConnectionType.Server == this.GetNetworkingMode()) {
                if (this.HasConnection(ConnectionType.Server)) {
                    if (this.GetConnection(ConnectionType.Server).IsConnected()) {
                        this.SendClockSynchronize(this.internalNetworkClock.Tick);
                    }
                }
            }
        }

        /// <summary>
        /// Enqueues an IEnumerator action to be executed as a coroutine.
        /// </summary>
        /// <param name="action">The IEnumerator action to enqueue.</param>
        public void Enqueue(IEnumerator action) {
            lock (_executionQueue) {
                _executionQueue.Enqueue(() => {
                    StartCoroutine(action);
                });
            }
        }

        /// <summary>
        /// Enqueues an IEnumerator action to be executed as a coroutine after a delay.
        /// </summary>
        /// <param name="action">The IEnumerator action to enqueue.</param>
        /// <param name="delay">The delay in milliseconds before the action is executed.</param>
        public void Enqueue(IEnumerator action, float delay) {
            lock (_delayedExecutionQueue) {
                float executionTime = (internalNetworkClock.Time) + (delay / 1000.0f);
                while (_delayedExecutionQueue.ContainsKey(executionTime)) {
                    executionTime += (1f / 1000f); // Add 1 millisecond to find a gap
                }
                _delayedExecutionQueue.Add(executionTime, () => {
                    StartCoroutine(action);
                });
            }
        }

        /// <summary>
        /// Enqueues an Action to be executed, potentially as a coroutine if configured.
        /// </summary>
        /// <param name="action">The Action to enqueue.</param>
        public void Enqueue(Action action) {
            if (this.internalUseCoroutine) {
                Enqueue(ActionWrapper(action));
            } else {
                lock (_executionQueue) {
                    _executionQueue.Enqueue(action);
                }
            }

        }

        /// <summary>
        /// Enqueues an Action to be executed after a delay, potentially as a coroutine if configured.
        /// </summary>
        /// <param name="action">The Action to enqueue.</param>
        /// <param name="delay">The delay in milliseconds before the action is executed.</param>
        public void Enqueue(Action action, float delay) {
            Enqueue(ActionWrapper(action), delay);
        }

        /// <summary>
        /// Enqueues an Action to be executed asynchronously and returns a Task representing the operation.
        /// </summary>
        /// <param name="action">The Action to enqueue.</param>
        /// <returns>A Task that completes when the action has been executed.</returns>
        public Task EnqueueAsync(Action action) {
            var tcs = new TaskCompletionSource<bool>();

            void WrappedAction() {
                try {
                    action();
                    tcs.TrySetResult(true);
                } catch (Exception ex) {
                    tcs.TrySetException(ex);
                }
            }

            Enqueue(ActionWrapper(WrappedAction));
            return tcs.Task;
        }

        /// <summary>
        /// Wraps an Action into an IEnumerator suitable for coroutine execution.
        /// </summary>
        /// <param name="actionToExecute">The Action to wrap.</param>
        /// <returns>An IEnumerator that executes the Action and yields.</returns>
        private IEnumerator ActionWrapper(Action actionToExecute) {
            actionToExecute();
            yield return null;
        }

        /// <summary>
        /// Sends a clock synchronization message to connected clients if the current tick is a multiple of the send rate.
        /// </summary>
        /// <param name="tick">The current tick count.</param>
        private void SendClockSynchronize(int tick) {
            // Send one synchronize per second based on the send rate
            if ((tick % this.GetSendRate()) == 0) {
                using (DataStream writer = new DataStream()) {
                    writer.Write(tick);
                    this.GetConnection(ConnectionType.Server).Send(CoreGameEvents.SynchronizeTick, writer, DeliveryMode.Unreliable);
                }
            }
        }
    }

}