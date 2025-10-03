using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Spawns and manages ambient wave effects across the ocean surface.
    /// Creates scattered wave crests, foam patterns, and surface details for ocean atmosphere.
    /// </summary>
    public class WaveEffectSpawner : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [Tooltip("Prefab for individual wave crest effects")]
        [SerializeField] private GameObject waveCrestPrefab;

        [Tooltip("Prefab for foam patch effects")]
        [SerializeField] private GameObject foamPatchPrefab;

        [Tooltip("Prefab for surface ripple effects")]
        [SerializeField] private GameObject ripplePrefab;

        [Header("Spawning Settings")]
        [Tooltip("Radius around camera to spawn wave effects")]
        [Range(500f, 2000f)]
        [SerializeField] private float spawnRadius = 1000f;

        [Tooltip("Target density of wave effects per 1000 square units")]
        [Range(1f, 20f)]
        [SerializeField] private float waveDensity = 8f;

        [Tooltip("Minimum distance between wave effects")]
        [Range(20f, 100f)]
        [SerializeField] private float minWaveSpacing = 50f;

        [Tooltip("Maximum distance from camera before despawning")]
        [Range(1000f, 3000f)]
        [SerializeField] private float despawnDistance = 1500f;

        [Header("Wave Types")]
        [Tooltip("Probability of spawning wave crests")]
        [Range(0f, 1f)]
        [SerializeField] private float crestProbability = 0.6f;

        [Tooltip("Probability of spawning foam patches")]
        [Range(0f, 1f)]
        [SerializeField] private float foamProbability = 0.3f;

        [Tooltip("Probability of spawning ripples")]
        [Range(0f, 1f)]
        [SerializeField] private float rippleProbability = 0.1f;

        [Header("Wave Animation")]
        [Tooltip("Base wave animation speed")]
        [Range(0.1f, 2f)]
        [SerializeField] private float waveAnimationSpeed = 0.8f;

        [Tooltip("Random speed variation factor")]
        [Range(0f, 0.5f)]
        [SerializeField] private float speedVariation = 0.2f;

        [Tooltip("Wave lifetime in seconds")]
        [Range(5f, 30f)]
        [SerializeField] private float waveLifetime = 15f;

        [Header("Performance")]
        [Tooltip("Maximum number of active wave effects")]
        [Range(50, 500)]
        [SerializeField] private int maxActiveWaves = 200;

        [Tooltip("Waves to spawn/despawn per frame")]
        [Range(1, 10)]
        [SerializeField] private int wavesPerFrame = 3;

        [Tooltip("Update frequency for spawning checks")]
        [Range(0.1f, 1f)]
        [SerializeField] private float updateInterval = 0.5f;

        [Header("Wind Effects")]
        [Tooltip("Wind direction for wave movement")]
        [SerializeField] private Vector2 windDirection = new Vector2(1f, 0.5f);

        [Tooltip("Wind strength affecting wave movement")]
        [Range(0f, 2f)]
        [SerializeField] private float windStrength = 0.5f;

        [Tooltip("Enable dynamic wind variation")]
        [SerializeField] private bool enableWindVariation = true;

        // Core Components
        private UnityEngine.Camera oceanCamera;
        private Transform cameraTransform;

        // Wave Management
        private List<WaveEffect> activeWaves;
        private Queue<WaveEffect> wavePool;
        private Transform waveContainer;

        // Spawning State
        private Vector3 lastCameraPosition;
        private float lastUpdateTime;
        private Unity.Mathematics.Random randomGenerator;

        // Performance Tracking
        private int wavesSpawnedThisFrame;
        private int wavesDespawnedThisFrame;

        private void Awake()
        {
            // Initialize collections
            activeWaves = new List<WaveEffect>();
            wavePool = new Queue<WaveEffect>();
            randomGenerator = new Unity.Mathematics.Random((uint)System.DateTime.Now.Millisecond);

            // Create wave container
            GameObject container = new GameObject("WaveEffects");
            waveContainer = container.transform;
            waveContainer.SetParent(transform);

            // Find camera
            oceanCamera = UnityEngine.Camera.main;
            if (oceanCamera != null)
            {
                cameraTransform = oceanCamera.transform;
                lastCameraPosition = cameraTransform.position;
            }
            else
            {
                DebugManager.LogError(DebugCategory.Ocean, "No camera found! Please assign oceanCamera or ensure Camera.main exists.", this);
            }
        }

        private void Start()
        {
            if (cameraTransform == null) return;

            // Pre-populate wave pool
            PopulateWavePool();

            // Spawn initial waves around camera
            SpawnInitialWaves();

            DebugManager.Log(DebugCategory.Ocean, $"Initialized with {maxActiveWaves} max waves, density: {waveDensity}", this);
        }

        private void Update()
        {
            if (cameraTransform == null) return;

            // Reset frame counters
            wavesSpawnedThisFrame = 0;
            wavesDespawnedThisFrame = 0;

            // Update existing waves
            UpdateActiveWaves();

            // Check spawning periodically
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateWaveSpawning();
                lastUpdateTime = Time.time;
                lastCameraPosition = cameraTransform.position;
            }

            // Update wind effects
            if (enableWindVariation)
            {
                UpdateWindEffects();
            }
        }

        private void PopulateWavePool()
        {
            // Pre-create wave objects for pooling
            int poolSize = Mathf.Min(maxActiveWaves, 100);

            for (int i = 0; i < poolSize; i++)
            {
                WaveEffect wave = CreateWaveObject();
                if (wave != null)
                {
                    wave.gameObject.SetActive(false);
                    wavePool.Enqueue(wave);
                }
            }

            DebugManager.Log(DebugCategory.Ocean, $"Pre-populated wave pool with {wavePool.Count} objects", this);
        }

        private WaveEffect CreateWaveObject()
        {
            // Choose random wave type
            GameObject prefabToSpawn = ChooseWavePrefab();
            if (prefabToSpawn == null) return null;

            // Instantiate wave object
            GameObject waveObj = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity, waveContainer);

            // Add or get WaveEffect component
            WaveEffect waveEffect = waveObj.GetComponent<WaveEffect>();
            if (waveEffect == null)
            {
                waveEffect = waveObj.AddComponent<WaveEffect>();
            }

            return waveEffect;
        }

        private GameObject ChooseWavePrefab()
        {
            float random = randomGenerator.NextFloat();

            if (random <= crestProbability && waveCrestPrefab != null)
                return waveCrestPrefab;
            else if (random <= crestProbability + foamProbability && foamPatchPrefab != null)
                return foamPatchPrefab;
            else if (ripplePrefab != null)
                return ripplePrefab;

            // Fallback to first available prefab
            return waveCrestPrefab ?? foamPatchPrefab ?? ripplePrefab;
        }

        private void SpawnInitialWaves()
        {
            Vector3 cameraPos = cameraTransform.position;
            int targetWaveCount = CalculateTargetWaveCount();

            for (int i = 0; i < targetWaveCount && activeWaves.Count < maxActiveWaves; i++)
            {
                Vector3 spawnPos = GenerateRandomSpawnPosition(cameraPos);
                SpawnWaveAt(spawnPos);
            }
        }

        private void UpdateActiveWaves()
        {
            Vector3 cameraPos = cameraTransform.position;

            // Update all active waves
            for (int i = activeWaves.Count - 1; i >= 0; i--)
            {
                WaveEffect wave = activeWaves[i];
                if (wave == null || !wave.gameObject.activeInHierarchy)
                {
                    activeWaves.RemoveAt(i);
                    continue;
                }

                // Update wave
                wave.UpdateWave(Time.deltaTime, windDirection * windStrength);

                // Check if wave should be despawned
                float distanceToCamera = Vector3.Distance(wave.transform.position, cameraPos);
                bool shouldDespawn = distanceToCamera > despawnDistance ||
                                   wave.GetAge() > waveLifetime ||
                                   wavesDespawnedThisFrame >= wavesPerFrame;

                if (shouldDespawn)
                {
                    DespawnWave(wave);
                    activeWaves.RemoveAt(i);
                    wavesDespawnedThisFrame++;
                }
            }
        }

        private void UpdateWaveSpawning()
        {
            Vector3 cameraPos = cameraTransform.position;
            float cameraMoveDistance = Vector3.Distance(cameraPos, lastCameraPosition);

            // Spawn new waves if camera moved significantly or we need more density
            bool needsNewWaves = cameraMoveDistance > spawnRadius * 0.3f ||
                               activeWaves.Count < CalculateTargetWaveCount() * 0.8f;

            if (needsNewWaves && wavesSpawnedThisFrame < wavesPerFrame)
            {
                SpawnWavesAroundCamera(cameraPos);
            }
        }

        private void SpawnWavesAroundCamera(Vector3 cameraPosition)
        {
            int wavesToSpawn = Mathf.Min(wavesPerFrame - wavesSpawnedThisFrame,
                                       maxActiveWaves - activeWaves.Count);

            for (int i = 0; i < wavesToSpawn; i++)
            {
                Vector3 spawnPos = GenerateRandomSpawnPosition(cameraPosition);

                // Check minimum spacing
                if (IsValidSpawnPosition(spawnPos))
                {
                    SpawnWaveAt(spawnPos);
                    wavesSpawnedThisFrame++;
                }
            }
        }

        private Vector3 GenerateRandomSpawnPosition(Vector3 center)
        {
            // Generate random position within spawn radius for 2D (XY plane)
            float angle = randomGenerator.NextFloat() * 2f * math.PI;
            float distance = randomGenerator.NextFloat(spawnRadius * 0.3f, spawnRadius);

            Vector3 offset = new Vector3(
                math.cos(angle) * distance,
                math.sin(angle) * distance,  // 2D uses Y axis
                0.5f  // 2D wave effects at Z=0.5 (behind ship, in front of ocean)
            );

            return center + offset;
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            // Check minimum spacing from existing waves
            foreach (WaveEffect existingWave in activeWaves)
            {
                if (existingWave != null)
                {
                    float distance = Vector3.Distance(position, existingWave.transform.position);
                    if (distance < minWaveSpacing)
                        return false;
                }
            }

            return true;
        }

        private void SpawnWaveAt(Vector3 position)
        {
            WaveEffect wave = GetPooledWave();
            if (wave == null) return;

            // Configure wave
            wave.transform.position = position;
            wave.transform.rotation = Quaternion.Euler(0f, 0f, randomGenerator.NextFloat() * 360f);  // 2D rotation around Z axis
            wave.gameObject.SetActive(true);

            // Initialize wave properties
            float animSpeed = waveAnimationSpeed * (1f + randomGenerator.NextFloat(-speedVariation, speedVariation));
            wave.Initialize(animSpeed, waveLifetime);

            // Add to active list
            activeWaves.Add(wave);
        }

        private WaveEffect GetPooledWave()
        {
            // Try to get from pool first
            if (wavePool.Count > 0)
            {
                return wavePool.Dequeue();
            }

            // Create new if pool is empty
            return CreateWaveObject();
        }

        private void DespawnWave(WaveEffect wave)
        {
            if (wave == null) return;

            // Reset wave
            wave.Reset();
            wave.gameObject.SetActive(false);

            // Return to pool
            wavePool.Enqueue(wave);
        }

        private int CalculateTargetWaveCount()
        {
            // Calculate based on spawn area and density
            float spawnArea = math.PI * spawnRadius * spawnRadius;
            int targetCount = Mathf.RoundToInt((spawnArea / 1000000f) * waveDensity);
            return Mathf.Min(targetCount, maxActiveWaves);
        }

        private void UpdateWindEffects()
        {
            // Subtle wind direction variation
            float windVariation = math.sin(Time.time * 0.1f) * 0.2f;
            windDirection = windDirection.normalized;

            // Rotate wind direction slightly
            float currentAngle = math.atan2(windDirection.y, windDirection.x);
            float newAngle = currentAngle + windVariation * Time.deltaTime;
            windDirection = new Vector2(math.cos(newAngle), math.sin(newAngle));
        }

        /// <summary>
        /// Get current wave spawner statistics
        /// </summary>
        public WaveSpawnerStats GetStats()
        {
            return new WaveSpawnerStats
            {
                activeWaveCount = activeWaves.Count,
                pooledWaveCount = wavePool.Count,
                targetWaveCount = CalculateTargetWaveCount(),
                spawnRadius = spawnRadius,
                windDirection = windDirection,
                windStrength = windStrength
            };
        }

        /// <summary>
        /// Force respawn all waves around current camera position
        /// </summary>
        public void RespawnAllWaves()
        {
            // Clear all active waves
            foreach (WaveEffect wave in activeWaves)
            {
                if (wave != null)
                    DespawnWave(wave);
            }
            activeWaves.Clear();

            // Spawn fresh waves
            if (cameraTransform != null)
            {
                SpawnInitialWaves();
            }

            DebugManager.Log(DebugCategory.Ocean, "Respawned all wave effects", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (cameraTransform == null) return;

            Vector3 cameraPos = cameraTransform.position;

            // Draw spawn radius (2D - XY plane)
            Gizmos.color = Color.cyan;
            DrawWireCircle(new Vector3(cameraPos.x, cameraPos.y, 0f), spawnRadius);

            // Draw despawn radius (2D - XY plane)
            Gizmos.color = Color.red;
            DrawWireCircle(new Vector3(cameraPos.x, cameraPos.y, 0f), despawnDistance);

            // Draw wind direction (2D - XY plane)
            Gizmos.color = Color.yellow;
            Vector3 windVector = new Vector3(windDirection.x, windDirection.y, 0f) * windStrength * 100f;
            Gizmos.DrawRay(cameraPos, windVector);

            // Draw active waves
            Gizmos.color = Color.white;
            foreach (WaveEffect wave in activeWaves)
            {
                if (wave != null)
                {
                    Gizmos.DrawWireSphere(wave.transform.position, 5f);
                }
            }
        }

        private void DrawWireCircle(Vector3 center, float radius)
        {
            const int segments = 32;
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        private void OnDestroy()
        {
            // Clean up all waves
            if (activeWaves != null)
            {
                foreach (WaveEffect wave in activeWaves)
                {
                    if (wave != null && wave.gameObject != null)
                        DestroyImmediate(wave.gameObject);
                }
                activeWaves.Clear();
            }
        }
    }

    /// <summary>
    /// Wave spawner statistics for debugging
    /// </summary>
    [System.Serializable]
    public struct WaveSpawnerStats
    {
        public int activeWaveCount;
        public int pooledWaveCount;
        public int targetWaveCount;
        public float spawnRadius;
        public Vector2 windDirection;
        public float windStrength;

        public override string ToString()
        {
            return $"Waves: {activeWaveCount}/{targetWaveCount} active, {pooledWaveCount} pooled, Wind: {windDirection}";
        }
    }
}