using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class PlayerEventsByCode : NetworkBehaviour {

        [SerializeField]
        private Renderer playerBodyRender;

        [SerializeField]
        private Color ColorOne = Color.blue;

        [SerializeField]
        private Color ColorTwo = Color.red;

        private const int CHANGE_TO_COLOR_ONE = 900001;

        private const int CHANGE_TO_COLOR_TWO = 900002;


        public override void OnNetworkStarted() {
            this.RegisterEvent(CHANGE_TO_COLOR_ONE, this.OnReceiveChangeToColorOne);
            this.RegisterEvent(CHANGE_TO_COLOR_TWO, this.OnReceiveChangeToColorTwo);
        }

        void ActiveUpdate() {
            // Key 1 will change to color 1
            if ( Input.GetKeyDown(KeyCode.Alpha1) ) {
                this.SendChangeToColorOneEvent();
            } else if ( Input.GetKeyDown(KeyCode.Alpha2) ) { // Key 1 will change to color 1
                this.SendChangeToColorTwoEvent();
            }
        }

        private void OnReceiveChangeToColorOne(IDataStream reader) {
            Color receivedColor = reader.Read<Color>();
            string receivedText = reader.Read<string>();
            NetworkDebugger.Log(receivedText);
            // Apply received color
            this.playerBodyRender.material.SetColor("_BaseColor", receivedColor);
        }

        private void OnReceiveChangeToColorTwo(IDataStream reader) {
            Color receivedColor = reader.Read<Color>();
            string receivedText = reader.Read<string>();
            NetworkDebugger.Log(receivedText);
            // Apply received color
            this.playerBodyRender.material.SetColor("_BaseColor", receivedColor);
        }

        private void SendChangeToColorOneEvent() {
            using (DataStream writer = new DataStream()) {
                writer.Write(this.ColorOne); // Write color
                writer.Write("Event to color one"); // Write a text
                this.Send(CHANGE_TO_COLOR_ONE, writer, DeliveryMode.Reliable); // Send event
            }
            // Apply color on active player
            this.playerBodyRender.material.SetColor("_BaseColor", this.ColorOne);
        }

        private void SendChangeToColorTwoEvent() {
            using (DataStream writer = new DataStream()) {
                writer.Write(this.ColorTwo); // Write color
                writer.Write("Event to color two"); // Write a text
                this.Send(CHANGE_TO_COLOR_TWO, writer, DeliveryMode.Reliable); // Send event
            }
            // Apply color on active player
            this.playerBodyRender.material.SetColor("_BaseColor", this.ColorTwo);
        }
    }
}