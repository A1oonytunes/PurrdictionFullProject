using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet.integration {
    /// <summary>
    /// UISelectMode handles the UI interactions for selecting the network mode (Server or Client)
    /// and initiating the network connection.
    /// </summary>
    public class UISelectMode : MonoBehaviour {

        // Button to start the server mode.
        public Button StartServerButton;

        // Button to start the client mode.
        public Button StartClientButton;

        // Input field to enter the IP address.
        public InputField ipAddress;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Here, it is used to set up button click listeners.
        /// </summary>
        void Awake() {
            // Add listener for the server start button to call StartServerMode method when clicked.
            this.StartServerButton.onClick.AddListener(StartServerMode);
            // Add listener for the client start button to call StartClientMode method when clicked.
            this.StartClientButton.onClick.AddListener(StartClientMode);
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// Here, it is used to initialize the IP address input field with the local IP.
        /// </summary>
        private void Start() {
            // Set the text of the IP address input field to the private IP of the network manager.
            this.ipAddress.text = NetworkManager.Instance().GetPrivateIp();
        }

        /// <summary>
        /// StartServerMode is called when the Start Server button is clicked.
        /// It configures the network manager to server mode, sets the server address, and starts the network.
        /// </summary>
        private void StartServerMode() {
            // Configure the network manager to operate in server mode.
            NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Server);
            // Set the server IP address from the input field.
            NetworkManager.Instance().SetServerAddress(this.ipAddress.text);
            // Start the network connection.
            NetworkManager.Instance().StartNetwork();
            // Deactivate the current game object (likely the UI panel).
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// StartClientMode is called when the Start Client button is clicked.
        /// It configures the network manager to client mode, sets the server address, and starts the network.
        /// </summary>
        private void StartClientMode() {
            // Configure the network manager to operate in client mode.
            NetworkManager.Instance().ConfigureMode(NetworkConnectionType.Client);
            // Set the server IP address from the input field.
            NetworkManager.Instance().SetServerAddress(this.ipAddress.text);
            // Start the network connection.
            NetworkManager.Instance().StartNetwork();
            // Deactivate the current game object (likely the UI panel).
            this.gameObject.SetActive(false);
        }
    }

}