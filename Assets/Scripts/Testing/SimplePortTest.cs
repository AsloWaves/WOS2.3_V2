using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Mirror;
using WOS.ScriptableObjects;
using WOS.Player;
using WOS.Camera;
using WOS.Debugging;

namespace WOS.Testing
{
    /// <summary>
    /// Networked port system for entering/exiting harbors
    /// Only local player can interact, server handles scene transitions
    /// </summary>
    public class SimplePortTest : NetworkBehaviour
    {
        [Header("Test Configuration")]
        public PortConfigurationSO portConfig;

        [Header("Real Game Objects")]
        [Tooltip("Drag your player ship GameObject here (or will auto-find)")]
        public Transform playerShip;

        [Tooltip("Drag your SimpleCameraController here (or will auto-find)")]
        public SimpleCameraController simpleCameraController;

        [Header("Test Results")]
        public bool isInProtectionZone = false;
        public bool isApproachingBoundary = false;
        public float distanceFromPort = 0f;

        [Header("Visual Settings")]
        [Tooltip("Distance from port edge where players get transported to harbor")]
        [Range(50f, 200f)]
        public float dockingTransportDistance = 100f;

        [Tooltip("Show gizmos even when not selected")]
        public bool alwaysShowGizmos = true;

        [Header("Port Entry/Exit System")]
        [Tooltip("Test the entry/exit position saving (E to simulate harbor entry, R to simulate exit)")]
        public bool testEntryExitSystem = true;

        [Tooltip("Show entry position marker in scene")]
        public bool showEntryPosition = true;

        [Header("Scene Transition")]
        [Tooltip("Enable actual scene loading when entering docking zone")]
        public bool enableSceneTransition = false;

        [Tooltip("Override the scene name from port config (for testing)")]
        public string overrideSceneName = "";

        private NetworkedNavalController shipController;
        private NetworkIdentity playerNetworkIdentity;

        // Entry/Exit state tracking
        private bool hasEntryPositionSaved = false;
        private Vector3 savedEntryPosition;
        private Quaternion savedEntryRotation;
        private bool isInHarbor = false;

        private void Start()
        {
            Debug.Log("=== REAL SHIP PORT TEST STARTED ===");

            // Check if port config is assigned
            if (portConfig == null)
            {
                Debug.LogError("NO PORT CONFIG ASSIGNED!");
                return;
            }

            Debug.Log($"Port config found: {portConfig.name}");
            Debug.Log($"Protection radius: {portConfig.protectionRadius}");
            Debug.Log($"Port name: {portConfig.portName}");

            // Find LOCAL networked player ship if not assigned
            if (playerShip == null)
            {
                FindLocalPlayerShip();
                if (playerShip == null)
                {
                    Debug.LogWarning("⚠️ No local networked player found yet - will try again each frame");
                    // Don't return - we'll keep trying in Update()
                }
            }
            else
            {
                // If manually assigned, get its network identity
                playerNetworkIdentity = playerShip.GetComponent<NetworkIdentity>();
            }

            // Find camera controller if not assigned
            if (simpleCameraController == null)
            {
                simpleCameraController = FindFirstObjectByType<SimpleCameraController>();
                if (simpleCameraController != null)
                {
                    Debug.Log($"Auto-found simple camera controller: {simpleCameraController.name}");
                }
                else
                {
                    Debug.LogWarning("No SimpleCameraController found! Camera won't follow the ship.");
                }
            }

            // Set up camera to follow the player ship
            if (simpleCameraController != null && playerShip != null)
            {
                simpleCameraController.SetTarget(playerShip);
                Debug.Log("✅ Camera set to follow player ship using SimpleCameraController!");
            }

            // Get ship controller reference
            if (playerShip != null)
            {
                shipController = playerShip.GetComponent<NetworkedNavalController>();
                if (shipController != null)
                {
                    Debug.Log("Networked ship controller found - ready for testing!");
                }
                else
                {
                    Debug.LogWarning("No NetworkedNavalController found on player ship");
                }

                Debug.Log($"Player ship position: {playerShip.position}");
                Debug.Log($"Port position: {transform.position}");
                Debug.Log("=== TEST SETUP COMPLETE - Drive your ship toward the port! ===");
            }
        }

