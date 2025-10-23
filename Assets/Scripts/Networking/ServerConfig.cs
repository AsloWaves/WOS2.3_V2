using UnityEngine;

namespace WOS.Networking
{
    /// <summary>
    /// Server configuration - stores Edgegap server IP
    /// Automatically uses localhost in Unity Editor for testing
    /// Uses production Edgegap IP in builds
    /// </summary>
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "WOS/Networking/Server Configuration", order = 1)]
    public class ServerConfig : ScriptableObject
    {
        [Header("Production Server (Edgegap)")]
        [Tooltip("Production Edgegap server IP and port (e.g., 172.234.24.224:31139)")]
        public string serverAddress = "172.234.24.224:31139";

        [Header("Local Testing")]
        [Tooltip("Use localhost when testing in Unity Editor (auto-enabled)")]
        public bool useLocalhostInEditor = true;

        [Tooltip("Local server address for Editor testing")]
        public string localServerAddress = "localhost:7777";

        [Header("Display Info")]
        [Tooltip("Server location for display purposes")]
        public string serverLocation = "Chicago, Illinois";

        [Tooltip("Show deployment info in UI")]
        public bool showServerInfo = true;

        /// <summary>
        /// Get the active server address (localhost in Editor if enabled, Edgegap otherwise)
        /// </summary>
        private string GetActiveAddress()
        {
#if UNITY_EDITOR
            if (useLocalhostInEditor)
            {
                return localServerAddress;
            }
#endif
            return serverAddress;
        }

        /// <summary>
        /// Get just the IP part (without port)
        /// Automatically uses localhost in Unity Editor if enabled
        /// </summary>
        public string GetServerIP()
        {
            string activeAddress = GetActiveAddress();
            if (string.IsNullOrEmpty(activeAddress)) return "";

            int colonIndex = activeAddress.IndexOf(':');
            if (colonIndex > 0)
            {
                return activeAddress.Substring(0, colonIndex);
            }

            return activeAddress;
        }

        /// <summary>
        /// Get just the port part
        /// </summary>
        public ushort GetServerPort()
        {
            string activeAddress = GetActiveAddress();
            if (string.IsNullOrEmpty(activeAddress)) return 7777;

            int colonIndex = activeAddress.IndexOf(':');
            if (colonIndex > 0 && colonIndex < activeAddress.Length - 1)
            {
                string portString = activeAddress.Substring(colonIndex + 1);
                if (ushort.TryParse(portString, out ushort port))
                {
                    return port;
                }
            }

            return 7777; // Default port
        }

        /// <summary>
        /// Get full address (IP:port)
        /// Automatically uses localhost in Unity Editor if enabled
        /// </summary>
        public string GetFullAddress()
        {
            return GetActiveAddress();
        }

        /// <summary>
        /// Check if currently using local server
        /// </summary>
        public bool IsUsingLocalServer()
        {
#if UNITY_EDITOR
            return useLocalhostInEditor;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get server type for display
        /// </summary>
        public string GetServerType()
        {
#if UNITY_EDITOR
            if (useLocalhostInEditor)
            {
                return "Local Test Server";
            }
#endif
            return "Edgegap Production";
        }
    }
}
