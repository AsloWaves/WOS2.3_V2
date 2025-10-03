using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using Unity.Mathematics;
using WOS.Debugging;
using WOS.ScriptableObjects;

namespace WOS.Environment
{
    /// <summary>
    /// Manages harbor ambient effects including particle systems, audio, and dynamic lighting.
    /// Optimized for URP 2D rendering pipeline with LOD and performance management.
    /// </summary>
    public class HarborEffects : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Port visual configuration for this harbor")]
        [SerializeField] private PortVisualConfigurationSO visualConfig;

        [Header("Effect Detection")]
        [Tooltip("Radius for detecting ships for ambient effects")]
        [Range(50f, 500f)]
        [SerializeField] private float detectionRadius = 200f;

        [Tooltip("Layer mask for ship detection")]
        [SerializeField] private LayerMask shipLayerMask = 1 << 8; // Assuming Player layer is 8

        [Header("Performance Settings")]
        [Tooltip("Update frequency for effect management")]
        [Range(0.1f, 2f)]
        [SerializeField] private float updateInterval = 0.5f;

        [Tooltip("Maximum distance for effects before culling")]
        [Range(100f, 1000f)]
        [SerializeField] private float maxEffectDistance = 600f;

        [Header("Audio Settings")]
        [Tooltip("Audio source for harbor ambient sounds")]
        [SerializeField] private AudioSource ambientAudioSource;

        [Tooltip("Audio source for interactive harbor sounds")]
        [SerializeField] private AudioSource interactiveAudioSource;

        [Header("Lighting Settings")]
        [Tooltip("Harbor lights that respond to time of day")]
        [SerializeField] private Light2D[] harborLights;

        [Tooltip("Enable dynamic lighting based on time of day")]
        [SerializeField] private bool enableDynamicLighting = true;

        // Core Components
        private UnityEngine.Camera playerCamera;
        private Transform cameraTransform;
        private EnvironmentLODManager lodManager;

        // Effect Management
        private List<ParticleSystem> waterEffects;
        private List<ParticleSystem> atmosphericEffects;
        private List<ParticleSystem> structuralEffects;
        private Transform effectsContainer;

        // Detection and State
        private List<Transform> detectedShips;
        private bool playerInRange = false;
        private bool effectsActive = false;
        private float lastUpdateTime;
        private LODLevel currentLOD = LODLevel.High;

        // Performance Tracking
        private int activeParticleCount;
        private float effectIntensityMultiplier = 1f;

        // Audio Management
        private float targetAmbientVolume = 0f;
        private float currentAmbientVolume = 0f;
        private bool audioInitialized = false;

        // Lighting Management
        private float timeOfDay = 0.5f; // 0 = midnight, 0.5 = noon, 1 = midnight
        private Color[] originalLightColors;
        private float[] originalLightIntensities;

        private void Awake()
        {
            // Initialize collections
            waterEffects = new List<ParticleSystem>();
            atmosphericEffects = new List<ParticleSystem>();
            structuralEffects = new List<ParticleSystem>();
            detectedShips = new List<Transform>();

            // Create effects container
            GameObject container = new GameObject("HarborEffects_Container");
            effectsContainer = container.transform;
            effectsContainer.SetParent(transform);
            effectsContainer.localPosition = Vector3.zero;

            // Find camera
            playerCamera = UnityEngine.Camera.main;
            if (playerCamera != null)
            {
                cameraTransform = playerCamera.transform;
            }

            // Store original lighting values
            if (harborLights != null && harborLights.Length > 0)
            {
                originalLightColors = new Color[harborLights.Length];
                originalLightIntensities = new float[harborLights.Length];

                for (int i = 0; i < harborLights.Length; i++)
                {
                    if (harborLights[i] != null)
                    {
                        originalLightColors[i] = harborLights[i].color;
                        originalLightIntensities[i] = harborLights[i].intensity;
                    }
                }
            }

            DebugManager.Log(DebugCategory.Environment, "HarborEffects initialized", this);
        }

