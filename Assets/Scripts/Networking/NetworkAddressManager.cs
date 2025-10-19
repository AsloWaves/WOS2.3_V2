using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WOS.Networking
{
    /// <summary>
    /// Manages server IP addresses using PlayerPrefs
    /// Stores recent server IPs and provides auto-fill functionality
    /// Supports multiple deployment types: local, cloud, and custom IPs
    /// </summary>
    public class NetworkAddressManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Maximum number of recent servers to remember")]
        public int maxRecentServers = 10;

        [Tooltip("Default server IP for first-time users")]
        public string defaultServerIP = "127.0.0.1";

        [Header("Predefined Servers")]
        [Tooltip("List of predefined servers (testing, staging, production)")]
        public List<ServerEntry> predefinedServers = new List<ServerEntry>();

        // PlayerPrefs keys
        private const string PREFS_LAST_SERVER = "WOS_LastServerIP";
        private const string PREFS_RECENT_SERVERS = "WOS_RecentServers";

        private List<string> recentServers = new List<string>();

        private void Awake()
        {
            LoadRecentServers();
        }

        #region Save/Load Server IPs

        /// <summary>
        /// Save server IP as the last used and add to recent list
        /// </summary>
        public void SaveServerIP(string serverIP)
        {
            if (string.IsNullOrEmpty(serverIP))
            {
                Debug.LogWarning("[NetworkAddressManager] Attempted to save empty server IP");
                return;
            }

            // Save as last used
            PlayerPrefs.SetString(PREFS_LAST_SERVER, serverIP);

            // Add to recent servers (if not already present)
            if (!recentServers.Contains(serverIP))
            {
                recentServers.Insert(0, serverIP);

                // Trim to max size
                if (recentServers.Count > maxRecentServers)
                {
                    recentServers = recentServers.Take(maxRecentServers).ToList();
                }

                SaveRecentServers();
            }

            PlayerPrefs.Save();
            Debug.Log($"[NetworkAddressManager] Saved server IP: {serverIP}");
        }

        /// <summary>
        /// Get the last used server IP
        /// </summary>
        public string GetLastServerIP()
        {
            return PlayerPrefs.GetString(PREFS_LAST_SERVER, defaultServerIP);
        }

        /// <summary>
        /// Get list of recent server IPs
        /// </summary>
        public List<string> GetRecentServers()
        {
            return new List<string>(recentServers); // Return copy
        }

        /// <summary>
        /// Clear all saved server IPs
        /// </summary>
        public void ClearAllServers()
        {
            PlayerPrefs.DeleteKey(PREFS_LAST_SERVER);
            PlayerPrefs.DeleteKey(PREFS_RECENT_SERVERS);
            recentServers.Clear();
            PlayerPrefs.Save();

            Debug.Log("[NetworkAddressManager] Cleared all saved servers");
        }

        /// <summary>
        /// Remove specific server from recent list
        /// </summary>
        public void RemoveServer(string serverIP)
        {
            if (recentServers.Remove(serverIP))
            {
                SaveRecentServers();
                Debug.Log($"[NetworkAddressManager] Removed server: {serverIP}");
            }
        }

        #endregion

        #region Predefined Servers

        /// <summary>
        /// Add a predefined server entry
        /// </summary>
        public void AddPredefinedServer(string name, string ip, string description = "")
        {
            predefinedServers.Add(new ServerEntry
            {
                serverName = name,
                serverIP = ip,
                description = description
            });
        }

        /// <summary>
        /// Get predefined server by name
        /// </summary>
        public ServerEntry GetPredefinedServer(string name)
        {
            return predefinedServers.FirstOrDefault(s => s.serverName == name);
        }

        /// <summary>
        /// Get all predefined servers
        /// </summary>
        public List<ServerEntry> GetAllPredefinedServers()
        {
            return new List<ServerEntry>(predefinedServers);
        }

        #endregion

        #region Internal Helpers

        private void LoadRecentServers()
        {
            string savedServers = PlayerPrefs.GetString(PREFS_RECENT_SERVERS, "");

            if (!string.IsNullOrEmpty(savedServers))
            {
                recentServers = savedServers.Split(';').ToList();
                Debug.Log($"[NetworkAddressManager] Loaded {recentServers.Count} recent servers");
            }
        }

        private void SaveRecentServers()
        {
            string serversString = string.Join(";", recentServers);
            PlayerPrefs.SetString(PREFS_RECENT_SERVERS, serversString);
            PlayerPrefs.Save();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate IP address format
        /// </summary>
        public bool IsValidIP(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            if (ip == "localhost") return true;

            // Check for IPv4 format
            string[] parts = ip.Split('.');
            if (parts.Length == 4)
            {
                foreach (string part in parts)
                {
                    if (!int.TryParse(part, out int value) || value < 0 || value > 255)
                    {
                        return false;
                    }
                }
                return true;
            }

            // Assume hostname if not IPv4
            return ip.Contains(".");
        }

        /// <summary>
        /// Get server type based on IP
        /// </summary>
        public ServerType GetServerType(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return ServerType.Unknown;

            if (ip == "localhost" || ip == "127.0.0.1")
                return ServerType.Local;

            if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                return ServerType.LAN;

            if (ip.Contains("edgegap") || ip.Contains("cloud"))
                return ServerType.Cloud;

            return ServerType.Remote;
        }

        #endregion

        #region Debug Helpers

        [ContextMenu("Print All Servers")]
        private void PrintAllServers()
        {
            Debug.Log("=== Saved Servers ===");
            Debug.Log($"Last Server: {GetLastServerIP()}");
            Debug.Log($"Recent Servers ({recentServers.Count}):");
            foreach (var server in recentServers)
            {
                Debug.Log($"  - {server}");
            }
            Debug.Log($"Predefined Servers ({predefinedServers.Count}):");
            foreach (var server in predefinedServers)
            {
                Debug.Log($"  - {server.serverName}: {server.serverIP}");
            }
        }

        [ContextMenu("Clear All Servers (Debug)")]
        private void DebugClearAllServers()
        {
            ClearAllServers();
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Predefined server entry
    /// </summary>
    [System.Serializable]
    public class ServerEntry
    {
        public string serverName;
        public string serverIP;
        public string description;
        public bool isOnline = true;

        public override string ToString()
        {
            return $"{serverName} ({serverIP}) - {description}";
        }
    }

    /// <summary>
    /// Server type classification
    /// </summary>
    public enum ServerType
    {
        Unknown,
        Local,      // localhost, 127.0.0.1
        LAN,        // 192.168.x.x, 10.x.x.x, etc.
        Cloud,      // Edgegap, AWS, etc.
        Remote      // Public IP
    }

    #endregion
}
