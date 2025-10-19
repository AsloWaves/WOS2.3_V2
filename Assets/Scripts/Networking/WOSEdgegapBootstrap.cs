using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace WOS.Networking
{
    /// <summary>
    /// Edgegap bootstrap script for WOS2.3 Naval MMO
    /// Validates server configuration and port mappings for Edgegap deployment
    ///
    /// IMPORTANT: This is a standalone validation script that does NOT require
    /// the Edgegap plugin classes. It works in both Editor and Runtime.
    ///
    /// This script is OPTIONAL - the server will work without it.
    /// It's provided for configuration validation and debugging.
    ///
    /// Requirements:
    /// 1. Attach to GameObject in first scene (MainMenu)
    /// 2. Edgegap port mapping: 7777 TCP (Telepathy default)
    /// 3. NetworkManager must have networkAddress = "localhost" or "0.0.0.0"
    ///
    /// Validation:
    /// - Checks Mirror transport configuration
    /// - Verifies NetworkManager address is set correctly
    /// - Logs warnings if configuration issues detected
    /// </summary>
    public class WOSEdgegapBootstrap : MonoBehaviour
    {
        [Header("WOS2.3 Configuration")]
        [Tooltip("Expected server port (should match Edgegap port mapping)")]
        public ushort expectedPort = 7777;

        [Tooltip("Expected transport protocol (UDP for KCP, TCP for Telepathy)")]
        public string expectedProtocol = "UDP";

        [Header("Debug")]
        [Tooltip("Enable verbose logging for debugging")]
        public bool verboseLogging = true;

        private void Awake()
        {
            Log("🌊 WOSEdgegapBootstrap initialized");
            ValidateServerConfiguration();
        }

        /// <summary>
        /// Validate server configuration for Edgegap deployment
        /// Works in both Editor and Runtime
        /// </summary>
        private void ValidateServerConfiguration()
        {
            Log("═══════════════════════════════════════════");
            Log("🔍 WOS2.3 Server Configuration Check");
            Log("═══════════════════════════════════════════");

            // Check if headless
            bool isHeadless = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
            Log($"Headless Mode: {isHeadless}");

#if UNITY_EDITOR
            Log("Running in Unity Editor");
#else
            Log("Running in build (non-editor)");
#endif

            // Find NetworkManager
            NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager != null)
            {
                Log("✅ NetworkManager found");

                // Check network address
                string address = networkManager.networkAddress;
                Log($"   Network Address: {address}");
                if (!string.IsNullOrEmpty(address) && address != "localhost" && address != "0.0.0.0")
                {
                    LogWarning($"⚠️ Network address should be 'localhost' or '0.0.0.0' for dedicated servers");
                    LogWarning($"   Current: '{address}' - may prevent external connections");
                }

                // Check transport
                var transport = networkManager.GetComponent<Transport>();
                if (transport != null && transport is PortTransport)
                {
                    string transportName = transport.GetType().Name;
                    ushort port = ((PortTransport)transport).Port;

                    Log($"   Transport: {transportName}");
                    Log($"   Port: {port}");

                    // Check against expected configuration
                    if (port != expectedPort)
                    {
                        LogWarning($"⚠️ Port mismatch: Expected {expectedPort}, got {port}");
                        LogWarning($"   Update Edgegap port mapping to {port} or change transport port to {expectedPort}");
                    }
                    else
                    {
                        Log($"   ✅ Port matches expected: {expectedPort}");
                    }

                    // Determine protocol
                    string protocol = DetermineProtocol(transportName);
                    Log($"   Protocol: {protocol}");

                    if (protocol != expectedProtocol)
                    {
                        LogWarning($"⚠️ Protocol mismatch: Expected {expectedProtocol}, got {protocol}");
                    }
                    else
                    {
                        Log($"   ✅ Protocol matches expected: {expectedProtocol}");
                    }
                }
                else
                {
                    LogWarning("⚠️ No PortTransport found on NetworkManager");
                }

                Log($"   Max Connections: {networkManager.maxConnections}");
            }
            else
            {
                LogError("❌ NetworkManager not found in scene!");
                LogError("   Add NetworkManager to scene for multiplayer");
            }

            // Check for ServerLauncher
            ServerLauncher serverLauncher = FindFirstObjectByType<ServerLauncher>();
            if (serverLauncher != null)
            {
                Log("✅ ServerLauncher found");
                Log($"   Auto-start in headless: {serverLauncher.autoStartInHeadless}");
                Log($"   Default port: {serverLauncher.defaultPort}");
                Log($"   Max connections: {serverLauncher.defaultMaxConnections}");

                if (serverLauncher.defaultPort != expectedPort)
                {
                    LogWarning($"⚠️ ServerLauncher port ({serverLauncher.defaultPort}) differs from expected ({expectedPort})");
                }
            }
            else
            {
                LogWarning("⚠️ ServerLauncher not found");
                LogWarning("   Server may not auto-start in headless builds");
                LogWarning("   Add ServerLauncher component to scene for automatic server startup");
            }

            // Check for WOSNetworkManager
            WOSNetworkManager wosNetManager = FindFirstObjectByType<WOSNetworkManager>();
            if (wosNetManager != null)
            {
                Log("✅ WOSNetworkManager found");
                Log($"   Spawn method: {wosNetManager.spawnMethod}");
                Log($"   Naval send rate: {wosNetManager.navalSendRate}Hz");
            }

            Log("═══════════════════════════════════════════");
            Log("✅ Configuration validation complete");
            Log("═══════════════════════════════════════════");
            Log("");
            Log("💡 Edgegap Deployment Checklist:");
            Log("   1. Create Linux server build (File → Build Settings → Server Build ✅)");
            Log("   2. Configure Edgegap plugin (Tools → Edgegap Hosting)");
            Log("   3. Set port mapping: 7777 TCP");
            Log("   4. Click 'Deploy to Edgegap'");
            Log("   5. Connect using server IP from Edgegap dashboard");
        }

        /// <summary>
        /// Determine protocol from transport name
        /// </summary>
        private string DetermineProtocol(string transportName)
        {
            switch (transportName)
            {
                case "KcpTransport":
                case "EdgegapKcpTransport":
                    return "UDP";

                case "SimpleWebTransport":
                    return "WS/WSS";

                case "TelepathyTransport":
                    return "TCP";

                default:
                    LogWarning($"⚠️ Unknown transport: {transportName} - assuming TCP");
                    return "TCP";
            }
        }

        #region Logging Helpers

        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[WOSEdgegapBootstrap] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[WOSEdgegapBootstrap] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[WOSEdgegapBootstrap] {message}");
        }

        #endregion

        #region Editor Helpers

        [ContextMenu("Validate Configuration Now")]
        private void ValidateConfigurationMenu()
        {
            ValidateServerConfiguration();
        }

        [ContextMenu("Open Edgegap Documentation")]
        private void OpenEdgegapDocs()
        {
            Application.OpenURL("https://docs.edgegap.com/docs/tools-and-integrations/unity-plugin-guide");
        }

        [ContextMenu("Open Edgegap Dashboard")]
        private void OpenEdgegapDashboard()
        {
            Application.OpenURL("https://app.edgegap.com/");
        }

        [ContextMenu("Open WOS Deployment Guide")]
        private void OpenDeploymentGuide()
        {
            string guidePath = System.IO.Path.Combine(Application.dataPath, "..", "EDGEGAP_DEPLOYMENT_GUIDE.md");
            if (System.IO.File.Exists(guidePath))
            {
                Application.OpenURL(guidePath);
            }
            else
            {
                Debug.LogWarning("EDGEGAP_DEPLOYMENT_GUIDE.md not found in project root");
            }
        }

        #endregion
    }
}
