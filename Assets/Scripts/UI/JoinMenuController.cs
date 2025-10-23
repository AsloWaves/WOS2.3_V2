using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;
using WOS.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.MUIP;

namespace WOS.UI
{
    /// <summary>
    /// Handles Join panel - connects to remote server using IP address
    /// Includes server status checking, auto-refresh, and secure server browser integration.
    ///
    /// SECURITY: Uses ServerBrowserManager to fetch servers from YOUR backend API.
    /// Backend keeps Edgegap API token secure (never exposed to clients).
    /// </summary>
    public class JoinMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Status text showing server status and connection progress")]
        public TextMeshProUGUI serverStatusText;

        [Tooltip("MUIP Connect button to enable/disable based on server status")]
        public ButtonManager connectButton;

        [Tooltip("MUIP Back button to return to main menu")]
        public ButtonManager backButton;

        [Header("Server Browser (Secure)")]
        [Tooltip("ServerBrowserManager that fetches servers from YOUR backend (not Edgegap directly)")]
        public ServerBrowserManager serverBrowser;

        [Tooltip("Auto-select best server on start")]
        public bool autoSelectBestServer = true;

        [Header("Configuration")]
        [Tooltip("Server configuration (auto-populated from server browser or fallback)")]
        public ServerConfig serverConfig;

        [Tooltip("Scene to load after connecting (usually Main scene)")]
        public string gameSceneName = "Main";

        [Tooltip("How often to check server status (seconds)")]
        public float statusCheckInterval = 30f;

        [Tooltip("Timeout for server status check (seconds)")]
        public float statusCheckTimeout = 5f;

        [Tooltip("Port for HTTP health endpoint (must match server configuration)")]
        public int healthCheckPort = 30407;  // Edgegap external health port (maps to internal 8080)

        private NetworkManager networkManager;
        private NetworkAddressManager addressManager;
        private ServerStatus currentServerStatus = ServerStatus.Checking;
        private Coroutine statusCheckCoroutine;
        private bool isConnecting = false;
        private bool runtimeTextCreated = false;

        private void Start()
        {
            Debug.Log("[JoinMenu] ========== START INITIALIZING ==========");

            InitializeComponents();

            // Log current state before auto-detection
            Debug.Log($"[JoinMenu] serverStatusText before auto-detect: {(serverStatusText == null ? "NULL" : serverStatusText.gameObject.name)}");
            Debug.Log($"[JoinMenu] connectButton before auto-detect: {(connectButton == null ? "NULL" : connectButton.gameObject.name)}");
            Debug.Log($"[JoinMenu] backButton before auto-detect: {(backButton == null ? "NULL" : backButton.gameObject.name)}");

            // Auto-detect UI components if not assigned
            AutoDetectUIComponents();

            // Log current state after auto-detection
            Debug.Log($"[JoinMenu] serverStatusText after auto-detect: {(serverStatusText == null ? "NULL" : serverStatusText.gameObject.name)}");
            Debug.Log($"[JoinMenu] connectButton after auto-detect: {(connectButton == null ? "NULL" : connectButton.gameObject.name)}");
            Debug.Log($"[JoinMenu] backButton after auto-detect: {(backButton == null ? "NULL" : backButton.gameObject.name)}");

            // Validate UI references after auto-detection
            if (serverStatusText == null)
            {
                Debug.LogError("[JoinMenu] ⚠️ Server Status Text could not be found! Please assign manually.");

                // List all TextMeshProUGUI components found
                TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
                Debug.LogError($"[JoinMenu] Found {allTexts.Length} TextMeshProUGUI components in children:");
                foreach (var text in allTexts)
                {
                    Debug.LogError($"[JoinMenu]   - {text.gameObject.name} (active: {text.gameObject.activeInHierarchy})");
                }
            }
            if (connectButton == null)
            {
                Debug.LogError("[JoinMenu] ⚠️ Connect Button could not be found! Please assign manually.");

                // List all ButtonManager components found
                ButtonManager[] allButtons = GetComponentsInChildren<ButtonManager>(true);
                Debug.LogError($"[JoinMenu] Found {allButtons.Length} ButtonManager components in children:");
                foreach (var btn in allButtons)
                {
                    Debug.LogError($"[JoinMenu]   - {btn.gameObject.name} (active: {btn.gameObject.activeInHierarchy})");
                }
            }
            if (backButton == null)
            {
                Debug.LogWarning("[JoinMenu] ⚠️ Back Button could not be found! Please assign manually.");
            }

            // SECURE SERVER BROWSER INTEGRATION
            InitializeServerBrowser();

            // Start server status checking
            UpdateStatus("Checking server status...");
            StartStatusChecking();

            Debug.Log("[JoinMenu] ========== INITIALIZATION COMPLETE ==========");
        }

