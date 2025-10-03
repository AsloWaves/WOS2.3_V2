using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using WOS.Debugging;
using WOS.ScriptableObjects;

namespace WOS.Environment
{
    /// <summary>
    /// Manages infinite ocean background using chunk-based tile system.
    /// Creates seamless ocean environment that spawns/despawns around camera.
    /// </summary>
    public class OceanChunkManager : MonoBehaviour
    {
        [Header("Ocean Configuration")]
        [Tooltip("Ocean biome configuration with depth-based spawning")]
        [SerializeField] private OceanBiomeConfigurationSO biomeConfig;

        [Tooltip("Default ocean tile prefab (used when biome config doesn't specify)")]
        [SerializeField] private GameObject defaultOceanTilePrefab;

        [Tooltip("Size of each ocean tile in Unity units")]
        [SerializeField] private float tileSize = 1024f;

        [Tooltip("Grid radius around camera (3 = 7x7 grid, 4 = 9x9 grid, 5 = 11x11 grid)")]
        [Range(1, 7)]
        [SerializeField] private int gridRadius = 4;

        [Tooltip("Distance from camera edge before spawning new tiles")]
        [SerializeField] private float spawnDistance = 512f;

        [Tooltip("Distance from camera before despawning tiles")]
        [SerializeField] private float despawnDistance = 2048f;

        [Header("Legacy Ocean Variations (Deprecated)")]
        [Tooltip("Legacy materials - use biome config instead")]
        [SerializeField] private Material[] legacyOceanMaterials;

        [Tooltip("Enable depth-based tile variation")]
        [SerializeField] private bool enableDepthVariations = true;

        [Tooltip("Seed for deterministic tile placement")]
        [SerializeField] private int randomSeed = 12345;

        [Header("Debug Visualization")]
        [Tooltip("Show depth values in console for debugging")]
        [SerializeField] private bool debugDepthValues = false;

        [Tooltip("Show biome regions with color coding")]
        [SerializeField] private bool visualizeBiomes = false;

        [Header("Performance")]
        [Tooltip("Maximum tiles to process per frame")]
        [Range(1, 50)]
        [SerializeField] private int tilesPerFrame = 25;

        [Header("Runtime Culling Controls")]
        [Tooltip("Runtime override: Disable all culling systems")]
        [SerializeField] private bool runtimeDisableCulling = false;

        [Tooltip("Runtime override: Force enable all tile renderers")]
        [SerializeField] private bool runtimeForceEnableRenderers = false;

        [Tooltip("Runtime override: Grid radius")]
        [Range(1, 10)]
        [SerializeField] private int runtimeGridRadius = 5;

        [Tooltip("Show runtime debug GUI")]
        [SerializeField] private bool showRuntimeGUI = true;

        [Header("Color Blending")]
        [Tooltip("Enable smooth color blending between neighboring tiles")]
        [SerializeField] private bool enableColorBlending = true;

        [Tooltip("Blend strength for color transitions")]
        [Range(0f, 1f)]
        [SerializeField] private float colorBlendStrength = 0.3f;

        [Tooltip("Update frequency in seconds")]
        [Range(0.1f, 2f)]
        [SerializeField] private float updateInterval = 0.2f;

        // Core Components
        private UnityEngine.Camera oceanCamera;
        private Transform cameraTransform;

        // Tile Management
        private Dictionary<Vector2Int, GameObject> activeTiles;
        private Queue<Vector2Int> tilesToSpawn;
        private Queue<Vector2Int> tilesToDespawn;
        private Transform tilesContainer;

        // State Tracking
        private Vector2Int lastCameraChunk;
        private float lastUpdateTime;
        private Unity.Mathematics.Random randomGenerator;

        // Performance Monitoring
        private int tilesSpawnedThisFrame;
        private int tilesDespawnedThisFrame;

