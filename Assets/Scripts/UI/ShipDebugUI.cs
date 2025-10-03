using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using WOS.Player;
using WOS.Debugging;
using WOS.Environment;
using WOS.ScriptableObjects;

namespace WOS.UI
{
    /// <summary>
    /// Real-time debug UI panel for displaying ship information and telemetry.
    /// Designed for use with MUIP (Multi-line UI Panel) components.
    /// </summary>
    public class ShipDebugUI : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("TextMeshPro component for displaying ship information (Primary - MUIP compatible)")]
        [SerializeField] private TextMeshProUGUI shipInfoText;

        [Tooltip("Alternative UI Text component if not using TextMeshPro")]
        [SerializeField] private Text legacyInfoText;

        [Tooltip("MUIP InputField component for multi-line display (if using MUIP package)")]
        [SerializeField] private TMP_InputField muipInputField;

        [Header("Update Settings")]
        [Tooltip("How often to update the display (times per second)")]
        [SerializeField] private float updateRate = 10f;

        [Tooltip("Number of decimal places for speed values")]
        [SerializeField] private int speedPrecision = 1;

        [Tooltip("Number of decimal places for angle values")]
        [SerializeField] private int anglePrecision = 1;

        [Header("MUIP Settings")]
        [Tooltip("Use MUIP InputField instead of regular text component")]
        [SerializeField] private bool useMuipInputField = false;

        [Tooltip("Make MUIP field read-only (recommended for display)")]
        [SerializeField] private bool muipReadOnly = true;

        [Tooltip("MUIP line spacing for better readability")]
        [SerializeField] private float muipLineSpacing = 1.2f;

        [Header("Input Controls")]
        [Tooltip("Enable F3 key to toggle debug panel visibility")]
        [SerializeField] private bool enableToggleKey = true;

        [Header("Ship Reference")]
        [Tooltip("Reference to the ship controller (auto-found if not assigned)")]
        [SerializeField] private SimpleNavalController shipController;

        [Header("Ocean Biome Reference")]
        [Tooltip("Reference to ocean chunk manager for depth info (auto-found if not assigned)")]
        [SerializeField] private OceanChunkManager oceanManager;

        // Update timing
        private float lastUpdateTime;
        private float updateInterval;

        // Cached values for rate calculations
        private float lastBearing;
        private float lastUpdateTimeForRates;
        private float rateOfTurn; // degrees per second

        // UI Text reference (either TMPro or legacy)
        private Component activeTextComponent;

        private void Start()
        {
            // Calculate update interval
            updateInterval = 1f / updateRate;
            lastUpdateTime = Time.time;
            lastUpdateTimeForRates = Time.time;

            // Auto-find ship controller if not assigned
            if (shipController == null)
            {
                shipController = FindFirstObjectByType<SimpleNavalController>();
                if (shipController == null)
                {
                    DebugManager.LogWarning(DebugCategory.UI, "No SimpleNavalController found in scene!", this);
                    enabled = false;
                    return;
                }
            }

            // Auto-find ocean manager if not assigned
            if (oceanManager == null)
            {
                oceanManager = FindFirstObjectByType<OceanChunkManager>();
                if (oceanManager == null)
                {
                    DebugManager.LogWarning(DebugCategory.UI, "No OceanChunkManager found in scene - depth info will not be available", this);
                }
            }

            // Auto-detect and configure text component
            if (muipInputField != null)
            {
                // MUIP InputField found - use it regardless of useMuipInputField setting
                activeTextComponent = muipInputField;
                useMuipInputField = true; // Auto-enable MUIP mode
                SetupMuipInputField();
                DebugManager.Log(DebugCategory.UI, "MUIP InputField detected and configured automatically", this);
            }
            else if (useMuipInputField && muipInputField == null)
            {
                // User enabled MUIP but no InputField assigned
                DebugManager.LogWarning(DebugCategory.UI, "'Use MUIP Input Field' is enabled but no InputField assigned! Falling back to TextMeshPro.", this);
                useMuipInputField = false;
            }

            // Fallback to standard text components if MUIP not available
            if (!useMuipInputField)
            {
                if (shipInfoText != null)
                {
                    activeTextComponent = shipInfoText;
                    DebugManager.Log(DebugCategory.UI, "Using TextMeshPro component for display", this);
                }
                else if (legacyInfoText != null)
                {
                    activeTextComponent = legacyInfoText;
                    DebugManager.Log(DebugCategory.UI, "Using Legacy UI Text component for display", this);
                }
                else
                {
                    DebugManager.LogError(DebugCategory.UI, "No text component assigned! Please assign either:\n" +
                                  "- muipInputField (TMP_InputField for MUIP)\n" +
                                  "- shipInfoText (TextMeshPro)\n" +
                                  "- legacyInfoText (UI Text)");
                    enabled = false;
                    return;
                }
            }

            // Initialize display
            UpdateDisplay();
        }

        private void Update()
        {
            // Handle F3 toggle key
            if (enableToggleKey && Input.GetKeyDown(KeyCode.F3))
            {
                ToggleVisibility();
            }

            // Update at specified rate
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateDisplay();
                lastUpdateTime = Time.time;
            }

            // Calculate rate of turn
            CalculateRateOfTurn();
        }

        private void UpdateDisplay()
        {
            if (shipController == null || activeTextComponent == null) return;

            // Get ship status
            var shipStatus = shipController.GetShipStatus();
            var shipConfig = shipController.GetShipConfiguration();

            // Build information string
            string infoText = BuildShipInfoText(shipStatus, shipConfig);

            // Update the active text component
            if (activeTextComponent is TMP_InputField inputField)
            {
                inputField.text = infoText;
            }
            else if (activeTextComponent is TextMeshProUGUI tmpText)
            {
                tmpText.text = infoText;
            }
            else if (activeTextComponent is Text uiText)
            {
                uiText.text = infoText;
            }
        }

        private string BuildShipInfoText(ShipStatus status, WOS.ScriptableObjects.ShipConfigurationSO config)
        {
            // Ship identification
            string shipName = config != null ? config.shipName : "Unknown Vessel";
            string shipClass = config != null ? config.shipClass.ToString() : "Unknown";

            // Speed and throttle information
            string currentSpeed = status.speed.ToString($"F{speedPrecision}");
            string throttleSetting = GetThrottleDescription(status.throttle);
            string targetSpeed = CalculateTargetSpeed(status.throttle, config).ToString($"F{speedPrecision}");

            // Navigation information
            string bearing = status.heading.ToString($"F{anglePrecision}");
            string rudderAngle = status.rudderAngle.ToString($"F{anglePrecision}");
            string turnRate = rateOfTurn.ToString($"F{anglePrecision}");

            // Ship specifications (if config available)
            string specs = "";
            if (config != null)
            {
                specs = $"\n<b>SPECIFICATIONS</b>\n" +
                       $"Max Speed: {config.maxSpeed:F0} knots\n" +
                       $"Length: {config.length:F0}m\n" +
                       $"Displacement: {config.displacement:F0} tons\n" +
                       $"Max Rudder: ±{config.maxRudderAngle:F0}°";
            }

            // Auto-navigation status
            string navStatus = status.isAutoNavigating ? $"AUTO NAV ({status.waypointCount} waypoints)" : "MANUAL";

            // Format for MUIP compatibility (better line spacing and structure)
            string separator = useMuipInputField ? "\n" : "\n";
            string header = useMuipInputField ? "=== SHIP TELEMETRY ===" : "<b>=== SHIP TELEMETRY ===</b>";

            // Build complete string with MUIP-optimized formatting
            // Get ocean depth and tile information
            string oceanInfo = GetOceanDepthInfo();

            return $"{header}{separator}" +
                   $"VESSEL: {shipName}{separator}" +
                   $"CLASS: {shipClass}{separator}" +
                   $"{separator}PROPULSION{separator}" +
                   $"Current Speed: {currentSpeed} kts{separator}" +
                   $"Target Speed: {targetSpeed} kts{separator}" +
                   $"Throttle: {throttleSetting}{separator}" +
                   $"{separator}NAVIGATION{separator}" +
                   $"Bearing: {bearing}°{separator}" +
                   $"Rate of Turn: {turnRate}°/s{separator}" +
                   $"Rudder Angle: {rudderAngle}°{separator}" +
                   $"Mode: {navStatus}{separator}" +
                   $"{separator}OCEAN{separator}" +
                   oceanInfo +
                   (useMuipInputField ? specs.Replace("<b>", "").Replace("</b>", "") : specs);
        }

        private string GetThrottleDescription(float throttle)
        {
            int throttleInt = Mathf.RoundToInt(throttle);
            return throttleInt switch
            {
                -4 => "Full Astern (-4)",
                -3 => "Half Astern (-3)",
                -2 => "Slow Astern (-2)",
                -1 => "Dead Slow Astern (-1)",
                0 => "Full Stop (0)",
                1 => "Slow Ahead (1)",
                2 => "Half Ahead (2)",
                3 => "Full Ahead (3)",
                4 => "Flank Speed (4)",
                _ => $"Unknown ({throttle:F1})"
            };
        }

        private float CalculateTargetSpeed(float throttle, WOS.ScriptableObjects.ShipConfigurationSO config)
        {
            if (config == null) return 0f;

            int throttleInt = Mathf.RoundToInt(throttle);
            return throttleInt switch
            {
                -4 => -config.maxSpeed,
                -3 => -config.maxSpeed * 0.66f,
                -2 => -config.maxSpeed * 0.33f,
                -1 => -config.maxSpeed * 0.15f,
                0 => 0f,
                1 => config.maxSpeed * 0.25f,
                2 => config.maxSpeed * 0.50f,
                3 => config.maxSpeed * 0.75f,
                4 => config.maxSpeed,
                _ => 0f
            };
        }

        private string GetOceanDepthInfo()
        {
            if (shipController == null)
                return "Ship: Not Available";

            if (oceanManager == null)
                return "Depth: No Ocean Manager\nTile: Not Available";

            // Get ship's current world position
            Vector3 shipWorldPos = shipController.transform.position;
            float2 shipPos2D = new float2(shipWorldPos.x, shipWorldPos.y);

            string depthInfo = "Depth: Unknown";
            string tileInfo = "Tile: Unknown Type";
            string biomeInfo = "";

            // Try to get biome configuration from ocean manager
            var biomeConfig = GetBiomeConfigFromOceanManager();
            if (biomeConfig != null)
            {
                // Calculate depth at ship position
                float depth = biomeConfig.CalculateDepthAtPosition(shipPos2D);
                depthInfo = $"Depth: {depth:F1}m";

                // Get tile type for this depth
                var tileType = biomeConfig.GetTileTypeForDepth(depth);
                if (tileType != null)
                {
                    tileInfo = $"Tile: {tileType.tileName}";
                    biomeInfo = $"Zone: {tileType.depthZone}";
                }
                else
                {
                    tileInfo = "Tile: No Match Found";
                    biomeInfo = "Zone: Undefined";
                }
            }
            else
            {
                depthInfo = "Depth: No Biome Config";
                tileInfo = "Tile: Legacy Mode";
            }

            return $"{depthInfo}\n{tileInfo}\n{biomeInfo}";
        }

        private OceanBiomeConfigurationSO GetBiomeConfigFromOceanManager()
        {
            if (oceanManager == null) return null;

            // Use reflection to get the biome config field from OceanChunkManager
            var field = typeof(OceanChunkManager).GetField("biomeConfig",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                return field.GetValue(oceanManager) as OceanBiomeConfigurationSO;
            }

            return null;
        }

        private void CalculateRateOfTurn()
        {
            if (shipController == null) return;

            float currentTime = Time.time;
            float deltaTime = currentTime - lastUpdateTimeForRates;

            if (deltaTime >= 0.1f) // Update rate of turn every 100ms
            {
                var shipStatus = shipController.GetShipStatus();
                float currentBearing = shipStatus.heading;

                // Calculate angular difference (handle 360° wraparound)
                float bearingDifference = Mathf.DeltaAngle(lastBearing, currentBearing);
                rateOfTurn = bearingDifference / deltaTime;

                lastBearing = currentBearing;
                lastUpdateTimeForRates = currentTime;
            }
        }

        private void SetupMuipInputField()
        {
            if (muipInputField == null) return;

            // Configure MUIP InputField for display purposes
            muipInputField.readOnly = muipReadOnly;
            muipInputField.lineType = TMP_InputField.LineType.MultiLineNewline;

            // Set text properties for better readability
            if (muipInputField.textComponent != null)
            {
                muipInputField.textComponent.lineSpacing = muipLineSpacing;
                muipInputField.textComponent.enableWordWrapping = true;
                muipInputField.textComponent.overflowMode = TextOverflowModes.Overflow;
            }

            // Disable interaction if read-only
            if (muipReadOnly)
            {
                muipInputField.interactable = false;
            }

            DebugManager.Log(DebugCategory.UI, "MUIP InputField configured for ship telemetry display", this);
        }

        #region Public Methods

        /// <summary>
        /// Manually set the ship controller reference
        /// </summary>
        public void SetShipController(SimpleNavalController controller)
        {
            shipController = controller;
        }

        /// <summary>
        /// Set the update rate for the display
        /// </summary>
        public void SetUpdateRate(float rate)
        {
            updateRate = Mathf.Clamp(rate, 0.1f, 60f);
            updateInterval = 1f / updateRate;
        }

        /// <summary>
        /// Toggle the debug panel visibility
        /// </summary>
        public void ToggleVisibility()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        /// <summary>
        /// Get the current rate of turn
        /// </summary>
        public float GetRateOfTurn()
        {
            return rateOfTurn;
        }

        #endregion

        #region Inspector Validation

        private void OnValidate()
        {
            // Ensure update rate is reasonable
            updateRate = Mathf.Clamp(updateRate, 0.1f, 60f);
            speedPrecision = Mathf.Clamp(speedPrecision, 0, 3);
            anglePrecision = Mathf.Clamp(anglePrecision, 0, 3);
            muipLineSpacing = Mathf.Clamp(muipLineSpacing, 0.5f, 3f);

            // Auto-detect MUIP InputField assignment
            if (muipInputField != null && !useMuipInputField)
            {
                DebugManager.Log(DebugCategory.UI, "MUIP InputField detected! Auto-enabling MUIP mode.", this);
                useMuipInputField = true;
            }

            // Validate component assignments
            int assignedComponents = 0;
            if (muipInputField != null) assignedComponents++;
            if (shipInfoText != null) assignedComponents++;
            if (legacyInfoText != null) assignedComponents++;

            if (assignedComponents == 0)
            {
                DebugManager.LogWarning(DebugCategory.UI, "No text component assigned! Please assign one of: MUIP InputField, TextMeshPro, or Legacy Text.", this);
            }
            else if (assignedComponents > 1)
            {
                DebugManager.LogWarning(DebugCategory.UI, "Multiple text components assigned. Priority: MUIP InputField > TextMeshPro > Legacy Text", this);
            }
        }

        #endregion
    }
}