        /// <summary>
        /// Initialize secure server browser integration.
        /// Fetches servers from YOUR backend (not Edgegap directly).
        /// </summary>
        private void InitializeServerBrowser()
        {
            if (serverBrowser != null)
            {
                Debug.Log("[JoinMenu] 🌐 ServerBrowserManager detected - fetching servers from backend...");

                // Subscribe to server list updates
                serverBrowser.OnServersUpdated += OnServerListUpdated;
                serverBrowser.OnError += OnServerBrowserError;

                // Fetch servers
                serverBrowser.RefreshServers();
            }
            else
            {
                Debug.LogWarning("[JoinMenu] ⚠️ ServerBrowserManager not assigned - using hardcoded ServerConfig");
            }
        }

        /// <summary>
        /// Called when server list is updated from backend.
        /// </summary>
        private void OnServerListUpdated(List<ServerInfo> servers)
        {
            Debug.Log($"[JoinMenu] 📋 Received {servers.Count} servers from backend");

            // Log all servers
            foreach (var server in servers)
            {
                string healthStatus = server.isHealthy ? "✅" : "❌";
                Debug.Log($"[JoinMenu]   {healthStatus} {server.GetDisplayName()} ({server.currentPlayers}/{server.maxPlayers} players) - {server.pingMs}ms - {server.GetConnectionAddress()}");
            }

            // Auto-select best server if enabled
            if (autoSelectBestServer && servers.Count > 0)
            {
                ServerInfo bestServer = serverBrowser.GetBestServer();
                if (bestServer != null)
                {
                    UpdateServerConfigFromServerInfo(bestServer);
                    Debug.Log($"[JoinMenu] 🎯 Auto-selected best server: {bestServer.GetDisplayName()}");

                    // Restart health check with updated server config
                    StopStatusChecking();
                    StartStatusChecking();
                    Debug.Log("[JoinMenu] 🔄 Restarted health check with updated server config");
                }
            }
        }

        /// <summary>
        /// Called when server browser encounters an error.
        /// </summary>
        private void OnServerBrowserError(string error)
        {
            Debug.LogError($"[JoinMenu] ❌ Server browser error: {error}");
            Debug.LogWarning("[JoinMenu] Falling back to hardcoded ServerConfig");
        }

        /// <summary>
        /// Update ServerConfig with data from ServerInfo (from backend).
        /// </summary>
        private void UpdateServerConfigFromServerInfo(ServerInfo serverInfo)
        {
            if (serverConfig == null)
            {
                Debug.LogError("[JoinMenu] ⚠️ ServerConfig is null, cannot update!");
                return;
            }

            // Update ServerConfig fields
            serverConfig.serverAddress = serverInfo.GetConnectionAddress();
            serverConfig.serverLocation = $"{serverInfo.city}, {serverInfo.country}";

            Debug.Log($"[JoinMenu] ✅ Updated ServerConfig:");
            Debug.Log($"[JoinMenu]   Address: {serverConfig.serverAddress}");
            Debug.Log($"[JoinMenu]   Location: {serverConfig.serverLocation}");
            Debug.Log($"[JoinMenu]   Players: {serverInfo.currentPlayers}/{serverInfo.maxPlayers}");
            Debug.Log($"[JoinMenu]   Ping: {serverInfo.pingMs}ms");
        }

