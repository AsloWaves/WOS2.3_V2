using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using WOS.Player;
using WOS.Debugging;
using WOS.Environment;
using WOS.ScriptableObjects;
using Mirror;

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
        [SerializeField] private MonoBehaviour shipController; // Can be SimpleNavalController or NetworkedNavalController

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

        // Initialization state
        private bool isInitialized = false;
        private float initializationStartTime;
        private const float INITIALIZATION_TIMEOUT = 30f; // Wait up to 30 seconds for player to spawn

        private void Start()
        {
            // Calculate update interval
            updateInterval = 1f / updateRate;
            lastUpdateTime = Time.time;
            lastUpdateTimeForRates = Time.time;
            initializationStartTime = Time.time;

            // Try initial setup (might not find player yet if they haven't spawned)
            TryInitialize();
        }

        private void TryInitialize()
        {
            // Auto-find ship controller if not assigned
            if (shipController == null)
            {
                // First try to find NetworkedNavalController (for multiplayer)
                var networkedController = FindFirstObjectByType<NetworkedNavalController>();
                if (networkedController != null)
                {
                    // Only show debug UI for LOCAL player in networked mode
                    if (networkedController.isLocalPlayer)
                    {
                        shipController = networkedController;
                        DebugManager.Log(DebugCategory.UI, "Found NetworkedNavalController (local player)", this);
                    }
                    else
                    {
                        DebugManager.Log(DebugCategory.UI, "NetworkedNavalController found but not local player - disabling debug UI", this);
                        enabled = false;
                        return;
                    }
                }
                else
                {
                    // Fallback to SimpleNavalController (for single-player testing)
                    shipController = FindFirstObjectByType<SimpleNavalController>();
                    if (shipController != null)
                    {
                        DebugManager.Log(DebugCategory.UI, "Found SimpleNavalController", this);
                    }
                }

                // If still no controller found, keep waiting (don't disable yet!)
                if (shipController == null)
                {
                    // Check if we've exceeded timeout
                    if (Time.time - initializationStartTime > INITIALIZATION_TIMEOUT)
                    {
                        DebugManager.LogWarning(DebugCategory.UI, "No ship controller found after 30 seconds - disabling debug UI", this);
                        enabled = false;
                        return;
                    }
                    // Otherwise, keep waiting - will retry in Update()
                    DebugManager.Log(DebugCategory.UI, "Waiting for player ship to spawn...", this);
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
            // If not initialized yet, keep trying to find player ship
            if (!isInitialized && shipController == null)
            {
                TryInitialize();
                if (shipController == null)
                {
                    return; // Don't try to update display yet - player hasn't spawned
                }
            }

            // Mark as initialized once controller and text component are found
            if (shipController != null && activeTextComponent != null && !isInitialized)
            {
                isInitialized = true;
                DebugManager.Log(DebugCategory.UI, "✅ ShipDebugUI initialization complete - displaying telemetry", this);
            }

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

            // Get ship status from either controller type
            ShipStatus shipStatus;
            ShipConfigurationSO shipConfig;

            if (shipController is NetworkedNavalController networkedController)
            {
                shipStatus = networkedController.GetShipStatus();
                shipConfig = networkedController.GetShipConfiguration();
            }
            else if (shipController is SimpleNavalController simpleController)
            {
                shipStatus = simpleController.GetShipStatus();
                shipConfig = simpleController.GetShipConfiguration();
            }
            else
            {
                DebugManager.LogError(DebugCategory.UI, $"Unknown ship controller type: {shipController.GetType().Name}", this);
                return;
            }

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

            // Get network statistics
            string networkInfo = GetNetworkStats();

            // Get port navigation information
            string portInfo = GetPortNavigationInfo();

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
                   $"{separator}NEAREST PORT{separator}" +
                   portInfo + separator +
                   $"{separator}OCEAN{separator}" +
                   oceanInfo +
                   $"{separator}NETWORK{separator}" +
                   networkInfo +
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
                // Get ship status from either controller type
                ShipStatus shipStatus;
                if (shipController is NetworkedNavalController networkedController)
                {
                    shipStatus = networkedController.GetShipStatus();
                }
                else if (shipController is SimpleNavalController simpleController)
                {
                    shipStatus = simpleController.GetShipStatus();
                }
                else
                {
                    return;
                }

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

        /// <summary>
        /// Get network statistics for multiplayer connections
        /// </summary>
        private string GetNetworkStats()
        {
            // Check if we're in networked mode
            if (shipController is not NetworkedNavalController)
            {
                return "Network: Single Player Mode";
            }

            // Check if client is active
            if (!NetworkClient.active)
            {
                return "Network: Not Connected";
            }

            // Get network statistics from Mirror
            string connectionStatus = NetworkClient.isConnected ? "Connected" : "Disconnected";

            // Calculate ping/RTT (Mirror uses milliseconds)
            // NetworkTime.rtt is Round Trip Time in seconds
            double rttSeconds = NetworkTime.rtt;
            int rttMs = Mathf.RoundToInt((float)(rttSeconds * 1000.0));

            // Get one-way latency (ping is half of RTT)
            int pingMs = rttMs / 2;

            // Connection quality indicator
            string qualityIndicator;
            if (pingMs < 50)
                qualityIndicator = "Excellent";
            else if (pingMs < 100)
                qualityIndicator = "Good";
            else if (pingMs < 150)
                qualityIndicator = "Fair";
            else if (pingMs < 250)
                qualityIndicator = "Poor";
            else
                qualityIndicator = "Critical";

            // Server/Client mode
            string mode = NetworkServer.active ? (NetworkClient.active ? "Host" : "Server") : "Client";

            return $"Status: {connectionStatus}\n" +
                   $"Mode: {mode}\n" +
                   $"Ping: {pingMs}ms\n" +
                   $"RTT: {rttMs}ms\n" +
                   $"Quality: {qualityIndicator}";
        }

        /// <summary>
        /// Find nearest port and calculate navigation data
        /// </summary>
        private string GetPortNavigationInfo()
        {
            if (shipController == null)
                return "Port: Not Available";

            // Get ship's current position
            Vector3 shipPos = shipController.transform.position;

            // Find all PortConfigurationSO instances in the scene
            // (This is a placeholder - in production you'd have a PortManager)
            var portConfigs = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            PortConfigurationSO nearestPort = null;
            float nearestDistance = float.MaxValue;
            Vector3 nearestPortPos = Vector3.zero;

            // Search for ports (placeholder implementation)
            // In a real implementation, you'd have a PortManager with registered ports
            foreach (var obj in portConfigs)
            {
                // Check if object has a PortConfigurationSO reference
                var portConfigField = obj.GetType().GetField("portConfig");
                if (portConfigField != null)
                {
                    var portConfig = portConfigField.GetValue(obj) as PortConfigurationSO;
                    if (portConfig != null)
                    {
                        float distance = Vector3.Distance(shipPos, obj.transform.position);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestPort = portConfig;
                            nearestPortPos = obj.transform.position;
                        }
                    }
                }
            }

            // If no ports found, return placeholder
            if (nearestPort == null)
            {
                return "Port: None Detected\n" +
                       "Bearing: ---°\n" +
                       "Distance: --- nm";
            }

            // Calculate absolute world bearing to port
            Vector3 directionToPort = nearestPortPos - shipPos;
            // Atan2(y, x) gives angle from positive X axis (which is Unity's 0° rotation)
            float absoluteBearingRadians = Mathf.Atan2(directionToPort.y, directionToPort.x);
            float absoluteBearingDegrees = absoluteBearingRadians * Mathf.Rad2Deg;

            // Normalize absolute bearing to 0-360
            if (absoluteBearingDegrees < 0)
                absoluteBearingDegrees += 360f;

            // Get ship's current heading (direction ship is facing)
            float shipHeading = shipController.transform.eulerAngles.z;

            // Calculate RELATIVE bearing (where ship SHOULD point to reach port)
            // Bearing = angle to turn from current heading
            // 0° = straight ahead (no turn needed)
            // 90° = turn 90° to starboard (right)
            // 270° = turn 90° to port (left)
            float relativeBearing = absoluteBearingDegrees - shipHeading;

            // Normalize relative bearing to 0-360
            if (relativeBearing < 0)
                relativeBearing += 360f;
            if (relativeBearing >= 360f)
                relativeBearing -= 360f;

            // Convert distance to nautical miles (1 Unity unit = 1 knot speed unit)
            // Assuming reasonable Unity scale: 1 Unity unit ≈ 0.01 nautical miles for distance
            float distanceNauticalMiles = nearestDistance * 0.01f;

            return $"Port: {nearestPort.portName}\n" +
                   $"Bearing: {relativeBearing:F0}°\n" +
                   $"Distance: {distanceNauticalMiles:F1} nm";
        }

        #region Public Methods

        /// <summary>
        /// Manually set the ship controller reference (accepts SimpleNavalController or NetworkedNavalController)
        /// </summary>
        public void SetShipController(MonoBehaviour controller)
        {
            if (controller is SimpleNavalController || controller is NetworkedNavalController)
            {
                shipController = controller;
            }
            else
            {
                DebugManager.LogError(DebugCategory.UI, $"Invalid controller type: {controller.GetType().Name}. Must be SimpleNavalController or NetworkedNavalController.", this);
            }
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