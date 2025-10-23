using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Michsky.MUIP;

namespace WOS.UI
{
    /// <summary>
    /// In-game pause/ESC menu controller for WOS Naval MMO
    /// Handles menu toggling, network disconnect, and game exit
    /// Note: Does NOT pause game time in multiplayer (other players continue playing)
    /// </summary>
    public class InGameMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [Tooltip("The main in-game menu panel (should be hidden by default)")]
        public GameObject menuPanel;

        [Tooltip("The settings panel (optional, should be hidden by default)")]
        public GameObject settingsPanel;

        [Tooltip("The quit confirmation panel (should be hidden by default)")]
        public GameObject quitConfirmationPanel;

        [Header("MUIP Buttons")]
        [Tooltip("Resume button - closes menu and returns to gameplay")]
        public ButtonManager resumeButton;

        [Tooltip("Settings button - opens settings menu (optional)")]
        public ButtonManager settingsButton;

        [Tooltip("Exit to Main Menu button - disconnects and loads MainMenu scene")]
        public ButtonManager exitToMenuButton;

        [Tooltip("Quit Game button - closes application")]
        public ButtonManager quitButton;

        [Header("Settings Panel Buttons")]
        [Tooltip("Back button on settings panel - returns to main menu")]
        public ButtonManager settingsBackButton;

        [Header("Quit Confirmation Buttons")]
        [Tooltip("Yes button on quit confirmation panel - confirms quit")]
        public ButtonManager quitYesButton;

        [Tooltip("No button on quit confirmation panel - cancels quit")]
        public ButtonManager quitNoButton;

        [Header("Configuration")]
        [Tooltip("Scene to load when exiting to main menu")]
        public string mainMenuSceneName = "MainMenu";

        [Tooltip("Lock cursor during gameplay")]
        public bool lockCursorInGame = true;

        [Tooltip("Show cursor when menu is open")]
        public bool showCursorInMenu = true;

        private bool isMenuOpen = false;
        private NetworkManager networkManager;

        private void Start()
        {
            Debug.Log("[InGameMenu] Initializing in-game menu...");

            // Find NetworkManager
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogWarning("[InGameMenu] NetworkManager not found! Disconnect functionality may not work.");
            }

