using UnityEngine;
using Mirror;
using WOS.Player;
using WOS.Camera;
using WOS.Debugging;

namespace WOS.Testing
{
    /// <summary>
    /// Manages networked player positioning when returning from harbor scenes.
    /// Works with Mirror NetworkManager - does NOT spawn players, only positions them.
    /// </summary>
    public class ScenePortManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        [Tooltip("NOTE: Player ships are spawned by NetworkManager, not this script.\nThis script only positions already-spawned networked players.")]
        public bool showNetworkDebug = true;

        [Header("Camera Configuration")]
        [Tooltip("Reference to the camera with SimpleCameraController")]
        public SimpleCameraController cameraController;

        [Header("Spawn Settings")]
        [Tooltip("Default spawn position if no port data")]
        public Vector3 defaultSpawnPosition = new Vector3(0, 0, 0);

        [Tooltip("Default spawn rotation if no port data")]
        public Vector3 defaultSpawnRotation = new Vector3(0, 0, 0);

        [Header("Ship Orientation")]
        [Tooltip("Offset angle for ship's forward direction. 90° if ship sprite points up, 0° if points right")]
        public float shipSpriteForwardOffset = 90f;

        [Header("Debug")]
        public bool showDebugInfo = true;

        private GameObject localPlayerShip;
        private bool hasCheckedPortReturn = false;

        private void Start()
        {
            // Wait for network spawn before checking port returns
            StartCoroutine(WaitForNetworkSpawnAndCheckPortReturn());
        }

        private System.Collections.IEnumerator WaitForNetworkSpawnAndCheckPortReturn()
        {
            // Wait for NetworkManager to spawn player
            yield return new WaitForSeconds(0.5f);

            // Find camera if not assigned
            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<SimpleCameraController>();
            }

            // Find local player ship (networked)
            FindLocalPlayerShip();

