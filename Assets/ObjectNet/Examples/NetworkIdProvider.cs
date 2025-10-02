using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class NetworkIdProvider : MonoBehaviour, IInformationProvider {

        [Serializable]
        public class NetworkIdByUser {
            public String   UserName;
            public int      NetworkId;
        }

        public int FixeNetworkId = 1;

        public List<NetworkIdByUser> NetworkIdMap = new List<NetworkIdByUser>();

        public void ConsumeLoginFailed(Exception error) {
            NetworkDebugger.LogError(String.Format("Login error intercepted : {0}", error.Message));
        }

        public int GetNetworkId(params object[] arguments) {
            return this.FixeNetworkId;
        }

        public int GetNetworkIdFromList(params object[] arguments) {
            int result = this.FixeNetworkId;
            string userName = (arguments[0] as string); // First argument into list is user id 
            foreach(NetworkIdByUser user in this.NetworkIdMap) {
                if (user.UserName.ToUpper().Equals(userName.ToUpper())) {
                    result = user.NetworkId;
                    break;
                }
            }
            return result;
        }

        public int GetNetworkIdFromWebService(params object[] arguments) {
            /*
            ServerAddressEntry[] servers = null;
            WebRequest request = WebRequest.Create("http://your-webservice.com/index.asp");
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());   
            string jsonContent = reader.ReadLine();  
            if (!String.IsNullOrEmpty(jsonContent)) {
                // json shall contains: '[{"Address": "127.0.0.1","Port": 50001}, {"Address": "127.0.0.2","Port": 50001}]'
                servers = JsonUtility.FromJson<ServerAddressEntry[]>(jsonContent);
            } else {
                throw new Exception("There's no avaiable servers to connect");
            }
            */
            return this.FixeNetworkId;
        }

        public int GetNetworkIdFromDatabase(params object[] arguments) {
            /*
            List<ServerAddressEntry> servers = new List<ServerAddressEntry>();
            //
            // Connect to any database and request list of servers and fill return array
            //
            return servers.ToArray<ServerAddressEntry>();
            */
            return this.FixeNetworkId;
        }

        public int GetNetworkIdFromSaveFile(params object[] arguments) {
            /*
            List<ServerAddressEntry> servers = new List<ServerAddressEntry>();
            //
            // Connect to any database and request list of servers and fill return array
            //
            return servers.ToArray<ServerAddressEntry>();
            */
            return this.FixeNetworkId;
        }
    }
}
