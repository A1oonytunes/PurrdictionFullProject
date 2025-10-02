#if UNITY_EDITOR
using UnityEditor;
#endif
namespace com.onlineobject.objectnet {
#if UNITY_EDITOR
    /// <summary>
    /// This class is responsible for monitoring network gameplay by keeping track of network threads and sockets.
    /// It ensures that all network resources are properly released when exiting the play mode in the Unity Editor.
    /// </summary>
    [InitializeOnLoadAttribute]
    public class NetworkGamePlayMonitor {

        static bool isPlaying = false;

        // Static constructor to subscribe to the play mode state change event.
        static NetworkGamePlayMonitor() {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state) {
            if (!NetworkGamePlayMonitor.isPlaying) {
                NetworkGamePlayMonitor.isPlaying = PlayModeStateChange.EnteredPlayMode.Equals(state);
            } else {
                NetworkGamePlayMonitor.isPlaying = PlayModeStateChange.ExitingPlayMode.Equals(state);
                if (!NetworkGamePlayMonitor.isPlaying) {
                    if (NetworkManager.Instance() != null) {
                        NetworkManager.Instance().StopNetwork();
                    }
                }
            }

        }

        public static bool IsPlaying() {
            return NetworkGamePlayMonitor.isPlaying;
        }
    }

#endif
}