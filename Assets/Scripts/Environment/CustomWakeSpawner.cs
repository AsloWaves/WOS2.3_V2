using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using WOS.Player;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Custom wake particle spawner with ship-specific control over size, density, shape, and lifetime.
    /// Replaces Unity's particle system for more realistic naval wake physics.
    /// </summary>
    public class CustomWakeSpawner : MonoBehaviour
    {
        [Header("Ship Configuration")]
        [Tooltip("Reference to the ship controller")]
        [SerializeField] private SimpleNavalController shipController;

        [Tooltip("Wake spawn offset relative to ship center")]
        [SerializeField] private Vector3 wakeSpawnOffset = new Vector3(0f, -2f, 0f);

        [Header("Wake Particle Settings")]
        [Tooltip("Base wake particle configuration")]
        [SerializeField] private WakeParticleSettings wakeSettings = WakeParticleSettings.Default();

        [Tooltip("Prefab for wake particles (should have CustomWakeParticle component)")]
        [SerializeField] private GameObject wakeParticlePrefab;

        [Header("Spawning Control")]
        [Tooltip("Base particles per second when moving")]
        [Range(1f, 50f)]
        [SerializeField] private float baseSpawnRate = 10f;

        [Tooltip("Maximum particles active at once")]
        [Range(50, 1000)]
        [SerializeField] private int maxActiveParticles = 300;

        [Tooltip("Minimum ship speed to start spawning (knots)")]
        [Range(0f, 5f)]
        [SerializeField] private float minimumSpeedThreshold = 0.5f;

        [Header("Ship-Specific Wake Properties")]
        [Tooltip("Wake density based on ship displacement")]
        [SerializeField] private AnimationCurve densityByDisplacement = AnimationCurve.Linear(1000f, 0.5f, 50000f, 2f);

        [Tooltip("Wake size based on ship length")]
        [SerializeField] private AnimationCurve sizeByLength = AnimationCurve.Linear(50f, 0.5f, 300f, 3f);

        [Tooltip("Wake lifetime based on ship beam (width)")]
        [SerializeField] private AnimationCurve lifetimeByBeam = AnimationCurve.Linear(10f, 0.8f, 50f, 1.5f);

        [Header("Wake Physics")]
        [Tooltip("Spread angle of wake particles (degrees)")]
        [Range(15f, 90f)]
        [SerializeField] private float wakeSpreadAngle = 45f;

        [Tooltip("Initial particle velocity relative to ship speed")]
        [Range(0.1f, 1f)]
        [SerializeField] private float velocityInheritance = 0.3f;

        [Tooltip("Random velocity variation")]
        [Range(0f, 2f)]
        [SerializeField] private float velocityVariation = 0.5f;

        [Header("Performance")]
        [Tooltip("Update frequency for spawning (lower = better performance)")]
        [Range(0.02f, 0.2f)]
        [SerializeField] private float updateInterval = 0.05f;

        [Tooltip("Distance from camera before culling particles")]
        [Range(500f, 3000f)]
        [SerializeField] private float cullDistance = 1500f;

        // Runtime State
        private List<CustomWakeParticle> activeParticles;
        private Queue<GameObject> particlePool;
        private Transform particleContainer;
        private Vector3 lastShipPosition;
        private float lastSpawnTime;
        private float accumulatedSpawnDebt;

        // Ship Properties Cache
        private float shipDisplacement;
        private float shipLength;
        private float shipBeam;
        private bool shipPropertiesCached = false;

        // Performance Tracking
        private int particlesSpawnedThisFrame;
        private int particlesCulledThisFrame;

        private void Awake()
        {
            // Initialize collections
            activeParticles = new List<CustomWakeParticle>();
            particlePool = new Queue<GameObject>();

            // Create container for organization
            GameObject container = new GameObject("CustomWakeParticles");
            particleContainer = container.transform;
            particleContainer.SetParent(transform);

            // Find ship controller if not assigned
            if (shipController == null)
                shipController = GetComponentInParent<SimpleNavalController>();

            // Validate prefab
            if (wakeParticlePrefab != null && wakeParticlePrefab.GetComponent<CustomWakeParticle>() == null)
            {
                DebugManager.LogError(DebugCategory.Ocean, "Wake particle prefab must have CustomWakeParticle component!", this);
            }
        }

        private void Start()
        {
            // Cache ship position
            lastShipPosition = transform.position;

            // Pre-populate particle pool
            PopulateParticlePool();

            DebugManager.Log(DebugCategory.Ocean, $"Initialized with max {maxActiveParticles} particles, spawn rate: {baseSpawnRate}/sec", this);
        }

        private void Update()
        {
            // Performance reset
            particlesSpawnedThisFrame = 0;
            particlesCulledThisFrame = 0;

            // Update particles and check for cleanup
            UpdateActiveParticles();

            // Check if it's time to spawn new particles
            if (Time.time - lastSpawnTime >= updateInterval)
            {
                UpdateWakeSpawning();
                lastSpawnTime = Time.time;
            }

            // Update position tracking
            lastShipPosition = transform.position;
        }

        private void PopulateParticlePool()
        {
            if (wakeParticlePrefab == null) return;

            // Pre-create pool of particles
            int poolSize = Mathf.Min(maxActiveParticles / 2, 100);

            for (int i = 0; i < poolSize; i++)
            {
                GameObject pooledParticle = Instantiate(wakeParticlePrefab, particleContainer);
                pooledParticle.SetActive(false);
                particlePool.Enqueue(pooledParticle);
            }

            DebugManager.Log(DebugCategory.Performance, $"Pre-populated particle pool with {poolSize} objects", this);
        }

        private void UpdateActiveParticles()
        {
            // Update and clean up active particles
            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                CustomWakeParticle particle = activeParticles[i];

                if (particle == null || !particle.gameObject.activeInHierarchy)
                {
                    // Remove destroyed particles
                    activeParticles.RemoveAt(i);
                    continue;
                }

                // Check if particle should be culled for performance
                if (ShouldCullParticle(particle))
                {
                    ReturnParticleToPool(particle);
                    activeParticles.RemoveAt(i);
                    particlesCulledThisFrame++;
                }
            }
        }

        private void UpdateWakeSpawning()
        {
            if (shipController == null) return;

            // Get current ship status
            var shipStatus = shipController.GetShipStatus();
            float currentSpeed = shipStatus.speed; // In knots

            // Don't spawn particles if ship is barely moving
            if (currentSpeed < minimumSpeedThreshold) return;

            // Cache ship properties for performance
            CacheShipProperties();

            // Calculate ship-specific wake properties
            WakeParticleSettings currentSettings = CalculateShipSpecificSettings(currentSpeed);

            // Calculate spawn rate based on ship properties and speed
            float speedMultiplier = Mathf.Clamp01(currentSpeed / 20f); // Normalize to 20 knots
            float spawnRate = baseSpawnRate * currentSettings.densityMultiplier * speedMultiplier;

            // Calculate how many particles to spawn this frame
            float deltaTime = Time.time - lastSpawnTime;
            accumulatedSpawnDebt += spawnRate * deltaTime;

            int particlesToSpawn = Mathf.FloorToInt(accumulatedSpawnDebt);
            accumulatedSpawnDebt -= particlesToSpawn;

            // Limit spawning to maintain performance
            particlesToSpawn = Mathf.Min(particlesToSpawn, 5); // Max 5 per update
            particlesToSpawn = Mathf.Min(particlesToSpawn, maxActiveParticles - activeParticles.Count);

            // Spawn particles
            for (int i = 0; i < particlesToSpawn; i++)
            {
                SpawnWakeParticle(currentSettings, currentSpeed);
                particlesSpawnedThisFrame++;
            }
        }

        private void CacheShipProperties()
        {
            if (shipPropertiesCached || shipController == null) return;

            var shipConfig = shipController.GetShipConfiguration();
            if (shipConfig == null) return;

            shipDisplacement = shipConfig.displacement;
            shipLength = shipConfig.length;
            shipBeam = shipConfig.beam;
            shipPropertiesCached = true;

            DebugManager.Log(DebugCategory.Ship, $"Cached ship properties: {shipDisplacement}t, {shipLength}m x {shipBeam}m", this);
        }

        private WakeParticleSettings CalculateShipSpecificSettings(float currentSpeed)
        {
            WakeParticleSettings settings = wakeSettings;

            // Apply ship-specific modifiers based on ship characteristics
            if (shipPropertiesCached)
            {
                settings.densityMultiplier = densityByDisplacement.Evaluate(shipDisplacement);
                settings.sizeMultiplier = sizeByLength.Evaluate(shipLength);
                settings.lifetimeMultiplier = lifetimeByBeam.Evaluate(shipBeam);

                // Speed affects turbulence and drag
                settings.turbulenceStrength = wakeSettings.turbulenceStrength * (1f + currentSpeed * 0.05f);
                settings.dragCoefficient = wakeSettings.dragCoefficient * (1f + currentSpeed * 0.02f);
            }

            return settings;
        }

        private void SpawnWakeParticle(WakeParticleSettings settings, float shipSpeed)
        {
            // Calculate spawn position with speed compensation
            Vector3 shipVelocity = (transform.position - lastShipPosition) / Time.deltaTime;
            Vector3 shipForward = transform.up; // 2D ship forward direction

            // Speed compensation to reduce gap at high speeds
            float speedCompensation = Mathf.Clamp(shipSpeed * 0.08f, 0f, 1.5f);
            Vector3 compensatedOffset = wakeSpawnOffset - (shipForward * speedCompensation);

            // Add random spread within wake angle
            float randomAngle = UnityEngine.Random.Range(-wakeSpreadAngle * 0.5f, wakeSpreadAngle * 0.5f);
            Vector3 spreadDirection = Quaternion.Euler(0f, 0f, randomAngle) * Vector3.right;

            Vector3 spawnPosition = transform.position + transform.TransformDirection(compensatedOffset);
            spawnPosition += transform.TransformDirection(spreadDirection) * UnityEngine.Random.Range(0f, 2f);

            // Calculate initial particle velocity
            Vector3 wakeDirection = (-shipForward + spreadDirection).normalized;
            Vector3 particleVelocity = shipVelocity * velocityInheritance;
            particleVelocity += wakeDirection * UnityEngine.Random.Range(0.5f, 2f);

            // Add velocity variation
            particleVelocity += new Vector3(
                UnityEngine.Random.Range(-velocityVariation, velocityVariation),
                UnityEngine.Random.Range(-velocityVariation, velocityVariation),
                UnityEngine.Random.Range(-velocityVariation * 0.5f, velocityVariation * 0.5f)
            );

            // Get particle from pool or create new one
            GameObject particleObj = GetPooledParticle();
            if (particleObj == null) return;

            // Initialize particle
            CustomWakeParticle particle = particleObj.GetComponent<CustomWakeParticle>();
            if (particle != null)
            {
                particle.Initialize(settings, particleVelocity, spawnPosition);
                activeParticles.Add(particle);
            }
        }

        private GameObject GetPooledParticle()
        {
            // Try to get from pool first
            if (particlePool.Count > 0)
            {
                GameObject pooledParticle = particlePool.Dequeue();
                pooledParticle.SetActive(true);
                return pooledParticle;
            }

            // Create new if pool is empty and under limit
            if (activeParticles.Count < maxActiveParticles && wakeParticlePrefab != null)
            {
                return Instantiate(wakeParticlePrefab, particleContainer);
            }

            return null;
        }

        private void ReturnParticleToPool(CustomWakeParticle particle)
        {
            if (particle?.gameObject != null)
            {
                particle.gameObject.SetActive(false);
                particlePool.Enqueue(particle.gameObject);
            }
        }

        private bool ShouldCullParticle(CustomWakeParticle particle)
        {
            if (particle == null) return true;

            // Cull based on distance from ship
            float distanceFromShip = Vector3.Distance(particle.transform.position, transform.position);
            return distanceFromShip > cullDistance;
        }

        /// <summary>
        /// Get current wake spawner statistics
        /// </summary>
        public CustomWakeStats GetStats()
        {
            return new CustomWakeStats
            {
                activeParticleCount = activeParticles.Count,
                pooledParticleCount = particlePool.Count,
                particlesSpawnedThisFrame = particlesSpawnedThisFrame,
                particlesCulledThisFrame = particlesCulledThisFrame,
                currentSpawnRate = baseSpawnRate,
                shipDisplacement = shipDisplacement,
                shipLength = shipLength,
                shipBeam = shipBeam
            };
        }

        /// <summary>
        /// Force clear all active particles
        /// </summary>
        public void ClearAllParticles()
        {
            foreach (CustomWakeParticle particle in activeParticles)
            {
                if (particle != null)
                    ReturnParticleToPool(particle);
            }
            activeParticles.Clear();

            DebugManager.Log(DebugCategory.Ocean, "Cleared all active particles", this);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw wake spawn area
            Gizmos.color = Color.cyan;
            Vector3 spawnPos = transform.position + transform.TransformDirection(wakeSpawnOffset);
            Gizmos.DrawWireSphere(spawnPos, 1f);

            // Draw wake spread angle
            Gizmos.color = Color.yellow;
            Vector3 forward = transform.up;
            float halfAngle = wakeSpreadAngle * 0.5f;

            Vector3 leftBound = Quaternion.Euler(0f, 0f, halfAngle) * forward * 5f;
            Vector3 rightBound = Quaternion.Euler(0f, 0f, -halfAngle) * forward * 5f;

            Gizmos.DrawRay(spawnPos, transform.TransformDirection(leftBound));
            Gizmos.DrawRay(spawnPos, transform.TransformDirection(rightBound));

            // Draw cull distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, cullDistance);
        }

        private void OnDestroy()
        {
            // Clean up all particles
            ClearAllParticles();
        }
    }

    /// <summary>
    /// Statistics for custom wake spawner
    /// </summary>
    [System.Serializable]
    public struct CustomWakeStats
    {
        public int activeParticleCount;
        public int pooledParticleCount;
        public int particlesSpawnedThisFrame;
        public int particlesCulledThisFrame;
        public float currentSpawnRate;
        public float shipDisplacement;
        public float shipLength;
        public float shipBeam;

        public override string ToString()
        {
            return $"CustomWake: {activeParticleCount} active, {pooledParticleCount} pooled, Rate: {currentSpawnRate:F1}/s";
        }
    }
}