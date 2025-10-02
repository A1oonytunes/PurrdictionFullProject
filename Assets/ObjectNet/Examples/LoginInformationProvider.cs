using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet.examples {
    public class LoginInformationProvider : MonoBehaviour, IInformationProvider {

        public InputField UserName;

        public InputField Password;

        public object[] GetLoginInformations() {
            if (!string.IsNullOrEmpty(UserName.text) &&
                !string.IsNullOrEmpty(Password.text)) {
                return new object[] {
                                        UserName.text,
                                        Password.text
                                    };
            } else {
                throw new System.Exception("Username and password must be filled");
            }
        }

        /// <summary>
        /// Return array containing all types retuned by "GetLoginInformations"
        /// 
        /// Note: On final implementations this method shall to return the same types returned by login provider method in same order
        /// </summary>
        /// <returns>
        /// Array containing list os types
        /// </returns>
        public Type[] GetLoginInformationsTypes() {
            return new Type[] { 
                                typeof(string),
                                typeof(string)
                               };
            
        }
    }
}
