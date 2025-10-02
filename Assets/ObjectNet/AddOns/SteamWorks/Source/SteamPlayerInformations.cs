using UnityEngine;

namespace com.onlineobject.objectnet {
    public class SteamPlayerInformations : MonoBehaviour {

        [HideInInspector]
        public int NetworkId;

        [HideInInspector]
        public ulong SteamId;

        [HideInInspector]
        public ushort PlayerId;

        [HideInInspector]
        public ushort PlayerIndex;

    }
}