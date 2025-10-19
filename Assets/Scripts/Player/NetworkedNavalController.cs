using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Mirror;
using WOS.ScriptableObjects;
using WOS.Debugging;

namespace WOS.Player
{
    /// <summary>
    /// Networked naval physics controller with Mirror integration
    /// Uses client-side prediction for smooth ship movement
    /// Server validates and syncs position/rotation via NetworkTransform
    /// NOTE: Manually add NetworkTransform component in Unity Inspector
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkedNavalController : NetworkBehaviour
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

        // Input System Integration (only active for local player)
        private InputActionAsset inputActions;
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
        private float autoSteeringInput;

        // Ship State - [SyncVar] for network synchronization
        [SyncVar(hook = nameof(OnThrottleChanged))]
        private float currentThrottle; // -4 to 4 (8-speed system)

        [SyncVar]
        private float rudderAngle; // Actual rudder position (-35 to +35 degrees)

        [SyncVar]
        private float effectiveRudderAngle; // Speed-adjusted rudder effectiveness

        [SyncVar(hook = nameof(OnSpeedChanged))]
        private float currentSpeed; // In knots

        private float3 velocity;

        [SyncVar]
        private bool emergencyStopActive;

        // Physics State
        private float inertia;
        private float3 centerOfMass;

        // Events (only invoked on local player)
        public static System.Action<float> OnSpeedChangedEvent;
        public static System.Action<float> OnThrottleChangedEvent;
        public static System.Action<Vector3> OnWaypointAdded;
        public static System.Action OnWaypointsCleared;
        public static System.Action<bool> OnAutoNavigationToggled;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            InitializeNavigation();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // Only initialize input for LOCAL player
            InitializeInputSystem();

