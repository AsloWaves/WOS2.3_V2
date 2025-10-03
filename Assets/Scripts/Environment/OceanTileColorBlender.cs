using UnityEngine;
using System.Collections.Generic;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Handles smooth color blending between ocean tiles based on neighboring tile depths.
    /// Creates natural color gradients across the ocean surface.
    /// </summary>
    public class OceanTileColorBlender : MonoBehaviour
    {
        [Header("Blending Configuration")]
        [Tooltip("Enable color blending with neighboring tiles")]
        [SerializeField] private bool enableBlending = true;

        [Tooltip("Radius in units to search for neighboring tiles")]
        [Range(10f, 200f)]
        [SerializeField] private float blendRadius = 80f;

        [Tooltip("How much to blend with neighbor colors (0 = no blend, 1 = full average)")]
        [Range(0f, 1f)]
        [SerializeField] private float blendStrength = 0.3f;

        [Tooltip("Use distance-weighted blending")]
        [SerializeField] private bool useDistanceWeighting = true;

        [Tooltip("Update frequency for blending calculations")]
        [Range(0.1f, 2f)]
        [SerializeField] private float blendUpdateInterval = 0.5f;

        [Header("Performance")]
        [Tooltip("Maximum neighbors to consider for blending")]
        [Range(4, 12)]
        [SerializeField] private int maxNeighbors = 8;

        [Tooltip("Use smooth transitions over time")]
        [SerializeField] private bool smoothTransition = true;

        [Tooltip("Color transition speed")]
        [Range(0.5f, 5f)]
        [SerializeField] private float transitionSpeed = 2f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool visualizeNeighbors = false;

        // Component references
        private SpriteRenderer spriteRenderer;
        private OceanTileController tileController;
        private OceanChunkManager chunkManager;

        // Blending state
        private Color originalColor;
        private Color targetBlendedColor;
        private Color currentColor;
        private float lastBlendUpdate;
        private List<OceanTileController> neighborTiles;
        private Vector2Int myChunkCoord;

        // Cache
        private static Dictionary<Vector2Int, OceanTileController> tileCache = new Dictionary<Vector2Int, OceanTileController>();

        private void Start()
        {
            // Get components
            spriteRenderer = GetComponent<SpriteRenderer>();
            tileController = GetComponent<OceanTileController>();
            chunkManager = FindFirstObjectByType<OceanChunkManager>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                currentColor = originalColor;
                targetBlendedColor = originalColor;
            }

            neighborTiles = new List<OceanTileController>(maxNeighbors);

            // Get my chunk coordinates from tile controller
            if (tileController != null)
            {
                var perfInfo = tileController.GetPerformanceInfo();
                myChunkCoord = perfInfo.chunkCoordinates;
            }

            // Register in cache
            if (!tileCache.ContainsKey(myChunkCoord))
            {
                tileCache[myChunkCoord] = tileController;
            }

            // Initial blend
            if (enableBlending)
            {
                UpdateColorBlend();
            }
        }

        private void Update()
        {
            if (!enableBlending || spriteRenderer == null) return;

            // Update blend calculation periodically
            if (Time.time - lastBlendUpdate > blendUpdateInterval)
            {
                UpdateColorBlend();
                lastBlendUpdate = Time.time;
            }

            // Smooth transition to target color
            if (smoothTransition && currentColor != targetBlendedColor)
            {
                currentColor = Color.Lerp(currentColor, targetBlendedColor, Time.deltaTime * transitionSpeed);
                spriteRenderer.color = currentColor;
            }
            else if (!smoothTransition)
            {
                spriteRenderer.color = targetBlendedColor;
            }
        }

        /// <summary>
        /// Calculate blended color based on neighboring tiles
        /// </summary>
        private void UpdateColorBlend()
        {
            if (spriteRenderer == null) return;

            // Store current color as original if first time
            if (originalColor == Color.white || originalColor == Color.clear)
            {
                originalColor = spriteRenderer.color;
            }

            // Find neighboring tiles
            FindNeighborTiles();

            if (neighborTiles.Count == 0)
            {
                targetBlendedColor = originalColor;
                return;
            }

            // Calculate blended color
            Color blendedColor = CalculateBlendedColor();

            // Apply blend strength
            targetBlendedColor = Color.Lerp(originalColor, blendedColor, blendStrength);

            if (showDebugInfo)
            {
                DebugManager.Log(DebugCategory.Ocean,
                    $"Tile {myChunkCoord}: Blending with {neighborTiles.Count} neighbors, " +
                    $"Original={originalColor}, Blended={targetBlendedColor}", this);
            }
        }

        /// <summary>
        /// Find neighboring tiles within blend radius
        /// </summary>
        private void FindNeighborTiles()
        {
            neighborTiles.Clear();

            // Method 1: Use cached tiles (fastest)
            if (tileCache.Count > 0)
            {
                FindNeighborsFromCache();
                return;
            }

            // Method 2: Physics overlap (fallback)
            FindNeighborsWithPhysics();
        }

        private void FindNeighborsFromCache()
        {
            Vector3 myPosition = transform.position;

            // Check tiles in a grid pattern around us
            int searchRadius = Mathf.CeilToInt(blendRadius / 64f); // Assuming tile size of 64

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    if (x == 0 && y == 0) continue; // Skip self

                    Vector2Int neighborCoord = myChunkCoord + new Vector2Int(x, y);

                    if (tileCache.TryGetValue(neighborCoord, out OceanTileController neighbor))
                    {
                        if (neighbor != null && neighbor != tileController)
                        {
                            float distance = Vector3.Distance(myPosition, neighbor.transform.position);
                            if (distance <= blendRadius)
                            {
                                neighborTiles.Add(neighbor);
                                if (neighborTiles.Count >= maxNeighbors) return;
                            }
                        }
                    }
                }
            }
        }

        private void FindNeighborsWithPhysics()
        {
            // Use physics overlap to find nearby tiles
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, blendRadius);

            foreach (var collider in nearbyColliders)
            {
                if (collider.gameObject == gameObject) continue;

                var neighborTile = collider.GetComponent<OceanTileController>();
                if (neighborTile != null)
                {
                    neighborTiles.Add(neighborTile);
                    if (neighborTiles.Count >= maxNeighbors) break;
                }
            }
        }

        /// <summary>
        /// Calculate the blended color from neighbor tiles
        /// </summary>
        private Color CalculateBlendedColor()
        {
            if (neighborTiles.Count == 0) return originalColor;

            Color totalColor = originalColor;
            float totalWeight = 1f;
            Vector3 myPosition = transform.position;

            foreach (var neighbor in neighborTiles)
            {
                if (neighbor == null) continue;

                var neighborRenderer = neighbor.GetComponent<SpriteRenderer>();
                if (neighborRenderer == null) continue;

                Color neighborColor = neighborRenderer.color;

                if (useDistanceWeighting)
                {
                    // Weight by inverse distance
                    float distance = Vector3.Distance(myPosition, neighbor.transform.position);
                    float weight = 1f - (distance / blendRadius);
                    weight = Mathf.Max(0f, weight);

                    totalColor += neighborColor * weight;
                    totalWeight += weight;
                }
                else
                {
                    // Simple average
                    totalColor += neighborColor;
                    totalWeight += 1f;
                }
            }

            // Average the colors
            if (totalWeight > 0f)
            {
                totalColor /= totalWeight;
            }

            // Preserve alpha
            totalColor.a = originalColor.a;

            return totalColor;
        }

        /// <summary>
        /// Force immediate color update
        /// </summary>
        public void ForceUpdate()
        {
            UpdateColorBlend();
            if (!smoothTransition && spriteRenderer != null)
            {
                spriteRenderer.color = targetBlendedColor;
            }
        }

        /// <summary>
        /// Set the original color (base color before blending)
        /// </summary>
        public void SetOriginalColor(Color color)
        {
            originalColor = color;
            if (!enableBlending && spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw blend radius
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, blendRadius);

            // Visualize neighbors
            if (visualizeNeighbors && neighborTiles != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var neighbor in neighborTiles)
                {
                    if (neighbor != null)
                    {
                        Gizmos.DrawLine(transform.position, neighbor.transform.position);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Remove from cache
            if (tileCache.ContainsKey(myChunkCoord))
            {
                tileCache.Remove(myChunkCoord);
            }
        }

        /// <summary>
        /// Clear the tile cache (call when rebuilding ocean)
        /// </summary>
        public static void ClearTileCache()
        {
            tileCache.Clear();
        }
    }
}