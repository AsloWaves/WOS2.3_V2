using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using WOS.Debugging;

namespace WOS.Camera
{
    /// <summary>
    /// Network-aware camera controller that only follows the LOCAL player.
    /// Handles following, zooming, and manual panning with smooth transitions.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class SimpleCameraController : MonoBehaviour
    {
        [Header("Target Following")]
        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = 2f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        [Header("Zoom Controls")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private bool invertZoom = false;

        [Header("Manual Camera Controls")]
        [SerializeField] private float panSpeed = 3f;
        [SerializeField] private bool enableManualPan = true;

        [Header("Look-Ahead")]
        [SerializeField] private bool enableLookAhead = true;
        [SerializeField] private float lookAheadDistance = 5f;
        [SerializeField] private float lookAheadSmoothTime = 0.5f;

        [Header("Centering")]
        [Tooltip("Keep ship perfectly centered (disables look-ahead)")]
        [SerializeField] private bool keepShipCentered = false;

        [Header("Input Control")]
        [SerializeField] private bool inputLocked = false; // Locks manual camera input while allowing automatic following

        // Components
        private UnityEngine.Camera cam;
        private Player.NetworkedNavalController shipController;

        // State
        private Vector3 targetPosition;
        private bool isManuallyPanning = false;
        private Vector3 lookAheadVelocity;
        private Vector3 currentLookAhead;

        // Input System
        private PlayerInput playerInput;
        private InputAction zoomAction;
        private InputAction panAction;

        private void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();

            // Disable legacy mouse events to prevent frustum warnings
            cam.eventMask = 0;

            // Setup Input System
            SetupInputSystem();

            // Auto-find the LOCAL player ship if no target assigned
            if (target == null)
            {
                FindLocalPlayer();
            }
            else if (target != null)
            {
                shipController = target.GetComponent<Player.NetworkedNavalController>();
            }

            // Initialize position
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        /// <summary>
        /// Finds and assigns the LOCAL player ship for camera following
        /// This ensures multiplayer cameras only follow their own ship
        /// </summary>
        private void FindLocalPlayer()
        {
            // Wait a frame for NetworkManager to spawn players
            StartCoroutine(FindLocalPlayerCoroutine());
        }

        private System.Collections.IEnumerator FindLocalPlayerCoroutine()
        {
            // Wait for network spawn
            yield return new WaitForSeconds(0.5f);

            // Find all ship controllers
            var allShips = FindObjectsByType<Player.NetworkedNavalController>(FindObjectsSortMode.None);

            foreach (var shipCtrl in allShips)
            {
                var networkIdentity = shipCtrl.GetComponent<NetworkIdentity>();
                if (networkIdentity != null && networkIdentity.isLocalPlayer)
                {
                    target = shipCtrl.transform;
                    shipController = shipCtrl;
                    DebugManager.Log(DebugCategory.Camera, $"🎥 Camera assigned to LOCAL player: {shipCtrl.gameObject.name}", this);

                    // Initialize position
                    transform.position = target.position + offset;
                    yield break;
                }
            }

            // Fallback for non-networked testing
            DebugManager.LogWarning(DebugCategory.Camera, "No networked local player found, using fallback search", this);
            GameObject fallbackShip = GameObject.FindWithTag("Player");
            if (fallbackShip == null)
                fallbackShip = GameObject.Find("PlayerShip");

            if (fallbackShip != null)
            {
                target = fallbackShip.transform;
                shipController = fallbackShip.GetComponent<Player.NetworkedNavalController>();
                transform.position = target.position + offset;
                DebugManager.Log(DebugCategory.Camera, $"Fallback: Assigned to follow: {fallbackShip.name}", this);
            }
        }

        private void SetupInputSystem()
        {
            playerInput = GetComponent<PlayerInput>();

            // If no PlayerInput component, try to find the player's
            if (playerInput == null && target != null)
            {
                playerInput = target.GetComponent<PlayerInput>();
            }

            // If still no PlayerInput, we'll use legacy input as fallback
            if (playerInput != null)
            {
                var actionMap = playerInput.currentActionMap;
                if (actionMap != null)
                {
                    zoomAction = actionMap.FindAction("Zoom");
                    panAction = actionMap.FindAction("Pan");

                    if (zoomAction != null)
                        zoomAction.Enable();
                    if (panAction != null)
                        panAction.Enable();
                }
            }
        }

        private void Update()
        {
            // DEBUG: Toggle centering with C key (remove this in production)
            if (Input.GetKeyDown(KeyCode.C))
            {
                keepShipCentered = !keepShipCentered;
                DebugManager.Log(DebugCategory.Camera, $"Toggled centered mode: {keepShipCentered}", this);
                if (keepShipCentered)
                {
                    // Reset look-ahead when centering
                    currentLookAhead = Vector3.zero;
                    // Immediately snap camera to centered position
                    if (target != null)
                    {
                        Vector3 centeredPosition = target.position + offset;
                        transform.position = centeredPosition;
                        targetPosition = centeredPosition;
                    }
                }
            }

            HandleZoom();
            HandleManualPanning();
        }

        private void LateUpdate()
        {
            if (target != null && !isManuallyPanning)
            {
                FollowTarget();
            }
        }

        private void FollowTarget()
        {
            Vector3 desiredPosition = target.position + offset;

            // Add look-ahead based on ship velocity (DISABLED when keepShipCentered = true)
            if (enableLookAhead && shipController != null && !keepShipCentered)
            {
                Vector3 shipVelocity = shipController.GetVelocity();
                Vector3 targetLookAhead = shipVelocity.normalized * Mathf.Min(shipVelocity.magnitude * lookAheadDistance, lookAheadDistance * 2f);

                // Smooth the look-ahead to prevent jitter
                currentLookAhead = Vector3.SmoothDamp(currentLookAhead, targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);
                desiredPosition += currentLookAhead;

                // Optional: Log look-ahead info for debugging
                // if (Time.time % 2f < Time.deltaTime)
                // {
                //     DebugManager.Log(DebugCategory.Camera, $"LOOK-AHEAD: Ship={target.position}, Velocity={shipVelocity}, LookAhead={currentLookAhead}", this);
                // }
            }
            else if (keepShipCentered)
            {
                // CENTERED MODE: Immediately remove any existing look-ahead offset
                currentLookAhead = Vector3.zero;

                // Optional: Log centered mode for debugging
                // if (Time.time % 2f < Time.deltaTime)
                // {
                //     DebugManager.Log(DebugCategory.Camera, $"CENTERED MODE: Ship={target.position}, No look-ahead applied, LookAhead forced to zero", this);
                // }
            }

            // Smooth camera movement (or immediate if centered)
            targetPosition = desiredPosition;

            if (keepShipCentered)
            {
                // CENTERED MODE: Immediate camera movement to stay perfectly centered
                transform.position = targetPosition;
            }
            else
            {
                // LOOK-AHEAD MODE: Smooth camera movement
                Vector3 newCameraPos = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                transform.position = newCameraPos;
            }

            // Optional: Log camera positioning for debugging
            // if (Time.time % 2f < Time.deltaTime)
            // {
            //     Vector3 cameraToShip = target.position - transform.position;
            //     DebugManager.Log(DebugCategory.Camera, $"POSITIONS: Camera={transform.position}, Target={targetPosition}, Ship={target.position}, Offset={cameraToShip}, CurrentLookAhead={currentLookAhead}", this);
            // }
        }

        private void HandleZoom()
        {
            // Skip zoom handling if input is locked (e.g., UI is open)
            if (inputLocked)
                return;

            float scrollInput = 0f;

            // Try Input System first
            if (zoomAction != null && zoomAction.enabled)
            {
                scrollInput = zoomAction.ReadValue<float>();
            }
            // Fallback to legacy input
            else
            {
                scrollInput = Input.GetAxis("Mouse ScrollWheel");
            }

            if (scrollInput != 0)
            {
                float zoomDirection = invertZoom ? -scrollInput : scrollInput;
                float newSize = cam.orthographicSize - (zoomDirection * zoomSpeed);
                cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            }
        }

        private void HandleManualPanning()
        {
            if (!enableManualPan) return;

            // Skip manual panning if input is locked (e.g., UI is open)
            if (inputLocked)
                return;

            bool startPanning = false;
            bool stopPanning = false;

            // Check for pan input
            if (panAction != null && panAction.enabled)
            {
                float panValue = panAction.ReadValue<float>();
                startPanning = panValue > 0.5f && !isManuallyPanning;
                stopPanning = panValue < 0.5f && isManuallyPanning;
            }
            else
            {
                // Fallback to legacy input - Middle mouse or Alt+Left mouse
                startPanning = Input.GetMouseButtonDown(2) || (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(0));
                stopPanning = Input.GetMouseButtonUp(2) || Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetMouseButtonUp(0);
            }

            if (startPanning)
            {
                isManuallyPanning = true;
            }

            if (stopPanning)
            {
                isManuallyPanning = false;
            }

            // Perform panning
            if (isManuallyPanning)
            {
                Vector3 mouseMovement = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0);
                mouseMovement *= panSpeed * (cam.orthographicSize / 5f); // Scale pan speed with zoom
                transform.position += mouseMovement;
            }

            // Return to following target with spacebar
            if (Input.GetKeyDown(KeyCode.Space) && target != null)
            {
                isManuallyPanning = false;
                DebugManager.Log(DebugCategory.Camera, "Returning to follow target", this);
            }
        }

        #region Public Methods

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            isManuallyPanning = false;

            if (target != null)
            {
                shipController = target.GetComponent<Player.NetworkedNavalController>();
            }
        }

