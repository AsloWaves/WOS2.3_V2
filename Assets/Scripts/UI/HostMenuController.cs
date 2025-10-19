using UnityEngine;
using TMPro;
using Mirror;

namespace WOS.UI
{
    /// <summary>
    /// Handles Host panel - starts local server and joins as client
    /// </summary>
    public class HostMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Status text for host messages")]
        public TextMeshProUGUI statusText;

        [Header("Configuration")]
        [Tooltip("Scene to load after hosting (usually Main scene)")]
        public string gameSceneName = "Main";

        private NetworkManager networkManager;

        private void Start()
        {
            InitializeNetworkManager();
            UpdateStatus("Ready to host server");
        }

        private void InitializeNetworkManager()
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("[HostMenu] NetworkManager not found in scene!");
            }
        }

        #region Public Button Methods

        /// <summary>
        /// Start hosting server
        /// Called by "Start Host" button
        /// </summary>
        public void OnStartHostButtonClicked()
        {
            if (networkManager == null)
            {
                UpdateStatus("Error: NetworkManager not found!", true);
                return;
            }

            UpdateStatus("Starting local host...");

            try
            {
                networkManager.networkAddress = "localhost";
                networkManager.StartHost();

                Debug.Log("[HostMenu] ✅ Local host started");
                UpdateStatus("Host started! Loading game...");
            }
            catch (System.Exception e)
            {
                UpdateStatus($"Host failed: {e.Message}", true);
                Debug.LogError($"[HostMenu] Host error: {e}");
            }
        }

        /// <summary>
        /// Return to connection menu
        /// Called by "Back" button
        /// </summary>
        public void OnBackButtonClicked()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowConnectionMenu();
            }
            else
            {
                Debug.LogWarning("[HostMenu] MenuManager not found!");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Update status text with message
        /// </summary>
        private void UpdateStatus(string message, bool isError = false)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
            }

            Debug.Log($"[HostMenu] {message}");
        }

        #endregion
    }
}
