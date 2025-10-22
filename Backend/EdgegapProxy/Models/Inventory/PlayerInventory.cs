using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EdgegapProxy.Models.Inventory;

/// <summary>
/// Player inventory stored in database
/// </summary>
public class PlayerInventory
{
    [Key]
    public Guid PlayerId { get; set; }

    [Required]
    public required string CargoGridJson { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public int Version { get; set; } = 1;
}

/// <summary>
/// Cargo grid structure (Tetris-style inventory)
/// </summary>
public class CargoGrid
{
    public int Width { get; set; } = 10;
    public int Height { get; set; } = 10;
    public List<List<string?>> Cells { get; set; } = new();
    public Dictionary<string, ItemData> Items { get; set; } = new();
}

/// <summary>
/// Individual item instance data
/// </summary>
public class ItemData
{
    public required string ItemId { get; set; }
    public required string ItemType { get; set; }
    public int Quantity { get; set; }
    public Position Position { get; set; } = new();
    public int Rotation { get; set; }
    public Size Size { get; set; } = new();
}

/// <summary>
/// Grid position
/// </summary>
public class Position
{
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// Item size (grid cells)
/// </summary>
public class Size
{
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Request to load inventory
/// </summary>
public class LoadInventoryResponse
{
    public bool Success { get; set; }
    public CargoGrid? Inventory { get; set; }
    public DateTime LastUpdated { get; set; }
    public int Version { get; set; }
}

/// <summary>
/// Request to save inventory
/// </summary>
public class SaveInventoryRequest
{
    [Required]
    public required CargoGrid Inventory { get; set; }

    public int Version { get; set; }
}

/// <summary>
/// Response from save inventory
/// </summary>
public class SaveInventoryResponse
{
    public bool Success { get; set; }
    public int NewVersion { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Error { get; set; }
}
