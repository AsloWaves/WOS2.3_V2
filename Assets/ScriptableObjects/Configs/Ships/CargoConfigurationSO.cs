using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject configuration for cargo system and spatial inventory.
    /// Defines grid-based inventory with Tetris-style cargo placement.
    /// </summary>
    [CreateAssetMenu(fileName = "CargoConfiguration", menuName = "WOS/Cargo Configuration")]
    public class CargoConfigurationSO : ScriptableObject
    {
        [Header("Grid Configuration")]
        [Tooltip("Width of cargo hold grid")]
        [Range(8, 24)]
        public int gridWidth = 12;

        [Tooltip("Height of cargo hold grid")]
        [Range(6, 20)]
        public int gridHeight = 10;

        [Tooltip("Size of each grid cell in Unity units")]
        [Range(0.5f, 2f)]
        public float cellSize = 1f;

        [Tooltip("Spacing between grid cells")]
        [Range(0f, 0.5f)]
        public float cellSpacing = 0.1f;

        [Header("Cargo Types")]
        [Tooltip("Available cargo item definitions")]
        public CargoItemTypeSO[] cargoTypes;

        [Header("Weight System")]
        [Tooltip("Maximum total weight capacity in tons")]
        [Range(50f, 2000f)]
        public float maxWeight = 500f;

        [Tooltip("Weight affects ship performance (speed, acceleration, maneuverability)")]
        public bool weightAffectsPerformance = true;

        [Tooltip("Weight threshold before performance penalties start (percentage of max weight)")]
        [Range(0.5f, 0.9f)]
        public float weightPenaltyThreshold = 0.75f;

        [Tooltip("Maximum performance penalty when overloaded (0-1 scale)")]
        [Range(0.1f, 0.8f)]
        public float maxWeightPenalty = 0.5f;

        [Header("Loading Rules")]
        [Tooltip("Items can be rotated when placing")]
        public bool allowRotation = true;

        [Tooltip("Minimum empty border around cargo hold")]
        [Range(0, 3)]
        public int borderSize = 1;

        [Tooltip("Allow items to be placed anywhere in grid (no support required)")]
        public bool freeFormPlacement = true;

        [Header("Visual Settings")]
        [Tooltip("Color for empty grid cells")]
        public Color emptyCellColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Tooltip("Color for occupied grid cells")]
        public Color occupiedCellColor = new Color(0.6f, 0.8f, 0.4f, 0.8f);

        [Tooltip("Color for invalid placement preview")]
        public Color invalidPlacementColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);

        [Tooltip("Color for valid placement preview")]
        public Color validPlacementColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);

        [Tooltip("Highlight color for selected items")]
        public Color selectionColor = new Color(1f, 1f, 0.2f, 0.8f);

        [Header("Audio Configuration")]
        [Tooltip("Sound when cargo is successfully placed")]
        public AudioClip cargoPlacedSound;

        [Tooltip("Sound when cargo placement fails")]
        public AudioClip cargoRejectedSound;

        [Tooltip("Sound when cargo is removed")]
        public AudioClip cargoRemovedSound;

        [Tooltip("Ambient cargo hold sounds")]
        public AudioClip[] ambientSounds;

        [Header("Performance Settings")]
        [Tooltip("Use Job System for cargo calculations")]
        public bool useJobSystem = true;

        [Tooltip("Enable cargo physics simulation")]
        public bool enablePhysics = false;

        [Tooltip("Maximum items to process per frame")]
        [Range(10, 100)]
        public int maxItemsPerFrame = 50;

        /// <summary>
        /// Get total grid area in cells
        /// </summary>
        public int GetTotalGridArea()
        {
            return gridWidth * gridHeight;
        }

        /// <summary>
        /// Get usable grid area (excluding borders)
        /// </summary>
        public int GetUsableGridArea()
        {
            int usableWidth = Mathf.Max(1, gridWidth - (borderSize * 2));
            int usableHeight = Mathf.Max(1, gridHeight - (borderSize * 2));
            return usableWidth * usableHeight;
        }

        /// <summary>
        /// Convert grid coordinates to world position
        /// </summary>
        public Vector3 GridToWorldPosition(int x, int y)
        {
            float worldX = (x - gridWidth * 0.5f) * (cellSize + cellSpacing);
            float worldZ = (y - gridHeight * 0.5f) * (cellSize + cellSpacing);
            return new Vector3(worldX, 0f, worldZ);
        }

        /// <summary>
        /// Convert world position to grid coordinates
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt((worldPos.x / (cellSize + cellSpacing)) + (gridWidth * 0.5f));
            int y = Mathf.RoundToInt((worldPos.z / (cellSize + cellSpacing)) + (gridHeight * 0.5f));
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Check if grid coordinates are valid
        /// </summary>
        public bool IsValidGridPosition(int x, int y)
        {
            return x >= borderSize && x < gridWidth - borderSize &&
                   y >= borderSize && y < gridHeight - borderSize;
        }

        /// <summary>
        /// Calculate total weight of all cargo in grid
        /// </summary>
        public float CalculateTotalWeight(NativeArray<CargoSlot> cargoGrid)
        {
            float totalWeight = 0f;

            for (int i = 0; i < cargoGrid.Length; i++)
            {
                var slot = cargoGrid[i];
                if (slot.isOccupied)
                {
                    totalWeight += slot.weight;
                }
            }

            return totalWeight;
        }

        /// <summary>
        /// Get performance penalty based on total weight (0 = no penalty, 1 = maximum penalty)
        /// </summary>
        public float GetWeightPerformancePenalty(float currentWeight)
        {
            if (!weightAffectsPerformance || currentWeight <= 0f) return 0f;

            float weightRatio = currentWeight / maxWeight;

            // No penalty until threshold is reached
            if (weightRatio <= weightPenaltyThreshold)
                return 0f;

            // Linear penalty from threshold to max capacity
            float penaltyRange = 1f - weightPenaltyThreshold;
            float excessWeight = weightRatio - weightPenaltyThreshold;

            return Mathf.Clamp01((excessWeight / penaltyRange) * maxWeightPenalty);
        }

        /// <summary>
        /// Get weight utilization percentage (0-1)
        /// </summary>
        public float GetWeightUtilization(float currentWeight)
        {
            return Mathf.Clamp01(currentWeight / maxWeight);
        }

        /// <summary>
        /// Check if adding weight would exceed capacity
        /// </summary>
        public bool CanAddWeight(float currentWeight, float additionalWeight)
        {
            return (currentWeight + additionalWeight) <= maxWeight;
        }

        /// <summary>
        /// Find cargo type by ID
        /// </summary>
        public CargoItemTypeSO GetCargoType(int typeId)
        {
            foreach (var cargoType in cargoTypes)
            {
                if (cargoType.typeId == typeId)
                    return cargoType;
            }
            return null;
        }

        private void OnValidate()
        {
            // Ensure reasonable grid dimensions
            gridWidth = Mathf.Max(4, gridWidth);
            gridHeight = Mathf.Max(4, gridHeight);
            borderSize = Mathf.Min(borderSize, Mathf.Min(gridWidth, gridHeight) / 2 - 1);

            // Validate cargo types
            if (cargoTypes != null)
            {
                for (int i = 0; i < cargoTypes.Length; i++)
                {
                    if (cargoTypes[i] != null)
                        cargoTypes[i].typeId = i;
                }
            }
        }
    }

    /// <summary>
    /// Individual cargo slot data for grid system
    /// </summary>
    [System.Serializable]
    public struct CargoSlot
    {
        public bool isOccupied;
        public int cargoTypeId;
        public float weight;
        public int itemInstanceId;

        public CargoSlot(bool occupied, int typeId, float itemWeight, int instanceId)
        {
            isOccupied = occupied;
            cargoTypeId = typeId;
            weight = itemWeight;
            itemInstanceId = instanceId;
        }
    }
}