            // Check for port return ONLY if we're the server or host
            if (NetworkServer.active && !hasCheckedPortReturn)
            {
                CheckAndHandlePortReturn();
                hasCheckedPortReturn = true;
            }
        }

        private void FindLocalPlayerShip()
        {
            // Find all networked ships
            var allShips = FindObjectsByType<NetworkedNavalController>(FindObjectsSortMode.None);

            foreach (var shipCtrl in allShips)
            {
                var networkIdentity = shipCtrl.GetComponent<NetworkIdentity>();
                if (networkIdentity != null && networkIdentity.isLocalPlayer)
                {
                    localPlayerShip = shipCtrl.gameObject;
                    if (showNetworkDebug)
                    {
                        Debug.Log($"🌊 ScenePortManager found LOCAL networked player: {shipCtrl.gameObject.name}");
                    }
                    return;
                }
            }

            if (showNetworkDebug)
            {
                Debug.LogWarning("⚠️ ScenePortManager: No local networked player found yet");
            }
        }

        private void CheckAndHandlePortReturn()
        {
            // Check if we're returning from a port
            if (PlayerPrefs.GetInt("PortExit_Valid", 0) == 1)
            {
                Debug.Log("🚢 PORT RETURN DETECTED - Calculating port-relative exit position");

                Vector3 exitPosition = defaultSpawnPosition;
                Quaternion exitRotation = Quaternion.Euler(defaultSpawnRotation);

                // NEW: Check for port-relative data first
                if (PlayerPrefs.HasKey("PortEntry_PortCenter") &&
                    PlayerPrefs.HasKey("PortEntry_RelativeOffset") &&
                    PlayerPrefs.HasKey("PortEntry_PortID"))
                {
                    string portID = PlayerPrefs.GetString("PortEntry_PortID");
                    Vector3 relativeOffset = JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString("PortEntry_RelativeOffset"));

                    // Find the port in this scene by ID
                    SimplePortTest targetPort = FindPortByID(portID);
                    if (targetPort != null)
                    {
                        Vector3 currentPortCenter = targetPort.transform.position;
                        exitPosition = currentPortCenter + relativeOffset;

                        // CRITICAL: Calculate rotation to face AWAY from port center
                        // This ensures ship always exits facing away, regardless of entry direction
                        Vector3 directionAwayFromPort = (exitPosition - currentPortCenter).normalized;

                        // Convert to 2D rotation (Z-axis for top-down naval game)
                        float angleInDegrees = Mathf.Atan2(directionAwayFromPort.y, directionAwayFromPort.x) * Mathf.Rad2Deg;

                        // Adjust for ship's default sprite orientation
                        // For 2D top-down: sprite pointing up (north) = 0° rotation in Unity
                        // We want the ship to face the direction vector, which already points away from port
                        exitRotation = Quaternion.Euler(0, 0, angleInDegrees);

                        Debug.Log($"🎯 Found port '{portID}' at {currentPortCenter}");
                        Debug.Log($"🚢 Calculated exit position: {currentPortCenter} + {relativeOffset} = {exitPosition}");
                        Debug.Log($"🧭 Direction away from port: {directionAwayFromPort}");
                        Debug.Log($"📐 Atan2 angle: {angleInDegrees}° (pointing direction)");
                        Debug.Log($"🔄 Exit rotation set to: {exitRotation.eulerAngles}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Could not find port with ID '{portID}' in scene - using saved port center");

                        // Use the saved port center from entry
                        Vector3 savedPortCenter = JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString("PortEntry_PortCenter"));
                        exitPosition = savedPortCenter + relativeOffset;

                        // Still calculate rotation away from port, even with saved center
                        Vector3 directionAwayFromPort = (exitPosition - savedPortCenter).normalized;
                        float angleInDegrees = Mathf.Atan2(directionAwayFromPort.y, directionAwayFromPort.x) * Mathf.Rad2Deg;
                        exitRotation = Quaternion.Euler(0, 0, angleInDegrees);

                        Debug.Log($"🔄 Using saved port center for rotation calculation: {savedPortCenter}");
                    }
                }
                else
                {
                    // FALLBACK: Use absolute position (old system)
                    if (PlayerPrefs.HasKey("PortExit_Position"))
                    {
                        string posJson = PlayerPrefs.GetString("PortExit_Position");
                        exitPosition = JsonUtility.FromJson<Vector3>(posJson);
                        Debug.Log("⚠️ Using absolute position fallback");
                    }

                    // Only use saved rotation if we're in fallback mode (no port-relative data)
                    // Otherwise we already calculated the correct directional rotation above
                    if (PlayerPrefs.HasKey("PortExit_Rotation"))
                    {
                        string rotJson = PlayerPrefs.GetString("PortExit_Rotation");
                        exitRotation = JsonUtility.FromJson<Quaternion>(rotJson);
                        Debug.Log("⚠️ Using saved rotation from fallback system");
                    }
                }

                // Position the networked player (server-side)
                if (NetworkServer.active)
                {
                    PositionNetworkedPlayer(exitPosition, exitRotation);
                }

                // Clear the flag
                PlayerPrefs.DeleteKey("PortExit_Valid");
                PlayerPrefs.Save();
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log("Normal scene start - no port return data");
                }
            }
        }

        /// <summary>
        /// Position an already-spawned networked player ship
        /// NOTE: This does NOT spawn ships - NetworkManager handles that
        /// </summary>
        private void PositionNetworkedPlayer(Vector3 position, Quaternion rotation)
        {
            // Find the local player's ship
            if (localPlayerShip == null)
            {
                FindLocalPlayerShip();
            }

            if (localPlayerShip == null)
            {
                Debug.LogError("❌ Cannot position player - no networked local player found!");
                return;
            }

            // SERVER-SIDE: Position the networked ship
            if (NetworkServer.active)
            {
                localPlayerShip.transform.position = position;
                localPlayerShip.transform.rotation = rotation;

                // Reset ship state after port return
                ResetShipStateAfterPort(localPlayerShip, rotation);

                Debug.Log($"🌊 SERVER: Positioned networked player at {position}, rotation: {rotation.eulerAngles}");
            }
        }

        private void ResetShipStateAfterPort(GameObject ship, Quaternion targetRotation)
        {
            var shipController = ship.GetComponent<NetworkedNavalController>();
            if (shipController == null) return;

            // Make sure the controller is enabled
            shipController.enabled = true;

            // Get the Rigidbody2D and reset physics
            var rb = ship.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Make sure it's Dynamic (not Kinematic)
                rb.bodyType = RigidbodyType2D.Dynamic;

                // CRITICAL: Set rotation AFTER body is Dynamic
                rb.rotation = targetRotation.eulerAngles.z;

                // Reset velocities
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                if (showDebugInfo)
                {
                    Debug.Log($"🔧 Rigidbody2D rotation synchronized: {rb.rotation}°");
                    Debug.Log($"🔧 Transform rotation: {ship.transform.rotation.eulerAngles.z}°");
                }
            }

            // Use reflection to reset networked controller states
            var shipType = typeof(NetworkedNavalController);

            // Reset throttle (SyncVar)
            var throttleField = shipType.GetField("currentThrottle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (throttleField != null)
            {
                throttleField.SetValue(shipController, 0f);
                Debug.Log("⚙️ Network throttle reset to 0");
            }

            // Reset speed (SyncVar)
            var speedField = shipType.GetField("currentSpeed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (speedField != null)
            {
                speedField.SetValue(shipController, 0f);
                Debug.Log("🛑 Network speed reset to 0");
            }

            // Reset rudder angle (SyncVar)
            var rudderField = shipType.GetField("effectiveRudderAngle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rudderField != null)
            {
                rudderField.SetValue(shipController, 0f);
                Debug.Log("🎚️ Rudder angle reset to 0");
            }

            Debug.Log("✅ Networked ship ready for player control");
            Debug.Log("⌨️ Use W/S to control throttle, A/D to steer");

            // Clean up port data
            CleanupPortData();
        }

        private void SetupCamera(GameObject ship)
        {
            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<SimpleCameraController>();
            }

            if (cameraController != null)
            {
                cameraController.SetTarget(ship.transform);
                Debug.Log("📷 Camera set to follow spawned ship");
            }
            else
            {
                Debug.LogWarning("No camera controller found to follow ship!");
            }
        }

        private SimplePortTest FindPortByID(string portID)
        {
            // Find all SimplePortTest components in the scene
            SimplePortTest[] allPorts = FindFirstObjectByType<SimplePortTest>().GetComponents<SimplePortTest>();

            // If only one port found, use FindFirstObjectByType instead
            if (allPorts == null || allPorts.Length == 0)
            {
                SimplePortTest singlePort = FindFirstObjectByType<SimplePortTest>();
                if (singlePort != null && singlePort.portConfig != null && singlePort.portConfig.name == portID)
                {
                    return singlePort;
                }
            }
            else
            {
                // Search through all ports for matching ID
                foreach (SimplePortTest port in allPorts)
                {
                    if (port.portConfig != null && port.portConfig.name == portID)
                    {
                        return port;
                    }
                }
            }

            // Alternative search: try to find by GameObject name if config name doesn't match
            SimplePortTest[] allPortsInScene = FindObjectsByType<SimplePortTest>(FindObjectsSortMode.None);
            foreach (SimplePortTest port in allPortsInScene)
            {
                if (port.portConfig != null && port.portConfig.name == portID)
                {
                    return port;
                }
            }

            return null;
        }

        private void CleanupPortData()
        {
            // Clean up all port-related PlayerPrefs
            PlayerPrefs.DeleteKey("PortEntry_Position");
            PlayerPrefs.DeleteKey("PortEntry_Rotation");
            PlayerPrefs.DeleteKey("PortEntry_PortID");
            PlayerPrefs.DeleteKey("PortEntry_ExitScene");
            PlayerPrefs.DeleteKey("PortEntry_PortCenter");
            PlayerPrefs.DeleteKey("PortEntry_RelativeOffset");
            PlayerPrefs.DeleteKey("PortExit_Position");
            PlayerPrefs.DeleteKey("PortExit_Rotation");
            PlayerPrefs.DeleteKey("PortExit_Throttle");
            PlayerPrefs.DeleteKey("PortExit_Speed");
            PlayerPrefs.Save();

            if (showDebugInfo)
            {
                Debug.Log("🧹 Port transition data cleaned up");
            }
        }

        // Helper method to manually trigger port return check
        [ContextMenu("Force Port Return Check")]
        public void ForcePortReturnCheck()
        {
            CheckAndHandlePortReturn();
        }
    }
}