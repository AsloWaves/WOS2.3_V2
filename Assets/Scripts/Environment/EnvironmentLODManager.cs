using UnityEngine;
using System.Collections.Generic;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Manages Level of Detail (LOD) for all environment systems.
    /// Coordinates performance optimization across ocean tiles, waves, and ship effects.
    /// </summary>
    public class EnvironmentLODManager : MonoBehaviour
    {
        [Header("LOD Configuration")]
        [Tooltip("Camera used for distance calculations")]
        [SerializeField] private UnityEngine.Camera lodCamera;

        [Tooltip("Update frequency for LOD calculations")]
        [Range(0.1f, 2f)]
        [SerializeField] private float lodUpdateInterval = 0.5f;

        [Header("Distance Thresholds")]
        [Tooltip("Distance for high detail rendering")]
        [Range(500f, 3000f)]
        [SerializeField] private float highDetailDistance = 1500f;

        [Tooltip("Distance for medium detail rendering")]
        [Range(1000f, 5000f)]
        [SerializeField] private float mediumDetailDistance = 3000f;

        [Tooltip("Distance for low detail rendering")]
        [Range(2000f, 8000f)]
        [SerializeField] private float lowDetailDistance = 5000f;

        [Tooltip("Distance where objects are culled completely")]
        [Range(3000f, 12000f)]
        [SerializeField] private float cullDistance = 8000f;

        [Header("Performance Targets")]
        [Tooltip("Target frame time in milliseconds")]
        [Range(8f, 32f)]
        [SerializeField] private float targetFrameTime = 16.6f; // 60 FPS

        [Tooltip("Enable dynamic LOD adjustment based on performance")]
        [SerializeField] private bool enableDynamicLOD = true;

        [Tooltip("Performance adjustment sensitivity")]
        [Range(0.1f, 2f)]
        [SerializeField] private float performanceSensitivity = 0.8f;

        [Header("System Controls")]
        [Tooltip("Enable ocean tile LOD management")]
        [SerializeField] private bool manageOceanTiles = true;

        [Tooltip("Enable wave effect LOD management")]
        [SerializeField] private bool manageWaveEffects = true;

        [Tooltip("Enable ship wake LOD management")]
        [SerializeField] private bool manageShipWakes = true;

        // Component References
        private OceanChunkManager oceanChunkManager;
        private WaveEffectSpawner waveEffectSpawner;
        private List<ShipWakeController> shipWakeControllers;
        private List<OceanTileController> oceanTileControllers;

        // Performance Monitoring
        private float lastLODUpdate;
        private float[] frameTimeHistory;
        private int frameTimeIndex;
        private float averageFrameTime;
        private float performanceMultiplier = 1f;

        // LOD State
        private LODLevel currentGlobalLOD = LODLevel.High;
        private Vector3 lastCameraPosition;

        private void Awake()
        {
            // Initialize collections
            shipWakeControllers = new List<ShipWakeController>();
            oceanTileControllers = new List<OceanTileController>();

            // Initialize performance monitoring
            frameTimeHistory = new float[30]; // 30 frame rolling average
            frameTimeIndex = 0;

            // Find camera if not assigned
            if (lodCamera == null)
                lodCamera = UnityEngine.Camera.main;

            if (lodCamera != null)
                lastCameraPosition = lodCamera.transform.position;
        }

        private void Start()
        {
            // Find environment systems
            FindEnvironmentSystems();

            DebugManager.Log(DebugCategory.Performance, $"Initialized - Ocean: {manageOceanTiles}, Waves: {manageWaveEffects}, Wakes: {manageShipWakes}", this);
        }

        private void Update()
        {
            // Update performance monitoring
            UpdatePerformanceMonitoring();

            // Update LOD at specified interval
            if (Time.time - lastLODUpdate >= lodUpdateInterval)
            {
                UpdateLODSystems();
                lastLODUpdate = Time.time;
            }
        }

        private void FindEnvironmentSystems()
        {
            // Find ocean chunk manager
            if (manageOceanTiles)
            {
                oceanChunkManager = FindFirstObjectByType<OceanChunkManager>();
                if (oceanChunkManager == null)
                {
                    DebugManager.LogWarning(DebugCategory.Performance, "OceanChunkManager not found", this);
                    manageOceanTiles = false;
                }
            }

            // Find wave effect spawner
            if (manageWaveEffects)
            {
                waveEffectSpawner = FindFirstObjectByType<WaveEffectSpawner>();
                if (waveEffectSpawner == null)
                {
                    DebugManager.LogWarning(DebugCategory.Performance, "WaveEffectSpawner not found", this);
                    manageWaveEffects = false;
                }
            }

            // Find ship wake controllers
            if (manageShipWakes)
            {
                ShipWakeController[] wakeControllers = FindObjectsByType<ShipWakeController>(FindObjectsSortMode.None);
                shipWakeControllers.AddRange(wakeControllers);

                if (shipWakeControllers.Count == 0)
                {
                    DebugManager.LogWarning(DebugCategory.Performance, "No ShipWakeControllers found", this);
                    manageShipWakes = false;
                }
            }

            // Find ocean tile controllers (they spawn dynamically)
            RefreshOceanTileControllers();
        }

        private void RefreshOceanTileControllers()
        {
            if (!manageOceanTiles) return;

            oceanTileControllers.Clear();
            OceanTileController[] tileControllers = FindObjectsByType<OceanTileController>(FindObjectsSortMode.None);
            oceanTileControllers.AddRange(tileControllers);
        }

        private void UpdatePerformanceMonitoring()
        {
            // Record frame time
            float currentFrameTime = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
            frameTimeHistory[frameTimeIndex] = currentFrameTime;
            frameTimeIndex = (frameTimeIndex + 1) % frameTimeHistory.Length;

            // Calculate rolling average
            float total = 0f;
            for (int i = 0; i < frameTimeHistory.Length; i++)
            {
                total += frameTimeHistory[i];
            }
            averageFrameTime = total / frameTimeHistory.Length;

            // Update performance multiplier for dynamic LOD
            if (enableDynamicLOD)
            {
                UpdateDynamicLOD();
            }
        }

        private void UpdateDynamicLOD()
        {
            // Calculate performance pressure
            float performancePressure = averageFrameTime / targetFrameTime;

            // Adjust performance multiplier
            if (performancePressure > 1.2f) // Performance worse than target
            {
                performanceMultiplier = Mathf.Max(0.5f, performanceMultiplier - Time.deltaTime * performanceSensitivity);
            }
            else if (performancePressure < 0.8f) // Performance better than target
            {
                performanceMultiplier = Mathf.Min(1.5f, performanceMultiplier + Time.deltaTime * performanceSensitivity * 0.5f);
            }

            // Update global LOD based on performance
            LODLevel newGlobalLOD = CalculateGlobalLOD();
            if (newGlobalLOD != currentGlobalLOD)
            {
                currentGlobalLOD = newGlobalLOD;
                DebugManager.Log(DebugCategory.Performance, $"Global LOD changed to {currentGlobalLOD} (Performance: {performancePressure:F2})", this);
            }
        }

        private LODLevel CalculateGlobalLOD()
        {
            float adjustedPerformance = averageFrameTime / targetFrameTime / performanceMultiplier;

            if (adjustedPerformance <= 0.8f)
                return LODLevel.High;
            else if (adjustedPerformance <= 1.2f)
                return LODLevel.Medium;
            else if (adjustedPerformance <= 1.8f)
                return LODLevel.Low;
            else
                return LODLevel.VeryLow;
        }

        private void UpdateLODSystems()
        {
            if (lodCamera == null) return;

            Vector3 cameraPosition = lodCamera.transform.position;
            bool cameraMoved = Vector3.Distance(cameraPosition, lastCameraPosition) > 50f;

            // Update ocean tiles LOD
            if (manageOceanTiles && (cameraMoved || Time.time - lastLODUpdate > 2f))
            {
                RefreshOceanTileControllers();
                UpdateOceanTilesLOD(cameraPosition);
            }

            // Update wave effects LOD
            if (manageWaveEffects)
            {
                UpdateWaveEffectsLOD();
            }

            // Update ship wakes LOD
            if (manageShipWakes)
            {
                UpdateShipWakesLOD(cameraPosition);
            }

            lastCameraPosition = cameraPosition;
        }

        private void UpdateOceanTilesLOD(Vector3 cameraPosition)
        {
            foreach (OceanTileController tileController in oceanTileControllers)
            {
                if (tileController == null) continue;

                float distance = Vector3.Distance(tileController.transform.position, cameraPosition);
                LODLevel tileLOD = CalculateLODLevel(distance);

                // Apply performance adjustment
                if (currentGlobalLOD < tileLOD)
                    tileLOD = currentGlobalLOD;

                ApplyTileLOD(tileController, tileLOD, distance);
            }
        }

        private void UpdateWaveEffectsLOD()
        {
            if (waveEffectSpawner == null) return;

            // Adjust wave spawner settings based on performance
            var stats = waveEffectSpawner.GetStats();

            // Reduce wave density for lower LOD
            float lodMultiplier = currentGlobalLOD switch
            {
                LODLevel.High => 1f,
                LODLevel.Medium => 0.75f,
                LODLevel.Low => 0.5f,
                LODLevel.VeryLow => 0.25f,
                _ => 1f
            };

            // Note: This would require exposing density control in WaveEffectSpawner
            // For now, we log the recommended adjustment
            if (currentGlobalLOD != LODLevel.High)
            {
                DebugManager.Log(DebugCategory.Performance, $"Recommend wave density reduction: {lodMultiplier:F2}x", this);
            }
        }

        private void UpdateShipWakesLOD(Vector3 cameraPosition)
        {
            foreach (ShipWakeController wakeController in shipWakeControllers)
            {
                if (wakeController == null) continue;

                float distance = Vector3.Distance(wakeController.transform.position, cameraPosition);
                LODLevel wakeLOD = CalculateLODLevel(distance);

                // Apply performance adjustment
                if (currentGlobalLOD < wakeLOD)
                    wakeLOD = currentGlobalLOD;

                ApplyWakeLOD(wakeController, wakeLOD);
            }
        }

        private LODLevel CalculateLODLevel(float distance)
        {
            float adjustedHighDistance = highDetailDistance * performanceMultiplier;
            float adjustedMediumDistance = mediumDetailDistance * performanceMultiplier;
            float adjustedLowDistance = lowDetailDistance * performanceMultiplier;

            if (distance <= adjustedHighDistance)
                return LODLevel.High;
            else if (distance <= adjustedMediumDistance)
                return LODLevel.Medium;
            else if (distance <= adjustedLowDistance)
                return LODLevel.Low;
            else if (distance <= cullDistance)
                return LODLevel.VeryLow;
            else
                return LODLevel.Culled;
        }

        private void ApplyTileLOD(OceanTileController tileController, LODLevel lodLevel, float distance)
        {
            switch (lodLevel)
            {
                case LODLevel.High:
                    tileController.SetVisibility(true);
                    tileController.SetDetailLevel(true);
                    break;

                case LODLevel.Medium:
                    tileController.SetVisibility(true);
                    tileController.SetDetailLevel(true);
                    break;

                case LODLevel.Low:
                    tileController.SetVisibility(true);
                    tileController.SetDetailLevel(false);
                    break;

                case LODLevel.VeryLow:
                    tileController.SetVisibility(true);
                    tileController.SetDetailLevel(false);
                    break;

                case LODLevel.Culled:
                    tileController.SetVisibility(false);
                    break;
            }
        }

        private void ApplyWakeLOD(ShipWakeController wakeController, LODLevel lodLevel)
        {
            // Note: This would require exposing LOD controls in ShipWakeController
            // For now, we log the recommended LOD level
            var wakeStats = wakeController.GetWakeStats();

            if (lodLevel == LODLevel.VeryLow || lodLevel == LODLevel.Culled)
            {
                DebugManager.Log(DebugCategory.Performance, $"Wake {wakeController.name} should use LOD {lodLevel}", this);
            }
        }

        /// <summary>
        /// Get current LOD manager statistics
        /// </summary>
        public LODManagerStats GetStats()
        {
            return new LODManagerStats
            {
                currentGlobalLOD = currentGlobalLOD,
                averageFrameTime = averageFrameTime,
                targetFrameTime = targetFrameTime,
                performanceMultiplier = performanceMultiplier,
                oceanTileCount = oceanTileControllers.Count,
                shipWakeCount = shipWakeControllers.Count,
                highDetailDistance = highDetailDistance * performanceMultiplier,
                mediumDetailDistance = mediumDetailDistance * performanceMultiplier,
                lowDetailDistance = lowDetailDistance * performanceMultiplier
            };
        }

        /// <summary>
        /// Force LOD update for all systems
        /// </summary>
        public void ForceUpdateLOD()
        {
            RefreshOceanTileControllers();
            UpdateLODSystems();
            DebugManager.Log(DebugCategory.Performance, "Forced LOD update complete", this);
        }

        /// <summary>
        /// Set manual LOD level (disables dynamic LOD)
        /// </summary>
        public void SetManualLOD(LODLevel lodLevel)
        {
            enableDynamicLOD = false;
            currentGlobalLOD = lodLevel;
            performanceMultiplier = 1f;

            DebugManager.Log(DebugCategory.Performance, $"Set manual LOD to {lodLevel}", this);
        }

        /// <summary>
        /// Re-enable dynamic LOD adjustment
        /// </summary>
        public void EnableDynamicLOD()
        {
            enableDynamicLOD = true;
            DebugManager.Log(DebugCategory.Performance, "Dynamic LOD re-enabled", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (lodCamera == null) return;

            Vector3 cameraPos = lodCamera.transform.position;

            // Draw LOD distance rings using wire spheres (flattened)
            Gizmos.color = Color.green;
            DrawWireCircle(new Vector3(cameraPos.x, 0f, cameraPos.z), highDetailDistance * performanceMultiplier);

            Gizmos.color = Color.yellow;
            DrawWireCircle(new Vector3(cameraPos.x, 0f, cameraPos.z), mediumDetailDistance * performanceMultiplier);

            Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Orange color
            DrawWireCircle(new Vector3(cameraPos.x, 0f, cameraPos.z), lowDetailDistance * performanceMultiplier);

            Gizmos.color = Color.red;
            DrawWireCircle(new Vector3(cameraPos.x, 0f, cameraPos.z), cullDistance);

            // Draw performance indicator
            Gizmos.color = Color.Lerp(Color.green, Color.red, averageFrameTime / (targetFrameTime * 2f));
            Gizmos.DrawWireCube(cameraPos + Vector3.up * 50f, Vector3.one * 20f);
        }

        private void DrawWireCircle(Vector3 center, float radius)
        {
            const int segments = 32;
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
    /// Level of Detail enumeration
    /// </summary>
    public enum LODLevel
    {
        High,       // Full detail, all effects
        Medium,     // Reduced effects, full geometry
        Low,        // Basic effects, simplified geometry
        VeryLow,    // Minimal effects, very basic geometry
        Culled      // Not rendered
    }

    /// <summary>
    /// LOD Manager statistics
    /// </summary>
    [System.Serializable]
    public struct LODManagerStats
    {
        public LODLevel currentGlobalLOD;
        public float averageFrameTime;
        public float targetFrameTime;
        public float performanceMultiplier;
        public int oceanTileCount;
        public int shipWakeCount;
        public float highDetailDistance;
        public float mediumDetailDistance;
        public float lowDetailDistance;

        public override string ToString()
        {
            return $"LOD: {currentGlobalLOD}, FrameTime: {averageFrameTime:F1}ms/{targetFrameTime:F1}ms, " +
                   $"Perf: {performanceMultiplier:F2}x, Tiles: {oceanTileCount}";
        }
    }
}