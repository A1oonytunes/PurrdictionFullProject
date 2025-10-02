using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing global resource paths and utility methods for accessing database paths.
    /// </summary>
    public static class GlobalResources {

        // Constant path format for prefab database, with placeholder for database name.
        public const string PREFABS_DATABASE_PATH = "Databases/{0}/NetworkPrefabsDatabase";

        // Constant path format for events database, with placeholder for database name.
        public const string EVENTS_DATABASE_PATH = "Databases/{0}/NetworkEventsDatabase";

        // Constant path for the network transports database.
        public const string TRANSPORTS_DATABASE_PATH = "Databases/NetworkTransportsDatabase";

        // Default database name.
        public const string DEFAULT_DATABASE = "Default";

        /// <summary>
        /// Retrieves the full path to a specified database.
        /// </summary>
        /// <param name="databasePath">The name of the database.</param>
        /// <returns>The formatted database path.</returns>
        public static string GetDatabasePath(string databasePath) {
            return string.Format("Databases/{0}", databasePath);
        }

        /// <summary>
        /// Retrieves the full path to the prefabs database, defaulting to the default database if none specified.
        /// </summary>
        /// <param name="databasePath">The name of the database (optional).</param>
        /// <returns>The formatted path to the prefabs database.</returns>
        public static string GetPrefabsDatabase(string databasePath = GlobalResources.DEFAULT_DATABASE) {
            return string.Format(GlobalResources.PREFABS_DATABASE_PATH, (string.IsNullOrEmpty(databasePath) ? DEFAULT_DATABASE : databasePath));
        }

        /// <summary>
        /// Retrieves the full path to the events database, defaulting to the default database if none specified.
        /// </summary>
        /// <param name="databasePath">The name of the database (optional).</param>
        /// <returns>The formatted path to the events database.</returns>
        public static string GetEventsDatabase(string databasePath = GlobalResources.DEFAULT_DATABASE) {
            return string.Format(GlobalResources.EVENTS_DATABASE_PATH, (string.IsNullOrEmpty(databasePath) ? DEFAULT_DATABASE : databasePath));
        }

        /// <summary>
        /// Retrieves an array of available database names, optionally returning the full path.
        /// </summary>
        /// <param name="returnFullPath">Whether to return the full path or just the database names.</param>
        /// <returns>An array of database names or full paths.</returns>
        public static string[] GetAvaiableDatabases(bool returnFullPath = true) {
#if UNITY_EDITOR
            // Get the full path to the resources folder.
            string resourcesFolder = ResourceUtils.ResourcesPath;
            string fullresFolder = Application.dataPath + resourcesFolder.Replace("Assets", "");
            // Ensure the resources directory exists.
            if (!System.IO.Directory.Exists(fullresFolder)) {
                System.IO.Directory.CreateDirectory(fullresFolder);
            }
            // Open or create the database folder.
            string databaseFolder = string.Format("{0}/{1}/", fullresFolder, "Databases");
            if (!System.IO.Directory.Exists(databaseFolder)) {
                System.IO.Directory.CreateDirectory(databaseFolder);
            }
            // Get directories representing databases.
            string[] result = Directory.GetDirectories(databaseFolder);
            // If only names are requested, strip the full path.
            if (!returnFullPath) {
                for (int index = 0; index < result.Length; index++) {
                    result[index] = Path.GetFileName(result[index]);
                }
            }
            // Convert to list for manipulation.
            List<string> databasesList = new List<string>();
            databasesList.AddRange(result);
            // Ensure the default database is first in the list.
            if (databasesList.Contains(GlobalResources.DEFAULT_DATABASE)) {
                int defaultIndex = databasesList.IndexOf(GlobalResources.DEFAULT_DATABASE);
                if (defaultIndex > -1) {
                    databasesList.RemoveAt(defaultIndex);
                    databasesList.Insert(0, GlobalResources.DEFAULT_DATABASE);
                    result = databasesList.ToArray<string>();
                }
            }
            return result;
#else
            // Return null if not in the Unity Editor.
            return null;
#endif
        }
    }

}