        private void Start()
        {
            // Validate configuration
            if (visualConfig == null)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "No PortVisualConfigurationSO assigned! Effects will be limited.", this);
                return;
            }

            // Find LOD manager
            lodManager = FindFirstObjectByType<EnvironmentLODManager>();
            if (lodManager == null)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "EnvironmentLODManager not found. LOD management disabled.", this);
            }

            // Initialize effects
            InitializeEffectSystems();
            InitializeAudioSystems();
            InitializeLightingSystems();

            DebugManager.Log(DebugCategory.Environment, $"Harbor effects started - Water:{waterEffects.Count}, Atmospheric:{atmosphericEffects.Count}, Structural:{structuralEffects.Count}", this);
        }

        private void Update()
        {
            // Update at specified interval
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateEffectSystems();
                lastUpdateTime = Time.time;
            }

            // Update audio smoothly every frame
            UpdateAudioSystems();

            // Update lighting if enabled
            if (enableDynamicLighting)
            {
                UpdateLightingSystems();
            }
        }

        private void InitializeEffectSystems()
        {
            if (visualConfig == null) return;

            // Initialize water effects (waves, splash, foam)
            CreateWaterEffects();

            // Initialize atmospheric effects (seagulls, wind, mist)
            CreateAtmosphericEffects();

            // Initialize structural effects (smoke from buildings, flags, etc.)
            CreateStructuralEffects();

            // Initially disable all effects
            SetEffectsActive(false);
        }

        private void CreateWaterEffects()
        {
            // Water splash effects around docks
            if (visualConfig.waterSplashPrefab != null)
            {
                for (int i = 0; i < visualConfig.waterEffectCount; i++)
                {
                    Vector3 spawnPos = transform.position + GetRandomWaterPosition();
                    GameObject effect = Instantiate(visualConfig.waterSplashPrefab, spawnPos, Quaternion.identity, effectsContainer);
                    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        waterEffects.Add(ps);
                        ConfigureWaterEffect(ps);
                    }
                }
            }

            // Foam effects near shore
            if (visualConfig.foamEffectPrefab != null)
            {
                for (int i = 0; i < visualConfig.foamEffectCount; i++)
                {
                    Vector3 spawnPos = transform.position + GetRandomShorePosition();
                    GameObject effect = Instantiate(visualConfig.foamEffectPrefab, spawnPos, Quaternion.identity, effectsContainer);
                    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        waterEffects.Add(ps);
                        ConfigureWaterEffect(ps);
                    }
                }
            }
        }

        private void CreateAtmosphericEffects()
        {
            // Mist/fog effects
            if (visualConfig.mistEffectPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 5f;
                GameObject effect = Instantiate(visualConfig.mistEffectPrefab, spawnPos, Quaternion.identity, effectsContainer);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    atmosphericEffects.Add(ps);
                    ConfigureAtmosphericEffect(ps);
                }
            }

            // Seagull effects (if available)
            if (visualConfig.seagullEffectPrefab != null)
            {
                for (int i = 0; i < visualConfig.seagullCount; i++)
                {
                    Vector3 spawnPos = transform.position + GetRandomSkyPosition();
                    GameObject effect = Instantiate(visualConfig.seagullEffectPrefab, spawnPos, Quaternion.identity, effectsContainer);
                    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        atmosphericEffects.Add(ps);
                        ConfigureAtmosphericEffect(ps);
                    }
                }
            }
        }

        private void CreateStructuralEffects()
        {
            // Smoke from chimneys
            if (visualConfig.smokeEffectPrefab != null)
            {
                for (int i = 0; i < visualConfig.smokeSourceCount; i++)
                {
                    Vector3 spawnPos = transform.position + GetRandomBuildingPosition();
                    GameObject effect = Instantiate(visualConfig.smokeEffectPrefab, spawnPos, Quaternion.identity, effectsContainer);
                    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        structuralEffects.Add(ps);
                        ConfigureStructuralEffect(ps);
                    }
                }
            }
        }

        private Vector3 GetRandomWaterPosition()
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(20f, detectionRadius * 0.8f);
            return new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                UnityEngine.Random.Range(-1f, 1f)
            );
        }

        private Vector3 GetRandomShorePosition()
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(detectionRadius * 0.6f, detectionRadius * 0.9f);
            return new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );
        }

        private Vector3 GetRandomSkyPosition()
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(30f, detectionRadius);
            return new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                UnityEngine.Random.Range(10f, 30f)
            );
        }

        private Vector3 GetRandomBuildingPosition()
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(10f, 50f);
            return new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                UnityEngine.Random.Range(8f, 20f)
            );
        }

        private void ConfigureWaterEffect(ParticleSystem ps)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.rateOverTime = visualConfig.waterEffectIntensity;

            // Configure for 2D rendering
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
        }

        private void ConfigureAtmosphericEffect(ParticleSystem ps)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.rateOverTime = visualConfig.atmosphericEffectIntensity;
        }

        private void ConfigureStructuralEffect(ParticleSystem ps)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.rateOverTime = visualConfig.structuralEffectIntensity;
        }

        private void InitializeAudioSystems()
        {
            if (visualConfig == null) return;

            // Setup ambient audio
            if (ambientAudioSource != null && visualConfig.ambientHarborSound != null)
            {
                ambientAudioSource.clip = visualConfig.ambientHarborSound;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0f;
                ambientAudioSource.spatialBlend = 0.7f; // Mostly 3D but some stereo
                ambientAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                ambientAudioSource.maxDistance = detectionRadius;
                ambientAudioSource.Play();
                audioInitialized = true;
            }

            // Setup interactive audio
            if (interactiveAudioSource != null)
            {
                interactiveAudioSource.loop = false;
                interactiveAudioSource.spatialBlend = 1f; // Full 3D
                interactiveAudioSource.rolloffMode = AudioRolloffMode.Linear;
                interactiveAudioSource.maxDistance = detectionRadius * 0.5f;
            }
        }

        private void InitializeLightingSystems()
        {
            if (!enableDynamicLighting || harborLights == null || harborLights.Length == 0)
                return;

            // Set initial lighting state based on current time of day
            UpdateLightingForTimeOfDay();
        }

        private void UpdateEffectSystems()
        {
            if (cameraTransform == null) return;

            // Check distance to camera for LOD
            float distanceToCamera = Vector3.Distance(transform.position, cameraTransform.position);
            bool inRange = distanceToCamera <= maxEffectDistance;

            // Detect ships in range
            DetectShipsInRange();

            // Update LOD level
            UpdateLODLevel(distanceToCamera);

            // Determine if effects should be active
            bool shouldBeActive = inRange && (playerInRange || detectedShips.Count > 0);

            if (shouldBeActive != effectsActive)
            {
                SetEffectsActive(shouldBeActive);
                effectsActive = shouldBeActive;
            }

            // Update effect intensity based on LOD and ship presence
            UpdateEffectIntensity();

            // Update audio target volume
            UpdateAudioTargetVolume();
        }

        private void DetectShipsInRange()
        {
            detectedShips.Clear();
            playerInRange = false;

            Collider2D[] ships = Physics2D.OverlapCircleAll(transform.position, detectionRadius, shipLayerMask);

            foreach (Collider2D ship in ships)
            {
                if (ship != null)
                {
                    detectedShips.Add(ship.transform);

                    // Check if this is the player ship
                    if (ship.CompareTag("Player"))
                    {
                        playerInRange = true;
                    }
                }
            }
        }

        private void UpdateLODLevel(float distanceToCamera)
        {
            LODLevel newLOD;

            if (lodManager != null)
            {
                var stats = lodManager.GetStats();
                newLOD = stats.currentGlobalLOD;
            }
            else
            {
                // Fallback LOD calculation
                if (distanceToCamera <= 150f)
                    newLOD = LODLevel.High;
                else if (distanceToCamera <= 300f)
                    newLOD = LODLevel.Medium;
                else if (distanceToCamera <= 500f)
                    newLOD = LODLevel.Low;
                else
                    newLOD = LODLevel.VeryLow;
            }

            if (newLOD != currentLOD)
            {
                currentLOD = newLOD;
                ApplyLODSettings();
            }
        }

        private void ApplyLODSettings()
        {
            float lodMultiplier = currentLOD switch
            {
                LODLevel.High => 1f,
                LODLevel.Medium => 0.75f,
                LODLevel.Low => 0.5f,
                LODLevel.VeryLow => 0.25f,
                LODLevel.Culled => 0f,
                _ => 1f
            };

            effectIntensityMultiplier = lodMultiplier;

            // Apply to all particle systems
            ApplyIntensityToEffects(waterEffects, lodMultiplier);
            ApplyIntensityToEffects(atmosphericEffects, lodMultiplier);
            ApplyIntensityToEffects(structuralEffects, lodMultiplier);
        }

        private void ApplyIntensityToEffects(List<ParticleSystem> effects, float multiplier)
        {
            foreach (ParticleSystem ps in effects)
            {
                if (ps != null)
                {
                    var emission = ps.emission;
                    var main = ps.main;

                    emission.rateOverTimeMultiplier = multiplier;
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * multiplier);
                }
            }
        }

        private void UpdateEffectIntensity()
        {
            if (visualConfig == null) return;

            // Base intensity on ship proximity and count
            float baseIntensity = playerInRange ? 1f : 0.3f;
            float shipMultiplier = 1f + (detectedShips.Count * 0.1f);
            float targetIntensity = baseIntensity * shipMultiplier * effectIntensityMultiplier;

            // Apply intensity to water effects (most responsive to ships)
            foreach (ParticleSystem ps in waterEffects)
            {
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTimeMultiplier = targetIntensity;
                }
            }

            // Atmospheric effects are less responsive
            float atmosphericIntensity = Mathf.Lerp(0.5f, targetIntensity, 0.3f);
            foreach (ParticleSystem ps in atmosphericEffects)
            {
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTimeMultiplier = atmosphericIntensity;
                }
            }
        }

        private void UpdateAudioTargetVolume()
        {
            if (!audioInitialized || visualConfig == null) return;

            float baseVolume = playerInRange ? visualConfig.maxAmbientVolume : 0f;
            float distanceFactor = 1f;

            if (cameraTransform != null)
            {
                float distance = Vector3.Distance(transform.position, cameraTransform.position);
                distanceFactor = Mathf.Clamp01(1f - (distance / detectionRadius));
            }

            targetAmbientVolume = baseVolume * distanceFactor * effectIntensityMultiplier;
        }

        private void UpdateAudioSystems()
        {
            if (!audioInitialized) return;

            // Smooth audio transitions
            currentAmbientVolume = Mathf.Lerp(currentAmbientVolume, targetAmbientVolume, Time.deltaTime * 2f);

            if (ambientAudioSource != null)
            {
                ambientAudioSource.volume = currentAmbientVolume;
            }
        }

        private void UpdateLightingSystems()
        {
            // Simple time of day simulation - in a full game this would come from a time manager
            timeOfDay = (timeOfDay + Time.deltaTime * visualConfig.timeOfDaySpeed) % 1f;
            UpdateLightingForTimeOfDay();
        }

        private void UpdateLightingForTimeOfDay()
        {
            if (harborLights == null || originalLightColors == null || originalLightIntensities == null)
                return;

            // Calculate lighting factors based on time of day
            float dayFactor = Mathf.Clamp01(1f - Mathf.Abs((timeOfDay - 0.5f) * 2f)); // Peak at noon
            float nightFactor = 1f - dayFactor;

            for (int i = 0; i < harborLights.Length; i++)
            {
                if (harborLights[i] != null && i < originalLightColors.Length && i < originalLightIntensities.Length)
                {
                    // Harbor lights are brighter at night
                    float intensity = originalLightIntensities[i] * (0.3f + nightFactor * 0.7f);
                    Color color = Color.Lerp(visualConfig.dayLightColor, visualConfig.nightLightColor, nightFactor);

                    harborLights[i].intensity = intensity;
                    harborLights[i].color = color;
                }
            }
        }

        private void SetEffectsActive(bool active)
        {
            // Enable/disable all particle systems
            foreach (ParticleSystem ps in waterEffects)
            {
                if (ps != null)
                {
                    if (active)
                        ps.Play();
                    else
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            foreach (ParticleSystem ps in atmosphericEffects)
            {
                if (ps != null)
                {
                    if (active)
                        ps.Play();
                    else
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            foreach (ParticleSystem ps in structuralEffects)
            {
                if (ps != null)
                {
                    if (active)
                        ps.Play();
                    else
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            DebugManager.Log(DebugCategory.Environment, $"Harbor effects {(active ? "activated" : "deactivated")}", this);
        }

        /// <summary>
        /// Play a one-shot interactive sound effect
        /// </summary>
        public void PlayInteractiveSound(AudioClip clip, float volume = 1f)
        {
            if (interactiveAudioSource != null && clip != null)
            {
                interactiveAudioSource.PlayOneShot(clip, volume);
            }
        }

        /// <summary>
        /// Get current harbor effects statistics
        /// </summary>
        public HarborEffectsStats GetStats()
        {
            return new HarborEffectsStats
            {
                effectsActive = effectsActive,
                playerInRange = playerInRange,
                detectedShipCount = detectedShips.Count,
                currentLOD = currentLOD,
                effectIntensity = effectIntensityMultiplier,
                ambientVolume = currentAmbientVolume,
                timeOfDay = timeOfDay
            };
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection radius
            Gizmos.color = Color.cyan;
            DrawWireCircle(transform.position, detectionRadius);

            // Draw max effect distance
            Gizmos.color = Color.red;
            DrawWireCircle(transform.position, maxEffectDistance);

            // Draw detected ships
            if (detectedShips != null && detectedShips.Count > 0)
            {
                Gizmos.color = Color.green;
                foreach (Transform ship in detectedShips)
                {
                    if (ship != null)
                    {
                        Gizmos.DrawLine(transform.position, ship.position);
                        Gizmos.DrawWireSphere(ship.position, 10f);
                    }
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
            // Clean up effects
            if (effectsContainer != null)
            {
                DestroyImmediate(effectsContainer.gameObject);
            }
        }
    }

    /// <summary>
    /// Harbor effects statistics for debugging and monitoring
    /// </summary>
    [System.Serializable]
    public struct HarborEffectsStats
    {
        public bool effectsActive;
        public bool playerInRange;
        public int detectedShipCount;
        public LODLevel currentLOD;
        public float effectIntensity;
        public float ambientVolume;
        public float timeOfDay;

        public override string ToString()
        {
            return $"Effects: {effectsActive}, Player: {playerInRange}, Ships: {detectedShipCount}, " +
                   $"LOD: {currentLOD}, Intensity: {effectIntensity:F2}, Volume: {ambientVolume:F2}";
        }
    }
}