using UnityEngine;
using TMPro;

namespace WOS.UI
{
    /// <summary>
    /// Main Menu controller for WOS2.3
    /// Simplified for dedicated Edgegap server deployment
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Status text to show messages")]
        public TextMeshProUGUI statusText;

        private void Start()
        {
            UpdateStatus("Waves of Steel - Naval MMO");
        }

        #region Public Button Methods (Called from UI)

        /// <summary>
        /// Start game - go directly to Join menu
        /// Called by "Play" or "Start" button
        /// </summary>
        public void OnPlayButtonClicked()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowJoinMenu();
            }
            else
            {
                Debug.LogError("[MainMenu] MenuManager not found!");
            }
        }

        /// <summary>
        /// Open options menu
        /// Called by "Options" button
        /// </summary>
        public void OnOptionsButtonClicked()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowOptionsMenu();
            }
            else
            {
                Debug.LogError("[MainMenu] MenuManager not found!");
            }
        }

        /// <summary>
        /// Quit the game
        /// Called by "Exit Game" button
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("🚪 Quitting game...");
            UpdateStatus("Exiting...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Helper Methods

        private void UpdateStatus(string message, bool isError = false)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.white;
            }

            Debug.Log($"[MainMenu] {message}");
        }

        #endregion
    }
}
