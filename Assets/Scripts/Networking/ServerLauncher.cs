using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Linq;

namespace WOS.Networking
{
    /// <summary>
    /// Automatic server launcher for headless builds
    /// Detects headless mode and automatically starts the server
    /// Supports command-line arguments for configuration
    ///
    /// Command-line arguments:
    /// -server : Force server mode
    /// -port <number> : Set server port (default: 7777)
    /// -maxplayers <number> : Set max connections (default: 300)
    /// -scene <name> : Set starting scene (default: PortHarbor)
    ///
    /// Example: WOS2.3_Server.exe -server -port 7777 -maxplayers 100 -scene PortHarbor
    /// </summary>
    public class ServerLauncher : MonoBehaviour
    {
        [Header("Server Configuration")]
        [Tooltip("Auto-start server in headless builds")]
        public bool autoStartInHeadless = true;

        [Tooltip("Server port (can be overridden by command-line)")]
        public ushort defaultPort = 7777;

        [Tooltip("Maximum concurrent connections")]
        public int defaultMaxConnections = 300;

        [Tooltip("Scene to load when server starts")]
        public string serverStartScene = "PortHarbor";

        [Header("Logging")]
        [Tooltip("Enable verbose server logging")]
        public bool verboseLogging = true;

        private NetworkManager networkManager;
        private bool serverStarted = false;

        private void Awake()
        {
            // Persist across scene loads
            DontDestroyOnLoad(gameObject);

            ParseCommandLineArguments();
        }

        private void Start()
        {
            InitializeNetworkManager();
            CheckAndStartServer();
        }

        private void InitializeNetworkManager()
        {
            networkManager = FindFirstObjectByType<NetworkManager>();

            if (networkManager == null)
            {
                LogError("NetworkManager not found! Cannot start server.");
                return;
            }

            // Apply configuration
            var transport = networkManager.GetComponent<Mirror.TelepathyTransport>();
            if (transport != null)
            {
                transport.port = defaultPort;
                Log($"Server configured on port {defaultPort}");
            }

            networkManager.maxConnections = defaultMaxConnections;
            Log($"Max connections set to {defaultMaxConnections}");
        }

        private void CheckAndStartServer()
        {
            if (serverStarted) return;

            bool shouldStartServer = false;

            // Check if headless build
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Log("🖥️ Headless build detected");
                shouldStartServer = autoStartInHeadless;
            }

            // Check for command-line server flag
            string[] args = System.Environment.GetCommandLineArgs();
            if (args.Contains("-server") || args.Contains("--server"))
            {
                Log("🚩 Server flag detected in command-line arguments");
                shouldStartServer = true;
            }

            if (shouldStartServer)
            {
                StartServer();
            }
            else
            {
                Log("Client mode - not starting server automatically");
            }
        }

        private void StartServer()
        {
            if (networkManager == null)
            {
                LogError("Cannot start server - NetworkManager is null");
                return;
            }

            if (serverStarted)
            {
                LogWarning("Server already started");
                return;
            }

            try
            {
                Log($"🌊 Starting WOS2.3 Dedicated Server...");
                Log($"Port: {defaultPort}");
                Log($"Max Players: {defaultMaxConnections}");
                Log($"Start Scene: {serverStartScene}");

                // Load the server start scene if needed
                if (!string.IsNullOrEmpty(serverStartScene) && SceneManager.GetActiveScene().name != serverStartScene)
                {
                    Log($"Loading scene: {serverStartScene}");
                    SceneManager.LoadScene(serverStartScene);
                }

                // Start the server
                networkManager.StartServer();
                serverStarted = true;

                Log("✅ Server started successfully!");
                Log($"Players can connect to this server using its IP address and port {defaultPort}");

                // Print connection info
                PrintConnectionInfo();
            }
            catch (Exception e)
            {
                LogError($"Failed to start server: {e.Message}");
                LogError($"Stack trace: {e.StackTrace}");

                // In production, you might want to exit the application
#if !UNITY_EDITOR
                Application.Quit(1); // Exit with error code
#endif
            }
        }

        private void ParseCommandLineArguments()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-port":
                    case "--port":
                        if (i + 1 < args.Length && ushort.TryParse(args[i + 1], out ushort port))
                        {
                            defaultPort = port;
                            Log($"Command-line: Port set to {port}");
                        }
                        break;

                    case "-maxplayers":
                    case "--maxplayers":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int maxPlayers))
                        {
                            defaultMaxConnections = maxPlayers;
                            Log($"Command-line: Max players set to {maxPlayers}");
                        }
                        break;

                    case "-scene":
                    case "--scene":
                        if (i + 1 < args.Length)
                        {
                            serverStartScene = args[i + 1];
                            Log($"Command-line: Start scene set to {serverStartScene}");
                        }
                        break;

                    case "-verbose":
                    case "--verbose":
                        verboseLogging = true;
                        Log("Command-line: Verbose logging enabled");
                        break;

                    case "-server":
                    case "--server":
                        Log("Command-line: Server mode enabled");
                        break;
                }
            }
        }

        private void PrintConnectionInfo()
        {
            // Get local IP addresses
            string localIP = "Unknown";
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                LogWarning($"Could not determine local IP: {e.Message}");
            }

            Log("═══════════════════════════════════════════");
            Log("🌐 SERVER CONNECTION INFORMATION");
            Log("═══════════════════════════════════════════");
            Log($"Local IP:  {localIP}:{defaultPort}");
            Log($"Localhost: 127.0.0.1:{defaultPort}");
            Log("");
            Log("Players can connect using the Local IP address");
            Log("For internet play, use your public IP or Edgegap");
            Log("═══════════════════════════════════════════");
        }

        private void OnApplicationQuit()
        {
            if (serverStarted && networkManager != null)
            {
                Log("🛑 Shutting down server...");
                networkManager.StopServer();
            }
        }

        #region Logging Helpers

        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[ServerLauncher] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[ServerLauncher] ⚠️ {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[ServerLauncher] ❌ {message}");
        }

        #endregion

        #region Public API (for manual server control)

        /// <summary>
        /// Manually start the server (useful for testing in Editor)
        /// </summary>
        [ContextMenu("Start Server Manually")]
        public void StartServerManually()
        {
            if (!serverStarted)
            {
                StartServer();
            }
            else
            {
                LogWarning("Server is already running");
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        [ContextMenu("Stop Server")]
        public void StopServer()
        {
            if (serverStarted && networkManager != null)
            {
                networkManager.StopServer();
                serverStarted = false;
                Log("Server stopped");
            }
        }

        /// <summary>
        /// Get server status information
        /// </summary>
        public string GetServerStatus()
        {
            if (!serverStarted) return "Server not running";

            if (networkManager != null && NetworkServer.active)
            {
                int connections = NetworkServer.connections.Count;
                return $"Server running | {connections}/{defaultMaxConnections} players | Port {defaultPort}";
            }

            return "Server state unknown";
        }

        #endregion
    }
}
