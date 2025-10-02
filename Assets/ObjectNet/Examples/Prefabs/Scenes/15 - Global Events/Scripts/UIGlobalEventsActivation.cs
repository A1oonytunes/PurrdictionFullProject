using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet.examples {
    public class UIGlobalEventsActivation : MonoBehaviour {

        public GameObject OnConnectedObject;

        public GameObject OnDisconnectedObject;

        public GameObject OnClientConnectedObject;

        public GameObject OnClientDisconnectedObject;

        public Text receivedMessages;

        private int receivedMessagesCount = 0;

        void Start() {
            // Deactivate al object on start to be activated when event occurs
            if (this.OnConnectedObject          != null) this.OnConnectedObject.SetActive(false);
            if (this.OnDisconnectedObject       != null) this.OnDisconnectedObject.SetActive(false);
            if (this.OnClientConnectedObject    != null) this.OnClientConnectedObject.SetActive(false);
            if (this.OnClientDisconnectedObject != null) this.OnClientDisconnectedObject.SetActive(false);
        }

        public void ConnectedOnServer(IClient client) {
            NetworkDebugger.Log("ConnectedOnServer executed");
            this.OnConnectedObject.SetActive(true);
        }

        public void DisconnectedFromServer(IClient client) {
            NetworkDebugger.Log("DisconnectedFromServer executed");
            this.OnDisconnectedObject.SetActive(true);
        }

        public void ClientConnectedOnServer(IClient client) {
            NetworkDebugger.Log("ClientConnectedOnServer executed");
            this.OnClientConnectedObject.SetActive(true);
        }

        public void ClientDisconnectedOnServer(IClient client) {
            NetworkDebugger.Log("ClientDisconnectedOnServer executed");
            this.OnClientDisconnectedObject.SetActive(true);
        }

        public void OnMessageReceived(IDataStream reader) {
            // Here you can also read message data using he following piece of code
            // int receivedEvent = reader.Read<int>();
            // string receivedString = reader.Read<string>();
            // Etc...
            this.receivedMessages.text = string.Format("Received messages <color=#00FF00>{0}</color>", ++this.receivedMessagesCount);
        }
    }
}