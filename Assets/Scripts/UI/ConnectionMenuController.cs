using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using WOS.Networking;

namespace WOS.UI
{
    /// <summary>
    /// Handles connection menu navigation (Host/Join buttons on Connection panel)
    /// Simplified to work with MenuManager's 5-panel system
    /// </summary>
    public class ConnectionMenuController : MonoBehaviour
    {
        [Header("Status Display")]
        [Tooltip("Status text for connection messages (optional)")]
        public TextMeshProUGUI statusText;

        private void Start()
        {
            UpdateStatus("Select Host or Join Server");
        }

        #region Public Button Methods (Called from Connection Panel UI)

        /// <summary>
        /// Show Host panel
        /// Called by "Host" button on Connection panel
        /// </summary>
        public void OnHostButtonClicked()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowHostMenu();
            }
            else
            {
                Debug.LogError("[ConnectionMenu] MenuManager not found!");
            }
        }

        /// <summary>
        /// Show Join panel
        /// Called by "Join Server" button on Connection panel
        /// </summary>
        public void OnJoinButtonClicked()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowJoinMenu();
            }
            else
            {
                Debug.LogError("[ConnectionMenu] MenuManager not found!");
            }
        }

        /// <summary>
        /// Return to main menu
        /// Called by "Back" button on Connection panel
        /// </summary>
        public void OnBackButtonClicked()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowMainMenu();
            }
            else
            {
                Debug.LogError("[ConnectionMenu] MenuManager not found!");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Update status text with message
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            Debug.Log($"[ConnectionMenu] {message}");
        }

        #endregion
    }
}
