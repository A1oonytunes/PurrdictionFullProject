#pragma warning disable 0168
#pragma warning disable 0219
#pragma warning disable 0414

using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class PlayerEventsDetection : MonoBehaviour {

        [Header("Use this checkboxes to send events")]
        [SerializeField]
        private bool IsPlayerDie = false;

        [SerializeField]
        private bool IsCollectCoin = false;

        [SerializeField]
        private bool IsToSendEventWithParams = false;

        [SerializeField]
        private string TextToSend = "";

        [SerializeField]
        private int NumberToSend = 0;

        public float GetFloatToSend() {
            return 10.0f;
        }
    }
}