        private void Awake()
        {
            // Validate configuration
            ValidateConfiguration();

            // Initialize systems
            activeTiles = new Dictionary<Vector2Int, GameObject>();
            tilesToSpawn = new Queue<Vector2Int>();
            tilesToDespawn = new Queue<Vector2Int>();
            randomGenerator = new Unity.Mathematics.Random((uint)randomSeed);

            // Create container for organization
            GameObject container = new GameObject("OceanTiles");
            tilesContainer = container.transform;
            tilesContainer.SetParent(transform);

            // Find camera
            if (oceanCamera == null)
                oceanCamera = UnityEngine.Camera.main;

            if (oceanCamera != null)
                cameraTransform = oceanCamera.transform;
            else
                DebugManager.LogError(DebugCategory.Ocean, "No camera found! Please assign oceanCamera or ensure Camera.main exists.", this);
        }

        private void Start()
        {
            if (cameraTransform == null) return;

            // Initialize ocean around starting position
            Vector2Int startChunk = GetChunkCoordinates(cameraTransform.position);
            lastCameraChunk = startChunk;

            // Spawn initial grid of tiles
            SpawnInitialTiles(startChunk);

            DebugManager.Log(DebugCategory.Ocean, $"Initialized with {gridRadius * 2 + 1}x{gridRadius * 2 + 1} grid, tile size: {tileSize}", this);
        }

        private void Update()
        {
            if (cameraTransform == null) return;

            // Reset frame counters
            tilesSpawnedThisFrame = 0;
            tilesDespawnedThisFrame = 0;

            // Check if enough time has passed for update
            if (Time.time - lastUpdateTime < updateInterval)
                return;

            lastUpdateTime = Time.time;

            // Check if camera moved to new chunk
            Vector2Int currentChunk = GetChunkCoordinates(cameraTransform.position);
            if (currentChunk != lastCameraChunk)
            {
                UpdateOceanChunks(currentChunk);
                lastCameraChunk = currentChunk;
            }

            // Process queued tile operations
            ProcessTileQueues();

            // Apply runtime culling overrides
            ApplyRuntimeCullingOverrides();
        }

        private Vector2Int GetChunkCoordinates(Vector3 worldPosition)
        {
            int chunkX = Mathf.FloorToInt(worldPosition.x / tileSize);
            int chunkY = Mathf.FloorToInt(worldPosition.y / tileSize); // 2D uses Y axis
            return new Vector2Int(chunkX, chunkY);
        }

        private Vector3 GetChunkWorldPosition(Vector2Int chunkCoord)
        {
            float worldX = chunkCoord.x * tileSize + (tileSize * 0.5f);
            float worldY = chunkCoord.y * tileSize + (tileSize * 0.5f);
            return new Vector3(worldX, worldY, 1f); // 2D positioning - Y axis, Z=1 (behind ship)
        }

        private void SpawnInitialTiles(Vector2Int centerChunk)
        {
            // Queue all initial tiles for performance-controlled spawning
            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int z = -gridRadius; z <= gridRadius; z++)
                {
                    Vector2Int chunkCoord = centerChunk + new Vector2Int(x, z);
                    if (!activeTiles.ContainsKey(chunkCoord))
                    {
                        tilesToSpawn.Enqueue(chunkCoord);
                    }
                }
            }

