using UnityEngine;

namespace WOS.Networking
{
    /// <summary>
    /// Server configuration - stores Edgegap server IP
    /// Update this asset when redeploying server to Edgegap
    /// </summary>
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "WOS/Networking/Server Configuration", order = 1)]
    public class ServerConfig : ScriptableObject
    {
        [Header("Edgegap Server")]
        [Tooltip("Current Edgegap server IP and port (e.g., 172.234.24.224:31139)")]
        public string serverAddress = "172.234.24.224:31139";

        [Header("Display Info")]
        [Tooltip("Server location for display purposes")]
        public string serverLocation = "Chicago, Illinois";

        [Tooltip("Show deployment info in UI")]
        public bool showServerInfo = true;

        /// <summary>
        /// Get just the IP part (without port)
        /// </summary>
        public string GetServerIP()
        {
            if (string.IsNullOrEmpty(serverAddress)) return "";

            int colonIndex = serverAddress.IndexOf(':');
            if (colonIndex > 0)
            {
                return serverAddress.Substring(0, colonIndex);
            }

            return serverAddress;
        }

        /// <summary>
        /// Get just the port part
        /// </summary>
        public ushort GetServerPort()
        {
            if (string.IsNullOrEmpty(serverAddress)) return 7777;

            int colonIndex = serverAddress.IndexOf(':');
            if (colonIndex > 0 && colonIndex < serverAddress.Length - 1)
            {
                string portString = serverAddress.Substring(colonIndex + 1);
                if (ushort.TryParse(portString, out ushort port))
                {
                    return port;
                }
            }

            return 7777; // Default port
        }

        /// <summary>
        /// Get full address (IP:port)
        /// </summary>
        public string GetFullAddress()
        {
            return serverAddress;
        }
    }
}
