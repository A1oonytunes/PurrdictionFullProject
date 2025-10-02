using System;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class EventsExampleProvider : MonoBehaviour {

        public bool EnableConsoleLogs = false;

        public void OnConnected(IClient client) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("Connection stablished sucesfully [{0}]", client.GetIpPort()));
            }
        }

        public void OnDisconnected(IClient client) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("Connection lost [{0}]", client.GetIpPort()));
            }
        }

        public void OnLoginFailed(Exception error) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("Login failed [{0}]", error.Message));
            }
        }

        public void OnLoginSucess(IClient client) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log("Login sucess");
            }
        }

        public void OnConnectionFailed(Exception error) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("Connection with server failed [{0}]", error.Message));
            }
        }

        public void OnClientConnectedOnServer(IClient client) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("New client connected [{0}]", client.GetIpPort()));
            }
        }

        public void OnClientDisconnectedOnServer(IClient client) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("Client disconnected[{0}]", client.GetIpPort()));
            }
        }

        public void OnClientLoginFailed(IClient client) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("Client login failed [{0}]", client.GetIpPort()));
            }
        }

        public void OnClientLoginSucess(IClient client) {
            if (EnableConsoleLogs) {
                NetworkDebugger.Log(string.Format("Client login sucess [{0}]", client.GetIpPort()));
            }
        }

        public void OnMessageReceived(IDataStream reader) {
            if (EnableConsoleLogs) {
                IClient client = (reader as INetworkStream).GetClient();
                NetworkDebugger.Log(string.Format("Message received from client [{0}]", (client != null) ? client.GetIpPort() : "unknow"));
            }
        }

        public String GetTeste() {
            return "";
        }

    }
}