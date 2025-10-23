using UnityEngine;
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
    /// Manages 6 separate debug panels displaying ship information across the bottom of the screen.
    /// Replaces monolithic ShipDebugUI with individual TextMeshProUGUI fields for better layout control.
    /// Always visible (no toggle), updates at 10Hz, local player only in multiplayer.
    /// </summary>
    public class ShipDebugUIManager : MonoBehaviour
    {
        #region Panel References

        [Header("Panel 1 - VESSEL & SPECS")]
        [Tooltip("Ship name (e.g., 'USS Constitution')")]
        public TextMeshProUGUI vesselName;

        [Tooltip("Ship class (e.g., 'Frigate')")]
        public TextMeshProUGUI vesselClass;

        [Tooltip("Ship length in meters")]
        public TextMeshProUGUI vesselLength;

        [Tooltip("Ship displacement in tons")]
        public TextMeshProUGUI vesselDisplacement;

        [Tooltip("Maximum rudder angle (±degrees)")]
        public TextMeshProUGUI vesselMaxRudder;

        [Header("Panel 2 - PROPULSION & OCEAN")]
        [Tooltip("Current speed in knots")]
        public TextMeshProUGUI propCurrentSpeed;

        [Tooltip("Target speed in knots")]
        public TextMeshProUGUI propTargetSpeed;

        [Tooltip("Throttle setting (e.g., 'FULL AHEAD')")]
        public TextMeshProUGUI propThrottle;

        [Tooltip("Maximum speed in knots")]
        public TextMeshProUGUI propMaxSpeed;

        [Tooltip("Ocean depth in meters")]
        public TextMeshProUGUI oceanDepth;

        [Tooltip("Ocean tile type")]
        public TextMeshProUGUI oceanTileType;

        [Tooltip("Ocean zone name")]
        public TextMeshProUGUI oceanZone;

        [Header("Panel 3 - NAVIGATION")]
        [Tooltip("Current bearing in degrees")]
        public TextMeshProUGUI navBearing;

        [Tooltip("Rate of turn in degrees per second")]
        public TextMeshProUGUI navRateOfTurn;

        [Tooltip("Current rudder angle in degrees")]
        public TextMeshProUGUI navRudderAngle;

        [Tooltip("Navigation mode (AUTO/MANUAL)")]
        public TextMeshProUGUI navMode;

        [Header("Panel 4 - NEAREST PORT")]
        [Tooltip("Name of nearest port")]
        public TextMeshProUGUI portName;

        [Tooltip("Bearing to port in degrees")]
        public TextMeshProUGUI portBearing;

        [Tooltip("Distance to port in nautical miles")]
        public TextMeshProUGUI portDistance;

        [Header("Panel 5 - NETWORK")]
        [Tooltip("Connection status (Connected/Disconnected)")]
        public TextMeshProUGUI netStatus;

        [Tooltip("Network mode (Host/Client/Server)")]
        public TextMeshProUGUI netMode;

        [Tooltip("Ping in milliseconds")]
        public TextMeshProUGUI netPing;

        [Tooltip("Round-trip time in milliseconds")]
        public TextMeshProUGUI netRTT;

        [Tooltip("Network quality indicator")]
        public TextMeshProUGUI netQuality;

        [Header("Panel 6 - RESERVED")]
        // Empty for now - user will add fields later

        #endregion

        #region Configuration

        [Header("Update Settings")]
        [Tooltip("How often to update the display (times per second)")]
        [SerializeField] private float updateRate = 10f;

        [Tooltip("Number of decimal places for speed values")]
        [SerializeField] private int speedPrecision = 1;

        [Tooltip("Number of decimal places for angle values")]
        [SerializeField] private int anglePrecision = 1;

        [Header("Ship Reference")]
        [Tooltip("Reference to the ship controller (auto-found if not assigned)")]
        [SerializeField] private MonoBehaviour shipController; // Can be SimpleNavalController or NetworkedNavalController

        [Header("Ocean Biome Reference")]
        [Tooltip("Reference to ocean chunk manager for depth info (auto-found if not assigned)")]
        [SerializeField] private OceanChunkManager oceanManager;

        #endregion

        #region Private Fields

        // Update timing
        private float lastUpdateTime;
        private float updateInterval;

        // Cached values for rate calculations
        private float lastBearing;
        private float lastUpdateTimeForRates;
        private float rateOfTurn; // degrees per second

        // Initialization state
        private bool isInitialized = false;
        private float initializationStartTime;
        private const float INITIALIZATION_TIMEOUT = 30f; // Wait up to 30 seconds for player to spawn

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // ALWAYS log this so we know component is running
            Debug.Log("=== SHIPDEBUGUI MANAGER START ===");

            // Calculate update interval
            updateInterval = 1f / updateRate;
            lastUpdateTime = Time.time;
            lastUpdateTimeForRates = Time.time;
            initializationStartTime = Time.time;

            // Try initial setup (might not find player yet if they haven't spawned)
            TryInitialize();

            Debug.Log("=== SHIPDEBUGUI MANAGER START COMPLETE ===");
        }

        private void Update()
        {
            // Keep retrying initialization until ship is found OR we timeout
            if (!isInitialized)
            {
                // Check if we've exceeded timeout
                if (Time.time - initializationStartTime > INITIALIZATION_TIMEOUT)
                {
                    Debug.LogError("[ShipDebugUI] ❌ Initialization TIMEOUT - no ship found after 30 seconds!");
                    enabled = false; // Disable this component
                    return;
                }

                // Retry initialization every second
                if (Time.time - lastUpdateTime >= 1f)
                {
                    Debug.Log("[ShipDebugUI] Retrying initialization...");
                    TryInitialize();
                    lastUpdateTime = Time.time;
                }
                return; // Don't try to update display yet - player hasn't spawned
            }

            // Update at specified rate
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateAllPanels();
                lastUpdateTime = Time.time;
            }

            // Calculate rate of turn
            CalculateRateOfTurn();
        }

        #endregion

        #region Initialization

        private void TryInitialize()
        {
            Debug.Log("[ShipDebugUI] TryInitialize() called");

            // Auto-find ship controller if not assigned
            if (shipController == null)
            {
                Debug.Log("[ShipDebugUI] Searching for ship controller...");

                // First try to find NetworkedNavalController (for multiplayer)
                var networkedController = FindFirstObjectByType<NetworkedNavalController>();
                if (networkedController != null)
                {
                    Debug.Log($"[ShipDebugUI] Found NetworkedNavalController: {networkedController.name}, isLocalPlayer: {networkedController.isLocalPlayer}");

                    // Only show debug UI for LOCAL player in networked mode
                    if (networkedController.isLocalPlayer)
                    {
                        shipController = networkedController;
                        isInitialized = true; // ← Mark as initialized!
                        Debug.Log($"[ShipDebugUI] ✅ Assigned ship controller: {networkedController.name}");
                        Debug.Log("[ShipDebugUI] ✅✅✅ INITIALIZATION COMPLETE - Displaying telemetry!");
                        DebugManager.Log(DebugCategory.UI, $"ShipDebugUIManager found NetworkedNavalController: {networkedController.name} (Local Player)", this);
                    }
                    else
                    {
                        Debug.LogWarning("[ShipDebugUI] Found NetworkedNavalController but it's NOT local player - skipping");
                        DebugManager.Log(DebugCategory.UI, "ShipDebugUIManager found NetworkedNavalController but it's not local player - skipping", this);
                    }
                }
                else
                {
                    Debug.Log("[ShipDebugUI] No NetworkedNavalController found, trying SimpleNavalController...");

                    // Fall back to SimpleNavalController (single-player mode)
                    var simpleController = FindFirstObjectByType<SimpleNavalController>();
                    if (simpleController != null)
                    {
                        shipController = simpleController;
                        isInitialized = true; // ← Mark as initialized!
                        Debug.Log($"[ShipDebugUI] ✅ Assigned SimpleNavalController: {simpleController.name}");
                        Debug.Log("[ShipDebugUI] ✅✅✅ INITIALIZATION COMPLETE - Displaying telemetry!");
                        DebugManager.Log(DebugCategory.UI, $"ShipDebugUIManager found SimpleNavalController: {simpleController.name}", this);
                    }
                    else
                    {
                        Debug.Log("[ShipDebugUI] ⚠️ No ship controller found yet");
                    }
                }
            }
            else
            {
                Debug.Log($"[ShipDebugUI] Ship controller already assigned: {shipController.name}");
            }

            // Auto-find ocean manager if not assigned
            if (oceanManager == null)
            {
                oceanManager = FindFirstObjectByType<OceanChunkManager>();
                if (oceanManager != null)
                {
                    Debug.Log("[ShipDebugUI] Found OceanChunkManager");
                    DebugManager.Log(DebugCategory.UI, "ShipDebugUIManager found OceanChunkManager", this);
                }
            }
        }

        #endregion

        #region Panel Update Methods

        private void UpdateAllPanels()
        {
            if (!isInitialized)
            {
                // Only log once per second to avoid spam
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log("[ShipDebugUI] UpdateAllPanels skipped - not initialized yet");
                }
                return;
            }

            if (shipController == null)
            {
                Debug.LogWarning("[ShipDebugUI] UpdateAllPanels skipped - shipController is null!");
                return;
            }

            // Log first update only to confirm panels are updating
            if (Time.frameCount % 600 == 0) // Every ~10 seconds
            {
                Debug.Log("[ShipDebugUI] 🔄 Updating all panels...");
            }

            // Get ship data from either controller type
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

            // Update all panels
            UpdatePanel1(shipConfig);
            UpdatePanel2(shipStatus, shipConfig);
            UpdatePanel3(shipStatus);
            UpdatePanel4();
            UpdatePanel5();
            // Panel 6 is reserved for future use
        }

        private void UpdatePanel1(ShipConfigurationSO config)
        {
            if (config == null) return;

            SafeSetText(vesselName, config.shipName); // No label - ship name is self-explanatory
            SafeSetText(vesselClass, $"Class: {config.shipClass.ToString()}");
            SafeSetText(vesselLength, $"Length: {config.length:F0}m");
            SafeSetText(vesselDisplacement, $"Displacement: {config.displacement:F0} tons");
            SafeSetText(vesselMaxRudder, $"Max Rudder: ±{config.maxRudderAngle:F0}°");
        }

        private void UpdatePanel2(ShipStatus status, ShipConfigurationSO config)
        {
            // Propulsion info
            SafeSetText(propCurrentSpeed, $"Speed: {status.speed.ToString($"F{speedPrecision}")} kts");
            SafeSetText(propTargetSpeed, $"Target: {CalculateTargetSpeed(status.throttle, config).ToString($"F{speedPrecision}")} kts");
            SafeSetText(propThrottle, $"Throttle: {GetThrottleDescription(status.throttle)}");

            if (config != null)
            {
                SafeSetText(propMaxSpeed, $"Max: {config.maxSpeed:F0} kts");
            }

            // Ocean info
            var oceanInfo = GetOceanDepthInfoStructured();
            SafeSetText(oceanDepth, $"Depth: {oceanInfo.depthText}");
            SafeSetText(oceanTileType, $"Tile: {oceanInfo.tileText}");
            SafeSetText(oceanZone, $"Zone: {oceanInfo.zoneText}");
        }

        private void UpdatePanel3(ShipStatus status)
        {
            SafeSetText(navBearing, $"Bearing: {status.heading.ToString($"F{anglePrecision}")}°");
            SafeSetText(navRateOfTurn, $"Turn Rate: {rateOfTurn.ToString($"F{anglePrecision}")}°/s");
            SafeSetText(navRudderAngle, $"Rudder: {status.rudderAngle.ToString($"F{anglePrecision}")}°");

            string navStatus = status.isAutoNavigating ? "AUTO" : "MANUAL";
            SafeSetText(navMode, $"Mode: {navStatus}");
        }

        private void UpdatePanel4()
        {
            var portInfo = GetPortNavigationInfoStructured();
            SafeSetText(portName, $"Port: {portInfo.portName}");
            SafeSetText(portBearing, $"Bearing: {portInfo.bearingText}");
            SafeSetText(portDistance, $"Distance: {portInfo.distanceText}");
        }

        private void UpdatePanel5()
        {
            var netStats = GetNetworkStatsStructured();
            SafeSetText(netStatus, $"Status: {netStats.status}");
            SafeSetText(netMode, $"Mode: {netStats.mode}");
            SafeSetText(netPing, $"Ping: {netStats.pingText}");
            SafeSetText(netRTT, $"RTT: {netStats.rttText}");
            SafeSetText(netQuality, $"Quality: {netStats.quality}");
        }

        #endregion

        #region Helper Methods

        private void SafeSetText(TextMeshProUGUI textField, string value)
        {
            if (textField != null)
            {
                textField.text = value;
            }
            else
            {
                // Only log first time to avoid spam
                if (Time.frameCount % 600 == 0) // Log every ~10 seconds at 60fps
                {
                    DebugManager.LogWarning(DebugCategory.UI, $"TMP field is null! Cannot set value: {value}", this);
                }
            }
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

        private float CalculateTargetSpeed(float throttle, ShipConfigurationSO config)
        {
            if (config == null) return 0f;

            float maxSpeed = config.maxSpeed;
            int throttleInt = Mathf.RoundToInt(throttle);

            return throttleInt switch
            {
                -4 => -maxSpeed * 0.6f,      // Full Astern: 60% of max speed
                -3 => -maxSpeed * 0.4f,      // Half Astern: 40% of max speed
                -2 => -maxSpeed * 0.25f,     // Slow Astern: 25% of max speed
                -1 => -maxSpeed * 0.1f,      // Dead Slow Astern: 10% of max speed
                0 => 0f,                     // Full Stop
                1 => maxSpeed * 0.25f,       // Slow Ahead: 25% of max speed
                2 => maxSpeed * 0.5f,        // Half Ahead: 50% of max speed
                3 => maxSpeed * 0.85f,       // Full Ahead: 85% of max speed
                4 => maxSpeed,               // Flank Speed: 100% of max speed
                _ => 0f
            };
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

        #endregion

        #region Data Extraction Methods

        private OceanInfo GetOceanDepthInfoStructured()
        {
            OceanInfo info = new OceanInfo
            {
                depthText = "Unknown",
                tileText = "Unknown",
                zoneText = "Unknown"
            };

            if (shipController == null) return info;

            // Get ship position
            Vector3 shipPos = shipController.transform.position;
            Unity.Mathematics.float2 worldPos2D = new Unity.Mathematics.float2(shipPos.x, shipPos.y);

            // Try to get biome configuration from ocean manager
            OceanBiomeConfigurationSO biomeConfig = GetBiomeConfigFromOceanManager();

            if (biomeConfig != null)
            {
                // Calculate depth at ship's position
                float depth = biomeConfig.CalculateDepthAtPosition(worldPos2D);
                info.depthText = $"{depth:F1}m";

                // Get tile type for this depth
                var tileType = biomeConfig.GetTileTypeForDepth(depth);
                if (tileType != null)
                {
                    info.tileText = tileType.tileName;
                    info.zoneText = tileType.depthZone.ToString();
                }
                else
                {
                    info.tileText = "No Match Found";
                    info.zoneText = "Undefined";
                }
            }
            else
            {
                info.depthText = "No Biome Config";
                info.tileText = "Legacy Mode";
                info.zoneText = "---";
            }

            return info;
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

        private NetworkStats GetNetworkStatsStructured()
        {
            NetworkStats stats = new NetworkStats
            {
                status = "Disconnected",
                mode = "Unknown",
                pingText = "---",
                rttText = "---",
                quality = "---"
            };

            // Check if we're in networked mode
            if (shipController is not NetworkedNavalController)
            {
                stats.status = "Single Player";
                stats.mode = "Local";
                return stats;
            }

            // Check if client is active
            if (!NetworkClient.active)
            {
                stats.status = "Not Connected";
                return stats;
            }

            // Get network statistics from Mirror
            stats.status = NetworkClient.isConnected ? "Connected" : "Disconnected";

            // Calculate ping/RTT (Mirror uses milliseconds)
            // NetworkTime.rtt is Round Trip Time in seconds
            double rttSeconds = NetworkTime.rtt;
            int rttMs = Mathf.RoundToInt((float)(rttSeconds * 1000.0));

            // Get one-way latency (ping is half of RTT)
            int pingMs = rttMs / 2;

            stats.pingText = $"{pingMs}ms";
            stats.rttText = $"{rttMs}ms";

            // Connection quality indicator
            if (pingMs < 50)
                stats.quality = "Excellent";
            else if (pingMs < 100)
                stats.quality = "Good";
            else if (pingMs < 150)
                stats.quality = "Fair";
            else if (pingMs < 250)
                stats.quality = "Poor";
            else
                stats.quality = "Critical";

            // Server/Client mode
            stats.mode = NetworkServer.active ? (NetworkClient.active ? "Host" : "Server") : "Client";

            return stats;
        }

        private PortInfo GetPortNavigationInfoStructured()
        {
            PortInfo info = new PortInfo
            {
                portName = "None Detected",
                bearingText = "---°",
                distanceText = "--- nm"
            };

            if (shipController == null)
                return info;

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
                return info;
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

            info.portName = nearestPort.portName;
            info.bearingText = $"{relativeBearing:F0}°";
            info.distanceText = $"{distanceNauticalMiles:F1} nm";

            return info;
        }

        #endregion

        #region Data Structures

        private struct OceanInfo
        {
            public string depthText;
            public string tileText;
            public string zoneText;
        }

        private struct PortInfo
        {
            public string portName;
            public string bearingText;
            public string distanceText;
        }

        private struct NetworkStats
        {
            public string status;
            public string mode;
            public string pingText;
            public string rttText;
            public string quality;
        }

        #endregion
    }
}
