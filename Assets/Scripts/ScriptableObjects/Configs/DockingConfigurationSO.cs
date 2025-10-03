using UnityEngine;
using System.Collections.Generic;
using WOS.Debugging;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Configuration for docking mechanics including animations, positioning, and timing.
    /// Controls how ships dock and undock from harbors.
    /// </summary>
    [CreateAssetMenu(fileName = "DockingConfig", menuName = "WOS/Environment/Docking Configuration")]
    public class DockingConfigurationSO : ScriptableObject
    {
        [System.Serializable]
        public enum DockingType
        {
            Pier,           // Dock alongside a pier
            Berth,          // Dock in an enclosed berth
            Anchor,         // Drop anchor in designated area
            Mooring         // Tie to mooring buoys
        }

        [System.Serializable]
        public enum CardinalDirection
        {
            North,
            South,
            East,
            West,
            Northeast,
            Northwest,
            Southeast,
            Southwest
        }

        [System.Serializable]
        public class DockingAnimation
        {
            [Header("Animation Timing")]
            [Tooltip("Time to dock from detection to final position")]
            [Range(1f, 10f)]
            public float dockingDuration = 3f;

            [Tooltip("Time to undock from start to clear")]
            [Range(1f, 8f)]
            public float undockingDuration = 2f;

            [Tooltip("Delay before starting undocking animation")]
            [Range(0f, 2f)]
            public float undockingDelay = 0.5f;

            [Header("Movement Curves")]
            [Tooltip("Speed curve during docking (0=start, 1=end)")]
            public AnimationCurve dockingSpeedCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

            [Tooltip("Rotation curve during docking")]
            public AnimationCurve dockingRotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            [Tooltip("Speed curve during undocking")]
            public AnimationCurve undockingSpeedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            [Header("Physics Interaction")]
            [Tooltip("Disable ship physics during docking animation")]
            public bool disablePhysicsDuringDocking = true;

            [Tooltip("Smoothly transition physics back after docking")]
            public bool smoothPhysicsTransition = true;

            [Tooltip("Time to fade physics back in")]
            [Range(0.1f, 2f)]
            public float physicsTransitionTime = 0.5f;
        }

        [System.Serializable]
        public class DockingZoneSettings
        {
            [Header("Zone Properties")]
            [Tooltip("Type of docking for this zone")]
            public DockingType dockingType = DockingType.Pier;

            [Tooltip("Direction ships should face when docked")]
            public CardinalDirection facingDirection = CardinalDirection.North;

            [Tooltip("Offset from zone center for final ship position")]
            public Vector3 dockingOffset = Vector3.zero;

            [Header("Detection")]
            [Tooltip("Radius for detecting ships wanting to dock")]
            [Range(10f, 100f)]
            public float detectionRadius = 30f;

            [Tooltip("Minimum speed required to initiate docking")]
            [Range(0f, 5f)]
            public float minimumDockingSpeed = 0.5f;

            [Tooltip("Maximum speed allowed for docking")]
            [Range(1f, 15f)]
            public float maximumDockingSpeed = 8f;

            [Header("Size Restrictions")]
            [Tooltip("Minimum ship size that can use this zone")]
            [Range(0f, 100f)]
            public float minimumShipSize = 0f;

            [Tooltip("Maximum ship size that can use this zone")]
            [Range(10f, 200f)]
            public float maximumShipSize = 50f;

            [Header("Approach Path")]
            [Tooltip("Preferred approach angle relative to facing direction")]
            [Range(-180f, 180f)]
            public float approachAngle = 0f;

            [Tooltip("Distance to start approach guidance")]
            [Range(20f, 200f)]
            public float approachDistance = 80f;

            [Tooltip("Waypoints for guided approach (optional)")]
            public Transform[] approachWaypoints;
        }

        [System.Serializable]
        public class UndockingSettings
        {
            [Header("Exit Behavior")]
            [Tooltip("Direction to move when undocking")]
            public CardinalDirection exitDirection = CardinalDirection.South;

            [Tooltip("Distance to travel during undocking")]
            [Range(10f, 100f)]
            public float exitDistance = 40f;

            [Tooltip("Speed during undocking")]
            [Range(1f, 15f)]
            public float exitSpeed = 5f;

            [Header("Clearance")]
            [Tooltip("Distance ship must travel before regaining full control")]
            [Range(20f, 150f)]
            public float clearanceDistance = 60f;

            [Tooltip("Time ship must wait before undocking again")]
            [Range(0f, 10f)]
            public float undockingCooldown = 2f;

            [Header("Path Guidance")]
            [Tooltip("Waypoints for guided exit (optional)")]
            public Transform[] exitWaypoints;

            [Tooltip("Return to autopilot after undocking")]
            public bool returnToAutopilot = false;
        }

        [System.Serializable]
        public class DockingFeedback
        {
            [Header("Visual Feedback")]
            [Tooltip("Color for available docking zone")]
            public Color availableColor = Color.green;

            [Tooltip("Color for occupied docking zone")]
            public Color occupiedColor = Color.red;

            [Tooltip("Color for docking zone during approach")]
            public Color approachColor = Color.yellow;

            [Tooltip("Show docking guidance lines")]
            public bool showGuidanceLines = true;

            [Tooltip("Show approach waypoints")]
            public bool showWaypoints = true;

            [Header("Audio Feedback")]
            [Tooltip("Sound when starting to dock")]
            public AudioClip dockingStartSound;

            [Tooltip("Sound when docking is complete")]
            public AudioClip dockingCompleteSound;

            [Tooltip("Sound when starting to undock")]
            public AudioClip undockingStartSound;

            [Tooltip("Sound when undocking is complete")]
            public AudioClip undockingCompleteSound;

            [Tooltip("Sound for invalid docking attempt")]
            public AudioClip invalidDockingSound;

            [Header("UI Feedback")]
            [Tooltip("Show docking progress UI")]
            public bool showDockingProgress = true;

            [Tooltip("Show docking instructions")]
            public bool showDockingInstructions = true;

            [Tooltip("Show speed/angle guidance")]
            public bool showGuidanceInfo = true;
        }

        [Header("General Docking Settings")]
        [Tooltip("Animation configuration for docking")]
        public DockingAnimation animationSettings = new DockingAnimation();

        [Tooltip("Default zone settings for new docking zones")]
        public DockingZoneSettings defaultZoneSettings = new DockingZoneSettings();

        [Tooltip("Undocking behavior configuration")]
        public UndockingSettings undockingSettings = new UndockingSettings();

        [Tooltip("Visual and audio feedback settings")]
        public DockingFeedback feedbackSettings = new DockingFeedback();

        [Header("Input Configuration")]
        [Tooltip("Key to trigger undocking")]
        public KeyCode undockingKey = KeyCode.Space;

        [Tooltip("Require holding key for undocking")]
        public bool requireHoldToUndock = false;

        [Tooltip("Time to hold key for undocking")]
        [Range(0.1f, 3f)]
        public float undockingHoldTime = 1f;

        [Header("Performance Settings")]
        [Tooltip("Update frequency for docking calculations")]
        [Range(0.02f, 0.5f)]
        public float updateInterval = 0.1f;

        [Tooltip("Maximum simultaneous docking animations")]
        [Range(1, 10)]
        public int maxSimultaneousDockings = 3;

        [Tooltip("Use simplified calculations for distant zones")]
        public bool enableLODOptimization = true;

        [Header("Integration Settings")]
        [Tooltip("Automatically save ship state when docked")]
        public bool autoSaveOnDocking = true;

        [Tooltip("Pause ship systems when docked")]
        public bool pauseSystemsWhenDocked = false;

        [Tooltip("Restore fuel/health when docked")]
        public bool enableDockingBenefits = true;

        /// <summary>
        /// Get the world direction vector for a cardinal direction
        /// </summary>
        public Vector3 GetDirectionVector(CardinalDirection direction)
        {
            return direction switch
            {
                CardinalDirection.North => Vector3.up,
                CardinalDirection.South => Vector3.down,
                CardinalDirection.East => Vector3.right,
                CardinalDirection.West => Vector3.left,
                CardinalDirection.Northeast => (Vector3.up + Vector3.right).normalized,
                CardinalDirection.Northwest => (Vector3.up + Vector3.left).normalized,
                CardinalDirection.Southeast => (Vector3.down + Vector3.right).normalized,
                CardinalDirection.Southwest => (Vector3.down + Vector3.left).normalized,
                _ => Vector3.up
            };
        }

        /// <summary>
        /// Get the rotation for a cardinal direction
        /// </summary>
        public float GetDirectionAngle(CardinalDirection direction)
        {
            return direction switch
            {
                CardinalDirection.North => 0f,
                CardinalDirection.Northeast => 45f,
                CardinalDirection.East => 90f,
                CardinalDirection.Southeast => 135f,
                CardinalDirection.South => 180f,
                CardinalDirection.Southwest => 225f,
                CardinalDirection.West => 270f,
                CardinalDirection.Northwest => 315f,
                _ => 0f
            };
        }

        /// <summary>
        /// Check if a ship size is compatible with zone settings
        /// </summary>
        public bool IsShipSizeCompatible(float shipSize, DockingZoneSettings zoneSettings)
        {
            return shipSize >= zoneSettings.minimumShipSize && shipSize <= zoneSettings.maximumShipSize;
        }

        /// <summary>
        /// Check if ship speed is valid for docking
        /// </summary>
        public bool IsSpeedValidForDocking(float speed, DockingZoneSettings zoneSettings)
        {
            return speed >= zoneSettings.minimumDockingSpeed && speed <= zoneSettings.maximumDockingSpeed;
        }

        /// <summary>
        /// Get docking duration based on ship size and zone type
        /// </summary>
        public float GetDockingDuration(float shipSize, DockingType dockingType)
        {
            float baseDuration = animationSettings.dockingDuration;

            // Larger ships take longer to dock
            float sizeMultiplier = 1f + (shipSize / 100f) * 0.5f;

            // Different docking types have different base times
            float typeMultiplier = dockingType switch
            {
                DockingType.Pier => 1f,
                DockingType.Berth => 1.2f,
                DockingType.Anchor => 0.8f,
                DockingType.Mooring => 1.1f,
                _ => 1f
            };

            return baseDuration * sizeMultiplier * typeMultiplier;
        }

        /// <summary>
        /// Validate configuration and log warnings
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            // Validate animation settings
            if (animationSettings.dockingDuration <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Docking duration must be greater than 0!", this);
                isValid = false;
            }

            if (animationSettings.undockingDuration <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Undocking duration must be greater than 0!", this);
                isValid = false;
            }

            // Validate zone settings
            if (defaultZoneSettings.detectionRadius <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Detection radius must be greater than 0!", this);
                isValid = false;
            }

            if (defaultZoneSettings.minimumShipSize >= defaultZoneSettings.maximumShipSize)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Minimum ship size must be less than maximum ship size!", this);
                isValid = false;
            }

            if (defaultZoneSettings.minimumDockingSpeed >= defaultZoneSettings.maximumDockingSpeed)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Minimum docking speed must be less than maximum docking speed!", this);
                isValid = false;
            }

            // Validate undocking settings
            if (undockingSettings.exitDistance <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Exit distance must be greater than 0!", this);
                isValid = false;
            }

            if (undockingSettings.exitSpeed <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Exit speed must be greater than 0!", this);
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Create default zone settings for a specific docking type
        /// </summary>
        public DockingZoneSettings CreateZoneSettingsForType(DockingType type)
        {
            var settings = new DockingZoneSettings
            {
                dockingType = type,
                facingDirection = defaultZoneSettings.facingDirection,
                dockingOffset = defaultZoneSettings.dockingOffset,
                detectionRadius = defaultZoneSettings.detectionRadius,
                minimumDockingSpeed = defaultZoneSettings.minimumDockingSpeed,
                maximumDockingSpeed = defaultZoneSettings.maximumDockingSpeed,
                approachAngle = defaultZoneSettings.approachAngle,
                approachDistance = defaultZoneSettings.approachDistance
            };

            // Customize based on docking type
            switch (type)
            {
                case DockingType.Pier:
                    settings.minimumShipSize = 5f;
                    settings.maximumShipSize = 80f;
                    break;
                case DockingType.Berth:
                    settings.minimumShipSize = 10f;
                    settings.maximumShipSize = 120f;
                    settings.detectionRadius *= 1.2f;
                    break;
                case DockingType.Anchor:
                    settings.minimumShipSize = 0f;
                    settings.maximumShipSize = 200f;
                    settings.detectionRadius *= 1.5f;
                    break;
                case DockingType.Mooring:
                    settings.minimumShipSize = 3f;
                    settings.maximumShipSize = 40f;
                    settings.detectionRadius *= 0.8f;
                    break;
            }

            return settings;
        }

        private void OnValidate()
        {
            // Ensure animation durations are positive
            if (animationSettings != null)
            {
                animationSettings.dockingDuration = Mathf.Max(0.1f, animationSettings.dockingDuration);
                animationSettings.undockingDuration = Mathf.Max(0.1f, animationSettings.undockingDuration);
                animationSettings.undockingDelay = Mathf.Max(0f, animationSettings.undockingDelay);
                animationSettings.physicsTransitionTime = Mathf.Max(0.1f, animationSettings.physicsTransitionTime);
            }

            // Ensure zone settings are valid
            if (defaultZoneSettings != null)
            {
                defaultZoneSettings.detectionRadius = Mathf.Max(5f, defaultZoneSettings.detectionRadius);
                defaultZoneSettings.minimumDockingSpeed = Mathf.Max(0f, defaultZoneSettings.minimumDockingSpeed);
                defaultZoneSettings.maximumDockingSpeed = Mathf.Max(defaultZoneSettings.minimumDockingSpeed + 0.1f, defaultZoneSettings.maximumDockingSpeed);
                defaultZoneSettings.minimumShipSize = Mathf.Max(0f, defaultZoneSettings.minimumShipSize);
                defaultZoneSettings.maximumShipSize = Mathf.Max(defaultZoneSettings.minimumShipSize + 1f, defaultZoneSettings.maximumShipSize);
                defaultZoneSettings.approachDistance = Mathf.Max(10f, defaultZoneSettings.approachDistance);
            }

            // Ensure undocking settings are valid
            if (undockingSettings != null)
            {
                undockingSettings.exitDistance = Mathf.Max(5f, undockingSettings.exitDistance);
                undockingSettings.exitSpeed = Mathf.Max(0.1f, undockingSettings.exitSpeed);
                undockingSettings.clearanceDistance = Mathf.Max(10f, undockingSettings.clearanceDistance);
                undockingSettings.undockingCooldown = Mathf.Max(0f, undockingSettings.undockingCooldown);
            }

            // Ensure performance settings are reasonable
            updateInterval = Mathf.Clamp(updateInterval, 0.02f, 2f);
            maxSimultaneousDockings = Mathf.Max(1, maxSimultaneousDockings);
            undockingHoldTime = Mathf.Max(0.1f, undockingHoldTime);
        }
    }
}