            DebugManager.Log(DebugCategory.Ship, $"🚢 Local player ship initialized: {gameObject.name}", this);
        }

        private void Start()
        {
            InitializePhysics();
            ValidateConfiguration();

            // Set layer for proper collision/rendering
            if (isLocalPlayer)
            {
                gameObject.layer = LayerMask.NameToLayer("Player");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
            }
        }

        private void Update()
        {
            // Only handle input and navigation for local player
            if (isLocalPlayer)
            {
                HandleSteering();
                UpdateNavigation();
            }

            // Update visuals for all players
            UpdatePhysics();
            UpdateDebugVisualization();
        }

        private void FixedUpdate()
        {
            // Physics run on owner's client for prediction
            if (isOwned)
            {
                ApplyNavalPhysics();
            }
        }

        private void OnDestroy()
        {
            // Clean up input events
            if (isLocalPlayer && inputActions != null)
            {
                throttleUpAction.performed -= OnThrottleUp;
                throttleDownAction.performed -= OnThrottleDown;
                emergencyStopAction.performed -= OnEmergencyStop;
                setWaypointAction.performed -= OnSetWaypoint;
                autoNavigateAction.performed -= OnToggleAutoNavigation;
                clearWaypointsAction.performed -= OnClearWaypoints;

                // Disable input actions
                inputActions.Disable();
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            shipRigidbody = GetComponent<Rigidbody2D>();

            // Configure Rigidbody2D for naval physics
            shipRigidbody.gravityScale = 0f;
            shipRigidbody.linearDamping = 0f; // No damping for direct velocity control (not force-based)
            shipRigidbody.angularDamping = 2f;
            shipRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void InitializeInputSystem()
        {
            // Load the input actions asset directly (no PlayerInput component needed for networked games)
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActions == null)
            {
                DebugManager.LogError(DebugCategory.Input, "Failed to load InputSystem_Actions! Place it in Resources folder.", this);
                return;
            }

            // Find the Naval action map
            var actionMap = inputActions.FindActionMap("Naval");
            if (actionMap == null)
            {
                DebugManager.LogError(DebugCategory.Input, "Naval action map not found!", this);
                return;
            }

            // Get action references
            steeringAction = actionMap.FindAction("Steering");
            throttleUpAction = actionMap.FindAction("ThrottleUp");
            throttleDownAction = actionMap.FindAction("ThrottleDown");
            emergencyStopAction = actionMap.FindAction("EmergencyStop");
            setWaypointAction = actionMap.FindAction("SetWaypoint");
            autoNavigateAction = actionMap.FindAction("AutoNavigate");
            clearWaypointsAction = actionMap.FindAction("ClearWaypoints");

            // Subscribe to input events
            if (throttleUpAction != null) throttleUpAction.performed += OnThrottleUp;
            if (throttleDownAction != null) throttleDownAction.performed += OnThrottleDown;
            if (emergencyStopAction != null) emergencyStopAction.performed += OnEmergencyStop;
            if (setWaypointAction != null) setWaypointAction.performed += OnSetWaypoint;
            if (autoNavigateAction != null) autoNavigateAction.performed += OnToggleAutoNavigation;
            if (clearWaypointsAction != null) clearWaypointsAction.performed += OnClearWaypoints;

            // Enable the action map
            actionMap.Enable();

            DebugManager.Log(DebugCategory.Input, $"✅ Input System initialized for local player", this);
        }

        private void InitializeNavigation()
        {
            waypoints = new List<float3>();
            currentWaypointIndex = 0;
            autoNavigationEnabled = false;

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

            inertia = shipConfig.displacement * 0.1f;
            centerOfMass = new float3(0f, 0f, 0f);
            shipRigidbody.mass = shipConfig.displacement * 0.001f;
        }

        private void ValidateConfiguration()
        {
            if (shipConfig == null)
            {
                DebugManager.LogError(DebugCategory.Ship, $"[{gameObject.name}] ShipConfigurationSO is required!", this);
                enabled = false;
                return;
            }

            if (isLocalPlayer)
            {
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
                    if (mainCamera == null)
                    {
                        DebugManager.LogWarning(DebugCategory.Camera, $"[{gameObject.name}] No main camera found", this);
                    }
                }
            }
        }

        #endregion

        #region Input Handling (Local Player Only)

        private void HandleSteering()
        {
            if (!isLocalPlayer || emergencyStopActive) return;

            // Get steering input
            Vector2 steeringInput = steeringAction.ReadValue<Vector2>();
            float targetRudderAngle = steeringInput.x * shipConfig.maxRudderAngle;

            // Apply rudder response rate
            float rudderChangeRate = shipConfig.rudderRate * Time.deltaTime;
            rudderAngle = Mathf.MoveTowards(rudderAngle, targetRudderAngle, rudderChangeRate);

            // Calculate effective rudder angle
            float propWashSpeed = 0.5f;
            float waterFlowSpeed = Mathf.Max(Mathf.Abs(currentSpeed), propWashSpeed);
            float steerageEffect = Mathf.InverseLerp(0f, shipConfig.steerageway, waterFlowSpeed);
            steerageEffect = Mathf.Max(steerageEffect, 0.15f);

            effectiveRudderAngle = rudderAngle * steerageEffect;
        }

        private void OnThrottleUp(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;

            // Send command to server
            CmdAdjustThrottle(1f);
        }

        private void OnThrottleDown(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;

            // Send command to server
            CmdAdjustThrottle(-1f);
        }

        private void OnEmergencyStop(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;

            // Send command to server
            CmdEmergencyStop();
        }

        private void OnSetWaypoint(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer || mainCamera == null) return;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = 0;

            CmdAddWaypoint(worldPos);
        }

        private void OnToggleAutoNavigation(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;

            CmdToggleAutoNavigation();
        }

        private void OnClearWaypoints(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;

            CmdClearWaypoints();
        }

        #endregion

        #region Network Commands (Client → Server)

        [Command]
        private void CmdAdjustThrottle(float direction)
        {
            if (emergencyStopActive) return;

            float newThrottle = Mathf.Clamp(currentThrottle + direction, -4f, 4f);
            currentThrottle = newThrottle;

            DebugManager.Log(DebugCategory.Ship, $"⚙️ Throttle: {currentThrottle}", this);
        }

        [Command]
        private void CmdEmergencyStop()
        {
            emergencyStopActive = true;
            currentThrottle = 0f;
            autoNavigationEnabled = false;

            DebugManager.Log(DebugCategory.Ship, "🛑 EMERGENCY STOP ACTIVATED", this);
        }

        [Command]
        private void CmdAddWaypoint(Vector3 position)
        {
            waypoints.Add(new float3(position.x, position.y, position.z));

            // Notify clients to update visuals
            RpcUpdateWaypoints();
        }

        [Command]
        private void CmdToggleAutoNavigation()
        {
            if (waypoints.Count == 0) return;

            autoNavigationEnabled = !autoNavigationEnabled;
            if (autoNavigationEnabled)
            {
                currentWaypointIndex = 0;
                emergencyStopActive = false;
            }

            RpcSetAutoNavigation(autoNavigationEnabled);
        }

        [Command]
        private void CmdClearWaypoints()
        {
            waypoints.Clear();
            currentWaypointIndex = 0;
            autoNavigationEnabled = false;

            RpcClearWaypoints();
        }

        #endregion

        #region Client RPCs (Server → Clients)

        [ClientRpc]
        private void RpcUpdateWaypoints()
        {
            // Update waypoint visualization
            UpdateCourseLineVisual();

            if (isLocalPlayer)
            {
                OnWaypointAdded?.Invoke(waypoints[waypoints.Count - 1]);
            }
        }

        [ClientRpc]
        private void RpcSetAutoNavigation(bool enabled)
        {
            if (isLocalPlayer)
            {
                OnAutoNavigationToggled?.Invoke(enabled);
            }
        }

        [ClientRpc]
        private void RpcClearWaypoints()
        {
            // Clear visual elements
            if (courseLineRenderer != null)
            {
                courseLineRenderer.positionCount = 0;
            }

            // Destroy waypoint markers
            if (waypointContainer != null)
            {
                foreach (Transform child in waypointContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            if (isLocalPlayer)
            {
                OnWaypointsCleared?.Invoke();
            }
        }

        #endregion

        #region SyncVar Hooks

        private void OnThrottleChanged(float oldValue, float newValue)
        {
            if (isLocalPlayer)
            {
                OnThrottleChangedEvent?.Invoke(newValue);
            }
        }

        private void OnSpeedChanged(float oldValue, float newValue)
        {
            if (isLocalPlayer)
            {
                OnSpeedChangedEvent?.Invoke(newValue);
            }
        }

        #endregion

        #region Physics & Navigation (Continued in next message due to length)

        private void UpdateNavigation()
        {
            if (!autoNavigationEnabled || waypoints == null || waypoints.Count == 0) return;

            if (currentWaypointIndex < waypoints.Count)
            {
                float3 currentPos = transform.position;
                float3 targetWaypoint = waypoints[currentWaypointIndex];
                Vector3 currentPosVec = new Vector3(currentPos.x, currentPos.y, currentPos.z);
                Vector3 targetWaypointVec = new Vector3(targetWaypoint.x, targetWaypoint.y, targetWaypoint.z);
                float distanceToWaypoint = Vector3.Distance(currentPosVec, targetWaypointVec);

                if (distanceToWaypoint < 5f)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Count)
                    {
                        autoNavigationEnabled = false;
                        if (isLocalPlayer)
                        {
                            OnAutoNavigationToggled?.Invoke(false);
                        }
                        return;
                    }
                }

                // Calculate auto-steering
                targetDirection = math.normalize(targetWaypoint - currentPos);
                float3 currentForward = new float3(math.sin(math.radians(transform.eulerAngles.z)),
                                                  math.cos(math.radians(transform.eulerAngles.z)),
                                                  0f);

                float angleToTarget = Vector3.SignedAngle(
                    new Vector3(currentForward.x, currentForward.y, 0f),
                    new Vector3(targetDirection.x, targetDirection.y, 0f),
                    Vector3.forward
                );

                autoSteeringInput = Mathf.Clamp(angleToTarget / shipConfig.maxRudderAngle, -1f, 1f);
            }
        }

        private void UpdatePhysics()
        {
            // Convert physics state
            velocity = new float3(shipRigidbody.linearVelocity.x, shipRigidbody.linearVelocity.y, 0f);
            // NOTE: currentSpeed is tracked separately in ApplyNavalPhysics() and stays in knots
            // Do NOT derive from velocity here, as velocity is affected by globalSpeedMultiplier
        }

        private void ApplyNavalPhysics()
        {
            if (shipConfig == null) return;

            // Get target speed from throttle setting
            float targetSpeed = 0f;
            if (currentThrottle > 0)
            {
                targetSpeed = Mathf.Lerp(0f, shipConfig.maxSpeed, currentThrottle / 4f);
            }
            else if (currentThrottle < 0)
            {
                // Reverse speed is typically 50% of max forward speed
                float maxReverseSpeed = shipConfig.maxSpeed * 0.5f;
                targetSpeed = Mathf.Lerp(0f, -maxReverseSpeed, -currentThrottle / 4f);
            }

            // Apply acceleration/deceleration
            // NOTE: Keep speed in knots for display/naval calculations (DO NOT apply globalSpeedMultiplier here)
            // globalSpeedMultiplier is applied ONLY to Unity physics velocity (see below)
            float accelRate = (targetSpeed > currentSpeed) ? shipConfig.acceleration : shipConfig.deceleration;
            float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

            // Apply turning with effective rudder angle
            float angularVelocity = 0f;
            if (Mathf.Abs(effectiveRudderAngle) > 0.01f && Mathf.Abs(newSpeed) > 0.1f)
            {
                // Calculate turn rate with realistic ship-length scaling
                // Base turn rate from ship configuration (already tuned per ship class)
                float baseTurnRate = shipConfig.rudderRate;

                // Apply gentle length-based scaling (sqrt relationship for realism)
                // Reference length: 100m (destroyer/cruiser baseline)
                // Smaller ships turn faster, larger ships turn slower
                const float referenceLength = 100f;
                float lengthRatio = shipConfig.length / referenceLength;
                float lengthScaling = 1f / Mathf.Sqrt(lengthRatio); // Gentler than direct division

                // Apply scaling to base turn rate
                float maxTurnRate = baseTurnRate * lengthScaling;
                float currentTurnRate = (effectiveRudderAngle / shipConfig.maxRudderAngle) * maxTurnRate;

                // Speed-based turning effectiveness (bell curve)
                // newSpeed is already in knots (matches game world units)
                float speedEffectiveness = CalculateTurningEffectiveness(Mathf.Abs(newSpeed), shipConfig.maxSpeed);
                currentTurnRate *= speedEffectiveness;

                // NEGATIVE SIGN: Unity 2D rotation is counterclockwise for positive values
                // We want positive rudder (right turn) to be clockwise, so negate
                angularVelocity = -currentTurnRate;
            }

            // Apply forces
            // FIXED: Use transform.right for ships facing right in 2D (default Unity 2D orientation)
            // If your ship sprite faces UP by default, change this back to transform.up
            Vector2 forwardDirection = transform.right;

            // Apply globalSpeedMultiplier to Unity physics movement only (not naval speed calculations)
            // This allows display to show full knots (e.g., 28 kts) while Unity movement is slower (e.g., 14 units/s)
            Vector2 newVelocity = forwardDirection * newSpeed * globalSpeedMultiplier;

            shipRigidbody.linearVelocity = newVelocity;
            shipRigidbody.angularVelocity = angularVelocity;

            // Update currentSpeed for display (stays in knots, not affected by globalSpeedMultiplier)
            currentSpeed = newSpeed;
        }

        /// <summary>
        /// Calculate speed-based turning effectiveness using a bell curve
        /// Peak effectiveness at ~75% of max speed (Full Ahead)
        /// </summary>
        private float CalculateTurningEffectiveness(float currentSpeedKnots, float maxSpeedKnots)
        {
            // Always have minimal turning from prop wash when stationary
            if (currentSpeedKnots <= 0f) return 0.1f;

            // Calculate speed as percentage of max speed
            float speedPercent = currentSpeedKnots / maxSpeedKnots;

            // Bell curve with peak at 75% speed (Full Ahead)
            // Uses a Gaussian-like curve for realistic naval physics

            if (speedPercent <= 0.1f)
            {
                // Very low speeds (0-10%): Poor steerage, prop wash only
                // Ramp from 10% to 30% effectiveness
                return Mathf.Lerp(0.1f, 0.3f, speedPercent / 0.1f);
            }
            else if (speedPercent <= 0.75f)
            {
                // Low to optimal speeds (10-75%): Increasing effectiveness
                // Smooth curve from 30% to 100% (peak at Full Ahead)
                float t = (speedPercent - 0.1f) / 0.65f; // Normalize 0.1-0.75 to 0-1
                // Use smooth ease-in-out curve
                float smoothT = t * t * (3f - 2f * t);
                return Mathf.Lerp(0.3f, 1.0f, smoothT);
            }
            else
            {
                // High speeds (75-100%+): Decreasing effectiveness due to momentum
                // Drop from 100% to 80% at flank speed, then further decline
                float t = (speedPercent - 0.75f) / 0.25f; // Normalize 0.75-1.0 to 0-1
                float dropOff = Mathf.Lerp(1.0f, 0.8f, t); // Drop to 80% at 100% speed

                // Beyond 100% speed (if ship goes faster than rated max), continue declining
                if (speedPercent > 1.0f)
                {
                    float excessSpeed = speedPercent - 1.0f;
                    dropOff *= Mathf.Exp(-excessSpeed * 2f); // Exponential decay
                }

                return dropOff;
            }
        }

        private void UpdateCourseLineVisual()
        {
            if (courseLineRenderer == null || waypoints.Count == 0) return;

            courseLineRenderer.positionCount = waypoints.Count + 1;
            courseLineRenderer.SetPosition(0, transform.position);

            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector3 waypointPos = new Vector3(waypoints[i].x, waypoints[i].y, waypoints[i].z);
                courseLineRenderer.SetPosition(i + 1, waypointPos);
            }
        }

        private void UpdateDebugVisualization()
        {
            if (!enableDebugVisualization) return;

            // Only show debug for local player
            if (isLocalPlayer)
            {
                // Can add debug visualizations here
            }
        }

        #endregion

        #region Public API for Other Systems

        /// <summary>
        /// Get current ship velocity for camera look-ahead
        /// </summary>
        public Vector3 GetVelocity()
        {
            if (shipRigidbody != null)
            {
                return new Vector3(shipRigidbody.linearVelocity.x, shipRigidbody.linearVelocity.y, 0f);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Get current ship speed in knots
        /// </summary>
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }

        /// <summary>
        /// Get current throttle setting
        /// </summary>
        public float GetCurrentThrottle()
        {
            return currentThrottle;
        }

        /// <summary>
        /// Get current ship status for UI display (ShipDebugUI compatibility)
        /// </summary>
        public ShipStatus GetShipStatus()
        {
            return new ShipStatus
            {
                speed = currentSpeed,
                throttle = currentThrottle,
                heading = transform.eulerAngles.z,
                rudderAngle = rudderAngle,
                isAutoNavigating = autoNavigationEnabled,
                waypointCount = waypoints != null ? waypoints.Count : 0,
                currentWaypoint = currentWaypointIndex
            };
        }

        /// <summary>
        /// Get the current ship configuration (ShipDebugUI compatibility)
        /// </summary>
        public ShipConfigurationSO GetShipConfiguration()
        {
            return shipConfig;
        }

        #endregion
    }
}