        /// <summary>
        /// Find the LOCAL networked player ship
        /// </summary>
        private void FindLocalPlayerShip()
        {
            var allShips = FindObjectsByType<NetworkedNavalController>(FindObjectsSortMode.None);

            foreach (var shipCtrl in allShips)
            {
                var networkIdentity = shipCtrl.GetComponent<NetworkIdentity>();
                if (networkIdentity != null && networkIdentity.isLocalPlayer)
                {
                    playerShip = shipCtrl.transform;
                    playerNetworkIdentity = networkIdentity;
                    shipController = shipCtrl;
                    Debug.Log($"🌊 Port found LOCAL networked player: {shipCtrl.gameObject.name}");
                    return;
                }
            }
        }

        private void Update()
        {
            if (portConfig == null) return;

            // Try to find player if not found yet
            if (playerShip == null)
            {
                FindLocalPlayerShip();
                return; // Wait until we find the local player
            }

            // Only process for LOCAL player
            if (playerNetworkIdentity == null || !playerNetworkIdentity.isLocalPlayer)
                return;

            // Handle entry/exit testing
            HandleEntryExitTesting();

            // Test protection zone with real ship position
            Vector3 portPos = transform.position;
            Vector3 shipPos = playerShip.position;

            // Check for automatic docking transport
            CheckDockingTransport(portPos, shipPos);

            // Check protection zone status
            bool wasInZone = isInProtectionZone;
            bool wasApproaching = isApproachingBoundary;

            isInProtectionZone = portConfig.IsWithinProtectionZone(shipPos, portPos);
            isApproachingBoundary = portConfig.IsApproachingProtectionBoundary(shipPos, portPos);
            distanceFromPort = Vector3.Distance(shipPos, portPos);

            // Log when entering/exiting zone
            if (wasInZone != isInProtectionZone)
            {
                if (isInProtectionZone)
                    Debug.Log($"🛡️ SHIP ENTERED PROTECTION ZONE at distance {distanceFromPort:F1}m");
                else
                    Debug.Log($"⚠️ SHIP EXITED PROTECTION ZONE at distance {distanceFromPort:F1}m");
            }

            // Log when approaching boundary
            if (!wasApproaching && isApproachingBoundary)
            {
                Debug.Log($"🔔 APPROACHING PROTECTION BOUNDARY at distance {distanceFromPort:F1}m");
            }

            // Show ship velocity if available
            if (shipController != null && Time.frameCount % 60 == 0) // Every second
            {
                Vector3 velocity = shipController.GetVelocity();
                DebugManager.Log(DebugCategory.Environment, $"Ship Status: Distance={distanceFromPort:F0}m, Speed={velocity.magnitude:F1}, Protected={isInProtectionZone}", this);
            }
        }

        private void HandleEntryExitTesting()
        {
            if (!testEntryExitSystem || playerShip == null) return;

            // E key to simulate harbor entry (or trigger real scene load if enabled)
            if (Input.GetKeyDown(KeyCode.E) && !isInHarbor)
            {
                // CRITICAL: Only allow entry if player is CURRENTLY within docking zone
                float currentDistance = Vector3.Distance(playerShip.position, transform.position);
                bool inDockingZoneNow = currentDistance <= dockingTransportDistance;

                if (!inDockingZoneNow)
                {
                    Debug.LogWarning("⚠️ Cannot enter harbor - Ship must be within docking zone (blue circle)!");
                    Debug.LogWarning($"📏 Distance to port: {currentDistance:F1}m / Required: ≤{dockingTransportDistance}m");
                    return;
                }

                if (enableSceneTransition && portConfig != null)
                {
                    // ALWAYS re-save position when pressing E to enter
                    // This ensures exit position updates each time you enter
                    SaveEntryPosition();
                    TransitionToHarborScene();
                }
                else
                {
                    SimulateHarborEntry();
                }
            }

            // R key to simulate harbor exit
            if (Input.GetKeyDown(KeyCode.R) && isInHarbor)
            {
                SimulateHarborExit();
            }
        }

