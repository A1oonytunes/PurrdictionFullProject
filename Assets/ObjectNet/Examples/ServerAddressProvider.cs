using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class ServerAddressProvider : MonoBehaviour, IInformationProvider {

        public ServerAddressEntry[] GetAvaiableServers() {
            List<ServerAddressEntry> servers = new List<ServerAddressEntry>();
            // Add first server
            servers.Add(new ServerAddressEntry() { Address = "127.0.0.1" });
            // Add second server
            servers.Add(new ServerAddressEntry() { Address = "127.0.0.2" });
            return servers.ToArray<ServerAddressEntry>();
        }

        public ServerAddressEntry[] GetAvaiableServersFromWebService() {
            ServerAddressEntry[] servers = null;
            WebRequest request = WebRequest.Create("http://your-webservice.com/index.asp");
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());   
            string jsonContent = reader.ReadLine();  
            if (!String.IsNullOrEmpty(jsonContent)) {
                // json shall contains: '[{"Address": "127.0.0.1" }, {"Address": "127.0.0.2" }]'
                servers = JsonUtility.FromJson<ServerAddressEntry[]>(jsonContent);
            } else {
                throw new Exception("There's no avaiable servers to connect");
            }
            return servers;
        }

        public ServerAddressEntry[] GetAvaiableServersFromDatabase() {
            List<ServerAddressEntry> servers = new List<ServerAddressEntry>();
            //
            // Connect to any database and request list of servers and fill return array
            //
            return servers.ToArray<ServerAddressEntry>();
        }

    }
}
