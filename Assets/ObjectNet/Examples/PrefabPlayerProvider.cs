using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class PrefabPlayerProvider : MonoBehaviour, IInformationProvider {

        public GameObject PrefabToSpawn;

        public GameObject GetPlayer() {
            return this.PrefabToSpawn;
        }

        public GameObject GetPlayerFromWebService() {
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
            return null;
        }

        public GameObject GetPlayerFromDatabase() {
            /*
            List<ServerAddressEntry> servers = new List<ServerAddressEntry>();
            //
            // Connect to any database and request list of servers and fill return array
            //
            return servers.ToArray<ServerAddressEntry>();
            */
            return null;
        }

        public GameObject GetPlayerFromSaveFile() {
            /*
            List<ServerAddressEntry> servers = new List<ServerAddressEntry>();
            //
            // Connect to any database and request list of servers and fill return array
            //
            return servers.ToArray<ServerAddressEntry>();
            */
            return null;
        }
    }
}
