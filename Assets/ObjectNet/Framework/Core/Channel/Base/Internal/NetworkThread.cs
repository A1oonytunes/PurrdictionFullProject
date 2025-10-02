using System;
using System.Threading;

namespace com.onlineobject.objectnet
{
    /// <summary>
    /// Represents a network thread that can be started, suspended, resumed, terminated, stopped, and force stopped.
    /// </summary>
    public class NetworkThread
    {

        private Thread dispatchThread; // The thread responsible for dispatching network operations.

        private Action onTerminateThread; // The action to be performed when the thread is terminated.

        private volatile bool suspend = false; // Flag indicating whether the thread is suspended.

        private volatile bool terminated = false; // Flag indicating whether the thread is terminated.

        /// <summary>
        /// Initializes a new instance of the NetworkThread class with the specified run action and optional onTerminate action.
        /// </summary>
        /// <param name="run">The action to be executed by the thread.</param>
        /// <param name="onTerminate">The action to be performed when the thread is terminated.</param>
        public NetworkThread(Action run, Action onTerminate = null)
        {
            this.dispatchThread = new Thread(new ThreadStart(run));
            this.onTerminateThread = onTerminate;
        }

        /// <summary>
        /// Starts the network thread.
        /// </summary>
        public void Start()
        {
            this.dispatchThread.Start();
        }

        /// <summary>
        /// Suspends the network thread.
        /// </summary>
        public void Suspend()
        {
            this.suspend = true;
        }

        /// <summary>
        /// Resumes the network thread.
        /// </summary>
        public void Resume()
        {
            this.suspend = false;
        }

        /// <summary>
        /// Checks if the network thread is suspended.
        /// </summary>
        /// <returns>True if the thread is suspended, otherwise false.</returns>
        public bool IsSuspended()
        {
            return this.suspend;
        }

        /// <summary>
        /// Terminates the network thread and performs the onTerminate action if provided.
        /// </summary>
        public void Terminate()
        {
            this.terminated = true;
            if (this.onTerminateThread != null)
            {
                this.onTerminateThread.Invoke();
            }
        }

        /// <summary>
        /// Stops the network thread by terminating it.
        /// </summary>
        public void Stop()
        {
            this.Terminate();
        }

        /// <summary>
        /// Forcefully stops the network thread by aborting the dispatch thread.
        /// </summary>
        public void ForceStop()
        {
            try
            {
                this.dispatchThread.Abort();
            }
            catch
            {
                // Do Nothing
            }
        }

        /// <summary>
        /// Checks if the network thread is terminated.
        /// </summary>
        /// <returns>True if the thread is terminated, otherwise false.</returns>
        public bool IsTerminated()
        {
            return this.terminated;
        }
    }
}
