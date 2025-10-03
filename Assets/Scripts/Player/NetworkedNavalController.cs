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
            if (isLocalPlayer && playerInput != null)
            {
                throttleUpAction.performed -= OnThrottleUp;
                throttleDownAction.performed -= OnThrottleDown;
                emergencyStopAction.performed -= OnEmergencyStop;
                setWaypointAction.performed -= OnSetWaypoint;
                autoNavigateAction.performed -= OnToggleAutoNavigation;
                clearWaypointsAction.performed -= OnClearWaypoints;
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            shipRigidbody = GetComponent<Rigidbody2D>();

            // Configure Rigidbody2D for naval physics
            shipRigidbody.gravityScale = 0f;
            shipRigidbody.linearDamping = 0.5f;
            shipRigidbody.angularDamping = 2f;
            shipRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void InitializeInputSystem()
        {
            // Add PlayerInput component for local player only
            playerInput = gameObject.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = gameObject.AddComponent<PlayerInput>();
            }

            // Load the input actions asset
            var inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActions == null)
            {
                DebugManager.LogError(DebugCategory.Input, "Failed to load InputSystem_Actions! Place it in Resources folder.", this);
                return;
            }

            playerInput.actions = inputActions;

            var actionMap = playerInput.actions.FindActionMap("Naval");
            if (actionMap == null)
            {
                DebugManager.LogError(DebugCategory.Input, "Naval action map not found!", this);
                return;
            }

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

            // Enable the action map
            actionMap.Enable();
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
            currentSpeed = math.length(velocity) * 1.94384f; // m/s to knots
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

            targetSpeed *= globalSpeedMultiplier;

            // Apply acceleration/deceleration
            float targetSpeedMs = targetSpeed / 1.94384f; // knots to m/s
            float currentSpeedMs = currentSpeed / 1.94384f;

            float accelRate = (targetSpeed > currentSpeed) ? shipConfig.acceleration : shipConfig.deceleration;
            float newSpeedMs = Mathf.MoveTowards(currentSpeedMs, targetSpeedMs, accelRate * Time.fixedDeltaTime);

            // Apply turning with effective rudder angle
            float angularVelocity = 0f;
            if (Mathf.Abs(effectiveRudderAngle) > 0.01f && Mathf.Abs(newSpeedMs) > 0.1f)
            {
                // Calculate turn rate from rudder angle and ship characteristics
                float turningFactor = (shipConfig.rudderRate / shipConfig.length) * 10f; // Simplified turning calculation
                float turnRate = turningFactor * (effectiveRudderAngle / shipConfig.maxRudderAngle);
                float speedFactor = Mathf.Clamp01(Mathf.Abs(newSpeedMs) / (shipConfig.maxSpeed / 1.94384f));
                angularVelocity = turnRate * speedFactor;
            }

            // Apply forces
            Vector2 forwardDirection = transform.up;
            Vector2 newVelocity = forwardDirection * newSpeedMs;

            shipRigidbody.linearVelocity = newVelocity;
            shipRigidbody.angularVelocity = angularVelocity;
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

        #endregion
    }
}
