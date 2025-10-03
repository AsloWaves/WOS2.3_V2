using UnityEngine;
using WOS.Player;
using WOS.Camera;
using WOS.Debugging;

namespace WOS.Testing
{
    /// <summary>
    /// Handles returning the player to the main scene after exiting a harbor.
    /// Should be attached to the player ship in the main scene.
    /// </summary>
    public class PortReturnHandler : MonoBehaviour
    {
        [Header("Return Configuration")]
        [Tooltip("Automatically check for port exit data on scene load")]
        public bool autoCheckOnStart = true;

        [Tooltip("Reference to the ship controller (auto-finds if null)")]
        public SimpleNavalController shipController;

        [Tooltip("Reference to the camera controller (auto-finds if null)")]
        public SimpleCameraController cameraController;

        [Header("Debug")]
        [Tooltip("Show debug messages")]
        public bool showDebugInfo = true;

        private void Start()
        {
            if (autoCheckOnStart)
            {
                CheckForPortReturn();
            }
        }

        public void CheckForPortReturn()
        {
            // Check if we have valid port exit data
            if (PlayerPrefs.GetInt("PortExit_Valid", 0) == 1)
            {
                Debug.Log("🚢 PORT RETURN DETECTED - Restoring ship position from harbor exit");
                RestoreShipFromPortExit();

                // Clear the flag so we don't restore again next time
                PlayerPrefs.DeleteKey("PortExit_Valid");
                PlayerPrefs.Save();
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log("No port exit data found - normal scene start");
                }
            }
        }

        private void RestoreShipFromPortExit()
        {
            // Get components if not assigned
            if (shipController == null)
            {
                shipController = GetComponent<SimpleNavalController>();
            }

            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<SimpleCameraController>();
            }

            // Read the exit position and rotation
            Vector3 exitPosition = Vector3.zero;
            Quaternion exitRotation = Quaternion.identity;

            if (PlayerPrefs.HasKey("PortExit_Position"))
            {
                string posJson = PlayerPrefs.GetString("PortExit_Position");
                exitPosition = JsonUtility.FromJson<Vector3>(posJson);
            }

            if (PlayerPrefs.HasKey("PortExit_Rotation"))
            {
                string rotJson = PlayerPrefs.GetString("PortExit_Rotation");
                exitRotation = JsonUtility.FromJson<Quaternion>(rotJson);
            }

            // Apply position and rotation
            transform.position = exitPosition;
            transform.rotation = exitRotation;

            Debug.Log($"✅ Ship restored to position: {exitPosition}");
            Debug.Log($"🔄 Ship rotation set to: {exitRotation.eulerAngles} (180° from entry)");

            // Reset physics and throttle
            ResetShipState();

            // Set camera to follow ship
            if (cameraController != null)
            {
                cameraController.SetTarget(transform);
                Debug.Log("📷 Camera set to follow ship");
            }

            // Clean up PlayerPrefs
            CleanupPortData();
        }

        private void ResetShipState()
        {
            if (shipController == null) return;

            // Get the Rigidbody2D and reset physics
            var rb = shipController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Make sure it's Dynamic (not Kinematic)
                rb.bodyType = RigidbodyType2D.Dynamic;

                // Reset velocities
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Use reflection to reset throttle and speed
            var shipType = typeof(SimpleNavalController);

            // Reset throttle
            var throttleField = shipType.GetField("currentThrottle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (throttleField != null)
            {
                throttleField.SetValue(shipController, 0f);
                Debug.Log("⚙️ Throttle reset to 0");
            }

            // Reset speed
            var speedField = shipType.GetField("currentSpeed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (speedField != null)
            {
                speedField.SetValue(shipController, 0f);
                Debug.Log("🛑 Speed reset to 0");
            }

            // Make sure controller is enabled
            shipController.enabled = true;

            Debug.Log("✅ Ship state reset - Ready for player control");
            Debug.Log("⌨️ Use W/S to control throttle and move");
        }

        private void CleanupPortData()
        {
            // Clean up all port-related PlayerPrefs
            PlayerPrefs.DeleteKey("PortEntry_Position");
            PlayerPrefs.DeleteKey("PortEntry_Rotation");
            PlayerPrefs.DeleteKey("PortEntry_PortID");
            PlayerPrefs.DeleteKey("PortEntry_ExitScene");
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

        // Manual trigger method if needed
        [ContextMenu("Force Port Return Check")]
        public void ForcePortReturnCheck()
        {
            CheckForPortReturn();
        }
    }
}