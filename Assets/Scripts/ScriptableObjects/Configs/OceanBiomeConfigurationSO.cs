using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using WOS.Debugging;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Configuration for ocean biome generation with depth-based tile spawning.
    /// Uses heatmaps or procedural noise to determine ocean depth and tile types.
    /// </summary>
    [CreateAssetMenu(fileName = "OceanBiomeConfig", menuName = "WOS/Environment/Ocean Biome Configuration")]
    public class OceanBiomeConfigurationSO : ScriptableObject
    {
        [System.Serializable]
        public enum OceanDepthZone
        {
            Coastal = 0,      // 0-5 meters (beaches, shallow water)
            Shallow = 1,      // 5-20 meters (light penetrates to bottom)
            Medium = 2,       // 20-100 meters (twilight zone)
            Deep = 3,         // 100-500 meters (dark waters)
            Abyssal = 4       // 500+ meters (ocean trenches)
        }

        [System.Serializable]
        public class OceanTileType
        {
            [Header("Tile Identity")]
            public string tileName = "Ocean Tile";
            public OceanDepthZone depthZone = OceanDepthZone.Medium;

            [Header("Depth Range")]
            [Tooltip("Minimum depth in meters for this tile type")]
            [Range(0f, 1000f)]
            public float minDepth = 20f;

            [Tooltip("Maximum depth in meters for this tile type")]
            [Range(0f, 1000f)]
            public float maxDepth = 100f;

            [Header("Visual Configuration")]
            [Tooltip("Base color for this depth zone")]
            public Color baseColor = new Color(0f, 0.3f, 0.6f, 1f);

            [Tooltip("Secondary color for variation")]
            public Color variationColor = new Color(0f, 0.25f, 0.5f, 1f);

            [Tooltip("Color variation strength")]
            [Range(0f, 1f)]
            public float colorVariation = 0.1f;

            [Tooltip("Wave intensity multiplier for this depth")]
            [Range(0f, 2f)]
            public float waveIntensity = 1f;

            // Constructor to ensure proper initialization
            public OceanTileType()
            {
                tileName = "Ocean Tile";
                depthZone = OceanDepthZone.Medium;
                minDepth = 20f;
                maxDepth = 100f;
                baseColor = new Color(0f, 0.3f, 0.6f, 1f);
                variationColor = new Color(0f, 0.25f, 0.5f, 1f);
                colorVariation = 0.1f;
                waveIntensity = 1f;
                spawnWeight = 1f;
                featureSpawnChance = 0.05f;
                possibleFeatures = new List<GameObject>();
            }

            // Method to reset to defaults if corrupted
            public void ResetToDefaults()
            {
                if (string.IsNullOrEmpty(tileName)) tileName = "Ocean Tile";
                if (minDepth < 0f || minDepth > 1000f) minDepth = 20f;
                if (maxDepth < 0f || maxDepth > 1000f || maxDepth <= minDepth) maxDepth = 100f;
                if (colorVariation < 0f || colorVariation > 1f) colorVariation = 0.1f;
                if (waveIntensity < 0f || waveIntensity > 2f) waveIntensity = 1f;
                if (spawnWeight < 0.1f || spawnWeight > 10f) spawnWeight = 1f;
                if (featureSpawnChance < 0f || featureSpawnChance > 1f) featureSpawnChance = 0.05f;
                if (possibleFeatures == null) possibleFeatures = new List<GameObject>();
            }

            [Header("Spawn Configuration")]
            [Tooltip("Prefab to spawn for this tile type (optional)")]
            public GameObject tilePrefab;

            [Tooltip("Probability weight for spawning this tile type")]
            [Range(0.1f, 10f)]
            public float spawnWeight = 1f;

            [Tooltip("Special features that can spawn in this zone")]
            public List<GameObject> possibleFeatures = new List<GameObject>();

            [Tooltip("Feature spawn probability")]
            [Range(0f, 1f)]
            public float featureSpawnChance = 0.05f;
        }

        [System.Serializable]
        public class NoiseLayerSettings
        {
            [Header("Noise Configuration")]
            [Tooltip("Scale of the noise pattern")]
            [Range(0.001f, 1f)]
            public float noiseScale = 0.1f;

            [Tooltip("Number of octaves for fractal noise")]
            [Range(1, 8)]
            public int octaves = 4;

            [Tooltip("Persistence (amplitude multiplier per octave)")]
            [Range(0f, 1f)]
            public float persistence = 0.5f;

            [Tooltip("Lacunarity (frequency multiplier per octave)")]
            [Range(1f, 4f)]
            public float lacunarity = 2f;

            [Tooltip("Weight of this noise layer")]
            [Range(0f, 1f)]
            public float weight = 1f;

            [Tooltip("Offset for this noise layer")]
            public float2 offset = float2.zero;
        }

        [Header("Ocean Tile Types")]
        [Tooltip("All available ocean tile types sorted by depth")]
        public List<OceanTileType> oceanTileTypes = new List<OceanTileType>();

        [Header("Depth Generation")]
        [Tooltip("Use custom depth map texture instead of procedural noise")]
        public bool useCustomDepthMap = false;

        [Tooltip("Custom depth map texture (white = deep, black = shallow)")]
        public Texture2D customDepthMap;

        [Tooltip("World size that the depth map covers in units")]
        public float depthMapWorldSize = 1000f;

        [Header("Procedural Noise Settings")]
        [Tooltip("Primary noise layer for base depth")]
        public NoiseLayerSettings primaryNoise = new NoiseLayerSettings();

        [Tooltip("Secondary noise layer for detail")]
        public NoiseLayerSettings secondaryNoise = new NoiseLayerSettings();

        [Tooltip("Tertiary noise layer for fine detail")]
        public NoiseLayerSettings tertiaryNoise = new NoiseLayerSettings();

        [Header("Depth Mapping")]
        [Tooltip("Maximum ocean depth in meters")]
        [Range(10f, 2000f)]
        public float maxOceanDepth = 500f;

        [Tooltip("Depth curve for remapping noise to depth values")]
        public AnimationCurve depthRemappingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Color Gradient")]
        [Tooltip("Overall color gradient based on depth (overrides tile colors if enabled)")]
        public bool useDepthGradient = true;

        [Tooltip("Color gradient from shallow to deep")]
        public Gradient depthColorGradient = new Gradient();

        [Header("Biome Regions")]
        [Tooltip("Enable distinct biome regions (tropical, arctic, etc.)")]
        public bool useBiomeRegions = false;

        [Tooltip("Biome region noise scale")]
        [Range(0.001f, 0.1f)]
        public float biomeNoiseScale = 0.01f;

        [Header("Performance")]
        [Tooltip("LOD distance for reducing tile quality")]
        [Range(50f, 500f)]
        public float lodDistance = 200f;

        [Tooltip("Maximum tiles to update per frame")]
        [Range(1, 20)]
        public int maxTilesPerFrameUpdate = 5;

        /// <summary>
        /// Calculate ocean depth at a given world position using noise or custom map
        /// </summary>
        public float CalculateDepthAtPosition(float2 worldPos)
        {
            if (useCustomDepthMap && customDepthMap != null)
            {
                return CalculateDepthFromTexture(worldPos);
            }
            else
            {
                return CalculateDepthFromNoise(worldPos);
            }
        }

        /// <summary>
        /// Get the appropriate tile type for a given depth
        /// </summary>
        public OceanTileType GetTileTypeForDepth(float depth)
        {
            // Sort by depth if needed
            if (oceanTileTypes == null || oceanTileTypes.Count == 0)
            {
                DebugManager.LogWarning(DebugCategory.Ocean, "No ocean tile types defined in configuration!", this);
                return null;
            }

            // Find the tile type that matches the depth range
            foreach (var tileType in oceanTileTypes)
            {
                if (depth >= tileType.minDepth && depth <= tileType.maxDepth)
                {
                    return tileType;
                }
            }

            // If no exact match, return the closest one
            OceanTileType closest = oceanTileTypes[0];
            float closestDistance = Mathf.Abs(depth - closest.minDepth);

            foreach (var tileType in oceanTileTypes)
            {
                float distance = Mathf.Min(
                    Mathf.Abs(depth - tileType.minDepth),
                    Mathf.Abs(depth - tileType.maxDepth)
                );

                if (distance < closestDistance)
                {
                    closest = tileType;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        /// <summary>
        /// Get the color for a specific depth using gradient or tile type
        /// </summary>
        public Color GetColorForDepth(float depth)
        {
            if (useDepthGradient)
            {
                float normalizedDepth = Mathf.Clamp01(depth / maxOceanDepth);
                Color gradientColor = depthColorGradient.Evaluate(normalizedDepth);
                DebugManager.Log(DebugCategory.Ocean, $"GetColorForDepth: Using gradient for depth {depth}m, normalized={normalizedDepth:F3}, color={gradientColor}", this);
                return gradientColor;
            }
            else
            {
                var tileType = GetTileTypeForDepth(depth);
                if (tileType != null)
                {
                    DebugManager.Log(DebugCategory.Ocean, $"GetColorForDepth: Found tileType '{tileType.tileName}' for depth {depth}m, baseColor={tileType.baseColor}", this);
                    // Return base color for consistent depth-based coloring
                    // Tile-specific variation is handled in ApplyBiomeVariation using deterministic seeds
                    return tileType.baseColor;
                }
                else
                {
                    DebugManager.LogWarning(DebugCategory.Ocean, $"GetColorForDepth: NO TILE TYPE found for depth {depth}m! Returning blue fallback", this);
                    return Color.blue;
                }
            }
        }

        private float CalculateDepthFromTexture(float2 worldPos)
        {
            // Convert world position to UV coordinates
            float u = (worldPos.x / depthMapWorldSize) + 0.5f;
            float v = (worldPos.y / depthMapWorldSize) + 0.5f;

            // Wrap UVs for tiling
            u = Mathf.Repeat(u, 1f);
            v = Mathf.Repeat(v, 1f);

            // Sample the texture
            Color depthSample = customDepthMap.GetPixelBilinear(u, v);

            // Use the red channel as depth value (0 = shallow, 1 = deep)
            float normalizedDepth = depthSample.r;

            // Remap through curve
            normalizedDepth = depthRemappingCurve.Evaluate(normalizedDepth);

            // Convert to actual depth
            return normalizedDepth * maxOceanDepth;
        }

        private float CalculateDepthFromNoise(float2 worldPos)
        {
            float depthValue = 0f;
            float totalWeight = 0f;

            // Primary noise layer
            if (primaryNoise.weight > 0f)
            {
                float noise = GenerateFractalNoise(worldPos, primaryNoise);
                depthValue += noise * primaryNoise.weight;
                totalWeight += primaryNoise.weight;
            }

            // Secondary noise layer
            if (secondaryNoise.weight > 0f)
            {
                float noise = GenerateFractalNoise(worldPos, secondaryNoise);
                depthValue += noise * secondaryNoise.weight;
                totalWeight += secondaryNoise.weight;
            }

            // Tertiary noise layer
            if (tertiaryNoise.weight > 0f)
            {
                float noise = GenerateFractalNoise(worldPos, tertiaryNoise);
                depthValue += noise * tertiaryNoise.weight;
                totalWeight += tertiaryNoise.weight;
            }

            // Normalize by total weight
            if (totalWeight > 0f)
            {
                depthValue /= totalWeight;
            }

            // Remap through curve
            depthValue = depthRemappingCurve.Evaluate(depthValue);

            // Convert to actual depth
            return depthValue * maxOceanDepth;
        }

        private float GenerateFractalNoise(float2 position, NoiseLayerSettings settings)
        {
            float value = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float maxValue = 0f;

            position = position * settings.noiseScale + settings.offset;

            for (int i = 0; i < settings.octaves; i++)
            {
                float2 samplePos = position * frequency;
                float noiseValue = Mathf.PerlinNoise(samplePos.x, samplePos.y);

                value += noiseValue * amplitude;
                maxValue += amplitude;

                amplitude *= settings.persistence;
                frequency *= settings.lacunarity;
            }

            // Normalize to 0-1 range
            return value / maxValue;
        }

        private void OnValidate()
        {
            // Validate and fix tile types without auto-sorting
            if (oceanTileTypes != null && oceanTileTypes.Count > 0)
            {
                // Fix any corrupted tile types and ensure proper values
                for (int i = 0; i < oceanTileTypes.Count; i++)
                {
                    if (oceanTileTypes[i] == null)
                    {
                        // Create new tile type if null
                        oceanTileTypes[i] = new OceanTileType();
                    }
                    else
                    {
                        // Reset any corrupted values to defaults
                        oceanTileTypes[i].ResetToDefaults();
                    }
                }
            }

            // Initialize list if null
            if (oceanTileTypes == null)
            {
                oceanTileTypes = new List<OceanTileType>();
            }

            // Initialize gradient if empty
            if (depthColorGradient == null || depthColorGradient.colorKeys.Length == 0)
            {
                depthColorGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(0.5f, 0.8f, 0.9f), 0f);    // Coastal
                colorKeys[1] = new GradientColorKey(new Color(0.2f, 0.6f, 0.8f), 0.25f);  // Shallow
                colorKeys[2] = new GradientColorKey(new Color(0.1f, 0.4f, 0.7f), 0.5f);   // Medium
                colorKeys[3] = new GradientColorKey(new Color(0.05f, 0.2f, 0.5f), 0.75f); // Deep
                colorKeys[4] = new GradientColorKey(new Color(0.02f, 0.1f, 0.3f), 1f);    // Abyssal

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                depthColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
    }
}