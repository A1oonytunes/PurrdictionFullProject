using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Manages the network debugging settings and applies them to the NetworkDebugger instance.
    /// </summary>
    public class NetworkDebuggerManager : MonoBehaviour {

        // Flag is this object shall keep persistent between scenes
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        // Indicates whether console logging is enabled or not.
        [SerializeField]
        private bool EnableConsoleLog = false; // Default is false, can be set in the Unity Inspector

        // Indicates whether console logging will appear on compiled vuild
        [SerializeField]
        private bool EnableOnBuild = false; // Default is false, can be set in the Unity Inspector

        // Indicated if need to capture warnings
        [SerializeField]
        private bool CaptureWarnings = true;

        // Indicated if need to capture errors
        [SerializeField]
        private bool CaptureErrors = true;

        // Indicated if need to capture logs
        [SerializeField]
        private bool CaptureLogs = true;

        // Indicated if need to disable objectnet gizmos
        [SerializeField]
        private bool ShowGizmos = true;

        // Indicated if need to disable network traffic monitoring
        [SerializeField]
        private bool NetworkTraffic = false;

        /// The window to analise and consolidate network data
        [SerializeField]
        public int NetworkTrafficWindow = 1;

        // Reference to the NetworkDebugger instance
        private NetworkDebugger Debugger;

        private static NetworkDebuggerManager instance;

        // Store message in wueue when "EnableOnBuild" is active
        private Queue InternalLogQueue = new Queue();

        private GUIStyle LogStyle = null;

        const uint QUEUE_SIZE = 100;  // number of messages to keep

        const uint KILOBYTE = 1024;  // number of bytes in 1K

        const uint MEGABYTE = 1024 * 1000;  // number of bytes in 1Mb

        const uint GIGABYTE = 1024 * 1000 * 1000;  // number of bytes in 1Gb

        /// <summary>
        /// Provides access to the singleton instance of the NetworkDebuggerManager.
        /// </summary>
        /// <returns>The singleton instance of the NetworkDebuggerManager.</returns>
        public static NetworkDebuggerManager Instance() {
            // Return the singleton instance
            return NetworkDebuggerManager.instance;
        }

        /// <summary>
        /// Detect ig another NetworkDebuggerManager is already instantiated
        /// </summary>
        /// <returns>True if NetworkDebuggerManager already exists, otherwise false</returns>
        public bool DetectDuplicated() {
            return (NetworkDebuggerManager.instance != null);
        }

        /// <summary>
        /// Flag current instance of NetworkDebuggerManager as the in use instance
        /// </summary>
        private void SetInstance() {
            NetworkDebuggerManager.instance = this;
        }


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Initializes the network manager, sets up prefabs, servers, and NAT traversal if necessary.
        /// </summary>
        void Awake() {
            if ((this.dontDestroyOnLoad) &&
                (this.DetectDuplicated())) {
                DestroyImmediate(this.gameObject);
            } else {
                // Flag on base class
                if (this.dontDestroyOnLoad) {
                    DontDestroyOnLoad(this);
                }
                this.SetInstance(); // Flag instance to be the current object                
            }
        }

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the NetworkDebugger instance with the console logging setting.
        /// </summary>
        private void Start() {
            // Initiate NetworkDebugger element
            if (NetworkDebugger.Instance() == null) {
                this.Debugger = new NetworkDebugger(this);
            } else {
                this.Debugger = NetworkDebugger.Instance();
            }
            // Set the console logging option of the NetworkDebugger instance based on the serialized field
            NetworkDebugger.Instance().Console              = this.EnableConsoleLog;
            // Set the console logging option of the NetworkDebugger instance based on the serialized field
            NetworkDebugger.Instance().OnBuild              = this.EnableOnBuild;
            // Flag if need to show gizmo
            NetworkDebugger.Instance().ShowGizmos           = this.ShowGizmos;
            // Flag if need to monitor traffic
            NetworkDebugger.Instance().TrafficStatistics    = this.NetworkTraffic;
            // Flag if need to monitor traffic
            NetworkDebugger.Instance().NetworkTrafficWindow = this.NetworkTrafficWindow;
        }

        void OnEnable() {
            if (this.EnableOnBuild) {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
                Application.logMessageReceived += HandleLog;
            }
        }

        void OnDisable() {
            if (this.EnableOnBuild) {                
                Application.logMessageReceived -= HandleLog;
            }
        }

        void HandleLog(string logString, string stackTrace, LogType type) {
            this.InternalLogQueue.Enqueue(string.Format("[{0}] {1} : {2}", System.DateTime.Now.ToString("HH:mm:ss.fff"), type.ToString().ToUpper(), logString));
            if (((type == LogType.Log) && (this.CaptureLogs)) ||
                ((type == LogType.Warning) && (this.CaptureWarnings)) ||
                (((type == LogType.Exception) || (type == LogType.Error)) && (this.CaptureErrors))) {
                this.InternalLogQueue.Enqueue(stackTrace);
            }
            while (this.InternalLogQueue.Count > QUEUE_SIZE) {
                this.InternalLogQueue.Dequeue();
            }
        }

        void OnGUI() {
            this.InitStyles();
            if (this.EnableOnBuild) {
                GUILayout.BeginArea(new Rect(Screen.width - (Screen.width / 2.0f), 10, (Screen.width / 2.0f) - 10, Screen.height - 20), this.LogStyle);
                GUILayout.Label("\n" + string.Join("\n", this.InternalLogQueue.ToArray().Reverse()));
                GUILayout.EndArea();
            }
            if ( this.NetworkTraffic ) {
                GUILayout.BeginArea(new Rect(10f, 10f, 250, 225), this.LogStyle);
                int inDataSize  = NetworkDebugger.Instance().GetInputDataAmount();
                int outDataSize = NetworkDebugger.Instance().GetOutputDataAmount();

                int inDataSizeTotal = NetworkDebugger.Instance().GetInputDataAmountTotal();
                int outDataSizeTotal = NetworkDebugger.Instance().GetOutputDataAmountTotal();

                GUILayout.Label(String.Format("On last {0} seconds", this.NetworkTrafficWindow));
                GUILayout.Label(String.Format("Received : {0} {1}",
                                              ((inDataSize < KILOBYTE) ? inDataSize : (inDataSize < MEGABYTE) ? this.ByteToKiloByte(inDataSize) : this.ByteToMegaByte(inDataSize)),
                                              ((inDataSize < KILOBYTE) ? "bytes" : (inDataSize < MEGABYTE) ? "K" : "MB")));
                GUILayout.Label(String.Format("Sent : {0} {1}",
                                              ((outDataSize < KILOBYTE) ? outDataSize : (outDataSize < MEGABYTE) ? this.ByteToKiloByte(outDataSize) : this.ByteToMegaByte(outDataSize)),
                                              ((outDataSize < KILOBYTE) ? "bytes" : (outDataSize < MEGABYTE) ? "K" : "MB")));

                GUILayout.Label("Total of traffic");
                GUILayout.Label(String.Format("Received : {0} {1}",
                                              ((inDataSizeTotal < KILOBYTE) ? inDataSizeTotal : (inDataSizeTotal < MEGABYTE) ? this.ByteToKiloByte(inDataSizeTotal) : this.ByteToMegaByte(inDataSizeTotal)),
                                              ((inDataSizeTotal < KILOBYTE) ? "bytes" : (inDataSizeTotal < MEGABYTE) ? "K" : "MB")));
                GUILayout.Label(String.Format("Sent : {0} {1}",
                                              ((outDataSizeTotal < KILOBYTE) ? outDataSizeTotal : (outDataSizeTotal < MEGABYTE) ? this.ByteToKiloByte(outDataSizeTotal) : this.ByteToMegaByte(outDataSizeTotal)),
                                              ((outDataSizeTotal < KILOBYTE) ? "bytes" : (outDataSizeTotal < MEGABYTE) ? "K" : "MB")));

                GUILayout.Label("\n" + string.Join("\n", this.InternalLogQueue.ToArray().Reverse()));
                GUILayout.EndArea();
            }
        }

        private void InitStyles() {
            if (this.LogStyle == null) {
                this.LogStyle = new GUIStyle(GUI.skin.box);
                Color bgColor = new Color(Color.black.r, Color.black.g, Color.black.b, 0.45f);
                this.LogStyle.normal.background = MakeTex((Screen.width / 2) - 10, Screen.height - 20, bgColor);
            }
        }

        private Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private float ByteToKiloByte(float dataSize) {
            float data = dataSize / KILOBYTE;
            float multiplier = data < 10 ? 100 : data < 100 ? 10 : 1; //ensure only 3 decimals are showing
            float decimals = MathF.Round(data * multiplier);
            return decimals / multiplier;
        }

        private float ByteToMegaByte(float dataSize) {
            float data = dataSize / MEGABYTE;
            float multiplier = data < 10 ? 100 : data < 100 ? 10 : 1; // ensure only 3 decimals are showing
            float decimals = MathF.Round(data * multiplier);
            return decimals / multiplier;
        }
    }
}