            // Ensure menu is hidden on start
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
                isMenuOpen = false;
            }
            else
            {
                Debug.LogError("[InGameMenu] Menu Panel not assigned!");
            }

            // Ensure settings panel is hidden on start
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Ensure quit confirmation panel is hidden on start
            if (quitConfirmationPanel != null)
            {
                quitConfirmationPanel.SetActive(false);
            }

            // Lock cursor for gameplay
            if (lockCursorInGame)
            {
                SetCursorState(true);
            }

            // Wire up button events
            WireUpButtons();

            Debug.Log("[InGameMenu] In-game menu initialized");
        }

        private void Update()
        {
            // Toggle menu with ESC key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
        }

        /// <summary>
        /// Toggle menu open/closed
        /// </summary>
        public void ToggleMenu()
        {
            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        /// <summary>
        /// Open the in-game menu
        /// </summary>
        public void OpenMenu()
        {
            if (menuPanel == null) return;

            isMenuOpen = true;
            menuPanel.SetActive(true);

            // Show cursor for menu interaction
            if (showCursorInMenu)
            {
                SetCursorState(false);
            }

            Debug.Log("[InGameMenu] Menu opened");
        }

        /// <summary>
        /// Close the in-game menu (Resume)
        /// </summary>
        public void CloseMenu()
        {
            if (menuPanel == null) return;

            isMenuOpen = false;
            menuPanel.SetActive(false);

            // Lock cursor for gameplay
            if (lockCursorInGame)
            {
                SetCursorState(true);
            }

            Debug.Log("[InGameMenu] Menu closed (resumed)");
        }

        /// <summary>
        /// Set cursor lock state
        /// </summary>
        private void SetCursorState(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        #region Button Callbacks

        /// <summary>
        /// Wire up button click events
        /// </summary>
        private void WireUpButtons()
        {
            if (resumeButton != null)
            {
                // MUIP buttons use onClick event
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (exitToMenuButton != null)
            {
                exitToMenuButton.onClick.AddListener(OnExitToMenuClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (settingsBackButton != null)
            {
                settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
            }

            if (quitYesButton != null)
            {
                quitYesButton.onClick.AddListener(OnQuitConfirmYes);
            }

            if (quitNoButton != null)
            {
                quitNoButton.onClick.AddListener(OnQuitConfirmNo);
            }
        }

        /// <summary>
        /// Resume button clicked - close menu and return to gameplay
        /// </summary>
        public void OnResumeClicked()
        {
            Debug.Log("[InGameMenu] Resume clicked");
            CloseMenu();
        }

        /// <summary>
        /// Settings button clicked - open settings panel
        /// </summary>
        public void OnSettingsClicked()
        {
            Debug.Log("[InGameMenu] Settings clicked");

            if (settingsPanel != null)
            {
                // Hide main menu, show settings
                if (menuPanel != null)
                {
                    menuPanel.SetActive(false);
                }
                settingsPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[InGameMenu] Settings panel not assigned!");
            }
        }

        /// <summary>
        /// Settings back button clicked - return to main menu
        /// </summary>
        public void OnSettingsBackClicked()
        {
            Debug.Log("[InGameMenu] Settings back clicked");

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Exit to Main Menu button clicked - disconnect from network and load MainMenu
        /// </summary>
        public void OnExitToMenuClicked()
        {
            Debug.Log("[InGameMenu] Exit to Main Menu clicked");

            // Close menu first
            CloseMenu();

            // Disconnect from network
            DisconnectFromNetwork();

            // Load main menu scene
            LoadMainMenu();
        }

        /// <summary>
        /// Quit button clicked - show confirmation dialog
        /// </summary>
        public void OnQuitClicked()
        {
            Debug.Log("[InGameMenu] Quit Game clicked - showing confirmation");

            if (quitConfirmationPanel != null)
            {
                // Hide main menu, show quit confirmation
                if (menuPanel != null)
                {
                    menuPanel.SetActive(false);
                }
                quitConfirmationPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[InGameMenu] Quit confirmation panel not assigned! Quitting immediately.");
                // Fallback: quit immediately if no confirmation panel
                DisconnectFromNetwork();
                QuitGame();
            }
        }

        /// <summary>
        /// Quit confirmation YES button - actually quit the game
        /// </summary>
        public void OnQuitConfirmYes()
        {
            Debug.Log("[InGameMenu] Quit confirmed - exiting game");

            // Hide confirmation panel
            if (quitConfirmationPanel != null)
            {
                quitConfirmationPanel.SetActive(false);
            }

            // Disconnect from network first
            DisconnectFromNetwork();

            // Quit application
            QuitGame();
        }

        /// <summary>
        /// Quit confirmation NO button - cancel quit and return to menu
        /// </summary>
        public void OnQuitConfirmNo()
        {
            Debug.Log("[InGameMenu] Quit cancelled - returning to menu");

            // Hide confirmation panel
            if (quitConfirmationPanel != null)
            {
                quitConfirmationPanel.SetActive(false);
            }

            // Show main menu again
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
            }
        }

        #endregion

        #region Network & Scene Management

        /// <summary>
        /// Disconnect from network (client or host)
        /// Properly handles both client and host scenarios
        /// </summary>
        private void DisconnectFromNetwork()
        {
            if (networkManager == null)
            {
                Debug.LogWarning("[InGameMenu] NetworkManager not found, skipping network disconnect");
                return;
            }

            // Check what role we're in
            bool wasHost = NetworkServer.active && NetworkClient.isConnected;
            bool wasClient = NetworkClient.isConnected && !NetworkServer.active;
            bool wasServer = NetworkServer.active && !NetworkClient.isConnected;

            if (wasHost)
            {
                Debug.Log("[InGameMenu] Stopping host...");
                networkManager.StopHost();
            }
            else if (wasClient)
            {
                Debug.Log("[InGameMenu] Stopping client...");
                networkManager.StopClient();
            }
            else if (wasServer)
            {
                Debug.Log("[InGameMenu] Stopping server...");
                networkManager.StopServer();
            }
            else
            {
                Debug.Log("[InGameMenu] Not connected to network");
            }
        }

        /// <summary>
        /// Load main menu scene
        /// </summary>
        private void LoadMainMenu()
        {
            Debug.Log($"[InGameMenu] Loading main menu scene: {mainMenuSceneName}");

            // Unlock cursor for main menu
            SetCursorState(false);

            // Load main menu scene
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Quit the application
        /// </summary>
        private void QuitGame()
        {
            Debug.Log("[InGameMenu] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check if menu is currently open
        /// </summary>
        public bool IsMenuOpen()
        {
            return isMenuOpen;
        }

        /// <summary>
        /// Force menu closed (useful for external systems)
        /// </summary>
        public void ForceCloseMenu()
        {
            CloseMenu();
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up button listeners
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (exitToMenuButton != null)
                exitToMenuButton.onClick.RemoveListener(OnExitToMenuClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);

            if (settingsBackButton != null)
                settingsBackButton.onClick.RemoveListener(OnSettingsBackClicked);

            if (quitYesButton != null)
                quitYesButton.onClick.RemoveListener(OnQuitConfirmYes);

            if (quitNoButton != null)
                quitNoButton.onClick.RemoveListener(OnQuitConfirmNo);
        }
    }
}
