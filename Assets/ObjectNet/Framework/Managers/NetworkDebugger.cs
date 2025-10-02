using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A singleton class for network debugging that allows for logging debug messages, errors, and warnings.
    /// </summary>
    public class NetworkDebugger {

        /// <summary>
        /// Flag to enable or disable console logging.
        /// </summary>
        public bool Console = true;

        /// <summary>
        /// Flag to enable or disable console logging on build.
        /// </summary>
        public bool OnBuild = true;

        /// <summary>
        /// Flag iof need to trace send and received messages
        /// </summary>
        public bool TraceMessages = false;

        /// <summary>
        /// Flag to enable or disable gizmos
        /// </summary>
        public bool ShowGizmos = true;

        /// <summary>
        /// Flag to enable or disable the network traffic statistics.
        /// </summary>
        public bool TrafficStatistics = false;

        /// <summary>
        /// The window to analise and consolidate network data
        /// </summary>
        public int NetworkTrafficWindow = 1;

        /// <summary>
        /// Store the Debugger manager responsible to handle with log
        /// </summary>
        private NetworkDebuggerManager debuggerManager;

        /// <summary>
        /// The amount of out data sent
        /// </summary>
        private int outputDataAmount = 0;

        /// <summary>
        /// The amount of in data received
        /// </summary>
        private int inputDataAmount = 0;

        /// <summary>
        /// The total of out data sent
        /// </summary>
        private int totalOutputDataAmount = 0;

        /// <summary>
        /// The total of in data received
        /// </summary>
        private int totalInputDataAmount = 0;

        /// <summary>
        /// The last amount of out data sent
        /// </summary>
        private int lastOutputDataAmount = 0;

        /// <summary>
        /// The last amount of in data received
        /// </summary>
        private int lastInputDataAmount = 0;

        /// <summary>
        /// Next time window reset for traffic analisys
        /// </summary>
        private float nextReset = 0f;

        /// <summary>
        /// The singleton instance of the NetworkDebugger.
        /// </summary>
        private static NetworkDebugger instance;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager"></param>
        public NetworkDebugger(NetworkDebuggerManager manager) {
            NetworkDebugger.instance = this;
            this.debuggerManager = manager;
        }

        /// <summary>
        /// Retrieves the singleton instance of the NetworkDebugger, creating it if it does not already exist.
        /// </summary>
        /// <returns>The singleton instance of the NetworkDebugger.</returns>
        public static NetworkDebugger Instance() {
            if (!InstanceExists()) {
                if (Application.isPlaying) {
                    NetworkDebugger.instance = new NetworkDebugger(null);                    
                }
            }
            return NetworkDebugger.instance;
        }

        /// <summary>
        /// Return if need to monitor network traffic statistics.
        /// </summary>
        /// <param name="enabled">If set to true, enables console logging.</param>
        public bool IsToMonitorNetwork() {
            return this.TrafficStatistics;
        }

        /// <summary>
        /// Checks if the singleton instance of the NetworkDebugger exists.
        /// </summary>
        /// <returns>True if the instance exists, false otherwise.</returns>
        private static bool InstanceExists() {
            return (NetworkDebugger.instance != null);
        }

        /// <summary>
        /// The last amount of out data sent
        /// </summary>
        /// <returns>Amount of data</returns>
        public int GetOutputDataAmount() {
            return this.lastOutputDataAmount;
        }

        /// <summary>
        /// The total of in data received
        /// </summary>
        /// <returns>Amount of data</returns>
        public int GetInputDataAmountTotal() {
            return this.totalInputDataAmount;
        }

        /// <summary>
        /// The total of out data sent
        /// </summary>
        /// <returns>Amount of data</returns>
        public int GetOutputDataAmountTotal() {
            return this.totalOutputDataAmount;
        }

        /// <summary>
        /// The last amount of in data received
        /// </summary>
        /// <returns>Amount of data</returns>
        public int GetInputDataAmount() {
            return this.lastInputDataAmount;
        }

        /// <summary>
        /// Logs a message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void Log(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }                
            } else {
                UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

        /// <summary>
        /// Logs a debug message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogDebug(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

        /// <summary>
        /// Logs an error message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log as an error.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogError(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.LogError((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.LogError((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
}

        /// <summary>
        /// Logs a warning message to the console if console logging is enabled.
        /// </summary>
        /// <param name="text">The text to log as a warning.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogWarning(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.LogWarning((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.LogWarning((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

        /// <summary>
        /// Logs some special traced code
        /// </summary>
        /// <param name="text">The text to log as a warning.</param>
        /// <param name="arguments">Optional arguments to format the text.</param>
        public static void LogTrace(string text, params object[] arguments) {
            if (NetworkDebugger.Instance() != null) {
                if ((NetworkDebugger.Instance().Console) || (NetworkDebugger.Instance().OnBuild)) {
                    UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
                }
            } else {
                UnityEngine.Debug.Log((arguments.Length > 0) ? string.Format(text, arguments) : text);
            }
        }

        /// <summary>
        /// Log network traffic
        /// </summary>
        /// <param name="inData">Input data</param>
        /// <param name="outData">Output data</param>
        public static void LogNetworkTraffic(byte[] inData, byte[] outData) {
            if (NetworkDebugger.Instance() != null) {
                if (NetworkDebugger.Instance().TrafficStatistics) {
                    if (inData != null) {
                        NetworkDebugger.Instance().inputDataAmount += inData.Length;
                    }
                    if (outData != null) {
                        NetworkDebugger.Instance().outputDataAmount += outData.Length;
                    }
                    if (NetworkDebugger.Instance().nextReset < Time.time) {
                        NetworkDebugger.Instance().nextReset             = (Time.time + (float)NetworkDebugger.Instance().NetworkTrafficWindow);
                        NetworkDebugger.Instance().totalInputDataAmount  += NetworkDebugger.Instance().inputDataAmount;
                        NetworkDebugger.Instance().totalOutputDataAmount += NetworkDebugger.Instance().outputDataAmount;
                        NetworkDebugger.Instance().lastInputDataAmount    = NetworkDebugger.Instance().inputDataAmount;
                        NetworkDebugger.Instance().lastOutputDataAmount   = NetworkDebugger.Instance().outputDataAmount;
                        NetworkDebugger.Instance().inputDataAmount        = 0;
                        NetworkDebugger.Instance().outputDataAmount       = 0;
                    }
                }
            }
        }
        
    }

}