using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using WOS.ScriptableObjects;
using WOS.Player;
using WOS.Debugging;

namespace WOS.Camera
{
    /// <summary>
    /// Advanced camera controller for naval gameplay with smooth follow, zoom, and effects.
    /// Integrates with Unity Input System for optimal performance.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CameraSettingsSO cameraSettings;
        [SerializeField] private Transform followTarget;
        [SerializeField] private bool enableSmartFollow = true;

        [Header("Centering Options")]
        [Tooltip("Keep ship perfectly centered (disables look-ahead)")]
        [SerializeField] private bool keepShipCentered = false;

        [Header("Input References")]
        [SerializeField] private InputActionReference panAction;
        [SerializeField] private InputActionReference zoomAction;
        [SerializeField] private InputActionReference rotateAction;

        // Core Components
        private UnityEngine.Camera mainCamera;
        private SimpleNavalController targetShipController;

        // Camera State
        private float3 targetPosition;
        private float3 currentPosition;
        private float currentZoom;
        private float targetZoom;
        private float currentRotation;
        private float targetRotation;

        // Follow System
        private float3 followOffset;
        private float3 lookAheadOffset;
        private bool isManuallyPanning;
        private float lastInputTime;
        private float returnTimer;

        // Shake System
        private bool isShaking;
        private float shakeTimer;
        private float shakeIntensity;
        private float3 shakeOffset;
        private float shakeFrequency;


        // Performance
        private float lastUpdateTime;
        private int frameSkipCounter;

