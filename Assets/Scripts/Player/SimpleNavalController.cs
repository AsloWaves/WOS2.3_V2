using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using WOS.ScriptableObjects;
using WOS.Debugging;

namespace WOS.Player
{
    /// <summary>
    /// Enhanced naval physics controller with authentic ship handling characteristics.
    /// Integrates Unity Input System with Job System optimization for performance.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class SimpleNavalController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ShipConfigurationSO shipConfig;
        [SerializeField] private bool enableDebugVisualization = true;

        [Header("Performance Tuning")]
        [Tooltip("Global speed multiplier - scales all ship speeds without changing configuration stats")]
        [Range(0.1f, 2.0f)]
        [SerializeField] private float globalSpeedMultiplier = 1.0f;

        [Header("Navigation")]
        [SerializeField] private Transform waypointContainer;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private LineRenderer courseLineRenderer;

        // Input System Integration
        private PlayerInput playerInput;
        private InputAction steeringAction;
        private InputAction throttleUpAction;
        private InputAction throttleDownAction;
        private InputAction emergencyStopAction;
        private InputAction setWaypointAction;
        private InputAction autoNavigateAction;
        private InputAction clearWaypointsAction;

        // Core Components
        private Rigidbody2D shipRigidbody;
        private UnityEngine.Camera mainCamera;

        // Navigation System
        private List<float3> waypoints;
        private int currentWaypointIndex;
        private bool autoNavigationEnabled;
        private float3 targetDirection;
        private float autoSteeringInput; // Auto-navigation steering input

        // Ship State
        private float currentThrottle; // -4 to 4 (8-speed system)
        private float rudderAngle;           // Actual rudder position (-35 to +35 degrees)
        private float effectiveRudderAngle;  // Speed-adjusted rudder effectiveness
        private float currentSpeed; // In knots
        private float3 velocity;
        private bool emergencyStopActive;

        // Physics State
        private float inertia;
        private float3 centerOfMass;

        // Events
        public static System.Action<float> OnSpeedChanged;
        public static System.Action<float> OnThrottleChanged;
        public static System.Action<Vector3> OnWaypointAdded;
        public static System.Action OnWaypointsCleared;
        public static System.Action<bool> OnAutoNavigationToggled;

        private void Awake()
        {
            InitializeComponents();
            InitializeInputSystem();
            InitializeNavigation();
        }

        private void Start()
        {
            InitializePhysics();
            ValidateConfiguration();
        }

