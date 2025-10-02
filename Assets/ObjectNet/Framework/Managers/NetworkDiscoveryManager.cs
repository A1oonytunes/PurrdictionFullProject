#pragma warning disable 0168
#pragma warning disable 0219
#pragma warning disable 0414

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace com.onlineobject.objectnet {
    public class NetworkDiscoveryManager : MonoBehaviour {

        [SerializeField]
        private bool IsServer = true;

        [SerializeField]
        private bool IsClient = false;

        [SerializeField]
        private bool AutoStart = false;

        [SerializeField]
        private int AutoStartDelay = 5;

        [SerializeField]
        private bool Started = false;

        [SerializeField]
        private bool ShowLogs = false;

        [SerializeField]
        private bool SimplePooling = true;

        [SerializeField]
        private bool FullPooling = false;

        [SerializeField]
        private bool Error = false;

        [SerializeField]
        private int BroastCastPort = PLAYER_MULTICAST_PORT;

        [SerializeField]
        private int BroadcastInterval = 5;

        [SerializeField]
        private List<string> avaiableClients = new List<string>();

        [SerializeField]
        private List<string> avaiableServers = new List<string>();

        // Event triggered when the object is spawned on the client side.
        [EventInformations(EventName = "OnServerDiscovered", ExecutionSide = EventReferenceSide.ClientSide, ParametersType = new Type[] { typeof(string) }, EventDescriptiom = "Trigger when a new server was discovered")]
        [SerializeField]
        public EventReference OnServerDiscovered;

        private bool Initialized = false;

        private string LocalIp;

        private UdpClient udpServer = null;

        private UdpClient udpClient = null;

        private IPEndPoint serverEndPoint = null;

        private IPEndPoint clientEndPoint = null;

        private float sendInterval = 0f;

        private bool isListening = false;

        public const float CLIENT_LISTEN_INTERVAL = 1f;

        public const float SERVER_SEND_INTERVAL = 1f;

        public const int PLAYER_MULTICAST_PORT = 11000;

        void Start() {
            if ( this.AutoStart == true ) {
                StartCoroutine(StartDelayed((float)this.AutoStartDelay));
            }
        }

        IEnumerator StartDelayed(float delay) {
            yield return new WaitForSeconds(delay);
            if ( this.IsServer ) {
                this.StartServer();
            } else if (this.IsClient) {
                this.StartClient();
            }
        }

        public void StartServer() {
            if (this.Initialized == false) {
                this.IsServer = true;
                this.IsClient = false;
                this.Started = true;
            }
        }

        public void StartClient() {
            if (this.Initialized == false) {
                this.IsServer = false;
                this.IsClient = true;
                this.Started = true;
            }
        }
        
        public void Stop() {

        }

        public string GetAvaiableServer() {
            string result = null;
            if ( this.avaiableServers.Count > 0 ) {
                result = this.avaiableServers[0];
            }
            return result;
        }

        public bool GetAvaiableServer(out string serverAddress) {
            bool result = false;
            serverAddress = null;
            if (this.avaiableServers.Count > 0) {
                serverAddress = this.avaiableServers[0];
            }
            return result;
        }

        public string[] GetAvaiableServers() {
            return this.avaiableServers.ToArray<string>();
        }

        public string[] GetAvaiableClients() {
            return this.avaiableClients.ToArray<string>();
        }

        void Update() {
            if ( this.Started == true ) {
                if (string.IsNullOrEmpty(this.LocalIp)) {
                    if (NetworkManager.Instance() != null) {
                        this.LocalIp = NetworkManager.Instance().GetPrivateIp();
                    }
                }
                if (string.IsNullOrEmpty(this.LocalIp) == false) {
                    if (this.Initialized == false) {
                        this.Initialized = true;
                        if (this.IsServer) {
                            this.InitializeServer();
                        } else if (this.IsClient) {
                            this.InitializeClient();
                        }
                        if (this.ShowLogs) {
                            NetworkDebugger.Log("[{0}] Discovery system started at {1}", (this.IsServer) ? "Server" : "Client", Time.time);
                        }
                    } else {
                        if (this.IsServer) {
                            if (this.sendInterval < Time.time) {
                                this.sendInterval = (Time.time + ((float)this.BroadcastInterval));
                                this.BroadcastServer();
                            }
                        } else if (this.IsClient) {
                            if (this.isListening == false) {
                                this.ListenClient();
                            }
                        }
                    }
                }
            }
        }

        private void InitializeServer() {
            this.udpServer = new UdpClient();
            this.udpServer.EnableBroadcast = true;
            this.udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);            
            this.serverEndPoint = new IPEndPoint(IPAddress.Any, PLAYER_MULTICAST_PORT);            
        }

        private void BroadcastServer() {
            var data = Encoding.UTF8.GetBytes(this.LocalIp);
            string[] ipOctects = this.LocalIp.Split('.');
            // Send an message to all nodes on the same network
            if (this.FullPooling == true) {
                for (int thirdNodeIndex = 0; thirdNodeIndex < 255; thirdNodeIndex++) {
                    for (int lastNodeIndex = 0; lastNodeIndex < 255; lastNodeIndex++) {
                        this.udpServer.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", ipOctects[0], ipOctects[1], thirdNodeIndex, lastNodeIndex)), PLAYER_MULTICAST_PORT));
                    }
                }
            } else if (this.SimplePooling == true) {
                for (int lastNodeIndex = 0; lastNodeIndex < 255; lastNodeIndex++) {
                    this.udpServer.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", ipOctects[0], ipOctects[1], ipOctects[2], lastNodeIndex)), PLAYER_MULTICAST_PORT));
                }
            }
        }

        private void InitializeClient() {
            try {
                this.udpClient = new UdpClient(PLAYER_MULTICAST_PORT);
                this.clientEndPoint = new IPEndPoint(IPAddress.Any, PLAYER_MULTICAST_PORT);
            } catch(Exception err) {
                NetworkDebugger.Log("Discovery client system doesn't support multiple game instances on the same hardware. Discovery will be disabled");
                NetworkDebugger.Log(err.Message);
                this.enabled = false;
            }
        }

        private void ListenClient() {
            this.isListening = true;
            this.udpClient.BeginReceive(new System.AsyncCallback(receivePacket), new object());            
        }

        private void receivePacket(IAsyncResult res) {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, PLAYER_MULTICAST_PORT);
            byte[] recvPacket = this.udpClient.EndReceive(res, ref remote);
            string receivedAddress = Encoding.ASCII.GetString(recvPacket);
            if (this.avaiableServers.Contains(receivedAddress) == false) {
                if (this.ShowLogs) {
                    NetworkDebugger.Log("New server discovered [{0}]", receivedAddress);
                }
                this.avaiableServers.Add(receivedAddress);
                if ( this.OnServerDiscovered != null ) {
                    NetworkManager.Instance().Enqueue(() => {
                        this.ExecuteOnServerDiscovered(receivedAddress);
                    });
                }
            }
            this.isListening = false;
        }

        private void ExecuteOnServerDiscovered(string address) {
            if (this.OnServerDiscovered != null) {
                if ((this.OnServerDiscovered.GetEventTarget()       != null) &&
                    (this.OnServerDiscovered.GetEventComponent()    != null) &&
                    (this.OnServerDiscovered.GetEventMethod()       != null)) {
                    MethodInfo executionMethod = this.OnServerDiscovered.GetEventComponent().GetType().GetMethod(this.OnServerDiscovered.GetEventMethod());
                    if (executionMethod != null) {
                        executionMethod.Invoke(this.OnServerDiscovered.GetEventComponent(), new object[] { address });                        
                    }
                }
            }
        }
    }
}