        public void SetFollowSpeed(float speed)
        {
            followSpeed = Mathf.Clamp(speed, 0.1f, 10f);
        }

        public void ToggleManualPanning(bool enabled)
        {
            enableManualPan = enabled;
            if (!enabled) isManuallyPanning = false;
        }

        public void SetLookAhead(bool enabled, float distance = 5f)
        {
            enableLookAhead = enabled;
            lookAheadDistance = distance;
        }

        /// <summary>
        /// Toggle between centered mode and look-ahead mode
        /// </summary>
        public void SetCenteredMode(bool centered)
        {
            keepShipCentered = centered;
            if (centered)
            {
                // Immediately remove look-ahead offset
                currentLookAhead = Vector3.zero;
                // Immediately snap camera to centered position
                if (target != null)
                {
                    Vector3 centeredPosition = target.position + offset;
                    transform.position = centeredPosition;
                    targetPosition = centeredPosition;
                    // DebugManager.Log(DebugCategory.Camera, $"SetCenteredMode: Snapped to centered position {centeredPosition}", this);
                }
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
        /// Lock manual camera input while allowing automatic following to continue
        /// Used by UI systems like inventory panels
        /// </summary>
        public void LockInput()
        {
            inputLocked = true;
            DebugManager.Log(DebugCategory.Camera, "Manual camera input locked - automatic following continues", this);
        }

        /// <summary>
        /// Unlock manual camera input to restore zoom and pan controls
        /// </summary>
        public void UnlockInput()
        {
            inputLocked = false;
            DebugManager.Log(DebugCategory.Camera, "Manual camera input unlocked - zoom and pan restored", this);
        }

        /// <summary>
        /// Check if manual camera input is currently locked
        /// </summary>
        public bool IsInputLocked()
        {
            return inputLocked;
        }

        #endregion
    }
}