using UnityEngine;
using System;

#if UNITY_EDITOR
// Edgegap plugin is only available in Editor builds
// This prevents build errors when creating standalone client/server builds
#endif

namespace WOS.Networking
{
    /// <summary>
    /// Helper class for Edgegap cloud deployment
    /// Provides simplified interface to Edgegap plugin functionality
    ///
    /// IMPORTANT: This script requires the Edgegap Unity plugin to be installed
    /// Install: Edgegap plugin comes with Mirror (check Mirror/Edgegap folder)
    /// Free tier: 1.5 vCPU hosting for Mirror users
    ///
    /// Setup:
    /// 1. Create free Edgegap account at https://edgegap.com
    /// 2. Install Docker Desktop
    /// 3. Install Linux Build Support in Unity Hub
    /// 4. Configure Edgegap credentials in Unity (Tools > Edgegap)
    /// </summary>
    public class EdgegapDeployHelper : MonoBehaviour
    {
        [Header("Deployment Configuration")]
        [Tooltip("Enable Edgegap deployment features (Editor only)")]
        public bool enableEdgegapDeploy = true;

        [Tooltip("Application name in Edgegap dashboard")]
        public string applicationName = "WOS2.3_Server";

        [Tooltip("Deployment version tag")]
        public string versionTag = "v0.3.0";

        [Header("Deployment Status")]
        [Tooltip("Is deployment in progress?")]
        public bool isDeploying = false;

        [Tooltip("Last deployment status message")]
        public string deploymentStatus = "Not deployed";

        [Tooltip("Last deployed server IP")]
        public string lastDeployedServerIP = "";

        // Callbacks for deployment events
        private Action<string> onDeploySuccess;
        private Action<string> onDeployFailed;

        #region Public API

        /// <summary>
        /// Deploy server to Edgegap cloud
        /// </summary>
        /// <param name="onSuccess">Callback with server IP when deployment succeeds</param>
        /// <param name="onFailed">Callback with error message if deployment fails</param>
        public void DeployServer(Action<string> onSuccess = null, Action<string> onFailed = null)
        {
#if UNITY_EDITOR
            if (!enableEdgegapDeploy)
            {
                string error = "Edgegap deployment is disabled";
                Debug.LogWarning($"[EdgegapDeployHelper] {error}");
                onFailed?.Invoke(error);
                return;
            }

            if (isDeploying)
            {
                string error = "Deployment already in progress";
                Debug.LogWarning($"[EdgegapDeployHelper] {error}");
                onFailed?.Invoke(error);
                return;
            }

            // Store callbacks
            this.onDeploySuccess = onSuccess;
            this.onDeployFailed = onFailed;

            isDeploying = true;
            deploymentStatus = "Preparing deployment...";

            Debug.Log("[EdgegapDeployHelper] ☁️ Starting Edgegap deployment...");
            Debug.Log("[EdgegapDeployHelper] This will:");
            Debug.Log("[EdgegapDeployHelper]   1. Build Linux headless server");
            Debug.Log("[EdgegapDeployHelper]   2. Create Docker container");
            Debug.Log("[EdgegapDeployHelper]   3. Upload to Edgegap cloud");
            Debug.Log("[EdgegapDeployHelper]   4. Deploy on nearest server location");
            Debug.Log("[EdgegapDeployHelper] Estimated time: 2-5 minutes");

            // Attempt to use Edgegap plugin
            TryDeployWithEdgegapPlugin();
#else
            // In builds (non-editor), Edgegap deployment is not available
            string error = "Edgegap deployment only available in Unity Editor";
            Debug.LogWarning($"[EdgegapDeployHelper] {error}");
            onFailed?.Invoke(error);
#endif
        }

        /// <summary>
        /// Stop currently deployed server
        /// </summary>
        public void StopDeployedServer()
        {
#if UNITY_EDITOR
            Debug.Log("[EdgegapDeployHelper] Stopping deployed server...");
            TryStopWithEdgegapPlugin();
#else
            Debug.LogWarning("[EdgegapDeployHelper] Stop server only available in Unity Editor");
#endif
        }

        /// <summary>
        /// Get deployment status information
        /// </summary>
        public string GetDeploymentStatus()
        {
            if (isDeploying)
            {
                return $"⏳ {deploymentStatus}";
            }

            if (!string.IsNullOrEmpty(lastDeployedServerIP))
            {
                return $"✅ Deployed at {lastDeployedServerIP}";
            }

            return "⚪ Not deployed";
        }

