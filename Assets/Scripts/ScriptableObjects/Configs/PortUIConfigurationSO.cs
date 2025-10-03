using UnityEngine;
using WOS.Debugging;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Configuration for port UI display including welcome panels, service menus, and information displays.
    /// Integrates with WOS2.3_V2's minimal UI overlay approach.
    /// </summary>
    [CreateAssetMenu(fileName = "PortUIConfig", menuName = "WOS/Environment/Port UI Configuration")]
    public class PortUIConfigurationSO : ScriptableObject
    {
        [System.Serializable]
        public enum UIStyle
        {
            Minimal,        // Simple text-based overlay
            Standard,       // Clean maritime-themed UI
            Detailed,       // Rich information display
            Immersive       // Diegetic in-world UI elements
        }

        [System.Serializable]
        public enum NotificationPosition
        {
            TopLeft,
            TopCenter,
            TopRight,
            BottomLeft,
            BottomCenter,
            BottomRight,
            Center
        }

        [System.Serializable]
        public class WelcomePanelSettings
        {
            [Header("Display Configuration")]
            [Tooltip("Show welcome panel when docking")]
            public bool showWelcomePanel = true;

            [Tooltip("Auto-hide welcome panel after time")]
            [Range(0f, 30f)]
            public float autoHideTime = 8f;

            [Tooltip("Allow manual dismiss with key/click")]
            public bool allowManualDismiss = true;

            [Header("Content")]
            [Tooltip("Welcome message template")]
            [TextArea(2, 4)]
            public string welcomeMessageTemplate = "Welcome to {PORT_NAME}!\nPress [TAB] to access services.";

            [Tooltip("Show port status information")]
            public bool showPortStatus = true;

            [Tooltip("Show ship status information")]
            public bool showShipStatus = true;

            [Tooltip("Show available services preview")]
            public bool showServicesPreview = true;

            [Header("Styling")]
            [Tooltip("Background color for welcome panel")]
            public Color backgroundColor = new Color(0f, 0.2f, 0.4f, 0.8f);

            [Tooltip("Text color for welcome panel")]
            public Color textColor = Color.white;

            [Tooltip("Border color for welcome panel")]
            public Color borderColor = new Color(0.4f, 0.7f, 1f, 1f);

            [Tooltip("Font size multiplier")]
            [Range(0.5f, 2f)]
            public float fontSizeMultiplier = 1f;
        }

        [System.Serializable]
        public class ServiceMenuSettings
        {
            [Header("Menu Behavior")]
            [Tooltip("Show service menu key hint")]
            public bool showMenuKeyHint = true;

            [Tooltip("Key to open service menu")]
            public KeyCode serviceMenuKey = KeyCode.Tab;

            [Tooltip("Auto-organize services by category")]
            public bool categorizeServices = true;

            [Tooltip("Show service costs in menu")]
            public bool showServiceCosts = true;

            [Tooltip("Show service timing estimates")]
            public bool showServiceTiming = true;

            [Header("Visual Style")]
            [Tooltip("Menu layout style")]
            public UIStyle menuStyle = UIStyle.Standard;

            [Tooltip("Maximum services per column")]
            [Range(3, 15)]
            public int maxServicesPerColumn = 8;

            [Tooltip("Service icon size")]
            [Range(16, 64)]
            public int serviceIconSize = 32;

            [Header("Service Categories")]
            [Tooltip("Color for essential services")]
            public Color essentialServiceColor = Color.green;

            [Tooltip("Color for convenience services")]
            public Color convenienceServiceColor = Color.yellow;

            [Tooltip("Color for premium services")]
            public Color premiumServiceColor = Color.cyan;

            [Tooltip("Color for unavailable services")]
            public Color unavailableServiceColor = Color.gray;
        }

        [System.Serializable]
        public class StatusDisplaySettings
        {
            [Header("Port Information")]
            [Tooltip("Show port name and type")]
            public bool showPortIdentity = true;

            [Tooltip("Show port prosperity and security")]
            public bool showPortMetrics = true;

            [Tooltip("Show current weather and time")]
            public bool showEnvironmentalInfo = true;

            [Tooltip("Show docking availability")]
            public bool showDockingStatus = true;

            [Header("Ship Information")]
            [Tooltip("Show ship status (fuel, health, cargo)")]
            public bool showShipStatus = true;

            [Tooltip("Show crew information")]
            public bool showCrewInfo = false;

            [Tooltip("Show ship financial status")]
            public bool showFinancialInfo = true;

            [Header("Display Position")]
            [Tooltip("Where to show status information")]
            public NotificationPosition statusPosition = NotificationPosition.TopLeft;

            [Tooltip("Compact status display")]
            public bool compactDisplay = true;

            [Tooltip("Update frequency for dynamic info")]
            [Range(0.5f, 5f)]
            public float updateFrequency = 2f;
        }

        [System.Serializable]
        public class NotificationSettings
        {
            [Header("Docking Notifications")]
            [Tooltip("Show docking progress notifications")]
            public bool showDockingProgress = true;

            [Tooltip("Show undocking notifications")]
            public bool showUndockingNotifications = true;

            [Tooltip("Show docking guidance tips")]
            public bool showGuidanceTips = true;

            [Header("Service Notifications")]
            [Tooltip("Show service completion notifications")]
            public bool showServiceNotifications = true;

            [Tooltip("Show service progress for long operations")]
            public bool showServiceProgress = true;

            [Tooltip("Show cost confirmations")]
            public bool showCostConfirmations = true;

            [Header("Alert Notifications")]
            [Tooltip("Show warnings and errors")]
            public bool showAlertNotifications = true;

            [Tooltip("Show emergency/urgent messages")]
            public bool showEmergencyAlerts = true;

            [Header("Visual Style")]
            [Tooltip("Notification display duration")]
            [Range(1f, 15f)]
            public float notificationDuration = 5f;

            [Tooltip("Notification position")]
            public NotificationPosition notificationPosition = NotificationPosition.BottomRight;

            [Tooltip("Enable notification sound effects")]
            public bool enableNotificationSounds = true;

            [Tooltip("Success notification color")]
            public Color successColor = Color.green;

            [Tooltip("Warning notification color")]
            public Color warningColor = Color.yellow;

            [Tooltip("Error notification color")]
            public Color errorColor = Color.red;

            [Tooltip("Info notification color")]
            public Color infoColor = Color.blue;
        }

        [System.Serializable]
        public class ProgressIndicatorSettings
        {
            [Header("Progress Display")]
            [Tooltip("Show progress bars for services")]
            public bool showProgressBars = true;

            [Tooltip("Show percentage completion")]
            public bool showPercentageText = true;

            [Tooltip("Show estimated time remaining")]
            public bool showTimeRemaining = true;

            [Tooltip("Show service description during progress")]
            public bool showServiceDescription = true;

            [Header("Visual Style")]
            [Tooltip("Progress bar color")]
            public Color progressBarColor = new Color(0.2f, 0.8f, 0.4f);

            [Tooltip("Progress bar background color")]
            public Color progressBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            [Tooltip("Progress bar height")]
            [Range(4, 32)]
            public int progressBarHeight = 16;

            [Tooltip("Allow canceling services in progress")]
            public bool allowServiceCancellation = true;

            [Tooltip("Show cancellation key hint")]
            public bool showCancellationHint = true;

            [Tooltip("Cancellation key")]
            public KeyCode cancellationKey = KeyCode.Escape;
        }

        [Header("General UI Settings")]
        [Tooltip("Overall UI style for port interfaces")]
        public UIStyle overallUIStyle = UIStyle.Standard;

        [Tooltip("Welcome panel configuration")]
        public WelcomePanelSettings welcomePanel = new WelcomePanelSettings();

        [Tooltip("Service menu configuration")]
        public ServiceMenuSettings serviceMenu = new ServiceMenuSettings();

        [Tooltip("Status display configuration")]
        public StatusDisplaySettings statusDisplay = new StatusDisplaySettings();

        [Tooltip("Notification configuration")]
        public NotificationSettings notifications = new NotificationSettings();

        [Tooltip("Progress indicator configuration")]
        public ProgressIndicatorSettings progressIndicators = new ProgressIndicatorSettings();

        [Header("Integration Settings")]
        [Tooltip("Use existing camera UI overlay system")]
        public bool useExistingUISystem = true;

        [Tooltip("UI layer for port interfaces")]
        [Range(0, 31)]
        public int uiLayer = 5;

        [Tooltip("UI canvas sort order")]
        [Range(0, 100)]
        public int canvasSortOrder = 10;

        [Header("Performance")]
        [Tooltip("UI update frequency")]
        [Range(0.1f, 1f)]
        public float uiUpdateFrequency = 0.2f;

        [Tooltip("Maximum UI elements active simultaneously")]
        [Range(5, 50)]
        public int maxActiveUIElements = 20;

        [Tooltip("Enable UI pooling for performance")]
        public bool enableUIPooling = true;

        [Header("Accessibility")]
        [Tooltip("Enable high contrast mode")]
        public bool enableHighContrast = false;

        [Tooltip("Font size scaling")]
        [Range(0.7f, 2f)]
        public float fontSizeScale = 1f;

        [Tooltip("Enable text-to-speech for notifications")]
        public bool enableTextToSpeech = false;

        [Tooltip("Colorblind-friendly color palette")]
        public bool useColorblindFriendlyColors = false;

        /// <summary>
        /// Get welcome message with port name substitution
        /// </summary>
        public string GetWelcomeMessage(string portName)
        {
            return welcomePanel.welcomeMessageTemplate.Replace("{PORT_NAME}", portName);
        }

        /// <summary>
        /// Get service category color
        /// </summary>
        public Color GetServiceCategoryColor(string serviceCategory)
        {
            return serviceCategory.ToLower() switch
            {
                "essential" or "repair" or "refuel" => serviceMenu.essentialServiceColor,
                "convenience" or "storage" or "information" => serviceMenu.convenienceServiceColor,
                "premium" or "upgrade" or "luxury" => serviceMenu.premiumServiceColor,
                "unavailable" or "disabled" => serviceMenu.unavailableServiceColor,
                _ => serviceMenu.essentialServiceColor
            };
        }

        /// <summary>
        /// Get notification color based on type
        /// </summary>
        public Color GetNotificationColor(string notificationType)
        {
            return notificationType.ToLower() switch
            {
                "success" or "complete" or "docked" => notifications.successColor,
                "warning" or "caution" => notifications.warningColor,
                "error" or "failed" or "unavailable" => notifications.errorColor,
                "info" or "tip" or "guidance" => notifications.infoColor,
                _ => notifications.infoColor
            };
        }

        /// <summary>
        /// Check if UI feature should be shown based on style
        /// </summary>
        public bool ShouldShowFeature(string featureName)
        {
            return overallUIStyle switch
            {
                UIStyle.Minimal => featureName switch
                {
                    "welcome_panel" => false,
                    "service_menu" => true,
                    "status_display" => false,
                    "progress_bars" => true,
                    "notifications" => true,
                    _ => false
                },
                UIStyle.Standard => true,
                UIStyle.Detailed => true,
                UIStyle.Immersive => featureName switch
                {
                    "welcome_panel" => true,
                    "service_menu" => true,
                    "status_display" => true,
                    "progress_bars" => true,
                    "notifications" => false, // Use in-world displays
                    _ => true
                },
                _ => true
            };
        }

        /// <summary>
        /// Get screen position for UI placement
        /// </summary>
        public Vector2 GetScreenPosition(NotificationPosition position, Vector2 screenSize)
        {
            float margin = 50f;

            return position switch
            {
                NotificationPosition.TopLeft => new Vector2(margin, screenSize.y - margin),
                NotificationPosition.TopCenter => new Vector2(screenSize.x * 0.5f, screenSize.y - margin),
                NotificationPosition.TopRight => new Vector2(screenSize.x - margin, screenSize.y - margin),
                NotificationPosition.BottomLeft => new Vector2(margin, margin),
                NotificationPosition.BottomCenter => new Vector2(screenSize.x * 0.5f, margin),
                NotificationPosition.BottomRight => new Vector2(screenSize.x - margin, margin),
                NotificationPosition.Center => new Vector2(screenSize.x * 0.5f, screenSize.y * 0.5f),
                _ => new Vector2(screenSize.x * 0.5f, screenSize.y * 0.5f)
            };
        }

        /// <summary>
        /// Apply accessibility modifications to colors
        /// </summary>
        public Color ApplyAccessibilityModifications(Color originalColor)
        {
            Color modifiedColor = originalColor;

            if (enableHighContrast)
            {
                // Increase contrast by pushing colors toward extremes
                modifiedColor.r = modifiedColor.r > 0.5f ? 1f : 0f;
                modifiedColor.g = modifiedColor.g > 0.5f ? 1f : 0f;
                modifiedColor.b = modifiedColor.b > 0.5f ? 1f : 0f;
            }

            if (useColorblindFriendlyColors)
            {
                // Apply colorblind-friendly transformations
                // This is a simplified approach - in production, use proper colorblind simulation
                if (modifiedColor == Color.red)
                    modifiedColor = new Color(0.8f, 0.2f, 0.2f); // Darker red
                else if (modifiedColor == Color.green)
                    modifiedColor = new Color(0.2f, 0.6f, 0.8f); // Blue-green
            }

            return modifiedColor;
        }

        /// <summary>
        /// Validate configuration and log warnings
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            // Check notification duration
            if (notifications.notificationDuration <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.UI, "Notification duration must be greater than 0!", this);
                isValid = false;
            }

            // Check welcome panel auto-hide time
            if (welcomePanel.autoHideTime < 0f)
            {
                DebugManager.LogWarning(DebugCategory.UI, "Welcome panel auto-hide time cannot be negative!", this);
                isValid = false;
            }

            // Check UI update frequency
            if (uiUpdateFrequency <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.UI, "UI update frequency must be greater than 0!", this);
                isValid = false;
            }

            // Check status update frequency
            if (statusDisplay.updateFrequency <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.UI, "Status update frequency must be greater than 0!", this);
                isValid = false;
            }

            // Check max UI elements
            if (maxActiveUIElements <= 0)
            {
                DebugManager.LogWarning(DebugCategory.UI, "Max active UI elements must be greater than 0!", this);
                isValid = false;
            }

            return isValid;
        }

        private void OnValidate()
        {
            // Ensure durations are positive
            if (notifications != null)
                notifications.notificationDuration = Mathf.Max(0.5f, notifications.notificationDuration);

            if (welcomePanel != null)
            {
                welcomePanel.autoHideTime = Mathf.Max(0f, welcomePanel.autoHideTime);
                welcomePanel.fontSizeMultiplier = Mathf.Clamp(welcomePanel.fontSizeMultiplier, 0.3f, 3f);
            }

            if (statusDisplay != null)
                statusDisplay.updateFrequency = Mathf.Max(0.1f, statusDisplay.updateFrequency);

            if (serviceMenu != null)
            {
                serviceMenu.maxServicesPerColumn = Mathf.Clamp(serviceMenu.maxServicesPerColumn, 1, 20);
                serviceMenu.serviceIconSize = Mathf.Clamp(serviceMenu.serviceIconSize, 8, 128);
            }

            if (progressIndicators != null)
                progressIndicators.progressBarHeight = Mathf.Clamp(progressIndicators.progressBarHeight, 2, 64);

            // Ensure frequencies are reasonable
            uiUpdateFrequency = Mathf.Clamp(uiUpdateFrequency, 0.02f, 2f);
            maxActiveUIElements = Mathf.Max(1, maxActiveUIElements);

            // Ensure layer and sort order are valid
            uiLayer = Mathf.Clamp(uiLayer, 0, 31);
            canvasSortOrder = Mathf.Max(0, canvasSortOrder);

            // Ensure accessibility settings are reasonable
            fontSizeScale = Mathf.Clamp(fontSizeScale, 0.5f, 3f);
        }
    }
}