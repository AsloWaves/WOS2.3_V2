using System.ComponentModel.DataAnnotations;

namespace EdgegapProxy.Models.Inventory;

/// <summary>
/// Item definition (type/template) from database
/// </summary>
public class ItemDefinition
{
    [Key]
    [StringLength(50)]
    public required string ItemId { get; set; }

    [Required]
    [StringLength(100)]
    public required string ItemName { get; set; }

    [Required]
    [StringLength(50)]
    public required string ItemType { get; set; }

    [Required]
    public required string GridSizeJson { get; set; }

    public string PropertiesJson { get; set; } = "{}";

    public int MaxStack { get; set; } = 1;

    public bool IsTradeable { get; set; } = true;

    public bool IsConsumable { get; set; } = false;

    public int BaseValue { get; set; }

    public float Weight { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Item definition response (sent to Unity)
/// </summary>
public class ItemDefinitionResponse
{
    public required string ItemId { get; set; }
    public required string ItemName { get; set; }
    public required string ItemType { get; set; }
    public Size GridSize { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
    public int MaxStack { get; set; }
    public bool IsTradeable { get; set; }
    public bool IsConsumable { get; set; }
    public int BaseValue { get; set; }
    public float Weight { get; set; }
}

/// <summary>
/// Response containing all item definitions
/// </summary>
public class ItemDefinitionsResponse
{
    public bool Success { get; set; }
    public List<ItemDefinitionResponse> Items { get; set; } = new();
}
