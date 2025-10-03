using UnityEngine;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Quick setup script to immediately disable all ocean culling systems.
    /// Add this to any GameObject in your scene for instant culling control.
    /// </summary>
    public class OceanDebugQuickSetup : MonoBehaviour
    {
        [Header("Instant Fixes")]
        [Tooltip("Apply fixes immediately on Start")]
        [SerializeField] private bool applyOnStart = true;

        [Tooltip("Show all tile information")]
        [SerializeField] private bool showTileInfo = true;

        private void Start()
        {
            if (applyOnStart)
            {
                ApplyAllFixes();
            }
        }

        /// <summary>
        /// Apply all known fixes for ocean tile culling issues
        /// </summary>
        [ContextMenu("Apply All Ocean Fixes")]
        public void ApplyAllFixes()
        {
            DebugManager.Log(DebugCategory.Ocean, "=== APPLYING ALL OCEAN CULLING FIXES ===", this);

            // 1. Fix OceanChunkManager
            FixOceanChunkManager();

            // 2. Fix EnvironmentLODManager
            FixEnvironmentLODManager();

            // 3. Fix all OceanTileController instances
            FixAllOceanTileControllers();

            // 4. Emergency enable all renderers
            EmergencyEnableAllOceanRenderers();

            DebugManager.Log(DebugCategory.Ocean, "=== ALL OCEAN FIXES APPLIED ===", this);
        }

        private void FixOceanChunkManager()
        {
            var oceanManager = FindFirstObjectByType<OceanChunkManager>();
            if (oceanManager != null)
            {
                // Use reflection to set private fields
                var gridRadiusField = typeof(OceanChunkManager).GetField("gridRadius",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var tilesPerFrameField = typeof(OceanChunkManager).GetField("tilesPerFrame",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                gridRadiusField?.SetValue(oceanManager, 5);
                tilesPerFrameField?.SetValue(oceanManager, 50);

                DebugManager.Log(DebugCategory.Ocean, "Fixed OceanChunkManager: gridRadius=5, tilesPerFrame=50", this);

                // Trigger rebuild
                oceanManager.ForceRebuildOcean();
            }
        }

        private void FixEnvironmentLODManager()
        {
            var lodManager = FindFirstObjectByType<EnvironmentLODManager>();
            if (lodManager != null)
            {
                // Disable ocean tile management
                var manageOceanTilesField = typeof(EnvironmentLODManager).GetField("manageOceanTiles",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                manageOceanTilesField?.SetValue(lodManager, false);

                DebugManager.Log(DebugCategory.Ocean, "Fixed EnvironmentLODManager: disabled ocean tile management", this);
            }
        }

        private void FixAllOceanTileControllers()
        {
            var tileControllers = FindObjectsOfType<OceanTileController>();
            int fixedCount = 0;

            foreach (var controller in tileControllers)
            {
                if (controller != null)
                {
                    // Disable culling
                    var enableCullingField = typeof(OceanTileController).GetField("enableCulling",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    enableCullingField?.SetValue(controller, false);

                    // Force visibility
                    controller.SetVisibility(true);
                    controller.SetDetailLevel(true);

                    fixedCount++;
                }
            }

            DebugManager.Log(DebugCategory.Ocean, $"Fixed {fixedCount} OceanTileController instances", this);
        }

        private void EmergencyEnableAllOceanRenderers()
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

        private void OnGUI()
        {
            if (!showTileInfo) return;

            var oceanManager = FindFirstObjectByType<OceanChunkManager>();
            if (oceanManager == null) return;

            var stats = oceanManager.GetOceanStats();
            var tileControllers = FindObjectsOfType<OceanTileController>();

            GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 280, 200));
            GUILayout.Label("<size=14><color=yellow><b>Ocean Debug Info</b></color></size>");
            GUILayout.Label($"<color=white>Active Tiles: {stats.activeTileCount}</color>");
            GUILayout.Label($"<color=white>Tile Controllers: {tileControllers.Length}</color>");
            GUILayout.Label($"<color=white>Spawn Queue: {stats.tilesInSpawnQueue}</color>");
            GUILayout.Label($"<color=white>Grid: {stats.gridRadius * 2 + 1}x{stats.gridRadius * 2 + 1}</color>");

            if (GUILayout.Button("Apply All Fixes"))
            {
                ApplyAllFixes();
            }

            GUILayout.EndArea();
        }
    }
}