        private void CheckDockingTransport(Vector3 portPos, Vector3 shipPos)
        {
            float distanceToPort = Vector3.Distance(shipPos, portPos);

            // Check if ship is within docking transport distance
            bool inDockingZone = distanceToPort <= dockingTransportDistance;

            // Reset the flag if ship leaves docking zone (allows re-entry)
            if (!inDockingZone && hasEntryPositionSaved && !isInHarbor)
            {
                hasEntryPositionSaved = false;
                Debug.Log("🔄 Ship left docking zone - entry flag reset");
            }

            // Auto-stop ship and save position when entering docking zone
            if (inDockingZone && !isInHarbor && !hasEntryPositionSaved)
            {
                Debug.Log($"🚢 DOCKING ZONE ENTERED: Ship within {dockingTransportDistance}m - AUTO-STOPPING");

                // Save position for entry/exit
                SaveEntryPosition();

                // STOP THE SHIP when entering docking zone
                StopShipForDocking();

                // If NOT using scene transition, auto-trigger simulation
                if (!enableSceneTransition)
                {
                    // Simulation mode - automatically enter harbor
                    SimulateHarborEntry();
                }
                else
                {
                    // Real scene mode - ship is stopped, wait for player to press E
                    Debug.Log("📝 Ship STOPPED - Press E to enter harbor");
                }
            }
        }