        /// <summary>
        /// Auto-detect UI components if not assigned in Inspector
        /// Searches children for matching components by name and type
        /// </summary>
        private void AutoDetectUIComponents()
        {
            // Search for Server Status Text if not assigned
            if (serverStatusText == null)
            {
                Debug.Log("[JoinMenu] Auto-detecting Server Status Text...");

                // Try to find by common names
                string[] statusTextNames = { "ServerStatusText", "StatusText", "Status", "ServerStatus", "Text" };

                foreach (string name in statusTextNames)
                {
                    Transform foundTransform = transform.Find(name);
                    if (foundTransform != null)
                    {
                        serverStatusText = foundTransform.GetComponent<TextMeshProUGUI>();
                        if (serverStatusText != null)
                        {
                            Debug.Log($"[JoinMenu] ✅ Found Server Status Text: {name}");
                            break;
                        }
                    }
                }

                // If still not found, search all children
                if (serverStatusText == null)
                {
                    TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var text in allTexts)
                    {
                        // Look for text with "status" or "server" in the name
                        string textName = text.gameObject.name.ToLower();
                        if (textName.Contains("status") || textName.Contains("server"))
                        {
                            serverStatusText = text;
                            Debug.Log($"[JoinMenu] ✅ Auto-detected Server Status Text: {text.gameObject.name}");
                            break;
                        }
                    }

                    // Last resort: use first TextMeshProUGUI found (if only one exists)
                    if (serverStatusText == null && allTexts.Length == 1)
                    {
                        serverStatusText = allTexts[0];
                        Debug.Log($"[JoinMenu] ⚠️ Using only TextMeshProUGUI found: {allTexts[0].gameObject.name}");
                    }
                }
            }

            // Search for Connect Button if not assigned
            if (connectButton == null)
            {
                Debug.Log("[JoinMenu] Auto-detecting Connect Button...");

                string[] connectButtonNames = { "ConnectButton", "Connect", "JoinButton", "Join", "Button" };

                foreach (string name in connectButtonNames)
                {
                    Transform foundTransform = transform.Find(name);
                    if (foundTransform != null)
                    {
                        connectButton = foundTransform.GetComponent<ButtonManager>();
                        if (connectButton != null)
                        {
                            Debug.Log($"[JoinMenu] ✅ Found Connect Button: {name}");
                            break;
                        }
                    }
                }

                // Search all children
                if (connectButton == null)
                {
                    ButtonManager[] allButtons = GetComponentsInChildren<ButtonManager>(true);
                    foreach (var button in allButtons)
                    {
                        string btnName = button.gameObject.name.ToLower();
                        if (btnName.Contains("connect") || btnName.Contains("join"))
                        {
                            connectButton = button;
                            Debug.Log($"[JoinMenu] ✅ Auto-detected Connect Button: {button.gameObject.name}");
                            break;
                        }
                    }
                }
            }

