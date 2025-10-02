using UnityEditor;

namespace com.onlineobject.objectnet {
#if UNITY_EDITOR
    /// <summary>
    /// Class ResourceUtils.
    /// </summary>
    public static class ResourceUtils {
        /// <summary>
        /// Gets the editors path.
        /// </summary>
        /// <value>The editors path.</value>
        public static string EditorsPath {
            get {
                string result = "";
                var g = AssetDatabase.FindAssets($"t:Script {nameof(ResourceUtils)}");
                foreach (var info in g) {
                    result = AssetDatabase.GUIDToAssetPath(info);
                    if (result.Trim().ToUpper().Contains("OBJECTNET")) {
                        break;
                    } else {
                        result = "";
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the resources path.
        /// </summary>
        /// <value>The resources path.</value>
        public static string ResourcesPath {
            get {
                var g = AssetDatabase.FindAssets($"t:Script {nameof(ResourceUtils)}");
                string result = "";
                foreach (var info in g) {
                    result = AssetDatabase.GUIDToAssetPath(info);
                    if (result.Trim().ToUpper().Contains("OBJECTNET")) {
                        result = result.Replace("ResourceUtils.cs", "");
                        result += "Resources";
                        break;
                    } else {
                        result = "";
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the database path.
        /// </summary>
        /// <value>The database path.</value>
        public static string DatabasePath {
            get {
                string result = "";
                var g = AssetDatabase.FindAssets($"t:Script {nameof(ResourceUtils)}");
                foreach (var info in g) {
                    result = AssetDatabase.GUIDToAssetPath(info);
                    if (result.Trim().ToUpper().Contains("OBJECTNET")) {
                        result = result.Replace("ResourceUtils.cs", "");
                        result += "Resources/Database";
                    } else {
                        result = "";
                    }
                }
                
                return result;
            }
        }
    }
#endif
}