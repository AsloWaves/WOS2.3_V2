using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WOS.Networking
{
    /// <summary>
    /// SECURE server browser that fetches server list from YOUR backend API.
    /// Backend handles Edgegap API communication (token stays server-side).
    ///
    /// Architecture:
    /// Unity Client → Your Backend API → Edgegap API
    ///
    /// Features:
    /// - Fetch active servers from YOUR backend
    /// - No API tokens in Unity (secure!)
    /// - Server list with player counts, ping, health status
    /// - Automatic best server selection
    /// - Fallback to hardcoded config if backend unavailable
    /// </summary>
    public class ServerBrowserManager : MonoBehaviour
    {
        [Header("Backend Configuration")]
        [Tooltip("URL of YOUR backend API (NOT Edgegap!)")]
        public string backendApiUrl = "http://localhost:5000";

        [Tooltip("Fallback ServerConfig if backend is unavailable")]
        public ServerConfig fallbackConfig;

        [Header("Settings")]
        [Tooltip("Timeout for backend API requests (seconds)")]
        public float apiTimeout = 10f;

        [Tooltip("Auto-refresh server list interval (seconds, 0 = manual only)")]
        public float autoRefreshInterval = 30f;

        [Header("Status (Read-Only)")]
        [SerializeField] private bool isFetching = false;
        [SerializeField] private int serversFound = 0;
        [SerializeField] private int healthyServers = 0;
        [SerializeField] private string lastError = "";

        // Cached server list
        private List<ServerInfo> serverList = new List<ServerInfo>();

        // Events
        public event Action<List<ServerInfo>> OnServersUpdated;
        public event Action<string> OnError;

        private Coroutine autoRefreshCoroutine;

        private void Start()
        {
            Debug.Log("[ServerBrowser] 🌐 Server Browser Manager initialized");
            Debug.Log($"[ServerBrowser] Backend URL: {backendApiUrl}");

            // Start auto-refresh if enabled
            if (autoRefreshInterval > 0)
            {
                autoRefreshCoroutine = StartCoroutine(AutoRefreshLoop());
            }
        }

        private void OnDestroy()
        {
            if (autoRefreshCoroutine != null)
            {
                StopCoroutine(autoRefreshCoroutine);
            }
        }

        /// <summary>
        /// Fetch server list from YOUR backend API.
        /// </summary>
        public void RefreshServers(bool forceRefresh = false)
        {
            if (isFetching)
            {
                Debug.LogWarning("[ServerBrowser] Already fetching servers...");
                return;
            }

            StartCoroutine(FetchServersFromBackend(forceRefresh));
        }

        private IEnumerator FetchServersFromBackend(bool forceRefresh)
        {
            isFetching = true;
            lastError = "";
            serverList.Clear();

            Debug.Log("[ServerBrowser] 🔄 Fetching servers from backend...");

            // Build API URL
            string apiUrl = $"{backendApiUrl}/api/servers";
            if (forceRefresh)
            {
                apiUrl += "?forceRefresh=true";
            }

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.timeout = (int)apiTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Parse JSON response
                    string json = request.downloadHandler.text;
                    bool parseSuccess = false;

                    // Try parsing (no yield inside try-catch!)
                    try
                    {
                        // Wrap in object for JsonUtility
                        string wrappedJson = $"{{\"servers\":{json}}}";
                        ServerListResponse response = JsonUtility.FromJson<ServerListResponse>(wrappedJson);

                        if (response != null && response.servers != null)
                        {
                            serverList = response.servers;
                            serversFound = serverList.Count;
                            healthyServers = serverList.Count(s => s.isHealthy);

                            Debug.Log($"[ServerBrowser] ✅ Fetched {serversFound} servers ({healthyServers} healthy)");

                            // Log each server
                            foreach (var server in serverList)
                            {
                                string healthStatus = server.isHealthy ? "✅" : "❌";
                                Debug.Log($"[ServerBrowser]   {healthStatus} {server.city} - {server.currentPlayers}/{server.maxPlayers} players, {server.pingMs}ms");
                            }

                            OnServersUpdated?.Invoke(serverList);
                            parseSuccess = true;
                        }
                        else
                        {
                            Debug.LogWarning("[ServerBrowser] ⚠️ Backend returned empty server list");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ServerBrowser] ❌ JSON parsing error: {e.Message}");
                        Debug.LogError($"[ServerBrowser] Response: {json}");
                        lastError = $"JSON parsing error: {e.Message}";
                    }

                    // Use fallback if parsing failed or empty list (outside try-catch!)
                    if (!parseSuccess)
                    {
                        yield return TryFallbackConfig();
                    }
                }
                else
                {
                    Debug.LogError($"[ServerBrowser] ❌ Backend API error: {request.error}");
                    Debug.LogError($"[ServerBrowser] URL: {apiUrl}");
                    Debug.LogError($"[ServerBrowser] Response Code: {request.responseCode}");

                    lastError = $"Backend error: {request.error}";
                    OnError?.Invoke(lastError);

                    // Use fallback for API errors
                    yield return TryFallbackConfig();
                }
            }

            isFetching = false;
        }

        /// <summary>
        /// Fallback to hardcoded ServerConfig if backend is unavailable.
        /// </summary>
        private IEnumerator TryFallbackConfig()
        {
            if (fallbackConfig == null)
            {
                Debug.LogError("[ServerBrowser] ❌ No fallback config assigned!");
                yield break;
            }

            Debug.LogWarning("[ServerBrowser] ⚠️ Using fallback ServerConfig");

            // Create ServerInfo from fallback config
            ServerInfo fallbackServer = new ServerInfo
            {
                serverId = "fallback",
                serverName = $"{fallbackConfig.serverLocation} (Fallback)",
                region = fallbackConfig.serverLocation,
                city = fallbackConfig.serverLocation,
                isHealthy = false,
                pingMs = 9999,
                status = "Unknown",
                healthPort = 30407  // Default health port for Edgegap deployments
            };

            // Parse IP:Port
            string[] parts = fallbackConfig.serverAddress.Split(':');
            if (parts.Length == 2)
            {
                fallbackServer.ipAddress = parts[0];
                if (int.TryParse(parts[1], out int port))
                {
                    fallbackServer.port = port;
                }
            }

            // Validate health via HTTP health check (keep existing health check logic)
            yield return ValidateServerHealth(fallbackServer);

            if (fallbackServer.isHealthy)
            {
                serverList.Add(fallbackServer);
                serversFound = 1;
                healthyServers = 1;

                Debug.Log($"[ServerBrowser] ✅ Fallback server is healthy");
                OnServersUpdated?.Invoke(serverList);
            }
            else
            {
                Debug.LogError("[ServerBrowser] ❌ Fallback server health check failed!");
                OnError?.Invoke("Backend unavailable and fallback server offline");
            }
        }

        /// <summary>
        /// Validate server health via HTTP health endpoint.
        /// </summary>
        private IEnumerator ValidateServerHealth(ServerInfo server)
        {
            // Use healthPort from server info, fallback to 8080 if not provided
            int port = server.healthPort > 0 ? server.healthPort : 8080;
            string healthUrl = $"http://{server.ipAddress}:{port}/health";

            using (UnityWebRequest request = UnityWebRequest.Get(healthUrl))
            {
                request.timeout = 5;

                float startTime = Time.realtimeSinceStartup;
                yield return request.SendWebRequest();
                float pingMs = (Time.realtimeSinceStartup - startTime) * 1000f;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    server.isHealthy = true;
                    server.pingMs = (int)pingMs;

                    // Parse player count from response
                    string response = request.downloadHandler.text;
                    if (response.Contains("\"players\":"))
                    {
                        try
                        {
                            HealthCheckResponse healthResponse = JsonUtility.FromJson<HealthCheckResponse>(response);
                            server.currentPlayers = healthResponse.players;
                            server.maxPlayers = healthResponse.maxPlayers;
                        }
                        catch
                        {
                            Debug.LogWarning($"[ServerBrowser] Failed to parse health response for {server.city}");
                        }
                    }
                }
                else
                {
                    server.isHealthy = false;
                    Debug.LogWarning($"[ServerBrowser] ❌ Health check failed for {server.city}: {request.error}");
                }
            }
        }

        /// <summary>
        /// Auto-refresh server list on interval.
        /// </summary>
        private IEnumerator AutoRefreshLoop()
        {
            // Initial fetch
            RefreshServers();

            while (true)
            {
                yield return new WaitForSeconds(autoRefreshInterval);
                RefreshServers();
            }
        }

        /// <summary>
        /// Get the best server based on health, players, and ping.
        /// </summary>
        public ServerInfo GetBestServer()
        {
            if (serverList.Count == 0)
            {
                Debug.LogWarning("[ServerBrowser] ⚠️ No servers available");
                return null;
            }

            // Backend already sorts by health → players → ping
            var bestServer = serverList.FirstOrDefault(s => s.isHealthy);

            if (bestServer != null)
            {
                Debug.Log($"[ServerBrowser] 🎯 Best server: {bestServer.city} ({bestServer.currentPlayers} players, {bestServer.pingMs}ms)");
            }

            return bestServer;
        }

        /// <summary>
        /// Get all servers.
        /// </summary>
        public List<ServerInfo> GetAllServers()
        {
            return new List<ServerInfo>(serverList);
        }

        /// <summary>
        /// Get server by ID.
        /// </summary>
        public ServerInfo GetServerById(string serverId)
        {
            return serverList.FirstOrDefault(s => s.serverId == serverId);
        }
    }

    // ===== DATA MODELS =====

    /// <summary>
    /// Server information model (matches backend response).
    /// </summary>
    [Serializable]
    public class ServerInfo
    {
        public string serverId;
        public string serverName;
        public string ipAddress;
        public int port;
        public int healthPort;
        public string region;
        public string city;
        public string country;
        public int currentPlayers;
        public int maxPlayers;
        public int pingMs;
        public bool isHealthy;
        public string status;
        public string[] tags;

        /// <summary>
        /// Get connection address (IP:Port).
        /// </summary>
        public string GetConnectionAddress()
        {
            return $"{ipAddress}:{port}";
        }

        /// <summary>
        /// Get display name for UI.
        /// </summary>
        public string GetDisplayName()
        {
            if (!isHealthy)
                return $"{city} - Offline";

            return $"{city} ({currentPlayers}/{maxPlayers} players) - {pingMs}ms";
        }
    }

    /// <summary>
    /// Wrapper for JSON array deserialization.
    /// </summary>
    [Serializable]
    public class ServerListResponse
    {
        public List<ServerInfo> servers;
    }

    /// <summary>
    /// Health check response from game server.
    /// </summary>
    [Serializable]
    public class HealthCheckResponse
    {
        public string status;
        public string server;
        public int players;
        public int maxPlayers;
        public int uptime;
        public long timestamp;
    }
}