        // Events
        public static System.Action<float> OnZoomChanged;
        public static System.Action<Vector3> OnCameraPositionChanged;
        public static System.Action<bool> OnCameraPanStateChanged;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeCameraSettings();
            ValidateConfiguration();
            SetupInputActions();
        }

        private void InitializeComponents()
        {
            mainCamera = GetComponent<UnityEngine.Camera>();
            if (mainCamera == null)
                mainCamera = gameObject.AddComponent<UnityEngine.Camera>();

            if (followTarget != null)
            {
                targetShipController = followTarget.GetComponent<SimpleNavalController>();
            }
        }


        private void InitializeCameraSettings()
        {
            if (cameraSettings == null)
            {
                DebugManager.LogError(DebugCategory.Camera, "Camera settings not assigned! Using defaults.", this);
                return;
            }

            // Set initial camera properties
            mainCamera.orthographic = cameraSettings.useOrthographic;
            mainCamera.backgroundColor = cameraSettings.backgroundColor;
            mainCamera.cullingMask = cameraSettings.cullingMask;

            // Disable legacy mouse events to prevent frustum warnings
            mainCamera.eventMask = 0;

            // Initialize position and zoom
            currentZoom = targetZoom = cameraSettings.defaultZoom;
            currentRotation = targetRotation = cameraSettings.defaultRotation;

            if (followTarget != null)
            {
                transform.position = new Vector3(
                    followTarget.position.x,
                    followTarget.position.y,
                    cameraSettings.cameraDepth
                );
            }

            ApplyZoom();
        }

        private void ValidateConfiguration()
        {
            if (cameraSettings == null)
            {
                DebugManager.LogError(DebugCategory.Camera, "CameraSettingsSO is required!", this);
                enabled = false;
                return;
            }

            if (followTarget == null)
            {
                DebugManager.LogWarning(DebugCategory.Camera, "No follow target assigned", this);
            }
        }

        private void SetupInputActions()
        {
            if (panAction?.action != null)
            {
                panAction.action.performed += OnPanInput;
                panAction.action.canceled += OnPanInputCanceled;
            }

            if (zoomAction?.action != null)
            {
                zoomAction.action.performed += OnZoomInput;
            }

            if (rotateAction?.action != null)
            {
                rotateAction.action.performed += OnRotateInput;
            }
        }

        private void Update()
        {
            // Only handle non-camera logic here (if any)
            // REMOVED: Camera logic moved to LateUpdate() to fix vibration
        }

        private void LateUpdate()
        {
            // CRITICAL: All camera logic consolidated to LateUpdate() to prevent ship vibration
            // This ensures camera updates AFTER ship physics and all other Update() calls

            // DEBUG: Toggle centering with C key
            if (Input.GetKeyDown(KeyCode.C))
            {
                SetCenteredMode(!keepShipCentered);
                DebugManager.Log(DebugCategory.Camera, $"[DEBUG] Toggled centered mode: {keepShipCentered}", this);
            }

            // Performance throttling
            if (!ShouldUpdate()) return;

            // Update camera logic (target calculation)
            UpdateCameraLogic();
            ProcessShakeEffects();
            HandleReturnToTarget();

            // Perform direct camera calculations
            UpdateCameraDirectly();

            // Apply calculated results
            ApplyCameraTransform();
        }

        private void UpdateCameraDirectly()
        {
            // Direct camera calculation
            float deltaTime = Time.deltaTime;

            // Update position with smoothing
            Vector3 currentPos = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
            Vector3 targetPos = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
            Vector3 lerpedPos = Vector3.Lerp(currentPos, targetPos, cameraSettings.followSmoothness * deltaTime);
            currentPosition = new float3(lerpedPos.x, lerpedPos.y, lerpedPos.z);

            // Update zoom with smoothing
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, cameraSettings.zoomSmoothness * deltaTime);

            // Update rotation with smoothing
            currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, cameraSettings.rotationSmoothness * deltaTime);

            // Apply shake offset
            currentPosition = new float3(currentPosition.x + shakeOffset.x, currentPosition.y + shakeOffset.y, currentPosition.z);
        }

        private bool ShouldUpdate()
        {
            if (cameraSettings == null) return false;

            float targetFrameTime = 1f / cameraSettings.updateFrequency;
            float currentTime = Time.time;

            if (currentTime - lastUpdateTime < targetFrameTime)
            {
                frameSkipCounter++;
                return frameSkipCounter > 2; // Force update every 3 frames minimum
            }

            lastUpdateTime = currentTime;
            frameSkipCounter = 0;
            return true;
        }

        private void UpdateCameraLogic()
        {
            if (followTarget == null) return;

            UpdateFollowBehavior();
            UpdateZoomBehavior();
            UpdateRotationBehavior();
            CheckBoundaryConstraints();
        }

        private void UpdateFollowBehavior()
        {
            float3 targetPos = followTarget.position;

            // DEBUG: Log centering status
            if (Time.time % 2f < Time.deltaTime) // Every 2 seconds
            {
                DebugManager.Log(DebugCategory.Camera, $"[DEBUG] keepShipCentered={keepShipCentered}, enableSmartFollow={enableSmartFollow}, targetShipController={targetShipController != null}", this);
            }

            if (enableSmartFollow && targetShipController != null && !keepShipCentered)
            {
                // Get ship velocity for look-ahead calculation
                var shipStatus = targetShipController.GetShipStatus();
                float3 shipVelocity = new float3(
                    math.sin(shipStatus.heading * Mathf.Deg2Rad) * shipStatus.speed,
                    0f,
                    math.cos(shipStatus.heading * Mathf.Deg2Rad) * shipStatus.speed
                );

                // Calculate look-ahead position (DISABLED when keepShipCentered = true)
                float3 lookAheadPos = cameraSettings.GetLookAheadPosition(targetPos, shipVelocity);
                targetPos = lookAheadPos;

                // DEBUG: Log look-ahead calculation
                if (Time.time % 2f < Time.deltaTime)
                {
                    Vector3 shipPos = followTarget.position;
                    Vector3 lookAheadVector = new Vector3(lookAheadPos.x, lookAheadPos.y, lookAheadPos.z);
                    Vector3 offset = lookAheadVector - shipPos;
                    DebugManager.Log(DebugCategory.Camera, $"[DEBUG] LOOK-AHEAD: Ship={shipPos}, LookAhead={lookAheadVector}, Offset={offset}", this);
                }

                // Apply speed-based zoom
                if (cameraSettings.speedBasedZoom)
                {
                    targetZoom = cameraSettings.GetEffectiveZoom(cameraSettings.defaultZoom, shipStatus.speed);
                }

                // Apply speed-based shake
                if (cameraSettings.enableScreenShake)
                {
                    float shakeAmount = cameraSettings.GetShakeIntensity(shipStatus.speed);
                    if (shakeAmount > 0.1f && !isShaking)
                    {
                        StartShake(shakeAmount, cameraSettings.shakeDuration);
                    }
                }
            }
            else if (enableSmartFollow && targetShipController != null && keepShipCentered)
            {
                // CENTERED MODE: Still apply speed-based effects but no look-ahead
                var shipStatus = targetShipController.GetShipStatus();

                // DEBUG: Log centered mode
                if (Time.time % 2f < Time.deltaTime)
                {
                    DebugManager.Log(DebugCategory.Camera, $"[DEBUG] CENTERED MODE: Ship={followTarget.position}, Target stays at ship position", this);
                }

                // Apply speed-based zoom (even in centered mode)
                if (cameraSettings.speedBasedZoom)
                {
                    targetZoom = cameraSettings.GetEffectiveZoom(cameraSettings.defaultZoom, shipStatus.speed);
                }

                // Apply speed-based shake (even in centered mode)
                if (cameraSettings.enableScreenShake)
                {
                    float shakeAmount = cameraSettings.GetShakeIntensity(shipStatus.speed);
                    if (shakeAmount > 0.1f && !isShaking)
                    {
                        StartShake(shakeAmount, cameraSettings.shakeDuration);
                    }
                }

                // targetPos remains at ship position (no look-ahead offset)
            }

            // Apply manual pan offset
            targetPos += followOffset;

            // DEBUG: Log final target position
            if (Time.time % 2f < Time.deltaTime)
            {
                DebugManager.Log(DebugCategory.Camera, $"[DEBUG] FINAL: Ship={followTarget.position}, Camera Target={targetPos}, Manual Offset={followOffset}", this);
            }

            // Store target for job system
            targetPosition = targetPos;
        }

        private void UpdateZoomBehavior()
        {
            targetZoom = Mathf.Clamp(targetZoom, cameraSettings.minZoom, cameraSettings.maxZoom);
        }

        private void UpdateRotationBehavior()
        {
            if (!cameraSettings.allowRotation) return;

            // Rotation is handled via input or can be automated based on ship heading
            targetRotation = Mathf.Clamp(targetRotation, -180f, 180f);
        }

        private void CheckBoundaryConstraints()
        {
            if (!cameraSettings.useBoundaries) return;

            targetPosition = cameraSettings.ClampToBounds(targetPosition);
        }

        private void ProcessShakeEffects()
        {
            if (!isShaking) return;

            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                isShaking = false;
                shakeOffset = float3.zero;
                return;
            }

            // Calculate shake offset
            float shakePhase = Time.time * shakeFrequency;
            float shakeFalloff = shakeTimer / cameraSettings.shakeDuration;
            float currentShakeIntensity = shakeIntensity * shakeFalloff;

            shakeOffset = new float3(
                math.sin(shakePhase) * currentShakeIntensity,
                math.cos(shakePhase * 1.5f) * currentShakeIntensity * 0.5f,
                0f
            );
        }

        private void HandleReturnToTarget()
        {
            if (!isManuallyPanning || cameraSettings.autoReturnDelay <= 0f) return;

            returnTimer += Time.deltaTime;
            if (returnTimer >= cameraSettings.autoReturnDelay)
            {
                // Gradually return to target
                float returnSpeed = cameraSettings.returnToTargetSpeed * Time.deltaTime;
                Vector3 currentOffset = new Vector3(followOffset.x, followOffset.y, followOffset.z);
                Vector3 lerpedOffset = Vector3.Lerp(currentOffset, Vector3.zero, returnSpeed);
                followOffset = new float3(lerpedOffset.x, lerpedOffset.y, lerpedOffset.z);

                Vector3 followOffsetVec = new Vector3(followOffset.x, followOffset.y, followOffset.z);
                if (followOffsetVec.magnitude < 0.1f)
                {
                    followOffset = float3.zero;
                    isManuallyPanning = false;
                    returnTimer = 0f;
                    OnCameraPanStateChanged?.Invoke(false);
                }
            }
        }


        private void ApplyCameraTransform()
        {
            // Apply position (currentPosition updated by UpdateCameraDirectly)
            transform.position = new Vector3(currentPosition.x, currentPosition.y, cameraSettings.cameraDepth);

            // Apply rotation
            if (cameraSettings.allowRotation)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            }

            // Apply zoom
            ApplyZoom();

            // Notify events
            OnCameraPositionChanged?.Invoke(transform.position);
            OnZoomChanged?.Invoke(currentZoom);
        }

        private void ApplyZoom()
        {
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = currentZoom;
            }
            else
            {
                // For perspective cameras, adjust distance instead
                Vector3 pos = transform.position;
                pos.z = cameraSettings.cameraDepth - currentZoom;
                transform.position = pos;
            }
        }

        #region Input Handlers

        private void OnPanInput(InputAction.CallbackContext context)
        {
            Vector2 panInput = context.ReadValue<Vector2>();
            float panSpeed = cameraSettings.panSpeed * Time.deltaTime;

            // Convert screen delta to world delta
            Vector3 worldDelta = new Vector3(panInput.x, panInput.y, 0f) * panSpeed;

            // Apply current camera rotation to pan direction
            if (cameraSettings.allowRotation)
            {
                worldDelta = Quaternion.Euler(0f, 0f, currentRotation) * worldDelta;
            }

            followOffset += new float3(worldDelta.x, worldDelta.y, 0f);

            // Clamp pan distance
            Vector3 followOffsetVec = new Vector3(followOffset.x, followOffset.y, followOffset.z);
            float panDistance = followOffsetVec.magnitude;
            if (panDistance > cameraSettings.maxPanDistance)
            {
                Vector3 normalizedOffset = followOffsetVec.normalized;
                followOffset = new float3(normalizedOffset.x, normalizedOffset.y, normalizedOffset.z) * cameraSettings.maxPanDistance;
            }

            isManuallyPanning = true;
            returnTimer = 0f;
            lastInputTime = Time.time;

            OnCameraPanStateChanged?.Invoke(true);
        }

        private void OnPanInputCanceled(InputAction.CallbackContext context)
        {
            // Start return timer when input stops
            returnTimer = 0f;
        }

        private void OnZoomInput(InputAction.CallbackContext context)
        {
            float scrollDelta = context.ReadValue<Vector2>().y;
            float zoomChange = scrollDelta * cameraSettings.zoomSpeed * Time.deltaTime;

            targetZoom = Mathf.Clamp(targetZoom - zoomChange, cameraSettings.minZoom, cameraSettings.maxZoom);
        }

        private void OnRotateInput(InputAction.CallbackContext context)
        {
            if (!cameraSettings.allowRotation) return;

            Vector2 rotateInput = context.ReadValue<Vector2>();
            float rotateChange = rotateInput.x * cameraSettings.rotationSpeed * Time.deltaTime;

            targetRotation += rotateChange;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set the target to follow
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            if (target != null)
            {
                targetShipController = target.GetComponent<SimpleNavalController>();
            }
        }

        /// <summary>
        /// Start screen shake effect
        /// </summary>
        public void StartShake(float intensity, float duration)
        {
            if (!cameraSettings.enableScreenShake) return;

            isShaking = true;
            shakeIntensity = intensity;
            shakeTimer = duration;
            shakeFrequency = cameraSettings.shakeFrequency;
        }

        /// <summary>
        /// Stop screen shake immediately
        /// </summary>
        public void StopShake()
        {
            isShaking = false;
            shakeTimer = 0f;
            shakeOffset = float3.zero;
        }

        /// <summary>
        /// Set zoom level directly
        /// </summary>
        public void SetZoom(float zoom)
        {
            targetZoom = Mathf.Clamp(zoom, cameraSettings.minZoom, cameraSettings.maxZoom);
        }

        /// <summary>
        /// Set camera rotation directly
        /// </summary>
        public void SetRotation(float rotation)
        {
            if (!cameraSettings.allowRotation) return;
            targetRotation = rotation;
        }

        /// <summary>
        /// Pan camera to specific world position
        /// </summary>
        public void PanToPosition(Vector3 worldPosition)
        {
            if (followTarget == null) return;

            Vector3 offset = worldPosition - followTarget.position;
            followOffset = new float3(offset.x, offset.y, 0f);

            // Clamp to max pan distance
            Vector3 offsetVec = new Vector3(followOffset.x, followOffset.y, followOffset.z);
            float distance = offsetVec.magnitude;
            if (distance > cameraSettings.maxPanDistance)
            {
                Vector3 normalizedOffsetVec = offsetVec.normalized;
                followOffset = new float3(normalizedOffsetVec.x, normalizedOffsetVec.y, normalizedOffsetVec.z) * cameraSettings.maxPanDistance;
            }

            isManuallyPanning = true;
            returnTimer = 0f;
        }

        /// <summary>
        /// Return camera to follow target immediately
        /// </summary>
        public void ReturnToTarget()
        {
            followOffset = float3.zero;
            isManuallyPanning = false;
            returnTimer = 0f;
            OnCameraPanStateChanged?.Invoke(false);
        }

        /// <summary>
        /// Toggle between centered mode and look-ahead mode
        /// </summary>
        public void SetCenteredMode(bool centered)
        {
            keepShipCentered = centered;
            if (centered)
            {
                // Immediately return to ship position when enabling centered mode
                ReturnToTarget();
            }
        }

        /// <summary>
        /// Check if camera is in centered mode
        /// </summary>
        public bool IsCenteredMode()
        {
            return keepShipCentered;
        }

        /// <summary>
        /// Get current camera status
        /// </summary>
        public CameraStatus GetCameraStatus()
        {
            return new CameraStatus
            {
                position = transform.position,
                zoom = currentZoom,
                rotation = currentRotation,
                isFollowingTarget = !isManuallyPanning,
                isPanning = isManuallyPanning,
                isShaking = isShaking,
                isCentered = keepShipCentered
            };
        }

        #endregion

        private void OnDestroy()
        {

            // Unsubscribe from input actions
            if (panAction?.action != null)
            {
                panAction.action.performed -= OnPanInput;
                panAction.action.canceled -= OnPanInputCanceled;
            }

            if (zoomAction?.action != null)
            {
                zoomAction.action.performed -= OnZoomInput;
            }

            if (rotateAction?.action != null)
            {
                rotateAction.action.performed -= OnRotateInput;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (cameraSettings == null) return;

            // Draw camera boundaries
            if (cameraSettings.useBoundaries)
            {
                Gizmos.color = Color.yellow;
                Vector3 boundCenter = new Vector3(
                    cameraSettings.worldBounds.center.x,
                    cameraSettings.worldBounds.center.y,
                    0f
                );
                Vector3 boundSize = new Vector3(
                    cameraSettings.worldBounds.width,
                    cameraSettings.worldBounds.height,
                    1f
                );
                Gizmos.DrawWireCube(boundCenter, boundSize);
            }

            // Draw follow target connection
            if (followTarget != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, followTarget.position);

                // Draw max follow distance
                Gizmos.color = Color.green;
                DrawWireCircle(followTarget.position, cameraSettings.maxFollowDistance);

                // Draw max pan distance
                Gizmos.color = Color.blue;
                DrawWireCircle(followTarget.position, cameraSettings.maxPanDistance);
            }
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
    /// Camera status data structure
    /// </summary>
    [System.Serializable]
    public struct CameraStatus
    {
        public Vector3 position;
        public float zoom;
        public float rotation;
        public bool isFollowingTarget;
        public bool isPanning;
        public bool isShaking;
        public bool isCentered;
    }
}