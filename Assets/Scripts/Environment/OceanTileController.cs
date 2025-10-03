using UnityEngine;
using Unity.Mathematics;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Controls individual ocean tile behavior, animations, and optimizations.
    /// Manages tile-specific effects like gentle wave motion and material properties.
    /// </summary>
    public class OceanTileController : MonoBehaviour
    {
        [Header("Tile Animation")]
        [Tooltip("Enable subtle wave animation on this tile")]
        [SerializeField] private bool enableWaveAnimation = true;

        [Tooltip("Wave animation speed multiplier")]
        [Range(0.1f, 2f)]
        [SerializeField] private float waveSpeed = 0.5f;

        [Tooltip("Wave height amplitude")]
        [Range(0f, 0.5f)]
        [SerializeField] private float waveAmplitude = 0.1f;

        [Header("Material Animation")]
        [Tooltip("Animate material texture offset")]
        [SerializeField] private bool animateTexture = true;

        [Tooltip("Texture scroll speed")]
        [SerializeField] private Vector2 textureScrollSpeed = new Vector2(0.02f, 0.01f);

        [Header("Performance")]
        [Tooltip("Distance from camera before reducing detail")]
        [SerializeField] private float lodDistance = 3000f;

        [Tooltip("Enable frustum culling optimization")]
        [SerializeField] private bool enableCulling = true;

        // Tile Properties
        private Vector2Int chunkCoordinates;
        private float tileSize;
        private bool isInitialized = false;

        // Animation State
        private float wavePhase;
        private Vector3 basePosition;
        private Renderer tileRenderer;
        private Material tileMaterial;
        private MaterialPropertyBlock propertyBlock;

        // Performance State
        private UnityEngine.Camera oceanCamera;
        private bool isVisible = true;
        private bool isHighDetail = true;
        private float lastVisibilityCheck;

        // Animation Properties
        private static readonly int MainTexOffset = Shader.PropertyToID("_MainTex_ST");
        private static readonly int WaveOffset = Shader.PropertyToID("_WaveOffset");
        private static readonly int TilePosition = Shader.PropertyToID("_TilePosition");

        private void Awake()
        {
            // Get components
            tileRenderer = GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileMaterial = tileRenderer.material;
                propertyBlock = new MaterialPropertyBlock();
            }

            // Find camera reference
            if (oceanCamera == null)
                oceanCamera = UnityEngine.Camera.main;

            // Store base position
            basePosition = transform.position;
        }

        /// <summary>
        /// Initialize tile with chunk data from OceanChunkManager
        /// </summary>
        public void Initialize(Vector2Int chunkCoord, float size)
        {
            chunkCoordinates = chunkCoord;
            tileSize = size;
            isInitialized = true;

            // Generate deterministic wave phase based on chunk position
            // Ensure seed is never zero by OR with 1 if needed
            uint seed = (uint)(chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663);
            if (seed == 0) seed = 1; // Unity.Mathematics.Random requires non-zero seed
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);
            wavePhase = random.NextFloat(0f, 2f * math.PI);

            // Set tile-specific shader properties
            if (propertyBlock != null && tileRenderer != null)
            {
                // Pass tile world position to shader
                propertyBlock.SetVector(TilePosition, new Vector4(transform.position.x, transform.position.z, 0, 0));
                tileRenderer.SetPropertyBlock(propertyBlock);
            }

            DebugManager.Log(DebugCategory.Ocean, $"Initialized tile at chunk {chunkCoord} with phase {wavePhase:F2}", this);
        }

        private void Update()
        {
            if (!isInitialized) return;

            // CRITICAL: Ensure tile position never changes (lock to basePosition)
            if (transform.position != basePosition)
            {
                transform.position = basePosition;
                DebugManager.LogWarning(DebugCategory.Ocean, $"{chunkCoordinates}: Position corrected to basePosition", this);
            }

            // Performance check every 0.2 seconds
            if (Time.time - lastVisibilityCheck > 0.2f)
            {
                UpdatePerformanceState();
                lastVisibilityCheck = Time.time;
            }

            // Only animate if visible
            if (isVisible)
            {
                if (enableWaveAnimation)
                    UpdateWaveAnimation();

                if (animateTexture && isHighDetail)
                    UpdateTextureAnimation();
            }
        }

        private void UpdatePerformanceState()
        {
            if (oceanCamera == null) return;

            // Check if tile is in camera frustum
            Bounds tileBounds = tileRenderer.bounds;
            bool inFrustum = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(oceanCamera), tileBounds);

            // Update visibility
            bool wasVisible = isVisible;
            isVisible = inFrustum;

            // Enable/disable renderer for culling optimization
            if (enableCulling && tileRenderer.enabled != isVisible)
            {
                tileRenderer.enabled = isVisible;
            }

            // Determine detail level based on distance
            float distanceToCamera = Vector3.Distance(transform.position, oceanCamera.transform.position);
            bool wasHighDetail = isHighDetail;
            isHighDetail = distanceToCamera <= lodDistance;

            // Log state changes for debugging
            if (wasVisible != isVisible || wasHighDetail != isHighDetail)
            {
                DebugManager.Log(DebugCategory.Ocean, $"{chunkCoordinates}: Visible={isVisible}, HighDetail={isHighDetail}, Distance={distanceToCamera:F1}", this);
            }
        }

        private void UpdateWaveAnimation()
        {
            if (!isHighDetail) return;

            // Calculate wave animation values for visual effects only
            float time = Time.time * waveSpeed;
            float waveValue = math.sin(time + wavePhase) * waveAmplitude;

            // Apply wave animation to material properties instead of position
            // Ocean tiles should stay stationary - only visual effects should animate
            if (propertyBlock != null && tileRenderer != null)
            {
                // Apply wave animation to material (this could control texture distortion, color shifts, etc.)
                propertyBlock.SetFloat(WaveOffset, time + wavePhase);

                // Add subtle wave-based color variation if material supports it
                if (tileMaterial != null)
                {
                    float colorVariation = (waveValue + 1f) * 0.5f; // Normalize to 0-1
                    // Material could use this for subtle color shifts or foam effects
                }
            }

            // DO NOT move the tile position - tiles should remain stationary like real ocean
            // transform.position should always stay at basePosition
        }

        private void UpdateTextureAnimation()
        {
            if (propertyBlock == null || tileRenderer == null) return;

            // Calculate texture offset
            Vector2 textureOffset = textureScrollSpeed * Time.time;

            // Apply to material property block to avoid material instancing
            propertyBlock.SetFloat(WaveOffset, Time.time * waveSpeed + wavePhase);

            // Apply texture scrolling if material supports it
            if (tileMaterial != null && tileMaterial.HasProperty(MainTexOffset))
            {
                Vector4 tilingOffset = tileMaterial.GetVector(MainTexOffset);
                tilingOffset.z = textureOffset.x % 1f; // Wrap UV coordinates
                tilingOffset.w = textureOffset.y % 1f;
                propertyBlock.SetVector(MainTexOffset, tilingOffset);
            }

            tileRenderer.SetPropertyBlock(propertyBlock);
        }

        /// <summary>
        /// Get tile performance statistics
        /// </summary>
        public TilePerformanceInfo GetPerformanceInfo()
        {
            float distanceToCamera = oceanCamera != null ?
                Vector3.Distance(transform.position, oceanCamera.transform.position) : 0f;

            return new TilePerformanceInfo
            {
                chunkCoordinates = chunkCoordinates,
                isVisible = isVisible,
                isHighDetail = isHighDetail,
                distanceToCamera = distanceToCamera,
                isAnimating = enableWaveAnimation && isVisible
            };
        }

        /// <summary>
        /// Set tile visibility state (called by LOD system)
        /// </summary>
        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            if (tileRenderer != null)
                tileRenderer.enabled = visible;
        }

        /// <summary>
        /// Set tile detail level (called by LOD system)
        /// </summary>
        public void SetDetailLevel(bool highDetail)
        {
            isHighDetail = highDetail;

            // Disable expensive effects for low detail
            if (!highDetail)
            {
                // Reset position to base for distant tiles
                transform.position = basePosition;
            }
        }

        /// <summary>
        /// Apply material variation to this tile
        /// </summary>
        public void ApplyMaterialVariation(Material newMaterial)
        {
            if (tileRenderer != null && newMaterial != null)
            {
                tileRenderer.material = newMaterial;
                tileMaterial = newMaterial;
            }
        }

        /// <summary>
        /// Cleanup tile resources
        /// </summary>
        public void Cleanup()
        {
            // Reset any animated properties
            if (tileRenderer != null)
            {
                transform.position = basePosition;
                tileRenderer.SetPropertyBlock(null);
            }

            // Clear references
            propertyBlock = null;
            tileMaterial = null;
            oceanCamera = null;

            DebugManager.Log(DebugCategory.Ocean, $"Cleaned up tile at chunk {chunkCoordinates}", this);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw tile bounds
            Gizmos.color = isVisible ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(tileSize, 1f, tileSize));

            // Draw LOD indicator
            if (oceanCamera != null)
            {
                Gizmos.color = isHighDetail ? Color.cyan : Color.yellow;
                Gizmos.DrawLine(transform.position, oceanCamera.transform.position);
            }

            // Draw chunk coordinates
            Vector3 labelPos = transform.position + Vector3.up * 10f;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"Chunk: {chunkCoordinates}\nVisible: {isVisible}\nDetail: {isHighDetail}");
            #endif
        }

        private void OnBecameVisible()
        {
            if (enableCulling)
                isVisible = true;
        }

        private void OnBecameInvisible()
        {
            if (enableCulling)
                isVisible = false;
        }
    }

    /// <summary>
    /// Performance information for an ocean tile
    /// </summary>
    [System.Serializable]
    public struct TilePerformanceInfo
    {
        public Vector2Int chunkCoordinates;
        public bool isVisible;
        public bool isHighDetail;
        public float distanceToCamera;
        public bool isAnimating;

        public override string ToString()
        {
            return $"Tile {chunkCoordinates}: Visible={isVisible}, Detail={isHighDetail}, Distance={distanceToCamera:F1}m";
        }
    }
}