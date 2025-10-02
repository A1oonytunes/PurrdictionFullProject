using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet.examples {
    public class UICustomEventsActivation : MonoBehaviour {

        public GameObject OnPlayerDieObject;

        public GameObject OnPlayerCollectedCoinObject;

        public GameObject OnPlayerEventWithParams;

        public Text ReceivedText;

        public Text ReceivedNumber;

        public Text ReceivedFloat;

        void Start() {
            // Deactivate al object on start to be activated when event occurs
            this.OnPlayerDieObject.SetActive(false);
            this.OnPlayerCollectedCoinObject.SetActive(false);
            this.OnPlayerEventWithParams.SetActive(false);
        }

        public void PlayerDieEvent(IDataStream reader) {
            this.OnPlayerDieObject.SetActive(true);
        }

        public void PlayerCollectedCoinEvent(IDataStream reader) {
            this.OnPlayerCollectedCoinObject.SetActive(true);
        }

        public void PlayerEventWithParamsEvent(IDataStream reader) {
            this.OnPlayerEventWithParams.SetActive(true);
            // Extract data from event and set
            this.ReceivedText.text      = reader.Read<string>();
            this.ReceivedNumber.text    = reader.Read<int>().ToString();
            this.ReceivedFloat.text     = reader.Read<float>().ToString();
        }
    }
}