            // Search for Back Button if not assigned
            if (backButton == null)
            {
                Debug.Log("[JoinMenu] Auto-detecting Back Button...");

                string[] backButtonNames = { "BackButton", "Back", "ReturnButton", "Return", "Cancel" };

                foreach (string name in backButtonNames)
                {
                    Transform foundTransform = transform.Find(name);
                    if (foundTransform != null)
                    {
                        backButton = foundTransform.GetComponent<ButtonManager>();
                        if (backButton != null)
                        {
                            Debug.Log($"[JoinMenu] ✅ Found Back Button: {name}");
                            break;
                        }
                    }
                }

                // Search all children
                if (backButton == null)
                {
                    ButtonManager[] allButtons = GetComponentsInChildren<ButtonManager>(true);
                    foreach (var button in allButtons)
                    {
                        string btnName = button.gameObject.name.ToLower();
                        if (btnName.Contains("back") || btnName.Contains("return") || btnName.Contains("cancel"))
                        {
                            backButton = button;
                            Debug.Log($"[JoinMenu] ✅ Auto-detected Back Button: {button.gameObject.name}");
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a new standalone TextMeshProUGUI at runtime if the existing one is controlled by MUIP
        /// This is a workaround for MUIP components that override text updates
        /// </summary>
        private void CreateRuntimeStatusText()
        {
            if (runtimeTextCreated)
            {
                Debug.Log("[JoinMenu] Runtime status text already created, skipping");
                return;
            }

            Debug.Log("[JoinMenu] Creating new standalone TextMeshProUGUI for server status...");

            // Create new GameObject for status text
            GameObject statusTextObj = new GameObject("RuntimeServerStatusText");
            statusTextObj.transform.SetParent(transform, false);

            // Add RectTransform (required for UI)
            RectTransform rectTransform = statusTextObj.AddComponent<RectTransform>();

            // Position it at the top center of the panel
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -50f); // 50 pixels from top
            rectTransform.sizeDelta = new Vector2(600f, 40f); // Width and height

            // Add TextMeshProUGUI component
            serverStatusText = statusTextObj.AddComponent<TextMeshProUGUI>();

            // Configure text properties
            serverStatusText.fontSize = 20;
            serverStatusText.alignment = TextAlignmentOptions.Center;
            serverStatusText.color = Color.white;
            serverStatusText.text = "Initializing...";
            serverStatusText.enableWordWrapping = false;
            serverStatusText.overflowMode = TextOverflowModes.Overflow;

            runtimeTextCreated = true;

            Debug.Log("[JoinMenu] ✅ Created standalone runtime TextMeshProUGUI");
            Debug.Log("[JoinMenu] Position: Top center, 50px from top");
            Debug.Log("[JoinMenu] This text is NOT controlled by any MUIP component");
        }

        private void OnEnable()
        {
            // Subscribe to Mirror network events for status updates
            NetworkClient.OnConnectedEvent += OnClientConnected;
            NetworkClient.OnDisconnectedEvent += OnClientDisconnected;

            // Resume status checking when panel becomes active
            if (statusCheckCoroutine == null && !isConnecting)
            {
                StartStatusChecking();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            NetworkClient.OnConnectedEvent -= OnClientConnected;
            NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;

            // Unsubscribe from server browser events
            if (serverBrowser != null)
            {
                serverBrowser.OnServersUpdated -= OnServerListUpdated;
                serverBrowser.OnError -= OnServerBrowserError;
            }

            // Stop status checking when panel is hidden
            StopStatusChecking();
        }

        private void InitializeComponents()
        {
            // Find NetworkManager
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("[JoinMenu] NetworkManager not found in scene!");
            }

            // Find or create NetworkAddressManager
            addressManager = FindFirstObjectByType<NetworkAddressManager>();
            if (addressManager == null)
            {
                Debug.Log("[JoinMenu] Creating NetworkAddressManager...");
                GameObject managerObj = new GameObject("NetworkAddressManager");
                addressManager = managerObj.AddComponent<NetworkAddressManager>();
            }
        }

        private string GetServerAddress()
        {
            if (serverConfig != null)
            {
                return serverConfig.GetFullAddress();
            }

            // Fallback to saved IP if no config
            if (addressManager != null)
            {
                string savedIP = addressManager.GetLastServerIP();
                if (!string.IsNullOrEmpty(savedIP))
                {
                    return savedIP;
                }
            }

            return "127.0.0.1:7777"; // Final fallback
        }

        private string GetDisplayAddress()
        {
            if (serverConfig != null)
            {
                string address = serverConfig.GetFullAddress();
                if (serverConfig.showServerInfo && !string.IsNullOrEmpty(serverConfig.serverLocation))
                {
                    return $"{address} ({serverConfig.serverLocation})";
                }
                return address;
            }

            return GetServerAddress();
        }

        #region Public Button Methods

        /// <summary>
        /// Connect to server using entered IP
        /// Called by "Connect" button
        /// Supports formats: "192.168.1.1" or "192.168.1.1:7777"
        /// </summary>
        public void OnConnectButtonClicked()
        {
            // Check server status first
            if (currentServerStatus != ServerStatus.Up)
            {
                UpdateStatus("Cannot connect: Server is down", true);
                return;
            }

            if (networkManager == null)
            {
                UpdateStatus("Error: NetworkManager not found!", true);
                return;
            }

            // Stop status checking during connection
            isConnecting = true;
            StopStatusChecking();

            // Get server address from ServerConfig
            string serverAddress = GetServerAddress();

            // Parse IP and optional port
            string serverIP;
            ushort? serverPort = null;

            if (serverAddress.Contains(":"))
            {
                // Format: IP:Port
                string[] parts = serverAddress.Split(':');
                serverIP = parts[0];

                if (parts.Length == 2 && ushort.TryParse(parts[1], out ushort port))
                {
                    serverPort = port;
                }
                else
                {
                    UpdateStatus($"Invalid port in: {serverAddress}", true);
                    isConnecting = false;
                    StartStatusChecking();
                    return;
                }
            }
            else
            {
                // Just IP, use default transport port
                serverIP = serverAddress;
            }

            // Validate IP format
            if (!IsValidIPAddress(serverIP))
            {
                UpdateStatus($"Invalid IP: {serverIP}", true);
                isConnecting = false;
                StartStatusChecking();
                return;
            }

            // Configure port if specified
            if (serverPort.HasValue)
            {
                ConfigureTransportPort(serverPort.Value);
                Debug.Log($"[JoinMenu] Configured transport port: {serverPort.Value}");
            }

            // Save full address for next time
            if (addressManager != null)
            {
                addressManager.SaveServerIP(serverAddress);
            }

            // Connect to server
            networkManager.networkAddress = serverIP;
            string displayAddress = serverPort.HasValue ? $"{serverIP}:{serverPort}" : serverIP;
            UpdateStatus($"Connecting to {displayAddress}...", false, true);

            try
            {
                networkManager.StartClient();
                Debug.Log($"[JoinMenu] Client started, connecting to {displayAddress}");
            }
            catch (System.Exception e)
            {
                UpdateStatus($"Connection failed: {e.Message}", true);
                Debug.LogError($"[JoinMenu] Connection error: {e}");
                isConnecting = false;
                StartStatusChecking();
            }
        }

        /// <summary>
        /// Called when client successfully connects to server
        /// </summary>
        private void OnClientConnected()
        {
            UpdateStatus("Connected! Loading game...", false, false);
            Debug.Log("[JoinMenu] Successfully connected to server");
        }

        /// <summary>
        /// Called when client disconnects from server
        /// </summary>
        private void OnClientDisconnected()
        {
            // Reset connecting flag
            isConnecting = false;

            // Only show disconnect message if we're still on the join menu
            if (this != null && gameObject.activeInHierarchy)
            {
                UpdateStatus("Disconnected from server", true, false);
                Debug.Log("[JoinMenu] Disconnected from server");

                // Resume status checking
                StartStatusChecking();
            }
        }

        /// <summary>
        /// Configure transport port dynamically
        /// Supports KCP, Telepathy, and other Mirror transports
        /// </summary>
        private void ConfigureTransportPort(ushort port)
        {
            if (networkManager == null) return;

            var transport = networkManager.GetComponent<Mirror.PortTransport>();
            if (transport != null)
            {
                transport.Port = port;
                Debug.Log($"[JoinMenu] Transport port set to {port}");
            }
            else
            {
                Debug.LogWarning("[JoinMenu] No PortTransport found - port not configured");
            }
        }

        /// <summary>
        /// Return to main menu
        /// Called by "Back" button
        /// </summary>
        public void OnBackButtonClicked()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowMainMenu();
            }
            else
            {
                Debug.LogWarning("[JoinMenu] MenuManager not found!");
            }
        }

        #endregion

        #region Server Status Checking

        /// <summary>
        /// Start periodic server status checking
        /// </summary>
        private void StartStatusChecking()
        {
            if (statusCheckCoroutine != null)
            {
                StopCoroutine(statusCheckCoroutine);
            }

            statusCheckCoroutine = StartCoroutine(ServerStatusCheckLoop());
        }

        /// <summary>
        /// Stop server status checking
        /// </summary>
        private void StopStatusChecking()
        {
            if (statusCheckCoroutine != null)
            {
                StopCoroutine(statusCheckCoroutine);
                statusCheckCoroutine = null;
            }
        }

        /// <summary>
        /// Periodic server status check loop
        /// </summary>
        private IEnumerator ServerStatusCheckLoop()
        {
            while (true)
            {
                yield return CheckServerStatus();

                // Wait before next check
                yield return new WaitForSeconds(statusCheckInterval);
            }
        }

        /// <summary>
        /// Check if server is responding
        /// NOTE: UDP servers (like KCP) cannot be checked with TCP connections
        /// This uses a simplified check - for production, add HTTP health endpoint
        /// </summary>
        private IEnumerator CheckServerStatus()
        {
            Debug.Log("[JoinMenu] ========== SERVER CHECK START ==========");

            if (isConnecting)
            {
                Debug.Log("[JoinMenu] Skip check - currently connecting");
                yield break;
            }

            UpdateServerStatus(ServerStatus.Checking);

            // Get server address from ServerConfig
            string serverAddress = GetServerAddress();
            Debug.Log($"[JoinMenu] Server address from config: '{serverAddress}'");

            if (string.IsNullOrEmpty(serverAddress))
            {
                Debug.LogError("[JoinMenu] Server address is empty!");
                UpdateServerStatus(ServerStatus.Down);
                yield break;
            }

            string serverIP;
            ushort serverPort;

            // Parse IP and port
            if (serverAddress.Contains(":"))
            {
                string[] parts = serverAddress.Split(':');
                serverIP = parts[0];
                if (!ushort.TryParse(parts[1], out serverPort))
                {
                    Debug.LogError($"[JoinMenu] Invalid port in address: {serverAddress}");
                    UpdateServerStatus(ServerStatus.Down);
                    yield break;
                }
            }
            else
            {
                serverIP = serverAddress;
                serverPort = 7777; // Default
            }

            Debug.Log($"[JoinMenu] Parsed - IP: {serverIP}, Port: {serverPort}");

            // Validate IP format
            if (!IsValidIPAddress(serverIP))
            {
                Debug.LogError($"[JoinMenu] Invalid IP address format: {serverIP}");
                UpdateServerStatus(ServerStatus.Down);
                yield break;
            }

            // Perform HTTP health check
            Debug.Log($"[JoinMenu] Performing HTTP health check to {serverIP}:{healthCheckPort}/health...");

            string healthUrl = $"http://{serverIP}:{healthCheckPort}/health";
            Debug.Log($"[JoinMenu] Health check URL: {healthUrl}");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(healthUrl))
            {
                webRequest.timeout = (int)statusCheckTimeout;

                // Send request
                yield return webRequest.SendWebRequest();

                Debug.Log("[JoinMenu] ========== SERVER CHECK END ==========");

                // Check result
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[JoinMenu] ✅ Health check SUCCESS - Response: {webRequest.downloadHandler.text}");

                    // Try to parse player count from response (optional)
                    try
                    {
                        string response = webRequest.downloadHandler.text;
                        if (response.Contains("\"players\""))
                        {
                            // Simple extraction (could use JSON parser for production)
                            int playerIndex = response.IndexOf("\"players\":");
                            if (playerIndex >= 0)
                            {
                                string playerSubstring = response.Substring(playerIndex + 10);
                                string playerCountStr = playerSubstring.Substring(0, playerSubstring.IndexOf(','));
                                Debug.Log($"[JoinMenu] 👥 Server has {playerCountStr.Trim()} players");
                            }
                        }
                    }
                    catch { /* Ignore parsing errors */ }

                    UpdateServerStatus(ServerStatus.Up);
                }
                else
                {
                    Debug.LogWarning($"[JoinMenu] ❌ Health check FAILED - Error: {webRequest.error}");
                    Debug.LogWarning($"[JoinMenu] Result: {webRequest.result}");
                    Debug.LogWarning($"[JoinMenu] Response Code: {webRequest.responseCode}");
                    UpdateServerStatus(ServerStatus.Down);
                }
            }
        }

        /// <summary>
        /// Update server status and UI
        /// </summary>
        private void UpdateServerStatus(ServerStatus status)
        {
            currentServerStatus = status;

            Debug.Log($"[JoinMenu] UpdateServerStatus called with: {status}");
            Debug.Log($"[JoinMenu] serverStatusText is null? {serverStatusText == null}");

            // Enable/disable connect button based on status
            if (connectButton != null)
            {
                connectButton.isInteractable = (status == ServerStatus.Up);
                Debug.Log($"[JoinMenu] Connect button interactable set to: {connectButton.isInteractable}");
            }

            // Update combined status display
            if (serverStatusText != null)
            {
                Debug.Log($"[JoinMenu] Before status update - text: '{serverStatusText.text}'");
                Debug.Log($"[JoinMenu] Component active? {serverStatusText.gameObject.activeInHierarchy}");
                Debug.Log($"[JoinMenu] Component enabled? {serverStatusText.enabled}");

                // FORCE the GameObject active if it's not
                if (!serverStatusText.gameObject.activeInHierarchy)
                {
                    serverStatusText.gameObject.SetActive(true);
                    Debug.LogWarning("[JoinMenu] ⚠️ Forced serverStatusText GameObject active!");
                }

                string newText = "";
                Color newColor = Color.white;

                switch (status)
                {
                    case ServerStatus.Up:
                        newText = $"Server Up - {GetDisplayAddress()}";
                        newColor = Color.green;
                        break;

                    case ServerStatus.Down:
                        newText = "Server Down for Maintenance";
                        newColor = Color.red;
                        break;

                    case ServerStatus.Checking:
                        newText = "Checking Server Status...";
                        newColor = Color.yellow;
                        break;
                }

                serverStatusText.text = newText;
                serverStatusText.color = newColor;

                // Force TextMeshPro to update immediately
                serverStatusText.ForceMeshUpdate();

                Debug.Log($"[JoinMenu] After status update - text: '{serverStatusText.text}'");
                Debug.Log($"[JoinMenu] Color set to: {serverStatusText.color}");

                // Check if text was cleared by something else
                if (string.IsNullOrEmpty(serverStatusText.text) || serverStatusText.text == "​")
                {
                    Debug.LogError("[JoinMenu] ⚠️ TEXT WAS CLEARED BY ANOTHER COMPONENT! The 'Text' GameObject likely has a MUIP component controlling it.");
                    Debug.LogError("[JoinMenu] ⚠️ Solution: Find a different TextMeshProUGUI or disable MUIP auto-update on this component.");
                }
            }
            else
            {
                Debug.LogError("[JoinMenu] serverStatusText is NULL in UpdateServerStatus!");
            }

            Debug.Log($"[JoinMenu] Server status: {status}");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validate IP address format (supports localhost, IPv4, hostnames)
        /// </summary>
        private bool IsValidIPAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            if (ip == "localhost") return true;

            // Check for IPv4 format (xxx.xxx.xxx.xxx)
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

            // Assume hostname if not IPv4 (e.g., edgegap URLs)
            return ip.Contains(".") || ip == "localhost";
        }

        /// <summary>
        /// Update status text with message
        /// </summary>
        private void UpdateStatus(string message, bool isError = false, bool isConnecting = false)
        {
            Debug.Log($"[JoinMenu] UpdateStatus called with: '{message}'");
            Debug.Log($"[JoinMenu] serverStatusText is null? {serverStatusText == null}");

            if (serverStatusText != null)
            {
                Debug.Log($"[JoinMenu] Before update - text: '{serverStatusText.text}'");
                Debug.Log($"[JoinMenu] Component active? {serverStatusText.gameObject.activeInHierarchy}");
                Debug.Log($"[JoinMenu] Component enabled? {serverStatusText.enabled}");

                serverStatusText.text = message;

                Debug.Log($"[JoinMenu] After update - text: '{serverStatusText.text}'");

                if (isError)
                {
                    serverStatusText.color = Color.red;
                }
                else if (isConnecting)
                {
                    serverStatusText.color = Color.yellow;
                }
                else
                {
                    serverStatusText.color = Color.white;
                }

                Debug.Log($"[JoinMenu] Color set to: {serverStatusText.color}");
            }
            else
            {
                Debug.LogError("[JoinMenu] serverStatusText is NULL in UpdateStatus!");
            }
        }

        #endregion
    }

    /// <summary>
    /// Server status states
    /// </summary>
    public enum ServerStatus
    {
        Checking,   // Currently checking server status
        Up,         // Server is online and accepting connections
        Down        // Server is offline or not responding
    }
}