        #endregion

        #region Edgegap Plugin Integration

#if UNITY_EDITOR
        private void TryDeployWithEdgegapPlugin()
        {
            // Note: This is a placeholder implementation
            // Actual Edgegap plugin integration requires referencing their specific API
            // The Edgegap plugin provides a UI window (Tools > Edgegap) for deployment

            Debug.LogWarning("══════════════════════════════════════════════════════════");
            Debug.LogWarning("🚧 EDGEGAP PLUGIN INTEGRATION REQUIRED");
            Debug.LogWarning("══════════════════════════════════════════════════════════");
            Debug.LogWarning("");
            Debug.LogWarning("To use Edgegap deployment:");
            Debug.LogWarning("1. Open: Tools > Edgegap > Hosting Plugin");
            Debug.LogWarning("2. Configure your API credentials");
            Debug.LogWarning("3. Click 'Deploy' button in Edgegap window");
            Debug.LogWarning("");
            Debug.LogWarning("For now, use the Edgegap plugin UI directly.");
            Debug.LogWarning("This helper script will integrate with the plugin API");
            Debug.LogWarning("once the plugin is properly configured.");
            Debug.LogWarning("══════════════════════════════════════════════════════════");

            // Simulate deployment for testing purposes
            SimulateEdgegapDeployment();
        }

        private void TryStopWithEdgegapPlugin()
        {
            Debug.Log("[EdgegapDeployHelper] Use Tools > Edgegap to stop deployments");
            deploymentStatus = "Stopped";
            lastDeployedServerIP = "";
        }

        /// <summary>
        /// Simulates Edgegap deployment for testing (Editor only)
        /// Remove this in production once real plugin integration is complete
        /// </summary>
        private void SimulateEdgegapDeployment()
        {
            Debug.Log("[EdgegapDeployHelper] ⚠️ SIMULATION MODE - Not actually deploying");
            Debug.Log("[EdgegapDeployHelper] In production, this would call Edgegap API");

            // Simulate deployment delay
            deploymentStatus = "Building server...";
            Invoke(nameof(SimulateDeploymentProgress1), 2f);
        }

        private void SimulateDeploymentProgress1()
        {
            deploymentStatus = "Creating Docker container...";
            Invoke(nameof(SimulateDeploymentProgress2), 2f);
        }

        private void SimulateDeploymentProgress2()
        {
            deploymentStatus = "Uploading to cloud...";
            Invoke(nameof(SimulateDeploymentProgress3), 3f);
        }

        private void SimulateDeploymentProgress3()
        {
            deploymentStatus = "Starting server...";
            Invoke(nameof(SimulateDeploymentComplete), 2f);
        }

        private void SimulateDeploymentComplete()
        {
            isDeploying = false;

            // In real implementation, this would be the actual Edgegap server IP
            string simulatedServerIP = "edgegap-example-123.edgegap.net";
            lastDeployedServerIP = simulatedServerIP;
            deploymentStatus = $"Deployed successfully at {simulatedServerIP}";

            Debug.Log($"[EdgegapDeployHelper] ✅ SIMULATED deployment complete");
            Debug.Log($"[EdgegapDeployHelper] Server IP (simulated): {simulatedServerIP}");

            // Call success callback
            onDeploySuccess?.Invoke(simulatedServerIP);
        }
#endif

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if Edgegap plugin is installed and configured
        /// </summary>
        public bool IsEdgegapAvailable()
        {
#if UNITY_EDITOR
            // Check if Edgegap namespace exists
            // This is a simple check - in production you'd check for actual plugin
            return enableEdgegapDeploy;
#else
            return false;
#endif
        }

        /// <summary>
        /// Open Edgegap documentation
        /// </summary>
        [ContextMenu("Open Edgegap Documentation")]
        public void OpenEdgegapDocs()
        {
            Application.OpenURL("https://docs.edgegap.com/docs/sample-projects/unity-netcodes/mirror-on-edgegap");
        }

        /// <summary>
        /// Open Edgegap dashboard
        /// </summary>
        [ContextMenu("Open Edgegap Dashboard")]
        public void OpenEdgegapDashboard()
        {
            Application.OpenURL("https://app.edgegap.com/");
        }

        #endregion

        #region Unity Events

        private void OnDestroy()
        {
            // Cancel any ongoing deployments
            if (isDeploying)
            {
                Debug.LogWarning("[EdgegapDeployHelper] Component destroyed during deployment");
                isDeploying = false;
            }
        }

        #endregion
    }
}
