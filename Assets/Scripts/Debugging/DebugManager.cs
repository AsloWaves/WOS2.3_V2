using UnityEngine;
using System.Collections.Generic;

namespace WOS.Debugging
{
    /// <summary>
    /// Centralized debug logging system for WOS naval game.
    /// Provides category-based filtering and clean console output control.
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        [Header("Debug Categories")]
        [Tooltip("Enable/disable debug messages by category")]
        [Space(5)]
        [SerializeField] private DebugSettings debugSettings = new DebugSettings();

        [Header("Global Settings")]
        [Tooltip("Master switch for all debug output")]
        [SerializeField] private bool enableAllDebug = true;

        [Tooltip("Show timestamps in debug messages")]
        [SerializeField] private bool showTimestamps = true;

        [Tooltip("Show frame count in debug messages")]
        [SerializeField] private bool showFrameCount = false;

        [Tooltip("Color-code debug messages by category")]
        [SerializeField] private bool useColorCoding = true;

        [Header("Performance")]
        [Tooltip("Maximum debug messages per frame (prevents spam)")]
        [Range(1, 100)]
        [SerializeField] private int maxMessagesPerFrame = 20;

        [Tooltip("Debug message throttling (seconds between identical messages)")]
        [Range(0f, 5f)]
        [SerializeField] private float messageThrottleTime = 0.5f;

        [Header("Quick Controls")]
        [Space(5)]
        [Tooltip("Quick enable/disable commonly used categories")]
        [SerializeField] private bool quickShipAndOcean = true;
        [SerializeField] private bool quickPerformanceOnly = false;
        [SerializeField] private bool quickErrorsOnly = false;

