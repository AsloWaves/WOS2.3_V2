using UnityEngine;
using WOS.Player;
using WOS.ScriptableObjects;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Controls ship wake and trail effects based on ship movement and physics.
    /// Integrates with SimpleNavalController for realistic wake generation.
    /// </summary>
    [RequireComponent(typeof(SimpleNavalController))]
    public class ShipWakeController : MonoBehaviour
    {
        [Header("Debug Settings")]
        [Tooltip("Disable particle systems to test for Job System memory leaks")]
        [SerializeField] private bool disableParticlesForTesting = false;

        [Header("Wake Configuration")]
        [Tooltip("Particle system for main wake trail")]
        [SerializeField] private ParticleSystem mainWakeParticles;

        [Tooltip("Particle system for turbulence and bubbles")]
        [SerializeField] private ParticleSystem turbulenceParticles;

        [Tooltip("Trail renderer for continuous wake line")]
        [SerializeField] private TrailRenderer wakeTrail;

        [Header("Wake Intensity")]
        [Tooltip("Base emission rate for wake particles")]
        [Range(10f, 200f)]
        [SerializeField] private float baseEmissionRate = 50f;

        [Tooltip("Maximum emission rate at full speed")]
        [Range(50f, 500f)]
        [SerializeField] private float maxEmissionRate = 250f;

        [Tooltip("Minimum speed before wake appears")]
        [Range(0.5f, 3f)]
        [SerializeField] private float minimumWakeSpeed = 1f;

        [Tooltip("Speed where wake reaches maximum intensity")]
        [Range(5f, 30f)]
        [SerializeField] private float maxIntensitySpeed = 15f;

        [Header("Wake Position")]
        [Tooltip("Offset from ship center for wake spawn point")]
        [SerializeField] private Vector3 wakeSpawnOffset = new Vector3(0f, -1f, -3f);

        [Tooltip("Enable wake position adjustment based on ship length")]
        [SerializeField] private bool adjustForShipLength = true;

        [Header("Turning Effects")]
        [Tooltip("Additional wake intensity when turning")]
        [Range(0f, 2f)]
        [SerializeField] private float turningIntensityMultiplier = 1.5f;

        [Tooltip("Rudder angle threshold for turning wake effects")]
        [Range(5f, 30f)]
        [SerializeField] private float turningThreshold = 10f;

        [Header("Performance")]
        [Tooltip("Distance from camera before reducing wake detail")]
        [SerializeField] private float lodDistance = 1000f;

        [Tooltip("Update frequency for wake calculations")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float updateInterval = 0.1f;

        // Component References
        private SimpleNavalController shipController;
        private ShipConfigurationSO shipConfig;
        private UnityEngine.Camera oceanCamera;

        // Wake State
        private float currentWakeIntensity;
        private bool wakeActive = false;
        private Vector3 lastShipPosition;
        private float lastUpdateTime;

        // Performance State
        private bool isHighDetail = true;
        private float lastLODCheck;

        // Particle System State
        private ParticleSystem.EmissionModule mainEmission;
        private ParticleSystem.EmissionModule turbulenceEmission;
        private ParticleSystem.VelocityOverLifetimeModule mainVelocity;

        private void Awake()
        {
            // Get required components
            shipController = GetComponent<SimpleNavalController>();
            if (shipController == null)
            {
                DebugManager.LogError(DebugCategory.Ocean, "SimpleNavalController not found!", this);
                enabled = false;
                return;
            }

            // Find camera
            oceanCamera = UnityEngine.Camera.main;

            // Store initial position
            lastShipPosition = transform.position;
        }

        private void Start()
        {
            // Get ship configuration
            shipConfig = shipController.GetShipConfiguration();

            // Adjust wake position for ship size
            if (adjustForShipLength && shipConfig != null)
            {
                float shipLength = shipConfig.length;
                wakeSpawnOffset.z = -(shipLength * 0.4f); // Wake at 40% back from center
            }

            // Initialize particle systems
            InitializeParticleSystems();

            // Subscribe to ship events
            SimpleNavalController.OnSpeedChanged += OnShipSpeedChanged;

            DebugManager.Log(DebugCategory.Ocean, $"Initialized for {shipConfig?.shipName ?? "Unknown Ship"}", this);
        }

        private void Update()
        {
            // Performance check
            if (Time.time - lastLODCheck > 0.5f)
            {
                UpdateLODState();
                lastLODCheck = Time.time;
            }

            // Update wake at specified interval
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateWakeEffects();
                lastUpdateTime = Time.time;
            }
        }

        private void InitializeParticleSystems()
        {
            // Check if particles are disabled for testing
            if (disableParticlesForTesting)
            {
                if (mainWakeParticles != null) mainWakeParticles.gameObject.SetActive(false);
                if (turbulenceParticles != null) turbulenceParticles.gameObject.SetActive(false);
                DebugManager.LogWarning(DebugCategory.Ocean, "[ShipWakeController] Particles disabled for Job System testing", this);
                return;
            }

            // Initialize main wake particles if assigned
            if (mainWakeParticles != null)
            {
                mainEmission = mainWakeParticles.emission;
                mainVelocity = mainWakeParticles.velocityOverLifetime;

                // Set to world simulation space so particles stay in water
                var mainSettings = mainWakeParticles.main;
                mainSettings.simulationSpace = ParticleSystemSimulationSpace.World;

                // Position at wake spawn point
                mainWakeParticles.transform.localPosition = wakeSpawnOffset;
            }

            // Initialize turbulence particles if assigned
            if (turbulenceParticles != null)
            {
                turbulenceEmission = turbulenceParticles.emission;

                // Set to world simulation space so particles stay in water
                var turbulenceSettings = turbulenceParticles.main;
                turbulenceSettings.simulationSpace = ParticleSystemSimulationSpace.World;

                // Position slightly behind main wake
                Vector3 turbulenceOffset = wakeSpawnOffset + Vector3.back * 2f;
                turbulenceParticles.transform.localPosition = turbulenceOffset;
            }

            // Initialize trail renderer if assigned
            if (wakeTrail != null)
            {
                // Position trail at water surface
                Vector3 trailOffset = wakeSpawnOffset;
                trailOffset.y = 0f; // At water surface
                wakeTrail.transform.localPosition = trailOffset;
            }
        }

        private void UpdateLODState()
        {
            if (oceanCamera == null) return;

            float distanceToCamera = Vector3.Distance(transform.position, oceanCamera.transform.position);
            bool wasHighDetail = isHighDetail;
            isHighDetail = distanceToCamera <= lodDistance;

            // Reduce detail for distant ships
            if (wasHighDetail != isHighDetail)
            {
                SetDetailLevel(isHighDetail);
            }
        }

        private void UpdateWakeEffects()
        {
            if (shipController == null || disableParticlesForTesting) return;

            // Get current ship status
            var shipStatus = shipController.GetShipStatus();
            float currentSpeed = shipStatus.speed;
            float rudderAngle = shipStatus.rudderAngle;

            // Calculate wake intensity
            float speedIntensity = CalculateSpeedIntensity(currentSpeed);
            float turningIntensity = CalculateTurningIntensity(rudderAngle, currentSpeed);
            currentWakeIntensity = speedIntensity * (1f + turningIntensity);

            // Update wake active state
            bool shouldBeActive = currentSpeed >= minimumWakeSpeed && isHighDetail;
            if (shouldBeActive != wakeActive)
            {
                SetWakeActive(shouldBeActive);
            }

            // Update particle systems
            if (wakeActive)
            {
                UpdateParticleIntensity();
                UpdateTrailProperties();
            }

            // Update position tracking
            lastShipPosition = transform.position;
        }

        private float CalculateSpeedIntensity(float speed)
        {
            if (speed < minimumWakeSpeed)
                return 0f;

            // Normalize speed to intensity
            float normalizedSpeed = Mathf.Clamp01(speed / maxIntensitySpeed);
            return Mathf.Lerp(0.2f, 1f, normalizedSpeed); // Never completely zero when moving
        }

        private float CalculateTurningIntensity(float rudderAngle, float speed)
        {
            float absRudder = Mathf.Abs(rudderAngle);
            if (absRudder < turningThreshold || speed < minimumWakeSpeed)
                return 0f;

            // Turning intensity based on rudder angle and speed
            float rudderFactor = Mathf.Clamp01(absRudder / 35f); // Normalize to max rudder
            float speedFactor = Mathf.Clamp01(speed / 10f); // More turning wake at higher speeds

            return rudderFactor * speedFactor * turningIntensityMultiplier;
        }

        private void SetWakeActive(bool active)
        {
            wakeActive = active;

            // Control particle systems
            if (mainWakeParticles != null)
            {
                var emission = mainWakeParticles.emission;
                emission.enabled = active;
            }

            if (turbulenceParticles != null)
            {
                var emission = turbulenceParticles.emission;
                emission.enabled = active;
            }

            // Control trail renderer
            if (wakeTrail != null)
            {
                wakeTrail.emitting = active;
            }
        }

        private void UpdateParticleIntensity()
        {
            float targetEmissionRate = Mathf.Lerp(baseEmissionRate, maxEmissionRate, currentWakeIntensity);

            // Update main wake particles
            if (mainWakeParticles != null && mainEmission.enabled)
            {
                mainEmission.rateOverTime = targetEmissionRate;

                // Adjust particle velocity and position based on ship movement for realistic wake physics
                if (mainVelocity.enabled)
                {
                    Vector3 shipVelocity = (transform.position - lastShipPosition) / Time.deltaTime;
                    mainVelocity.space = ParticleSystemSimulationSpace.World;

                    // Calculate ship's forward direction
                    Vector3 shipForward = transform.up; // In 2D, ship's forward is typically up
                    Vector3 shipRight = transform.right;

                    // Calculate ship speed in knots
                    float shipSpeedKnots = shipVelocity.magnitude * 1.944f; // m/s to knots conversion

                    // Dynamic spawn offset compensation for high speeds
                    // At higher speeds, spawn particles further back to close the gap
                    float speedCompensation = Mathf.Clamp(shipSpeedKnots * 0.1f, 0f, 2f); // Max 2 units back
                    Vector3 dynamicOffset = wakeSpawnOffset - (shipForward * speedCompensation);

                    // Update particle system position with compensation
                    mainWakeParticles.transform.localPosition = dynamicOffset;

                    // Wake particles spread outward and backward from ship
                    float wakeSpreadSpeed = 2f + (shipSpeedKnots * 0.05f); // Increase spread with speed

                    // Set particle initial velocity to counteract ship movement and add wake physics
                    mainVelocity.radial = wakeSpreadSpeed * currentWakeIntensity;

                    // Add backward drift that increases with ship speed
                    Vector3 velocityCompensation = -shipVelocity * 0.3f; // Stronger compensation

                    // Apply velocity (this creates the "staying in water" effect)
                    var velocityOverLifetime = mainWakeParticles.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
                    velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(wakeSpreadSpeed, wakeSpreadSpeed * 0.5f);

                    // Add linear velocity to counteract ship movement
                    // Note: Unity's particle system uses x, y, z properties for linear velocity
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(velocityCompensation.x);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(velocityCompensation.y);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(velocityCompensation.z);
                }
            }

            // Update turbulence particles (lower rate)
            if (turbulenceParticles != null && turbulenceEmission.enabled)
            {
                turbulenceEmission.rateOverTime = targetEmissionRate * 0.3f;

                // Apply world simulation space to turbulence particles too
                var turbulenceMain = turbulenceParticles.main;
                turbulenceMain.simulationSpace = ParticleSystemSimulationSpace.World;

                // Apply same speed compensation to turbulence particles
                Vector3 shipVelocity = (transform.position - lastShipPosition) / Time.deltaTime;
                Vector3 shipForward = transform.up;
                float shipSpeedKnots = shipVelocity.magnitude * 1.944f;
                float speedCompensation = Mathf.Clamp(shipSpeedKnots * 0.1f, 0f, 2f);

                // Turbulence spawns slightly further back
                Vector3 turbulenceOffset = wakeSpawnOffset + Vector3.back * 2f - (shipForward * speedCompensation);
                turbulenceParticles.transform.localPosition = turbulenceOffset;

                // Turbulence particles have more random movement with velocity compensation
                var turbulenceVelocity = turbulenceParticles.velocityOverLifetime;
                turbulenceVelocity.enabled = true;
                turbulenceVelocity.space = ParticleSystemSimulationSpace.World;
                turbulenceVelocity.radial = new ParticleSystem.MinMaxCurve(1f, 3f); // More random spread
                // Add linear velocity compensation for turbulence particles
                Vector3 turbulenceCompensation = -shipVelocity * 0.2f; // Less compensation than main wake
                turbulenceVelocity.x = new ParticleSystem.MinMaxCurve(turbulenceCompensation.x);
                turbulenceVelocity.y = new ParticleSystem.MinMaxCurve(turbulenceCompensation.y);
                turbulenceVelocity.z = new ParticleSystem.MinMaxCurve(turbulenceCompensation.z);
            }
        }

        private void UpdateTrailProperties()
        {
            if (wakeTrail == null) return;

            // Adjust trail width based on wake intensity
            float baseWidth = 2f;
            float targetWidth = baseWidth * (0.5f + currentWakeIntensity * 0.5f);
            wakeTrail.widthMultiplier = targetWidth;

            // Adjust trail time based on ship speed
            var shipStatus = shipController.GetShipStatus();
            float trailTime = Mathf.Lerp(10f, 30f, currentWakeIntensity);
            wakeTrail.time = trailTime;
        }

        private void SetDetailLevel(bool highDetail)
        {
            isHighDetail = highDetail;

            // Reduce particle counts for low detail
            if (mainWakeParticles != null)
            {
                var main = mainWakeParticles.main;
                main.maxParticles = highDetail ? 1000 : 200;
            }

            if (turbulenceParticles != null)
            {
                var main = turbulenceParticles.main;
                main.maxParticles = highDetail ? 500 : 100;
            }

            // Disable trail for very low detail
            if (wakeTrail != null)
            {
                wakeTrail.enabled = highDetail;
            }
        }

        private void OnShipSpeedChanged(float newSpeed)
        {
            // Immediate response to speed changes for better feedback
            if (newSpeed < minimumWakeSpeed && wakeActive)
            {
                SetWakeActive(false);
            }
            else if (newSpeed >= minimumWakeSpeed && !wakeActive && isHighDetail)
            {
                SetWakeActive(true);
            }
        }

        /// <summary>
        /// Get current wake statistics for debugging
        /// </summary>
        public WakeStats GetWakeStats()
        {
            var shipStatus = shipController?.GetShipStatus() ?? new ShipStatus();

            return new WakeStats
            {
                isActive = wakeActive,
                intensity = currentWakeIntensity,
                speed = shipStatus.speed,
                rudderAngle = shipStatus.rudderAngle,
                isHighDetail = isHighDetail,
                particleCount = mainWakeParticles != null ? mainWakeParticles.particleCount : 0
            };
        }

        /// <summary>
        /// Manually trigger wake burst (for special events)
        /// </summary>
        public void TriggerWakeBurst(float burstIntensity = 2f)
        {
            if (!wakeActive) return;

            if (mainWakeParticles != null)
            {
                var burst = new ParticleSystem.Burst(0f, (short)(baseEmissionRate * burstIntensity));
                var emission = mainWakeParticles.emission;
                emission.SetBurst(0, burst);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            SimpleNavalController.OnSpeedChanged -= OnShipSpeedChanged;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw wake spawn position
            Gizmos.color = wakeActive ? Color.cyan : Color.gray;
            Vector3 wakePos = transform.position + transform.TransformDirection(wakeSpawnOffset);
            Gizmos.DrawWireSphere(wakePos, 1f);

            // Draw wake intensity indicator
            if (wakeActive)
            {
                Gizmos.color = Color.Lerp(Color.yellow, Color.red, currentWakeIntensity);
                Gizmos.DrawRay(wakePos, Vector3.up * (currentWakeIntensity * 10f));
            }

            // Draw LOD distance
            Gizmos.color = isHighDetail ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, lodDistance);
        }
    }

    /// <summary>
    /// Wake effect statistics for debugging
    /// </summary>
    [System.Serializable]
    public struct WakeStats
    {
        public bool isActive;
        public float intensity;
        public float speed;
        public float rudderAngle;
        public bool isHighDetail;
        public int particleCount;

        public override string ToString()
        {
            return $"Wake: Active={isActive}, Intensity={intensity:F2}, Speed={speed:F1}kts, Particles={particleCount}";
        }
    }
}