        private void StopShipForDocking()
        {
            if (shipController == null) return;

            var rb = shipController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Stop all movement
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Reset networked controller state using reflection
            var shipType = typeof(NetworkedNavalController);
            var throttleField = shipType.GetField("currentThrottle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (throttleField != null)
            {
                throttleField.SetValue(shipController, 0f);
            }

            // Reset speed
            var speedField = shipType.GetField("currentSpeed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (speedField != null)
            {
                speedField.SetValue(shipController, 0f);
            }

            Debug.Log("🛑 Networked ship auto-stopped for docking");
        }

        private void SaveEntryPosition()
        {
            if (playerShip == null) return;

            // Save absolute position for local use
            savedEntryPosition = playerShip.position;
            savedEntryRotation = playerShip.rotation;
            hasEntryPositionSaved = true;

            // Calculate port-relative offset for cross-scene persistence
            Vector3 portCenter = transform.position;
            Vector3 relativeOffset = playerShip.position - portCenter;

            Debug.Log($"💾 ENTRY POSITION SAVED: Pos={savedEntryPosition}, Rot={savedEntryRotation.eulerAngles}");
            Debug.Log($"🏗️ Port Center: {portCenter}, Relative Offset: {relativeOffset}");
            Debug.Log("📝 Press E to simulate harbor entry, R to simulate exit with 180° rotation");
        }

        private void SimulateHarborEntry()
        {
            if (!hasEntryPositionSaved)
            {
                SaveEntryPosition();
            }

            // Stop ship and reset throttle when entering harbor
            if (shipController != null)
            {
                // IMPORTANT: Disable the ship controller to prevent any movement
                shipController.enabled = false;
                Debug.Log("🔒 Ship controller DISABLED");

                // Reset ship physics completely
                var rb = shipController.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Stop all movement
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;

                    // Make the rigidbody kinematic to prevent any physics movement
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    Debug.Log("🔐 Rigidbody set to Kinematic - no physics movement");
                }

                // Reset networked ship controller throttle and speed states
                var shipType = typeof(NetworkedNavalController);

                // Reset throttle (SyncVar)
                var throttleField = shipType.GetField("currentThrottle",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (throttleField != null)
                {
                    throttleField.SetValue(shipController, 0f);
                    Debug.Log($"✅ Network throttle reset to 0");
                }

                // Reset current speed (SyncVar)
                var currentSpeedField = shipType.GetField("currentSpeed",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (currentSpeedField != null)
                {
                    currentSpeedField.SetValue(shipController, 0f);
                }

                // Reset rudder angle (SyncVar)
                var rudderField = shipType.GetField("effectiveRudderAngle",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (rudderField != null)
                {
                    rudderField.SetValue(shipController, 0f);
                }

                Debug.Log("🛑 HARBOR ENTRY: Ship completely stopped and locked");
            }

            isInHarbor = true;
            Debug.Log("🏗️ SIMULATED: Player entered harbor scene");
            Debug.Log("⚡ Player is now in harbor interface (simulated)");
        }

        private void SimulateHarborExit()
        {
            if (!hasEntryPositionSaved)
            {
                Debug.LogWarning("No entry position saved! Cannot exit properly.");
                return;
            }

            // Calculate exit position and rotation
            Vector3 exitPosition = savedEntryPosition;
            Quaternion exitRotation = savedEntryRotation * Quaternion.Euler(0, 180, 0); // 180-degree turn

            // Apply the exit position and rotation
            playerShip.position = exitPosition;
            playerShip.rotation = exitRotation;

            // Reset states
            isInHarbor = false;
            hasEntryPositionSaved = false;

            Debug.Log($"🚢 HARBOR EXIT: Player returned to {exitPosition}");
            Debug.Log($"🔄 ROTATED 180°: New rotation = {exitRotation.eulerAngles}");
            Debug.Log("✅ Player is now facing away from port");

            // Completely stop ship and reset throttle state
            if (shipController != null)
            {
                // Reset ship physics and restore to Dynamic mode
                var rb = shipController.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Restore rigidbody to Dynamic for normal physics
                    rb.bodyType = RigidbodyType2D.Dynamic;

                    // Then stop all movement
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    Debug.Log("🔓 Rigidbody restored to Dynamic mode");
                }

                // Reset networked ship controller state BEFORE re-enabling
                var shipType = typeof(NetworkedNavalController);

                // Reset throttle (SyncVar)
                var throttleField = shipType.GetField("currentThrottle",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (throttleField != null)
                {
                    throttleField.SetValue(shipController, 0f);
                    Debug.Log("✅ EXIT: Network throttle reset to 0");
                }

                // Reset current speed (SyncVar)
                var currentSpeedField = shipType.GetField("currentSpeed",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (currentSpeedField != null)
                {
                    currentSpeedField.SetValue(shipController, 0f);
                }

                // Reset rudder angle (SyncVar)
                var rudderField = shipType.GetField("effectiveRudderAngle",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (rudderField != null)
                {
                    rudderField.SetValue(shipController, 0f);
                }

                // IMPORTANT: Re-enable the ship controller AFTER resetting values
                shipController.enabled = true;

                Debug.Log("🛑 NETWORKED SHIP STATE RESET: Speed=0, Throttle=0, Rudder=0, Physics stopped");
                Debug.Log("🔓 Networked ship controller RE-ENABLED");
                Debug.Log("⌨️ Player must use W/S to engage throttle and move");
            }
        }

        private void TransitionToHarborScene()
        {
            string sceneName = !string.IsNullOrEmpty(overrideSceneName) ? overrideSceneName : portConfig.harborSceneName;

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("No harbor scene name configured!");
                return;
            }

            // Save all necessary data before scene transition
            SavePlayerDataForTransition();

            // Load the harbor scene
            if (portConfig.useAsyncLoading)
            {
                StartCoroutine(LoadHarborSceneAsync(sceneName));
            }
            else
            {
                LoadHarborScene(sceneName);
            }
        }

        private void SavePlayerDataForTransition()
        {
            // Calculate port-relative data for cross-scene persistence
            Vector3 portCenter = transform.position;
            Vector3 relativeOffset = savedEntryPosition - portCenter;

            // Create a data container to persist across scene load
            PortTransitionData transitionData = new PortTransitionData
            {
                entryPosition = savedEntryPosition,
                entryRotation = savedEntryRotation,
                portID = portConfig.name,
                playerShipName = playerShip.name,
                exitSceneName = SceneManager.GetActiveScene().name
            };

            // Store in PlayerPrefs (enhanced with port-relative data)
            PlayerPrefs.SetString("PortEntry_Position", JsonUtility.ToJson(transitionData.entryPosition));
            PlayerPrefs.SetString("PortEntry_Rotation", JsonUtility.ToJson(transitionData.entryRotation));
            PlayerPrefs.SetString("PortEntry_PortID", transitionData.portID);
            PlayerPrefs.SetString("PortEntry_ExitScene", transitionData.exitSceneName);

            // NEW: Save port center and relative offset for proper exit positioning
            PlayerPrefs.SetString("PortEntry_PortCenter", JsonUtility.ToJson(portCenter));
            PlayerPrefs.SetString("PortEntry_RelativeOffset", JsonUtility.ToJson(relativeOffset));
            PlayerPrefs.Save();

            Debug.Log($"💾 Saved transition data: Exit scene={transitionData.exitSceneName}");
            Debug.Log($"🏗️ Port center: {portCenter}, Relative offset: {relativeOffset}");
        }

        private void LoadHarborScene(string sceneName)
        {
            Debug.Log($"🏗️ Loading harbor scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator LoadHarborSceneAsync(string sceneName)
        {
            Debug.Log($"🏗️ Loading harbor scene async: {sceneName}");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Show loading progress
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                Debug.Log($"Loading progress: {progress * 100}%");
                yield return null;
            }

            Debug.Log("✅ Harbor scene loaded!");
        }

        // Data structure to hold transition information
        [System.Serializable]
        public class PortTransitionData
        {
            public Vector3 entryPosition;
            public Quaternion entryRotation;
            public string portID;
            public string playerShipName;
            public string exitSceneName;
        }

        [ContextMenu("Manual Test")]
        public void ManualTest()
        {
            if (portConfig == null)
            {
                Debug.LogError("No port config assigned!");
                return;
            }

            Debug.Log("=== MANUAL TEST ===");
            Debug.Log($"Port: {portConfig.portName}");
            Debug.Log($"Protection Radius: {portConfig.protectionRadius}");
            Debug.Log($"Warning Distance: {portConfig.protectionWarningDistance}");

            Vector3 portPos = transform.position;
            Vector3 testPos1 = portPos; // Center
            Vector3 testPos2 = portPos + Vector3.right * (portConfig.protectionRadius * 0.5f); // Inside
            Vector3 testPos3 = portPos + Vector3.right * (portConfig.protectionRadius * 1.1f); // Outside

            Debug.Log($"Center (0m): Protected = {portConfig.IsWithinProtectionZone(testPos1, portPos)}");
            Debug.Log($"Inside ({portConfig.protectionRadius * 0.5f:F0}m): Protected = {portConfig.IsWithinProtectionZone(testPos2, portPos)}");
            Debug.Log($"Outside ({portConfig.protectionRadius * 1.1f:F0}m): Protected = {portConfig.IsWithinProtectionZone(testPos3, portPos)}");
        }

        private void OnDrawGizmos()
        {
            if (alwaysShowGizmos)
            {
                DrawPortGizmos();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!alwaysShowGizmos)
            {
                DrawPortGizmos();
            }
        }

        private void DrawPortGizmos()
        {
            if (portConfig == null) return;

            Vector3 portCenter = transform.position;

            // Draw Protection Zone (Green circle)
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Semi-transparent green
            DrawWireCircle(portCenter, portConfig.protectionRadius);

            // Draw filled protection zone (very transparent)
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            DrawFilledCircle(portCenter, portConfig.protectionRadius);

            // Draw Protection Warning Boundary (Yellow circle)
            float warningRadius = portConfig.protectionRadius - portConfig.protectionWarningDistance;
            if (warningRadius > 0)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // Semi-transparent yellow
                DrawWireCircle(portCenter, warningRadius);
            }

            // Draw Docking Transport Zone (Blue circle)
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f); // Semi-transparent blue
            DrawWireCircle(portCenter, dockingTransportDistance);

            // Draw filled docking zone (very transparent)
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
            DrawFilledCircle(portCenter, dockingTransportDistance);

            // Draw port center marker
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(portCenter, 10f);

            // Draw connection line to player ship if it exists
            if (playerShip != null)
            {
                // Color code the line based on protection status
                if (isInProtectionZone)
                    Gizmos.color = Color.green;
                else if (isApproachingBoundary)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawLine(portCenter, playerShip.position);

                // Draw ship position marker
                Gizmos.DrawWireSphere(playerShip.position, 15f);

                // Draw distance text position
                Vector3 midPoint = (portCenter + playerShip.position) * 0.5f;
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(midPoint, Vector3.one * 5f);
            }

            // Draw entry position marker if saved
            DrawEntryPositionGizmo();

            // Draw labels using GUI (will show in scene view)
            DrawGizmoLabels(portCenter);
        }

        private void DrawWireCircle(Vector3 center, float radius)
        {
            const int segments = 64;
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        private void DrawFilledCircle(Vector3 center, float radius)
        {
            // Draw filled circle using matrix and DrawMesh if available, or approximate with lines
            const int segments = 32;
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);

                // Draw triangle from center to edge
                Gizmos.DrawLine(center, point1);
                Gizmos.DrawLine(point1, point2);
                Gizmos.DrawLine(point2, center);
            }
        }

        private void DrawGizmoLabels(Vector3 portCenter)
        {
            // This method can be expanded to show text labels in the scene view
            // Labels would require OnDrawGizmos to be called from editor scripts

            // Draw directional indicators for zones
            if (portConfig != null)
            {
                // Protection zone indicator
                Vector3 protectionPoint = portCenter + Vector3.right * portConfig.protectionRadius;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(protectionPoint, protectionPoint + Vector3.up * 50f);

                // Docking zone indicator
                Vector3 dockingPoint = portCenter + Vector3.right * dockingTransportDistance;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(dockingPoint, dockingPoint + Vector3.up * 30f);
            }
        }

        private void DrawEntryPositionGizmo()
        {
            if (!showEntryPosition || !hasEntryPositionSaved) return;

            // Draw saved entry position marker
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(savedEntryPosition, 20f);

            // Draw entry direction arrow
            Vector3 forwardDirection = savedEntryRotation * Vector3.up;
            Vector3 arrowEnd = savedEntryPosition + forwardDirection * 40f;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(savedEntryPosition, arrowEnd);

            // Draw arrow head
            Vector3 arrowLeft = arrowEnd + (savedEntryRotation * Vector3.left) * 10f;
            Vector3 arrowRight = arrowEnd + (savedEntryRotation * Vector3.right) * 10f;
            Gizmos.DrawLine(arrowEnd, arrowLeft);
            Gizmos.DrawLine(arrowEnd, arrowRight);

            // Draw exit direction (180 degrees rotated)
            Quaternion exitRotation = savedEntryRotation * Quaternion.Euler(0, 180, 0);
            Vector3 exitDirection = exitRotation * Vector3.up;
            Vector3 exitArrowEnd = savedEntryPosition + exitDirection * 40f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(savedEntryPosition, exitArrowEnd);

            // Draw exit arrow head
            Vector3 exitArrowLeft = exitArrowEnd + (exitRotation * Vector3.left) * 10f;
            Vector3 exitArrowRight = exitArrowEnd + (exitRotation * Vector3.right) * 10f;
            Gizmos.DrawLine(exitArrowEnd, exitArrowLeft);
            Gizmos.DrawLine(exitArrowEnd, exitArrowRight);

            // Draw status indicator
            if (isInHarbor)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(savedEntryPosition + Vector3.up * 60f, Vector3.one * 15f);
            }
        }
    }
}