        // Singleton instance
        private static DebugManager instance;
        public static DebugManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<DebugManager>();
                return instance;
            }
        }

        // Performance tracking
        private int messagesThisFrame = 0;
        private Dictionary<string, float> lastMessageTime = new Dictionary<string, float>();

        // Color codes for categories
        private static readonly Dictionary<DebugCategory, string> categoryColors = new Dictionary<DebugCategory, string>
        {
            { DebugCategory.Ship, "#4CAF50" },          // Green
            { DebugCategory.Ocean, "#2196F3" },         // Blue
            { DebugCategory.Environment, "#8BC34A" },   // Light Green
            { DebugCategory.Camera, "#FF9800" },        // Orange
            { DebugCategory.Input, "#9C27B0" },         // Purple
            { DebugCategory.Physics, "#F44336" },       // Red
            { DebugCategory.Performance, "#FFEB3B" },   // Yellow
            { DebugCategory.UI, "#00BCD4" },            // Cyan
            { DebugCategory.Economy, "#4CAF50" },       // Green
            { DebugCategory.Audio, "#E91E63" },         // Pink
            { DebugCategory.Networking, "#607D8B" },    // Blue Gray
            { DebugCategory.System, "#795548" }         // Brown
        };

        private void Awake()
        {
            // Singleton setup
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // Reset frame counter
            messagesThisFrame = 0;

            Debug.Log("<color=#00FF00>[DebugManager]</color> Initialized - Centralized debug logging active");
        }

        private void LateUpdate()
        {
            // Reset message counter each frame
            messagesThisFrame = 0;
        }

        /// <summary>
        /// Log a debug message with category filtering
        /// </summary>
        public static void Log(DebugCategory category, string message, Object context = null)
        {
            if (Instance == null || !Instance.enableAllDebug) return;

            if (!Instance.IsCategoryEnabled(category)) return;

            Instance.LogInternal(category, LogType.Log, message, context);
        }

        /// <summary>
        /// Log a warning message with category filtering
        /// </summary>
        public static void LogWarning(DebugCategory category, string message, Object context = null)
        {
            if (Instance == null || !Instance.enableAllDebug) return;

            if (!Instance.IsCategoryEnabled(category)) return;

            Instance.LogInternal(category, LogType.Warning, message, context);
        }

        /// <summary>
        /// Log an error message (always shown regardless of category settings)
        /// </summary>
        public static void LogError(DebugCategory category, string message, Object context = null)
        {
            if (Instance == null) return;

            Instance.LogInternal(category, LogType.Error, message, context);
        }

        /// <summary>
        /// Log a performance-related message with timing
        /// </summary>
        public static void LogPerformance(string operation, float timeMs, Object context = null)
        {
            if (Instance == null || !Instance.IsCategoryEnabled(DebugCategory.Performance)) return;

            string perfMessage = $"[PERF] {operation}: {timeMs:F2}ms";
            Instance.LogInternal(DebugCategory.Performance, LogType.Log, perfMessage, context);
        }

        /// <summary>
        /// Log a message only once (prevents spam)
        /// </summary>
        public static void LogOnce(DebugCategory category, string message, Object context = null)
        {
            if (Instance == null || !Instance.enableAllDebug) return;

            if (!Instance.IsCategoryEnabled(category)) return;

            string key = $"{category}:{message}";
            if (Instance.lastMessageTime.ContainsKey(key)) return;

            Instance.lastMessageTime[key] = Time.time;
            Instance.LogInternal(category, LogType.Log, message, context);
        }

        private void LogInternal(DebugCategory category, LogType logType, string message, Object context)
        {
            // Check frame limit
            if (messagesThisFrame >= maxMessagesPerFrame) return;

            // Check throttling for identical messages
            string throttleKey = $"{category}:{message}";
            if (lastMessageTime.ContainsKey(throttleKey))
            {
                if (Time.time - lastMessageTime[throttleKey] < messageThrottleTime)
                    return;
            }
            lastMessageTime[throttleKey] = Time.time;

            // Format message
            string formattedMessage = FormatMessage(category, message);

            // Log using Unity's system
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(formattedMessage, context);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(formattedMessage, context);
                    break;
                case LogType.Error:
                    Debug.LogError(formattedMessage, context);
                    break;
            }

            messagesThisFrame++;
        }

        private string FormatMessage(DebugCategory category, string message)
        {
            string formatted = "";

            // Add timestamp
            if (showTimestamps)
            {
                formatted += $"[{Time.time:F2}s] ";
            }

            // Add frame count
            if (showFrameCount)
            {
                formatted += $"[F{Time.frameCount}] ";
            }

            // Add category with color coding
            if (useColorCoding && categoryColors.ContainsKey(category))
            {
                string color = categoryColors[category];
                formatted += $"<color={color}>[{category.ToString().ToUpper()}]</color> ";
            }
            else
            {
                formatted += $"[{category.ToString().ToUpper()}] ";
            }

            // Add the actual message
            formatted += message;

            return formatted;
        }

        private bool IsCategoryEnabled(DebugCategory category)
        {
            return category switch
            {
                DebugCategory.Ship => debugSettings.enableShipDebug,
                DebugCategory.Ocean => debugSettings.enableOceanDebug,
                DebugCategory.Environment => debugSettings.enableEnvironmentDebug,
                DebugCategory.Camera => debugSettings.enableCameraDebug,
                DebugCategory.Input => debugSettings.enableInputDebug,
                DebugCategory.Physics => debugSettings.enablePhysicsDebug,
                DebugCategory.Performance => debugSettings.enablePerformanceDebug,
                DebugCategory.UI => debugSettings.enableUIDebug,
                DebugCategory.Economy => debugSettings.enableEconomyDebug,
                DebugCategory.Audio => debugSettings.enableAudioDebug,
                DebugCategory.Networking => debugSettings.enableNetworkingDebug,
                DebugCategory.System => debugSettings.enableSystemDebug,
                _ => false
            };
        }

        /// <summary>
        /// Enable all debug categories
        /// </summary>
        [ContextMenu("Enable All Debug")]
        public void EnableAllCategories()
        {
            debugSettings.EnableAll();
            Debug.Log("<color=#00FF00>[DebugManager]</color> All debug categories enabled");
        }

        /// <summary>
        /// Disable all debug categories
        /// </summary>
        [ContextMenu("Disable All Debug")]
        public void DisableAllCategories()
        {
            debugSettings.DisableAll();
            Debug.Log("<color=#FF0000>[DebugManager]</color> All debug categories disabled");
        }

        /// <summary>
        /// Clear console and reset throttling
        /// </summary>
        [ContextMenu("Clear Console")]
        public void ClearConsole()
        {
            lastMessageTime.Clear();
            messagesThisFrame = 0;

            // Note: This doesn't actually clear Unity's console, just resets our internal state
            Debug.Log("<color=#00FF00>[DebugManager]</color> Debug state cleared");
        }

        /// <summary>
        /// Get debug statistics
        /// </summary>
        public DebugStats GetStats()
        {
            return new DebugStats
            {
                messagesThisFrame = messagesThisFrame,
                maxMessagesPerFrame = maxMessagesPerFrame,
                throttledMessages = lastMessageTime.Count,
                enabledCategories = GetEnabledCategoryCount()
            };
        }

        private int GetEnabledCategoryCount()
        {
            int count = 0;
            foreach (DebugCategory category in System.Enum.GetValues(typeof(DebugCategory)))
            {
                if (IsCategoryEnabled(category)) count++;
            }
            return count;
        }

        /// <summary>
        /// Quick setup for common debug configurations
        /// </summary>
        [ContextMenu("Setup: Ship & Ocean Only")]
        public void SetupShipAndOcean()
        {
            debugSettings.DisableAll();
            debugSettings.enableShipDebug = true;
            debugSettings.enableOceanDebug = true;
            debugSettings.enableSystemDebug = true;
            quickShipAndOcean = true;
            quickPerformanceOnly = false;
            quickErrorsOnly = false;
        }

        [ContextMenu("Setup: Performance Only")]
        public void SetupPerformanceOnly()
        {
            debugSettings.DisableAll();
            debugSettings.enablePerformanceDebug = true;
            debugSettings.enableSystemDebug = true;
            quickShipAndOcean = false;
            quickPerformanceOnly = true;
            quickErrorsOnly = false;
        }

        [ContextMenu("Setup: Errors Only")]
        public void SetupErrorsOnly()
        {
            debugSettings.DisableAll();
            debugSettings.enableSystemDebug = true;
            quickShipAndOcean = false;
            quickPerformanceOnly = false;
            quickErrorsOnly = true;
        }

        private void OnValidate()
        {
            // Ensure settings are valid
            maxMessagesPerFrame = Mathf.Clamp(maxMessagesPerFrame, 1, 100);
            messageThrottleTime = Mathf.Clamp(messageThrottleTime, 0f, 5f);

            // Apply quick controls if changed
            if (quickShipAndOcean && !quickPerformanceOnly && !quickErrorsOnly)
            {
                // Already handled by SetupShipAndOcean if user clicks it
            }
            else if (quickPerformanceOnly && !quickShipAndOcean && !quickErrorsOnly)
            {
                // Already handled by SetupPerformanceOnly if user clicks it
            }
            else if (quickErrorsOnly && !quickShipAndOcean && !quickPerformanceOnly)
            {
                // Already handled by SetupErrorsOnly if user clicks it
            }
        }
    }

    /// <summary>
    /// Debug categories for organizing log messages
    /// </summary>
    public enum DebugCategory
    {
        Ship,           // Ship physics, movement, controls
        Ocean,          // Ocean tiles, wake effects, environment
        Environment,    // Environment systems, ports, LOD, effects
        Camera,         // Camera movement, effects, LOD
        Input,          // User input, controls, navigation
        Physics,        // Physics calculations, collisions
        Performance,    // Performance metrics, optimization
        UI,             // User interface, HUD, menus
        Economy,        // Trading, cargo, ports
        Audio,          // Sound effects, music
        Networking,     // Multiplayer, server communication
        System          // Core systems, initialization
    }

    /// <summary>
    /// Debug settings for category control
    /// </summary>
    [System.Serializable]
    public class DebugSettings
    {
        [Header("Core Systems")]
        public bool enableShipDebug = true;
        public bool enableOceanDebug = true;
        public bool enableEnvironmentDebug = true;
        public bool enableCameraDebug = false;
        public bool enableInputDebug = false;

        [Header("Technical")]
        public bool enablePhysicsDebug = false;
        public bool enablePerformanceDebug = true;
        public bool enableSystemDebug = true;

        [Header("Features")]
        public bool enableUIDebug = false;
        public bool enableEconomyDebug = true;
        public bool enableAudioDebug = false;
        public bool enableNetworkingDebug = false;

        public void EnableAll()
        {
            enableShipDebug = true;
            enableOceanDebug = true;
            enableEnvironmentDebug = true;
            enableCameraDebug = true;
            enableInputDebug = true;
            enablePhysicsDebug = true;
            enablePerformanceDebug = true;
            enableUIDebug = true;
            enableEconomyDebug = true;
            enableAudioDebug = true;
            enableNetworkingDebug = true;
            enableSystemDebug = true;
        }

        public void DisableAll()
        {
            enableShipDebug = false;
            enableOceanDebug = false;
            enableEnvironmentDebug = false;
            enableCameraDebug = false;
            enableInputDebug = false;
            enablePhysicsDebug = false;
            enablePerformanceDebug = false;
            enableUIDebug = false;
            enableEconomyDebug = false;
            enableAudioDebug = false;
            enableNetworkingDebug = false;
            enableSystemDebug = false;
        }
    }

    /// <summary>
    /// Debug statistics for monitoring
    /// </summary>
    [System.Serializable]
    public struct DebugStats
    {
        public int messagesThisFrame;
        public int maxMessagesPerFrame;
        public int throttledMessages;
        public int enabledCategories;

        public override string ToString()
        {
            return $"Debug: {messagesThisFrame}/{maxMessagesPerFrame} msgs/frame, {enabledCategories} categories, {throttledMessages} throttled";
        }
    }
}