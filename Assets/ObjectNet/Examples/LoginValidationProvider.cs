using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class LoginValidationProvider : MonoBehaviour, IInformationProvider {

        [Serializable]
        public class NetworkIdUserInformation {
            public String   UserName;
            public String   Password;
        }

        public List<NetworkIdUserInformation> NetworkUsersMap = new List<NetworkIdUserInformation>();

        public bool IsLoginValid(object[] arguments) {
            bool result = false;
            string userName = (arguments[0] as string); // First argument is user id 
            string password = (arguments[1] as string); // Second argument is password
            foreach(NetworkIdUserInformation userLogin in this.NetworkUsersMap) {
                if ((userLogin.UserName.ToUpper().Equals(userName.ToUpper())) &&
                    (userLogin.Password.Equals(password))) {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool IsLoginValidFromWebService(object[] arguments) {
            bool result = true;
            /*
            ServerAddressEntry[] servers = null;
            WebRequest request = WebRequest.Create("http://your-webservice.com/index.asp");
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());   
            string jsonContent = reader.ReadLine();  
            if (!String.IsNullOrEmpty(jsonContent)) {
                // Check user informations
                servers = JsonUtility.FromJson<???????>(jsonContent);
            } else {
                throw new Exception("Not data found");
            }
            */
            return result;
        }

        public bool IsLoginValidFromDatabase(object[] arguments) {
            bool result = true;
            /*
            //
            // Connect to any database and validate data
            //
            */
            return result;
        }

        public bool IsLoginValidFromSaveFile(object[] arguments) {
            bool result = true;
            /*
            //
            // Check information on save file
            //
            */
            return result;
        }
    }
}