            DebugManager.Log(DebugCategory.Ocean, $"Queued {tilesToSpawn.Count} tiles for initial spawn", this);
        }

        private void UpdateOceanChunks(Vector2Int newCenterChunk)
        {
            // Clear queues
            tilesToSpawn.Clear();
            tilesToDespawn.Clear();

            // Find tiles that should exist around new center
            HashSet<Vector2Int> requiredTiles = new HashSet<Vector2Int>();
            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int z = -gridRadius; z <= gridRadius; z++)
                {
                    Vector2Int chunkCoord = newCenterChunk + new Vector2Int(x, z);
                    requiredTiles.Add(chunkCoord);
                }
            }

            // Queue tiles for spawning if they don't exist
            foreach (Vector2Int requiredChunk in requiredTiles)
            {
                if (!activeTiles.ContainsKey(requiredChunk))
                {
                    tilesToSpawn.Enqueue(requiredChunk);
                }
            }

            // Queue tiles for despawning if they're too far
            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var kvp in activeTiles)
            {
                Vector2Int chunkCoord = kvp.Key;
                if (!requiredTiles.Contains(chunkCoord))
                {
                    float distance = Vector2.Distance(new Vector2(chunkCoord.x, chunkCoord.y),
                                                    new Vector2(newCenterChunk.x, newCenterChunk.y));
                    if (distance > gridRadius + 1)
                    {
                        tilesToDespawn.Enqueue(chunkCoord);
                        chunksToRemove.Add(chunkCoord);
                    }
                }
            }
        }

        private void ProcessTileQueues()
        {
            // Process spawning queue
            while (tilesToSpawn.Count > 0 && tilesSpawnedThisFrame < tilesPerFrame)
            {
                Vector2Int chunkCoord = tilesToSpawn.Dequeue();
                SpawnTile(chunkCoord);
                tilesSpawnedThisFrame++;
            }

            // Process despawning queue
            while (tilesToDespawn.Count > 0 && tilesDespawnedThisFrame < tilesPerFrame)
            {
                Vector2Int chunkCoord = tilesToDespawn.Dequeue();
                DespawnTile(chunkCoord);
                tilesDespawnedThisFrame++;
            }
        }

        private void SpawnTile(Vector2Int chunkCoord)
        {
            // Check if we have any prefab to spawn
            if (defaultOceanTilePrefab == null || activeTiles.ContainsKey(chunkCoord))
                return;

            // Calculate world position
            Vector3 worldPos = GetChunkWorldPosition(chunkCoord);
            float2 worldPos2D = new float2(worldPos.x, worldPos.y);

            // Determine tile type based on depth
            GameObject tilePrefab = defaultOceanTilePrefab;
            OceanBiomeConfigurationSO.OceanTileType tileType = null;
            float depth = 50f; // Default depth
            Color tileColor = Color.blue; // Default color

            if (biomeConfig != null)
            {
                // Calculate depth at this position
                depth = biomeConfig.CalculateDepthAtPosition(worldPos2D);

                // Get appropriate tile type
                tileType = biomeConfig.GetTileTypeForDepth(depth);

                // Use custom prefab if specified
                if (tileType != null && tileType.tilePrefab != null)
                {
                    tilePrefab = tileType.tilePrefab;
                }

                // Get color for this depth
                tileColor = biomeConfig.GetColorForDepth(depth);

                if (debugDepthValues)
                {
                    DebugManager.Log(DebugCategory.Ocean, $"Tile {chunkCoord}: Depth={depth:F1}m, Type={tileType?.tileName ?? "Default"}, GetColorForDepth returned: {tileColor}", this);
                }
            }

            // Instantiate tile
            GameObject tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, tilesContainer);
            tile.name = "OceanTile_" + chunkCoord.x + "_" + chunkCoord.y + "_D" + depth.ToString("F0");

            // Debug: Check prefab's default color
            if (debugDepthValues)
            {
                SpriteRenderer prefabRenderer = tile.GetComponent<SpriteRenderer>();
                if (prefabRenderer != null)
                {
                    DebugManager.Log(DebugCategory.Ocean, $"Tile {chunkCoord}: Prefab default color: {prefabRenderer.color}", this);
                }
            }

            // Apply biome-based variation if biome config is assigned (prioritize over legacy system)
            if (biomeConfig != null)
            {
                ApplyBiomeVariation(tile, chunkCoord, depth, tileColor, tileType);

                // Debug: Check final color after ApplyBiomeVariation
                if (debugDepthValues)
                {
                    SpriteRenderer finalRenderer = tile.GetComponent<SpriteRenderer>();
                    if (finalRenderer != null)
                    {
                        DebugManager.Log(DebugCategory.Ocean, $"Tile {chunkCoord}: FINAL color after ApplyBiomeVariation: {finalRenderer.color}", this);
                    }
                }
            }
            else if (legacyOceanMaterials != null && legacyOceanMaterials.Length > 0)
            {
                // Fallback to legacy variation system only when no biome config
                ApplyTileVariation(tile, chunkCoord);

                if (debugDepthValues)
                {
                    DebugManager.LogWarning(DebugCategory.Ocean, $"Tile {chunkCoord}: Using LEGACY color system!", this);
                }
            }
            else
            {
                // If no biome config and no legacy materials, keep original prefab color
                if (debugDepthValues)
                {
                    DebugManager.LogWarning(DebugCategory.Ocean, $"Tile {chunkCoord}: NO color system applied - keeping prefab color!", this);
                }
            }

            // Scale tile to match tile size for 2D sprites
            // For 2D sprites, we scale directly to tile size (configurable, default 64 units)
            tile.transform.localScale = new Vector3(tileSize, tileSize, 1f);

            // Add to active tiles
            activeTiles.Add(chunkCoord, tile);

            // Initialize OceanTileController if present (should be on prefab for performance)
            OceanTileController controller = tile.GetComponent<OceanTileController>();
            if (controller != null)
            {
                controller.Initialize(chunkCoord, tileSize);
            }
            else
            {
                // Add component dynamically only if not on prefab (less performant)
                controller = tile.AddComponent<OceanTileController>();
                controller.Initialize(chunkCoord, tileSize);
                DebugManager.LogWarning(DebugCategory.Performance, "OceanTileController not on prefab - add it to improve performance", this);
            }

            // Add color blending component for smooth transitions
            OceanTileColorBlender blender = tile.GetComponent<OceanTileColorBlender>();
            if (blender == null && enableColorBlending)
            {
                blender = tile.AddComponent<OceanTileColorBlender>();
            }
        }

        private void DespawnTile(Vector2Int chunkCoord)
        {
            if (activeTiles.TryGetValue(chunkCoord, out GameObject tile))
            {
                activeTiles.Remove(chunkCoord);

                if (tile != null)
                {
                    // Get controller for cleanup
                    OceanTileController controller = tile.GetComponent<OceanTileController>();
                    controller?.Cleanup();

                    // Destroy tile
                    DestroyImmediate(tile);
                }
            }
        }

        private void ApplyTileVariation(GameObject tile, Vector2Int chunkCoord)
        {
            // Legacy variation system - use biome system instead
            SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            // Deterministic variation based on chunk coordinates
            uint seed = (uint)((chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663) + randomSeed);
            if (seed == 0) seed = (uint)randomSeed | 1; // Ensure non-zero
            Unity.Mathematics.Random localRandom = new Unity.Mathematics.Random(seed);

            // Legacy ocean color variations for 2D sprites
            Color[] oceanColors = {
                new Color(0.2f, 0.4f, 0.8f, 1f),   // Base ocean blue
                new Color(0.3f, 0.6f, 0.9f, 1f),   // Shallow blue
                new Color(0.15f, 0.35f, 0.75f, 1f), // Deep blue
                new Color(0.25f, 0.45f, 0.85f, 1f)  // Medium blue
            };

            // Get random color
            int colorIndex = localRandom.NextInt(0, oceanColors.Length);
            spriteRenderer.color = oceanColors[colorIndex];
        }

        private void ApplyBiomeVariation(GameObject tile, Vector2Int chunkCoord, float depth, Color baseColor, OceanBiomeConfigurationSO.OceanTileType tileType)
        {
            // Use SpriteRenderer color for 2D
            SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            // Deterministic variation based on chunk coordinates
            uint seed = (uint)((chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663) + randomSeed);
            if (seed == 0) seed = (uint)randomSeed | 1; // Ensure non-zero
            Unity.Mathematics.Random localRandom = new Unity.Mathematics.Random(seed);

            // Apply base color with optional slight variation
            Color finalColor = baseColor;

            // Debug color assignment
            if (debugDepthValues)
            {
                DebugManager.Log(DebugCategory.Ocean, $"Tile {chunkCoord}: BaseColor={baseColor}, TileType={tileType?.tileName ?? "null"}, Depth={depth:F1}m", this);
            }

            // Safety check: If baseColor is white/default, use tileType baseColor instead
            if (baseColor == Color.white && tileType != null)
            {
                finalColor = tileType.baseColor;
                if (debugDepthValues)
                {
                    DebugManager.LogWarning(DebugCategory.Ocean, $"Tile {chunkCoord}: Using tileType.baseColor {tileType.baseColor} instead of white baseColor", this);
                }
            }

            if (tileType != null && tileType.colorVariation > 0f)
            {
                // Apply subtle color variation by modifying brightness of the CORRECTED color
                float variation = localRandom.NextFloat(-tileType.colorVariation, tileType.colorVariation);
                float brightness = Mathf.Clamp(1.0f + variation, 0.1f, 2.0f); // Clamp to prevent invalid colors

                // Use the corrected finalColor (not the original white baseColor)
                Color colorToVary = finalColor;
                finalColor = new Color(
                    Mathf.Clamp01(colorToVary.r * brightness),
                    Mathf.Clamp01(colorToVary.g * brightness),
                    Mathf.Clamp01(colorToVary.b * brightness),
                    colorToVary.a
                );

                if (debugDepthValues)
                {
                    DebugManager.Log(DebugCategory.Ocean, $"Tile {chunkCoord}: Applied variation {variation:F3}, brightness={brightness:F3}, finalColor={finalColor}", this);
                }
            }

            // Apply biome visualization if enabled
            if (visualizeBiomes && tileType != null)
            {
                Color biomeOverlay = GetBiomeColor(tileType.depthZone);
                finalColor = Color.Lerp(finalColor, biomeOverlay, 0.3f);
            }

            spriteRenderer.color = finalColor;

            // Spawn features if specified
            if (tileType != null && tileType.possibleFeatures.Count > 0 && localRandom.NextFloat() < tileType.featureSpawnChance)
            {
                int featureIndex = localRandom.NextInt(0, tileType.possibleFeatures.Count);
                GameObject featurePrefab = tileType.possibleFeatures[featureIndex];
                if (featurePrefab != null)
                {
                    Vector3 featurePos = tile.transform.position + new Vector3(
                        localRandom.NextFloat(-tileSize * 0.4f, tileSize * 0.4f),
                        localRandom.NextFloat(-tileSize * 0.4f, tileSize * 0.4f),
                        -0.1f
                    );
                    GameObject feature = Instantiate(featurePrefab, featurePos, Quaternion.identity, tile.transform);
                    feature.name = "Feature_" + featurePrefab.name + "_" + chunkCoord.x + "_" + chunkCoord.y;
                }
            }
        }

        private Color GetBiomeColor(OceanBiomeConfigurationSO.OceanDepthZone zone)
        {
            switch (zone)
            {
                case OceanBiomeConfigurationSO.OceanDepthZone.Coastal:
                    return Color.cyan;
                case OceanBiomeConfigurationSO.OceanDepthZone.Shallow:
                    return Color.green;
                case OceanBiomeConfigurationSO.OceanDepthZone.Medium:
                    return Color.blue;
                case OceanBiomeConfigurationSO.OceanDepthZone.Deep:
                    return Color.magenta;
                case OceanBiomeConfigurationSO.OceanDepthZone.Abyssal:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Get current ocean statistics for debugging
        /// </summary>
        public OceanStats GetOceanStats()
        {
            return new OceanStats
            {
                activeTileCount = activeTiles.Count,
                tilesInSpawnQueue = tilesToSpawn.Count,
                tilesInDespawnQueue = tilesToDespawn.Count,
                currentChunk = lastCameraChunk,
                tileSize = tileSize,
                gridRadius = gridRadius
            };
        }

        /// <summary>
        /// Force rebuild ocean around current camera position
        /// </summary>
        public void RebuildOcean()
        {
            if (cameraTransform == null) return;

            // Clear all tiles
            foreach (var tile in activeTiles.Values)
            {
                if (tile != null)
                    DestroyImmediate(tile);
            }
            activeTiles.Clear();

            // Rebuild around current position
            Vector2Int currentChunk = GetChunkCoordinates(cameraTransform.position);
            SpawnInitialTiles(currentChunk);
            lastCameraChunk = currentChunk;

            DebugManager.Log(DebugCategory.Ocean, "Ocean rebuilt around camera position", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (cameraTransform == null) return;

            // Draw current chunk grid
            Vector2Int currentChunk = GetChunkCoordinates(cameraTransform.position);

            Gizmos.color = Color.cyan;
            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int z = -gridRadius; z <= gridRadius; z++)
                {
                    Vector2Int chunkCoord = currentChunk + new Vector2Int(x, z);
                    Vector3 chunkCenter = GetChunkWorldPosition(chunkCoord);

                    // Draw wireframe cube for each chunk
                    Gizmos.DrawWireCube(chunkCenter, new Vector3(tileSize, 1f, tileSize));
                }
            }

            // Draw camera position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cameraTransform.position, 50f);
        }

        private void OnDestroy()
        {
            // Cleanup all tiles
            if (activeTiles != null)
            {
                foreach (var tile in activeTiles.Values)
                {
                    if (tile != null)
                        DestroyImmediate(tile);
                }
                activeTiles.Clear();
            }
        }

        private void ValidateConfiguration()
        {
            // Validate biome configuration
            if (biomeConfig == null)
            {
                DebugManager.LogWarning(DebugCategory.Ocean, "No biome configuration assigned - using legacy random tiles", this);
            }
            else
            {
                // Validate biome config has tile types
                if (biomeConfig.oceanTileTypes == null || biomeConfig.oceanTileTypes.Count == 0)
                {
                    DebugManager.LogWarning(DebugCategory.Ocean, "Biome config has no tile types defined - using default tiles", this);
                }

                // Validate depth ranges don't have gaps
                var sortedTileTypes = new List<OceanBiomeConfigurationSO.OceanTileType>(biomeConfig.oceanTileTypes);
                sortedTileTypes.Sort((a, b) => a.minDepth.CompareTo(b.minDepth));

                for (int i = 0; i < sortedTileTypes.Count - 1; i++)
                {
                    if (sortedTileTypes[i].maxDepth < sortedTileTypes[i + 1].minDepth)
                    {
                        DebugManager.LogWarning(DebugCategory.Ocean,
                            "Depth gap detected between " + sortedTileTypes[i].tileName + " (" + sortedTileTypes[i].maxDepth + "m) and " + sortedTileTypes[i + 1].tileName + " (" + sortedTileTypes[i + 1].minDepth + "m)", this);
                    }
                }

                // Log biome configuration summary
                DebugManager.Log(DebugCategory.Ocean,
                    "Ocean Biome Config: " + (biomeConfig.oceanTileTypes?.Count ?? 0) + " tile types, Max depth: " + biomeConfig.maxOceanDepth + "m, Custom map: " + biomeConfig.useCustomDepthMap, this);
            }

            // Validate prefab
            if (defaultOceanTilePrefab == null)
            {
                DebugManager.LogError(DebugCategory.Ocean, "No default ocean tile prefab assigned!", this);
            }

            // Validate performance settings
            if (tilesPerFrame > 10)
            {
                DebugManager.LogWarning(DebugCategory.Performance, "TilesPerFrame (" + tilesPerFrame + ") is high - may cause frame drops", this);
            }
        }

        /// <summary>
        /// Apply runtime culling override controls
        /// </summary>
        private void ApplyRuntimeCullingOverrides()
        {
            // Override grid radius if different
            if (runtimeGridRadius != gridRadius)
            {
                gridRadius = runtimeGridRadius;
                DebugManager.Log(DebugCategory.Ocean, $"Runtime override: gridRadius set to {gridRadius}", this);
            }

            // Force enable all tile renderers if requested
            if (runtimeForceEnableRenderers)
            {
                ForceEnableAllTileRenderers();
            }

            // Disable culling on all tiles if requested
            if (runtimeDisableCulling)
            {
                DisableCullingOnAllTiles();
            }
        }

        /// <summary>
        /// Force enable all ocean tile renderers
        /// </summary>
        private void ForceEnableAllTileRenderers()
        {
            foreach (var tile in activeTiles.Values)
            {
                if (tile != null)
                {
                    var renderer = tile.GetComponent<Renderer>();
                    if (renderer != null && !renderer.enabled)
                    {
                        renderer.enabled = true;
                    }

                    var tileController = tile.GetComponent<OceanTileController>();
                    if (tileController != null)
                    {
                        tileController.SetVisibility(true);
                        tileController.SetDetailLevel(true);
                    }
                }
            }
        }

        /// <summary>
        /// Disable culling on all ocean tiles
        /// </summary>
        private void DisableCullingOnAllTiles()
        {
            foreach (var tile in activeTiles.Values)
            {
                if (tile != null)
                {
                    var tileController = tile.GetComponent<OceanTileController>();
                    if (tileController != null)
                    {
                        // Force visibility regardless of culling systems
                        tileController.SetVisibility(true);
                        tileController.SetDetailLevel(true);

                        // Override culling setting via reflection if needed
                        var enableCullingField = typeof(OceanTileController).GetField("enableCulling",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (enableCullingField != null)
                        {
                            enableCullingField.SetValue(tileController, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Force rebuild ocean with current runtime settings
        /// </summary>
        [ContextMenu("Force Rebuild Ocean")]
        public void ForceRebuildOcean()
        {
            RebuildOcean();
            DebugManager.Log(DebugCategory.Ocean, "Ocean rebuilt with runtime culling overrides", this);
        }

        /// <summary>
        /// Update color blending on all tiles
        /// </summary>
        private void UpdateColorBlending()
        {
            var blenders = FindObjectsOfType<OceanTileColorBlender>();
            foreach (var blender in blenders)
            {
                if (blender != null)
                {
                    blender.enabled = enableColorBlending;
                    if (enableColorBlending)
                    {
                        blender.ForceUpdate();
                    }
                }
            }
            DebugManager.Log(DebugCategory.Ocean, $"Color blending {(enableColorBlending ? "enabled" : "disabled")} on {blenders.Length} tiles", this);
        }

        /// <summary>
        /// Update blend strength on all tiles
        /// </summary>
        private void UpdateBlendStrength()
        {
            var blenders = FindObjectsOfType<OceanTileColorBlender>();
            foreach (var blender in blenders)
            {
                if (blender != null)
                {
                    // Use reflection to set blend strength since field is private
                    var strengthField = typeof(OceanTileColorBlender).GetField("blendStrength",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    strengthField?.SetValue(blender, colorBlendStrength);
                    blender.ForceUpdate();
                }
            }
        }

        private void OnGUI()
        {
            if (!showRuntimeGUI) return;

            // Ocean Debug UI
            GUILayout.BeginArea(new Rect(10, 10, 350, 300));

            GUILayout.Label("<size=16><color=white><b>Ocean Runtime Controls</b></color></size>");

            GUILayout.Label($"<color=white>Active Tiles: {activeTiles.Count}</color>");
            GUILayout.Label($"<color=white>Spawn Queue: {tilesToSpawn.Count}</color>");
            GUILayout.Label($"<color=white>Despawn Queue: {tilesToDespawn.Count}</color>");

            // Show culling stats
            var tileControllers = FindObjectsOfType<OceanTileController>();
            int visibleCount = 0;
            int highDetailCount = 0;
            foreach (var controller in tileControllers)
            {
                var perfInfo = controller.GetPerformanceInfo();
                if (perfInfo.isVisible) visibleCount++;
                if (perfInfo.isHighDetail) highDetailCount++;
            }
            GUILayout.Label($"<color=yellow>Visible: {visibleCount}/{tileControllers.Length}</color>");
            GUILayout.Label($"<color=cyan>High Detail: {highDetailCount}/{tileControllers.Length}</color>");

            GUILayout.Space(10);

            // Color blending controls
            bool newBlending = GUILayout.Toggle(enableColorBlending, "Enable Color Blending");
            if (newBlending != enableColorBlending)
            {
                enableColorBlending = newBlending;
                UpdateColorBlending();
            }

            if (enableColorBlending)
            {
                GUILayout.Label($"<color=white>Blend Strength: {colorBlendStrength:F2}</color>");
                float newStrength = GUILayout.HorizontalSlider(colorBlendStrength, 0f, 1f);
                if (Mathf.Abs(newStrength - colorBlendStrength) > 0.01f)
                {
                    colorBlendStrength = newStrength;
                    UpdateBlendStrength();
                }
            }

            GUILayout.Space(10);

            // Runtime controls
            bool newDisableCulling = GUILayout.Toggle(runtimeDisableCulling, "Disable All Culling");
            if (newDisableCulling != runtimeDisableCulling)
            {
                runtimeDisableCulling = newDisableCulling;
                DebugManager.Log(DebugCategory.Ocean, $"Runtime: Disable Culling = {runtimeDisableCulling}", this);
            }

            bool newForceEnable = GUILayout.Toggle(runtimeForceEnableRenderers, "Force Enable Renderers");
            if (newForceEnable != runtimeForceEnableRenderers)
            {
                runtimeForceEnableRenderers = newForceEnable;
                DebugManager.Log(DebugCategory.Ocean, $"Runtime: Force Enable = {runtimeForceEnableRenderers}", this);
            }

            GUILayout.Label($"<color=white>Grid Radius: {runtimeGridRadius} ({runtimeGridRadius * 2 + 1}x{runtimeGridRadius * 2 + 1})</color>");
            int newGridRadius = (int)GUILayout.HorizontalSlider(runtimeGridRadius, 1, 10);
            if (newGridRadius != runtimeGridRadius)
            {
                runtimeGridRadius = newGridRadius;
                DebugManager.Log(DebugCategory.Ocean, $"Runtime: Grid Radius = {runtimeGridRadius}", this);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Force Rebuild Ocean"))
            {
                ForceRebuildOcean();
            }

            if (GUILayout.Button("Emergency Enable All Renderers"))
            {
                var allRenderers = FindObjectsOfType<Renderer>();
                int count = 0;
                foreach (var r in allRenderers)
                {
                    if (r.gameObject.name.Contains("OceanTile") && !r.enabled)
                    {
                        r.enabled = true;
                        count++;
                    }
                }
                DebugManager.LogWarning(DebugCategory.Ocean, $"EMERGENCY: Enabled {count} tile renderers", this);
            }

            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// Ocean statistics for debugging and monitoring
    /// </summary>
    [System.Serializable]
    public struct OceanStats
    {
        public int activeTileCount;
        public int tilesInSpawnQueue;
        public int tilesInDespawnQueue;
        public Vector2Int currentChunk;
        public float tileSize;
        public int gridRadius;

        public override string ToString()
        {
            return $"Ocean Stats: {activeTileCount} tiles active, Chunk: {currentChunk}, Queue: +{tilesInSpawnQueue}/-{tilesInDespawnQueue}";
        }
    }
}