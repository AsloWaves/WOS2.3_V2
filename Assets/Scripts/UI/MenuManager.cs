using UnityEngine;
using System.Collections.Generic;

namespace WOS.UI
{
    /// <summary>
    /// Manages menu panel transitions (MainMenu, ConnectionMenu, OptionsMenu)
    /// Simple panel switching system for MUIP layouts
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        [Tooltip("Main menu panel with Start, Options, Exit buttons")]
        public GameObject mainMenuPanel;

        [Tooltip("Connection menu panel with Host, Join, Back buttons")]
        public GameObject connectionMenuPanel;

        [Tooltip("Host panel with hosting interface")]
        public GameObject hostPanel;

        [Tooltip("Join panel with IP input and Connect button")]
        public GameObject joinPanel;

        [Tooltip("Options menu panel (future implementation)")]
        public GameObject optionsMenuPanel;

        [Header("Configuration")]
        [Tooltip("Panel to show on startup")]
        public MenuPanel startingPanel = MenuPanel.MainMenu;

        // Singleton for easy access
        public static MenuManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ValidatePanels();

            // Force all panels to be hidden initially (in case they're active in scene)
            HideAllPanels();
        }

        private void Start()
        {
            // Show starting panel after all components have initialized
            ShowPanel(startingPanel);
        }

        /// <summary>
        /// Show specific menu panel, hide all others
        /// </summary>
        public void ShowPanel(MenuPanel panel)
        {
            // Hide all panels first
            HideAllPanels();

            // Show requested panel
            switch (panel)
            {
                case MenuPanel.MainMenu:
                    if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                    break;

                case MenuPanel.ConnectionMenu:
                    if (connectionMenuPanel != null) connectionMenuPanel.SetActive(true);
                    break;

                case MenuPanel.HostMenu:
                    if (hostPanel != null) hostPanel.SetActive(true);
                    break;

                case MenuPanel.JoinMenu:
                    if (joinPanel != null) joinPanel.SetActive(true);
                    break;

                case MenuPanel.OptionsMenu:
                    if (optionsMenuPanel != null) optionsMenuPanel.SetActive(true);
                    break;
            }

            Debug.Log($"[MenuManager] Showing panel: {panel}");
        }

        /// <summary>
        /// Hide all menu panels
        /// </summary>
        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (connectionMenuPanel != null) connectionMenuPanel.SetActive(false);
            if (hostPanel != null) hostPanel.SetActive(false);
            if (joinPanel != null) joinPanel.SetActive(false);
            if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        }

        /// <summary>
        /// Return to main menu from any panel
        /// </summary>
        public void ShowMainMenu()
        {
            ShowPanel(MenuPanel.MainMenu);
        }

        /// <summary>
        /// Show connection menu (Host/Join buttons)
        /// </summary>
        public void ShowConnectionMenu()
        {
            ShowPanel(MenuPanel.ConnectionMenu);
        }

        /// <summary>
        /// Show host panel
        /// </summary>
        public void ShowHostMenu()
        {
            ShowPanel(MenuPanel.HostMenu);
        }

        /// <summary>
        /// Show join panel
        /// </summary>
        public void ShowJoinMenu()
        {
            ShowPanel(MenuPanel.JoinMenu);
        }

        /// <summary>
        /// Show options menu
        /// </summary>
        public void ShowOptionsMenu()
        {
            ShowPanel(MenuPanel.OptionsMenu);
        }

        private void ValidatePanels()
        {
            if (mainMenuPanel == null)
                Debug.LogWarning("[MenuManager] Main Menu Panel not assigned!");

            if (connectionMenuPanel == null)
                Debug.LogWarning("[MenuManager] Connection Menu Panel not assigned!");

            if (hostPanel == null)
                Debug.LogWarning("[MenuManager] Host Panel not assigned!");

            if (joinPanel == null)
                Debug.LogWarning("[MenuManager] Join Panel not assigned!");

            if (optionsMenuPanel == null)
                Debug.LogWarning("[MenuManager] Options Menu Panel not assigned! (Optional for now)");
        }

        #region Public API for Button OnClick Events

        /// <summary>
        /// Called by Main Menu "Start" button
        /// Goes directly to Join menu (simplified for dedicated server)
        /// </summary>
        public void OnStartButtonClicked()
        {
            ShowJoinMenu();
        }

        /// <summary>
        /// Called by Main Menu "Options" button
        /// </summary>
        public void OnOptionsButtonClicked()
        {
            ShowOptionsMenu();
        }

        /// <summary>
        /// Called by Main Menu "Exit" button
        /// </summary>
        public void OnExitButtonClicked()
        {
            QuitGame();
        }

        /// <summary>
        /// Called by any "Back" button to return to main menu
        /// </summary>
        public void OnBackButtonClicked()
        {
            ShowMainMenu();
        }

        #endregion

        #region Game Control

        /// <summary>
        /// Quit the application
        /// </summary>
        private void QuitGame()
        {
            Debug.Log("[MenuManager] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }

    /// <summary>
    /// Available menu panels
    /// </summary>
    public enum MenuPanel
    {
        MainMenu,
        ConnectionMenu,
        HostMenu,
        JoinMenu,
        OptionsMenu
    }
}
