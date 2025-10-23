using UnityEngine;
using Mirror;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace WOS.Networking
{
    /// <summary>
    /// HTTP health endpoint for server status monitoring.
    /// Provides a simple HTTP server that responds to health checks.
    /// Only active in headless server builds.
    ///
    /// Endpoint: GET http://SERVER_IP:8080/health
    /// Response: {"status":"ok","players":5,"uptime":3600,"scene":"Main"}
    /// </summary>
    public class ServerHealthEndpoint : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Port for HTTP health endpoint (different from game port)")]
        public int healthPort = 8080;

        [Tooltip("Enable the HTTP health endpoint (only works in server builds)")]
        public bool enableHealthEndpoint = true;

        [Header("Status Info")]
        [Tooltip("Auto-detected from NetworkManager")]
        public string serverStatus = "Stopped";

        [Tooltip("Current player count")]
        public int playerCount = 0;

        [Tooltip("Server uptime in seconds")]
        public float uptime = 0f;

        private HttpListener httpListener;
        private Thread listenerThread;
        private bool isRunning = false;
        private NetworkManager networkManager;
        private float startTime;

        private void Start()
        {
            // Only run in headless server builds
            if (!SystemInfo.graphicsDeviceType.ToString().Contains("Null"))
            {
                Debug.Log("[HealthEndpoint] Not running in headless mode - health endpoint disabled");
                return;
            }

            if (!enableHealthEndpoint)
            {
                Debug.Log("[HealthEndpoint] Health endpoint disabled in configuration");
                return;
            }

            // Find NetworkManager
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogWarning("[HealthEndpoint] NetworkManager not found - health endpoint disabled");
                return;
            }

            startTime = Time.time;

            // Start HTTP listener
            StartHealthEndpoint();
        }

        private void Update()
        {
            // Update runtime stats
            if (isRunning && networkManager != null)
            {
                uptime = Time.time - startTime;
                playerCount = NetworkServer.connections.Count;
                serverStatus = NetworkServer.active ? "Running" : "Stopped";
            }
        }

        private void StartHealthEndpoint()
        {
            try
            {
                Debug.Log($"[HealthEndpoint] 🏥 Starting HTTP health endpoint on port {healthPort}...");

                // Create HttpListener
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://+:{healthPort}/");
                httpListener.Start();

                isRunning = true;

                // Start listener thread
                listenerThread = new Thread(new ThreadStart(ListenForRequests));
                listenerThread.IsBackground = true;
                listenerThread.Start();

                Debug.Log($"[HealthEndpoint] ✅ Health endpoint started successfully");
                Debug.Log($"[HealthEndpoint] 🌐 Accessible at: http://SERVER_IP:{healthPort}/health");
                Debug.Log($"[HealthEndpoint] 💡 Configure Edgegap to expose port {healthPort} (TCP/HTTP)");
            }
            catch (HttpListenerException e)
            {
                Debug.LogError($"[HealthEndpoint] ❌ Failed to start HTTP listener: {e.Message}");
                Debug.LogError($"[HealthEndpoint] Error code: {e.ErrorCode}");

                if (e.ErrorCode == 5) // Access denied
                {
                    Debug.LogError($"[HealthEndpoint] Access denied - may need admin privileges or port {healthPort} is in use");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HealthEndpoint] ❌ Unexpected error starting health endpoint: {e.Message}");
            }
        }

        private void ListenForRequests()
        {
            Debug.Log("[HealthEndpoint] 👂 Listener thread started");

            while (isRunning && httpListener != null && httpListener.IsListening)
            {
                try
                {
                    // Wait for request (blocking call)
                    HttpListenerContext context = httpListener.GetContext();
                    ThreadPool.QueueUserWorkItem((_) => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Listener stopped - exit gracefully
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[HealthEndpoint] Error in listener loop: {e.Message}");
                }
            }

            Debug.Log("[HealthEndpoint] 🛑 Listener thread stopped");
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                // Log request
                Debug.Log($"[HealthEndpoint] 📥 {request.HttpMethod} {request.Url.AbsolutePath} from {request.RemoteEndPoint}");

                // CORS headers (allow monitoring from web dashboards)
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                // Handle OPTIONS preflight
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                // Only allow GET requests
                if (request.HttpMethod != "GET")
                {
                    response.StatusCode = 405; // Method Not Allowed
                    byte[] errorBuffer = Encoding.UTF8.GetBytes("{\"error\":\"Method not allowed\"}");
                    response.ContentType = "application/json";
                    response.ContentLength64 = errorBuffer.Length;
                    response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
                    response.Close();
                    return;
                }

                // Route handling
                string path = request.Url.AbsolutePath.ToLower();

                if (path == "/" || path == "/health")
                {
                    HandleHealthCheck(response);
                }
                else if (path == "/info")
                {
                    HandleServerInfo(response);
                }
                else
                {
                    // 404 Not Found
                    response.StatusCode = 404;
                    byte[] notFoundBuffer = Encoding.UTF8.GetBytes("{\"error\":\"Endpoint not found\"}");
                    response.ContentType = "application/json";
                    response.ContentLength64 = notFoundBuffer.Length;
                    response.OutputStream.Write(notFoundBuffer, 0, notFoundBuffer.Length);
                    response.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HealthEndpoint] Error handling request: {e.Message}");
                try
                {
                    response.StatusCode = 500;
                    response.Close();
                }
                catch { }
            }
        }

        private void HandleHealthCheck(HttpListenerResponse response)
        {
            // Simple health check response
            string jsonResponse = $@"{{
    ""status"": ""ok"",
    ""server"": ""running"",
    ""players"": {playerCount},
    ""maxPlayers"": {(networkManager != null ? networkManager.maxConnections : 0)},
    ""uptime"": {(int)uptime},
    ""timestamp"": {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}
}}";

            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();

            Debug.Log($"[HealthEndpoint] ✅ Health check response sent: {playerCount} players, {(int)uptime}s uptime");
        }

        private void HandleServerInfo(HttpListenerResponse response)
        {
            // Detailed server info
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            string jsonResponse = $@"{{
    ""status"": ""{serverStatus}"",
    ""players"": {{
        ""current"": {playerCount},
        ""max"": {(networkManager != null ? networkManager.maxConnections : 0)}
    }},
    ""uptime"": {(int)uptime},
    ""scene"": ""{currentScene}"",
    ""version"": ""{Application.version}"",
    ""platform"": ""Linux Server"",
    ""timestamp"": {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}
}}";

            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();

            Debug.Log($"[HealthEndpoint] 📊 Server info response sent");
        }

        private void OnDestroy()
        {
            StopHealthEndpoint();
        }

        private void OnApplicationQuit()
        {
            StopHealthEndpoint();
        }

        private void StopHealthEndpoint()
        {
            if (!isRunning) return;

            Debug.Log("[HealthEndpoint] 🛑 Stopping health endpoint...");

            isRunning = false;

            try
            {
                if (httpListener != null && httpListener.IsListening)
                {
                    httpListener.Stop();
                    httpListener.Close();
                }

                if (listenerThread != null && listenerThread.IsAlive)
                {
                    listenerThread.Join(1000); // Wait up to 1 second
                }

                Debug.Log("[HealthEndpoint] ✅ Health endpoint stopped");
            }
            catch (Exception e)
            {
                Debug.LogError($"[HealthEndpoint] Error stopping health endpoint: {e.Message}");
            }
        }
    }
}