        private void InitializeComponents()
        {
            shipRigidbody = GetComponent<Rigidbody2D>();
            playerInput = GetComponent<PlayerInput>();
            mainCamera = UnityEngine.Camera.main;

            // Configure Rigidbody2D for naval physics
            shipRigidbody.gravityScale = 0f;
            shipRigidbody.linearDamping = 0.5f; // Natural water resistance
            shipRigidbody.angularDamping = 2f; // Rotational resistance

            // CRITICAL: Enable interpolation for smooth movement between FixedUpdate calls
            // This prevents vibration when physics (50Hz) and rendering (60Hz) are out of sync
            shipRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void InitializeInputSystem()
        {
            var actionMap = playerInput.actions.FindActionMap("Naval");

            steeringAction = actionMap.FindAction("Steering");
            throttleUpAction = actionMap.FindAction("ThrottleUp");
            throttleDownAction = actionMap.FindAction("ThrottleDown");
            emergencyStopAction = actionMap.FindAction("EmergencyStop");
            setWaypointAction = actionMap.FindAction("SetWaypoint");
            autoNavigateAction = actionMap.FindAction("AutoNavigate");
            clearWaypointsAction = actionMap.FindAction("ClearWaypoints");

            // Subscribe to input events
            throttleUpAction.performed += OnThrottleUp;
            throttleDownAction.performed += OnThrottleDown;
            emergencyStopAction.performed += OnEmergencyStop;
            setWaypointAction.performed += OnSetWaypoint;
            autoNavigateAction.performed += OnToggleAutoNavigation;
            clearWaypointsAction.performed += OnClearWaypoints;
        }

        private void InitializeNavigation()
        {
            waypoints = new List<float3>();
            currentWaypointIndex = 0;
            autoNavigationEnabled = false;

            // Initialize course line renderer
            if (courseLineRenderer != null)
            {
                courseLineRenderer.positionCount = 0;
                courseLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                courseLineRenderer.startColor = Color.cyan;
                courseLineRenderer.endColor = Color.cyan;
                courseLineRenderer.startWidth = 0.1f;
                courseLineRenderer.endWidth = 0.1f;
            }
        }

        private void InitializePhysics()
        {
            if (shipConfig == null)
            {
                DebugManager.LogError(DebugCategory.Ship, "Ship configuration not assigned!", this);
                return;
            }

            // Calculate ship inertia based on displacement
            inertia = shipConfig.displacement * 0.1f; // Simplified calculation
            centerOfMass = new float3(0f, 0f, 0f);

            // Set initial physics properties
            shipRigidbody.mass = shipConfig.displacement * 0.001f; // Convert tons to Unity mass scale
        }

        private void ValidateConfiguration()
        {
            if (shipConfig == null)
            {
                DebugManager.LogError(DebugCategory.Ship, $"[{gameObject.name}] ShipConfigurationSO is required!", this);
                enabled = false;
                return;
            }

            if (playerInput == null)
            {
                DebugManager.LogError(DebugCategory.Input, $"[{gameObject.name}] PlayerInput component is required!", this);
                enabled = false;
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
                if (mainCamera == null)
                {
                    DebugManager.LogWarning(DebugCategory.Camera, $"[{gameObject.name}] No main camera found for input processing", this);
                }
            }
        }

        private void Update()
        {
            HandleSteering();
            UpdateNavigation();
            UpdatePhysics();
            UpdateDebugVisualization();

            // DEBUGGING: Force throttle to 0 for testing (remove after debugging)
            if (Input.GetKeyDown(KeyCode.T))
            {
                DebugManager.Log(DebugCategory.Ship, $"[DEBUG] FORCING throttle to 0. Was: {currentThrottle}", this);
                currentThrottle = 0f;
                emergencyStopActive = false;
                autoNavigationEnabled = false;
            }
        }

        private void FixedUpdate()
        {
            ApplyNavalPhysics();
        }

        private void HandleSteering()
        {
            if (emergencyStopActive) return;

            // Get steering input
            Vector2 steeringInput = steeringAction.ReadValue<Vector2>();
            float targetRudderAngle = steeringInput.x * shipConfig.maxRudderAngle;

            // Apply rudder response rate - rudder can move even when stationary
            float rudderChangeRate = shipConfig.rudderRate * Time.deltaTime;
            rudderAngle = Mathf.MoveTowards(rudderAngle, targetRudderAngle, rudderChangeRate);

            // Calculate effective rudder angle based on water flow over rudder
            // Water flow = ship speed + prop wash (minimum flow even when stationary)
            float propWashSpeed = 0.5f; // Minimum water flow from propeller
            float waterFlowSpeed = Mathf.Max(Mathf.Abs(currentSpeed), propWashSpeed);

            // Steerageway effect - but never completely zero
            float steerageEffect = Mathf.InverseLerp(0f, shipConfig.steerageway, waterFlowSpeed);
            steerageEffect = Mathf.Max(steerageEffect, 0.15f); // Minimum 15% effectiveness from prop wash

            // Store effective rudder angle separately (don't modify actual rudder position)
            effectiveRudderAngle = rudderAngle * steerageEffect;
        }

        private void UpdateNavigation()
        {
            if (!autoNavigationEnabled || waypoints == null || waypoints.Count == 0) return;

            // Check if we've reached the current waypoint
            if (currentWaypointIndex < waypoints.Count)
            {
                float3 currentPos = transform.position;
                float3 targetWaypoint = waypoints[currentWaypointIndex];
                Vector3 currentPosVec = new Vector3(currentPos.x, currentPos.y, currentPos.z);
                Vector3 targetWaypointVec = new Vector3(targetWaypoint.x, targetWaypoint.y, targetWaypoint.z);
                float distanceToWaypoint = Vector3.Distance(currentPosVec, targetWaypointVec);

                if (distanceToWaypoint < 5f) // Waypoint reached threshold
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Count)
                    {
                        // All waypoints reached
                        autoNavigationEnabled = false;
                        OnAutoNavigationToggled?.Invoke(false);
                        return;
                    }
                }

                // Calculate navigation direction
                Vector3 directionVec = targetWaypointVec - currentPosVec;
                Vector3 normalizedDirection = directionVec.normalized;
                targetDirection = new float3(normalizedDirection.x, normalizedDirection.y, normalizedDirection.z);

                // Direct navigation calculation (no Job System needed for simple math)
                CalculateNavigationDirect(currentPos, targetDirection);
            }
        }

        private void CalculateNavigationDirect(float3 currentPos, float3 targetDir)
        {
            // Calculate optimal heading to target
            float currentHeading = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            float targetHeading = Mathf.Atan2(targetDir.x, targetDir.z);
            float headingDifference = targetHeading - currentHeading;

            // Normalize heading difference to [-π, π]
            while (headingDifference > Mathf.PI) headingDifference -= 2f * Mathf.PI;
            while (headingDifference < -Mathf.PI) headingDifference += 2f * Mathf.PI;

            // Calculate required steering input
            float steeringInput = headingDifference * shipConfig.helmResponse;
            steeringInput = Mathf.Clamp(steeringInput, -1f, 1f);

            // Apply auto-navigation steering (this would be used by the steering system)
            autoSteeringInput = steeringInput;
        }

        private void UpdatePhysics()
        {
            // Update velocity for display/debug purposes only
            velocity = new float3(shipRigidbody.linearVelocity.x, 0f, shipRigidbody.linearVelocity.y);
            // NOTE: currentSpeed is controlled by ApplyNavalPhysics(), not derived from velocity

            // Update center of mass (simplified - would integrate with cargo system)
            // centerOfMass calculation would come from CargoSystem

            OnSpeedChanged?.Invoke(currentSpeed);
        }

        private void ApplyNavalPhysics()
        {
            if (shipConfig == null) return;

            // Convert throttle to target speed
            float targetSpeed = ConvertThrottleToSpeed(currentThrottle);

            // Simple speed transition logic from working version
            float speedDifference = targetSpeed - currentSpeed;
            float acceleration = speedDifference > 0 ? shipConfig.acceleration : shipConfig.deceleration;

            // Apply naval physics smoothly

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

            // Apply forward movement based on ship's facing direction (ship sprites face right)
            Vector2 forwardDirection = transform.right;
            // Apply globalSpeedMultiplier to Unity physics movement only (not naval calculations)
            Vector2 targetVelocity = forwardDirection * currentSpeed * globalSpeedMultiplier;

            // Apply naval physics with scalable momentum system
            float responseRate = CalculateShipResponsiveness(targetVelocity);
            shipRigidbody.linearVelocity = Vector2.Lerp(shipRigidbody.linearVelocity, targetVelocity, responseRate);

            // Debug responsiveness system
            if (enableDebugVisualization && Time.fixedTime % 1f < Time.fixedDeltaTime)
            {
                float velocityDelta = (targetVelocity - shipRigidbody.linearVelocity).magnitude;
                DebugManager.Log(DebugCategory.Physics, $"[MOMENTUM] ResponseRate: {responseRate:F3} | VelocityDelta: {velocityDelta:F2} | " +
                         $"Displacement: {shipConfig.displacement}t | Length: {shipConfig.length}m", this);
            }

            // Apply rudder turning physics
            ApplyRudderTurning();

            // Optional debug visualization for ship movement
            if (enableDebugVisualization)
            {
                string waypointInfo = autoNavigationEnabled ? $"AUTO_NAV ({waypoints.Count} waypoints)" : "MANUAL";
                string speedInfo = globalSpeedMultiplier != 1.0f ? $" | Scale: {globalSpeedMultiplier:F1}x" : "";
                DebugManager.Log(DebugCategory.Ship, $"[SHIP] Speed: {currentSpeed:F1}kts | Target: {targetSpeed:F1}kts | Mode: {waypointInfo}{speedInfo}", this);
            }
        }

        private float ConvertThrottleToSpeed(float throttle)
        {
            // Convert throttle setting to target speed (keep original for naval physics calculations)
            int throttleInt = Mathf.RoundToInt(throttle);

            return throttleInt switch
            {
                -4 => -shipConfig.maxSpeed,           // Full Astern - 100% reverse
                -3 => -shipConfig.maxSpeed * 0.66f,   // Half Astern - 66% reverse
                -2 => -shipConfig.maxSpeed * 0.33f,   // Slow Astern - 33% reverse
                -1 => -shipConfig.maxSpeed * 0.15f,   // Dead Slow Astern - 15% reverse
                0 => 0f,                              // Full Stop - 0%
                1 => shipConfig.maxSpeed * 0.25f,     // Slow Ahead - 25%
                2 => shipConfig.maxSpeed * 0.50f,     // Half Ahead - 50%
                3 => shipConfig.maxSpeed * 0.75f,     // Full Ahead - 75%
                4 => shipConfig.maxSpeed,             // Flank Ahead - 100%
                _ => 0f
            };
        }

        private float CalculateShipResponsiveness(Vector2 targetVelocity)
        {
            if (shipConfig == null) return Time.fixedDeltaTime * 3f;

            // === SCALABLE MOMENTUM SYSTEM ===
            // Factors that affect ship responsiveness:

            // 1. Base ship characteristics (displacement = mass/inertia)
            float normalizedMass = Mathf.Clamp01(shipConfig.displacement / 50000f);
            float massInertia = Mathf.Lerp(0.1f, 0.6f, normalizedMass); // Heavy ships = more inertia

            // 2. Speed factor - faster changes require more time
            float currentVelocityMag = shipRigidbody.linearVelocity.magnitude;
            float targetVelocityMag = targetVelocity.magnitude;
            float velocityDelta = Mathf.Abs(targetVelocityMag - currentVelocityMag);
            float speedChangeFactor = Mathf.Clamp01(velocityDelta / 10f); // Normalize speed changes

            // 3. Direction change factor - turning while moving is harder
            float directionChange = 0f;
            if (currentVelocityMag > 0.1f && targetVelocityMag > 0.1f)
            {
                Vector2 currentDir = shipRigidbody.linearVelocity.normalized;
                Vector2 targetDir = targetVelocity.normalized;
                directionChange = (1f - Vector2.Dot(currentDir, targetDir)) * 0.5f; // 0-0.5 range
            }

            // 4. Ship design characteristics
            float lengthFactor = Mathf.Clamp01(shipConfig.length / 200f); // Longer ships = less responsive
            float designResponsiveness = 1f - (lengthFactor * 0.3f); // Reduce by up to 30%

            // 5. Water resistance (drag coefficient)
            float dragResponse = Mathf.Lerp(1.2f, 0.8f, shipConfig.dragCoefficient / 2f);

            // === COMBINE ALL FACTORS ===
            float totalInertia = massInertia + (speedChangeFactor * 0.4f) + (directionChange * 0.3f);
            totalInertia *= (2f - designResponsiveness); // Apply design factor
            totalInertia /= dragResponse; // Higher drag = more responsive stopping

            // Convert inertia to response rate (higher inertia = lower response rate)
            float baseResponseRate = (1f - Mathf.Clamp01(totalInertia)) * Time.fixedDeltaTime;

            // Scale response rate based on frame time and ship characteristics
            float scaledResponseRate = baseResponseRate * 8f; // Increase base responsiveness

            // Ensure minimum responsiveness for gameplay
            float minResponse = Time.fixedDeltaTime * 2f;
            float maxResponse = 1f; // Never instant

            return Mathf.Clamp(scaledResponseRate, minResponse, maxResponse);
        }

        private float CalculateTurningEffectiveness(float speed)
        {
            // Enhanced turning effectiveness curve:
            // 0 knots = minimal turning from prop wash (0.1)
            // 1 knot = 1/4 turn rate (0.25)
            // 4-7 knots = 1/2 turn rate (0.5)
            // Higher speeds = full turn rate (1.0)

            // Always have some turning ability from prop wash
            if (speed <= 0f) return 0.1f; // Minimal turning from prop wash when stationary

            if (speed <= 1f)
            {
                // Linear ramp from 0.1 to 0.25 over 0-1 knots
                return Mathf.Lerp(0.1f, 0.25f, speed);
            }
            else if (speed <= 4f)
            {
                // Ramp from 0.25 to 0.5 over 1-4 knots
                float t = (speed - 1f) / 3f; // Normalize 1-4 to 0-1
                return Mathf.Lerp(0.25f, 0.5f, t);
            }
            else if (speed <= 7f)
            {
                // Maintain 0.5 effectiveness from 4-7 knots
                return 0.5f;
            }
            else
            {
                // Ramp from 0.5 to 1.0 over 7+ knots
                float t = Mathf.Clamp01((speed - 7f) / 7f); // Normalize 7-14 to 0-1
                return Mathf.Lerp(0.5f, 1.0f, t);
            }
        }

        private void ApplyRudderTurning()
        {
            if (shipConfig == null || Mathf.Abs(effectiveRudderAngle) < 0.01f) return;

            // Custom turning effectiveness curve based on speed
            float absSpeed = Mathf.Abs(currentSpeed);
            float steerageEffect = CalculateTurningEffectiveness(absSpeed);

            // Calculate turning rate from effective rudder angle and ship configuration
            float maxTurnRate = shipConfig.rudderRate; // degrees per second at full rudder
            float currentTurnRate = (effectiveRudderAngle / shipConfig.maxRudderAngle) * maxTurnRate * steerageEffect;

            // Apply turning with speed-dependent effectiveness (invert for correct direction)
            float turnAmount = -currentTurnRate * Time.fixedDeltaTime;
            transform.Rotate(0, 0, turnAmount);

            // Debug rudder turning
            if (enableDebugVisualization && Mathf.Abs(rudderAngle) > 0.1f)
            {
                DebugManager.Log(DebugCategory.Physics, $"[RUDDER DEBUG] RudderAngle: {rudderAngle:F1}°, TurnRate: {currentTurnRate:F1}°/s, " +
                         $"Speed: {currentSpeed:F1}, SteerageEffect: {steerageEffect:F2}", this);
            }
        }


        private void UpdateDebugVisualization()
        {
            if (!enableDebugVisualization) return;

            // Update course line
            UpdateCourseLineVisualization();

            // Draw debug information
            DrawDebugInfo();
        }

        private void UpdateCourseLineVisualization()
        {
            if (courseLineRenderer == null || waypoints.Count == 0) return;

            // Update line renderer with current course
            int positionCount = waypoints.Count + 1;
            courseLineRenderer.positionCount = positionCount;

            courseLineRenderer.SetPosition(0, transform.position);
            for (int i = 0; i < waypoints.Count; i++)
            {
                courseLineRenderer.SetPosition(i + 1, new Vector3(waypoints[i].x, waypoints[i].y, waypoints[i].z));
            }
        }

        private void DrawDebugInfo()
        {
            // Draw heading indicator (ship faces right)
            Debug.DrawRay(transform.position, transform.right * 5f, Color.green);

            // Draw velocity vector
            Vector3 velocityVector = new Vector3(velocity.x, velocity.y, velocity.z);
            Debug.DrawRay(transform.position, velocityVector, Color.blue);

            // Draw rudder angle (ship faces right)
            Vector3 rudderDirection = Quaternion.Euler(0, 0, rudderAngle) * transform.right;
            Debug.DrawRay(transform.position, rudderDirection * 3f, Color.red);
        }

        #region Input Event Handlers

        private void OnThrottleUp(InputAction.CallbackContext context)
        {
            if (emergencyStopActive) return;

            currentThrottle = Mathf.Clamp(currentThrottle + 1f, -4f, 4f);
            OnThrottleChanged?.Invoke(currentThrottle);
        }

        private void OnThrottleDown(InputAction.CallbackContext context)
        {
            if (emergencyStopActive) return;

            currentThrottle = Mathf.Clamp(currentThrottle - 1f, -4f, 4f);
            OnThrottleChanged?.Invoke(currentThrottle);
        }

        private void OnEmergencyStop(InputAction.CallbackContext context)
        {
            emergencyStopActive = true;
            currentThrottle = 0f;
            OnThrottleChanged?.Invoke(currentThrottle);
        }

        private void OnSetWaypoint(InputAction.CallbackContext context)
        {
            if (mainCamera == null || Mouse.current == null) return;

            // Get mouse position and validate it's within screen bounds
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            // Check if mouse position is valid (not 0,0 and within screen bounds)
            if (mouseScreenPos.x <= 0 || mouseScreenPos.y <= 0 ||
                mouseScreenPos.x >= Screen.width || mouseScreenPos.y >= Screen.height)
            {
                if (enableDebugVisualization)
                    DebugManager.LogWarning(DebugCategory.Input, $"Invalid mouse position: {mouseScreenPos}. Screen size: {Screen.width}x{Screen.height}", this);
                return;
            }

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
            mouseWorldPos.z = 0f;

            waypoints.Add(new float3(mouseWorldPos.x, mouseWorldPos.y, mouseWorldPos.z));
            OnWaypointAdded?.Invoke(mouseWorldPos);

            // Create waypoint visualization
            if (waypointPrefab != null && waypointContainer != null)
            {
                GameObject waypointObj = Instantiate(waypointPrefab, mouseWorldPos, Quaternion.identity, waypointContainer);
                waypointObj.name = $"Waypoint_{waypoints.Count}";
            }

            if (enableDebugVisualization)
                DebugManager.Log(DebugCategory.Input, $"Waypoint set at: {mouseWorldPos} (Screen: {mouseScreenPos})", this);
        }

        private void OnToggleAutoNavigation(InputAction.CallbackContext context)
        {
            if (waypoints.Count == 0) return;

            autoNavigationEnabled = !autoNavigationEnabled;
            if (autoNavigationEnabled)
            {
                currentWaypointIndex = 0;
            }

            OnAutoNavigationToggled?.Invoke(autoNavigationEnabled);
        }

        private void OnClearWaypoints(InputAction.CallbackContext context)
        {
            waypoints.Clear();
            currentWaypointIndex = 0;
            autoNavigationEnabled = false;

            // Clear waypoint visualizations
            if (waypointContainer != null)
            {
                for (int i = waypointContainer.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(waypointContainer.GetChild(i).gameObject);
                }
            }

            OnWaypointsCleared?.Invoke();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get current ship status for UI display
        /// </summary>
        public ShipStatus GetShipStatus()
        {
            return new ShipStatus
            {
                speed = currentSpeed,
                throttle = currentThrottle,
                heading = transform.rotation.eulerAngles.z,
                rudderAngle = rudderAngle,
                isAutoNavigating = autoNavigationEnabled,
                waypointCount = waypoints.Count,
                currentWaypoint = currentWaypointIndex
            };
        }

        /// <summary>
        /// Set throttle directly (for AI or external control)
        /// </summary>
        public void SetThrottle(float throttleValue)
        {
            currentThrottle = Mathf.Clamp(throttleValue, -4f, 4f);
            OnThrottleChanged?.Invoke(currentThrottle);
        }

        /// <summary>
        /// Add waypoint programmatically
        /// </summary>
        public void AddWaypoint(Vector3 position)
        {
            waypoints.Add(new float3(position.x, position.y, position.z));
            OnWaypointAdded?.Invoke(position);
        }

        /// <summary>
        /// Get the ship's current velocity for camera look-ahead
        /// </summary>
        public Vector3 GetVelocity()
        {
            if (shipRigidbody != null)
                return shipRigidbody.linearVelocity;
            return Vector3.zero;
        }

        /// <summary>
        /// Get the ship's current speed in meters per second
        /// </summary>
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }

        /// <summary>
        /// Get the ship's current throttle setting
        /// </summary>
        public float GetThrottle()
        {
            return currentThrottle;
        }

        /// <summary>
        /// Get the current ship configuration
        /// </summary>
        public WOS.ScriptableObjects.ShipConfigurationSO GetShipConfiguration()
        {
            return shipConfig;
        }

        #endregion

        private void OnDestroy()
        {
            CleanupResources();
        }

        private void OnDisable()
        {
            CleanupResources();
        }

        private void CleanupResources()
        {
            // Clear waypoints list
            if (waypoints != null)
            {
                waypoints.Clear();
            }

            // Unsubscribe from input events
            if (throttleUpAction != null) throttleUpAction.performed -= OnThrottleUp;
            if (throttleDownAction != null) throttleDownAction.performed -= OnThrottleDown;
            if (emergencyStopAction != null) emergencyStopAction.performed -= OnEmergencyStop;
            if (setWaypointAction != null) setWaypointAction.performed -= OnSetWaypoint;
            if (autoNavigateAction != null) autoNavigateAction.performed -= OnToggleAutoNavigation;
            if (clearWaypointsAction != null) clearWaypointsAction.performed -= OnClearWaypoints;
        }

        private void OnDrawGizmosSelected()
        {
            if (shipConfig == null) return;

            // Draw ship dimensions
            Gizmos.color = Color.yellow;
            Vector3 shipSize = new Vector3(shipConfig.beam, shipConfig.length, 1f);
            Gizmos.DrawWireCube(transform.position, shipSize);

            // Draw turning circle
            Gizmos.color = Color.cyan;
            float turningRadius = shipConfig.GetTurningRadius();
            DrawWireCircle(transform.position, turningRadius);

            // Draw center of mass
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + new Vector3(centerOfMass.x, centerOfMass.y, centerOfMass.z), 0.5f);
        }

        /// <summary>
        /// Helper method to draw wire circle using Gizmos
        /// </summary>
        private static void DrawWireCircle(Vector3 center, float radius, int segments = 32)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }


    /// <summary>
    /// Ship status data structure for UI and external systems
    /// </summary>
    [System.Serializable]
    public struct ShipStatus
    {
        public float speed;
        public float throttle;
        public float heading;
        public float rudderAngle;
        public bool isAutoNavigating;
        public int waypointCount;
        public int currentWaypoint;
    }
}