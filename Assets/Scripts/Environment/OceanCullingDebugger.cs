using UnityEngine;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Runtime debugging tool for ocean tile culling systems.
    /// Provides sliders and controls to override all culling behaviors during gameplay.
    /// </summary>
    public class OceanCullingDebugger : MonoBehaviour
    {
        [Header("Master Culling Controls")]
        [Tooltip("Completely disable ALL culling systems")]
        [SerializeField] private bool disableAllCulling = true;

        [Tooltip("Force enable all ocean tile renderers")]
        [SerializeField] private bool forceEnableAllTiles = true;

        [Header("Distance Override Controls")]
        [Tooltip("Override LOD distances with these values")]
        [SerializeField] private bool overrideLODDistances = true;

        [Range(0f, 20000f)]
        [SerializeField] private float forceCullDistance = 10000f;

        [Range(0f, 15000f)]
        [SerializeField] private float forceLowDetailDistance = 8000f;

        [Range(0f, 10000f)]
        [SerializeField] private float forceMediumDetailDistance = 5000f;

        [Range(0f, 5000f)]
        [SerializeField] private float forceHighDetailDistance = 2000f;

        [Header("Grid Control")]
        [Tooltip("Override ocean chunk grid radius")]
        [SerializeField] private bool overrideGridRadius = true;

        [Range(1, 10)]
        [SerializeField] private int forceGridRadius = 5;

        [Range(1, 100)]
        [SerializeField] private int forceTilesPerFrame = 50;

        [Header("Debug Information")]
        [SerializeField] private bool showTileCount = true;
        [SerializeField] private bool logCullingActions = true;

        // Component References
        private OceanChunkManager oceanChunkManager;
        private EnvironmentLODManager environmentLODManager;
        private OceanTileController[] oceanTileControllers;

        // Update tracking
        private float lastUpdateTime;
        private int lastTileCount;

        private void Start()
        {
            // Find components
            oceanChunkManager = FindFirstObjectByType<OceanChunkManager>();
            environmentLODManager = FindFirstObjectByType<EnvironmentLODManager>();

            // Initial override
            ApplyOverrides();

            DebugManager.Log(DebugCategory.Ocean, "OceanCullingDebugger initialized - All culling systems under manual control", this);
        }

        private void Update()
        {
            // Apply overrides every frame if needed
            if (Time.time - lastUpdateTime > 0.5f)
            {
                ApplyOverrides();
                UpdateTileControllers();
                lastUpdateTime = Time.time;
            }
        }

        private void ApplyOverrides()
        {
            // Override OceanChunkManager settings
            if (oceanChunkManager != null && overrideGridRadius)
            {
                // Use reflection to set private fields if needed, or expose public setters
                OverrideOceanChunkManager();
            }

            // Override EnvironmentLODManager settings
            if (environmentLODManager != null && overrideLODDistances)
            {
                OverrideEnvironmentLODManager();
            }

            // Force enable all tiles if requested
            if (forceEnableAllTiles)
            {
                ForceEnableAllTileRenderers();
            }
        }

        private void OverrideOceanChunkManager()
        {
            // Force settings via reflection since fields are private
            var gridRadiusField = typeof(OceanChunkManager).GetField("gridRadius",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tilesPerFrameField = typeof(OceanChunkManager).GetField("tilesPerFrame",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (gridRadiusField != null)
            {
                gridRadiusField.SetValue(oceanChunkManager, forceGridRadius);
                if (logCullingActions)
                {
                    DebugManager.Log(DebugCategory.Ocean, $"Forced gridRadius to {forceGridRadius}", this);
                }
            }

            if (tilesPerFrameField != null)
            {
                tilesPerFrameField.SetValue(oceanChunkManager, forceTilesPerFrame);
                if (logCullingActions)
                {
                    DebugManager.Log(DebugCategory.Ocean, $"Forced tilesPerFrame to {forceTilesPerFrame}", this);
                }
            }
        }

        private void OverrideEnvironmentLODManager()
        {
            // Force LOD manager to use our distances
            var cullDistanceField = typeof(EnvironmentLODManager).GetField("cullDistance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lowDetailField = typeof(EnvironmentLODManager).GetField("lowDetailDistance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var mediumDetailField = typeof(EnvironmentLODManager).GetField("mediumDetailDistance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var highDetailField = typeof(EnvironmentLODManager).GetField("highDetailDistance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var manageOceanTilesField = typeof(EnvironmentLODManager).GetField("manageOceanTiles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (disableAllCulling && manageOceanTilesField != null)
            {
                manageOceanTilesField.SetValue(environmentLODManager, false);
                if (logCullingActions)
                {
                    DebugManager.Log(DebugCategory.Ocean, "Disabled EnvironmentLODManager ocean tile management", this);
                }
            }

            if (cullDistanceField != null) cullDistanceField.SetValue(environmentLODManager, forceCullDistance);
            if (lowDetailField != null) lowDetailField.SetValue(environmentLODManager, forceLowDetailDistance);
            if (mediumDetailField != null) mediumDetailField.SetValue(environmentLODManager, forceMediumDetailDistance);
            if (highDetailField != null) highDetailField.SetValue(environmentLODManager, forceHighDetailDistance);
        }

        private void UpdateTileControllers()
        {
            // Find all active ocean tile controllers
            oceanTileControllers = FindObjectsOfType<OceanTileController>();

            if (showTileCount && oceanTileControllers.Length != lastTileCount)
            {
                DebugManager.Log(DebugCategory.Ocean, $"Active Ocean Tiles: {oceanTileControllers.Length}", this);
                lastTileCount = oceanTileControllers.Length;
            }
        }

        private void ForceEnableAllTileRenderers()
        {
            if (oceanTileControllers == null) return;

            int enabledCount = 0;
            foreach (var tileController in oceanTileControllers)
            {
                if (tileController != null)
                {
                    // Force visibility and detail level
                    tileController.SetVisibility(true);
                    tileController.SetDetailLevel(true);

                    // Also force the renderer directly
                    var renderer = tileController.GetComponent<Renderer>();
                    if (renderer != null && !renderer.enabled)
                    {
                        renderer.enabled = true;
                        enabledCount++;
                    }

                    // Override culling settings via reflection
                    if (disableAllCulling)
                    {
                        var enableCullingField = typeof(OceanTileController).GetField("enableCulling",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (enableCullingField != null)
                        {
                            enableCullingField.SetValue(tileController, false);
                        }
                    }
                }
            }

            if (enabledCount > 0 && logCullingActions)
            {
                DebugManager.Log(DebugCategory.Ocean, $"Force-enabled {enabledCount} tile renderers", this);
            }
        }

        /// <summary>
        /// Force rebuild the entire ocean with current settings
        /// </summary>
        [ContextMenu("Force Rebuild Ocean")]
        public void ForceRebuildOcean()
        {
            if (oceanChunkManager != null)
            {
                ApplyOverrides();

                // Call rebuild method via reflection
                var rebuildMethod = typeof(OceanChunkManager).GetMethod("RebuildOcean",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (rebuildMethod != null)
                {
                    rebuildMethod.Invoke(oceanChunkManager, null);
                    DebugManager.Log(DebugCategory.Ocean, "Forced ocean rebuild with new culling settings", this);
                }
            }
        }

        /// <summary>
        /// Emergency: Enable all renderers in scene
        /// </summary>
        [ContextMenu("Emergency Enable All Renderers")]
        public void EmergencyEnableAllRenderers()
        {
            var allRenderers = FindObjectsOfType<Renderer>();
            int enabledCount = 0;

            foreach (var renderer in allRenderers)
            {
                if (renderer.gameObject.name.Contains("OceanTile") && !renderer.enabled)
                {
                    renderer.enabled = true;
                    enabledCount++;
                }
            }

            DebugManager.LogWarning(DebugCategory.Ocean, $"EMERGENCY: Force-enabled {enabledCount} ocean tile renderers", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (oceanChunkManager == null) return;

            // Draw grid visualization
            Gizmos.color = Color.green;
            Vector3 center = transform.position;
            float tileSize = 1024f; // Default tile size

            // Draw grid bounds
            float gridSize = forceGridRadius * 2 + 1;
            float totalSize = gridSize * tileSize;

            Gizmos.DrawWireCube(center, new Vector3(totalSize, 10f, totalSize));

            // Draw distance circles
            Gizmos.color = Color.yellow;
            DrawCircle(center, forceHighDetailDistance);

            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
            DrawCircle(center, forceMediumDetailDistance);

            Gizmos.color = Color.red;
            DrawCircle(center, forceCullDistance);
        }

        private void DrawCircle(Vector3 center, float radius)
        {
            const int segments = 32;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        private void OnGUI()
        {
            if (!showTileCount) return;

            // Show debug info on screen
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"<size=14><color=white><b>Ocean Culling Debug</b></color></size>");
            GUILayout.Label($"<color=white>Active Tiles: {(oceanTileControllers?.Length ?? 0)}</color>");
            GUILayout.Label($"<color=white>Grid Radius: {forceGridRadius} ({(forceGridRadius * 2 + 1)}x{(forceGridRadius * 2 + 1)})</color>");
            GUILayout.Label($"<color=white>Culling Disabled: {disableAllCulling}</color>");
            GUILayout.Label($"<color=white>Force Enable: {forceEnableAllTiles}</color>");

            if (GUILayout.Button("Force Rebuild Ocean"))
            {
                ForceRebuildOcean();
            }

            if (GUILayout.Button("Emergency Enable All"))
            {
                EmergencyEnableAllRenderers();
            }

            GUILayout.EndArea();
        }
    }
}