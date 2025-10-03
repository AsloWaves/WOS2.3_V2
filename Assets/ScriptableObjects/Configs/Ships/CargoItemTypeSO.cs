using UnityEngine;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Definition for a type of cargo item
    /// </summary>
    [CreateAssetMenu(fileName = "CargoItemType", menuName = "WOS/Cargo Item Type")]
    public class CargoItemTypeSO : ScriptableObject
    {
        [Header("Basic Properties")]
        public int typeId;
        public string itemName = "Cargo Container";
        [TextArea(2, 4)]
        public string description = "Standard shipping container";

        [Header("Physical Dimensions")]
        [Tooltip("Width in grid cells")]
        [Range(1, 6)]
        public int width = 2;

        [Tooltip("Height in grid cells")]
        [Range(1, 6)]
        public int height = 2;

        [Tooltip("Weight per unit in tons")]
        [Range(0.1f, 50f)]
        public float unitWeight = 5f;

        [Tooltip("Can this item be rotated when placing?")]
        public bool canRotate = true;

        [Tooltip("Can this item be stacked on top of others?")]
        public bool canStack = true;

        [Tooltip("Can other items be placed on top of this?")]
        public bool canSupportOthers = true;

        [Header("Economic Properties")]
        [Tooltip("Base value per unit")]
        [Range(10f, 10000f)]
        public float baseValue = 100f;

        [Tooltip("How common this cargo type is")]
        [Range(0f, 1f)]
        public float rarity = 0.5f;

        [Tooltip("Cargo category for trading")]
        public CargoCategory category = CargoCategory.General;

        [Header("Visual Settings")]
        [Tooltip("Sprite for inventory display")]
        public Sprite inventoryIcon;

        [Tooltip("3D model for cargo hold visualization")]
        public GameObject cargoModel;

        [Tooltip("Color tint for this cargo type")]
        public Color itemColor = Color.white;

        [Header("Special Properties")]
        [Tooltip("Requires special handling or storage")]
        public bool isHazardous = false;

        [Tooltip("Affected by temperature")]
        public bool isPerishable = false;

        [Tooltip("High value, affects piracy risk")]
        public bool isValuable = false;

        [Tooltip("Essential for ship operation (fuel, ammo, etc.)")]
        public bool isEssential = false;

        [Tooltip("Can be consumed during ship operations")]
        public bool isConsumable = false;

        [Tooltip("Consumption rate per hour (for fuel) or per use (for ammo)")]
        [Range(0f, 100f)]
        public float consumptionRate = 0f;

        /// <summary>
        /// Get total weight for this item
        /// </summary>
        public float GetTotalWeight()
        {
            return unitWeight * width * height;
        }

        /// <summary>
        /// Get area occupied by this item
        /// </summary>
        public int GetArea()
        {
            return width * height;
        }

        /// <summary>
        /// Get rotated dimensions
        /// </summary>
        public Vector2Int GetRotatedDimensions(bool rotated)
        {
            return rotated && canRotate ? new Vector2Int(height, width) : new Vector2Int(width, height);
        }
    }

    /// <summary>
    /// Cargo categories for naval game simulation
    /// </summary>
    public enum CargoCategory
    {
        General,        // Standard cargo
        Food,          // Consumables and provisions
        Fuel,          // Ship fuel and energy (consumable)
        Equipment,     // Ship parts and tools
        Ammunition,    // Weapons ammunition (consumable)
        Luxury,        // High-value trade goods
        Raw,           // Raw materials
        Manufactured,  // Finished products
        Military,      // Military equipment and weapons
        Medical,       // Medical supplies and equipment
        Hazardous      // Dangerous or special handling cargo
    }
}