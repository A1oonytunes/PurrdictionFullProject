using UnityEngine;
using UnityEngine.UI;

namespace com.onlineobject.objectnet.integration {
    /// <summary>
    /// Represents a UI element in a lobby, such as a menu item or a selectable option.
    /// </summary>
    public class UILobbyItem : MonoBehaviour {

        /// <summary>
        /// The text component that displays the label of the lobby item.
        /// </summary>
        public Text label;

        /// <summary>
        /// The button component that can be interacted with by the user.
        /// </summary>
        public Button button;
    }

}