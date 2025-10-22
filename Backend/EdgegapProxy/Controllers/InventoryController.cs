using Microsoft.AspNetCore.Mvc;
using EdgegapProxy.Models.Inventory;
using EdgegapProxy.Services;
using System.Text.Json;

namespace EdgegapProxy.Controllers;

/// <summary>
/// Inventory management controller
/// Endpoints: /api/inventory/{playerId}, /api/items/definitions
/// </summary>
[ApiController]
[Route("api")]
public class InventoryController : ControllerBase
{
    private readonly DatabaseService _database;
    private readonly JwtService _jwt;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(DatabaseService database, JwtService jwt, ILogger<InventoryController> logger)
    {
        _database = database;
        _jwt = jwt;
        _logger = logger;
    }

    /// <summary>
    /// Load player inventory
    /// GET /api/inventory/{playerId}
    /// Authorization: Bearer {token}
    /// </summary>
    [HttpGet("inventory/{playerId}")]
    public async Task<IActionResult> GetInventory(string playerId)
    {
        try
        {
            // Validate JWT token
            var (authorized, requestingPlayerId) = ValidateAuthorization();
            if (!authorized || requestingPlayerId == null)
            {
                return Unauthorized(new { error = "Unauthorized" });
            }

            // Verify requesting user owns this inventory
            if (!Guid.TryParse(playerId, out var playerIdGuid))
            {
                return BadRequest(new { error = "Invalid player ID format" });
            }

            if (requestingPlayerId.Value != playerIdGuid)
            {
                return Unauthorized(new { error = "Unauthorized - cannot access another player's inventory" });
            }

            // Load inventory
            var (grid, lastUpdated, version) = await _database.LoadPlayerInventoryAsync(playerIdGuid);

            if (grid == null)
            {
                return NotFound(new { error = "Inventory not found" });
            }

            _logger.LogInformation("Loaded inventory for player {PlayerId} (version {Version})", playerId, version);

            return Ok(new LoadInventoryResponse
            {
                Success = true,
                Inventory = grid,
                LastUpdated = lastUpdated,
                Version = version
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory for player {PlayerId}", playerId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Save player inventory
    /// POST /api/inventory/{playerId}
    /// Authorization: Bearer {token}
    /// </summary>
    [HttpPost("inventory/{playerId}")]
    public async Task<IActionResult> SaveInventory(string playerId, [FromBody] SaveInventoryRequest request)
    {
        try
        {
            // Validate JWT token
            var (authorized, requestingPlayerId) = ValidateAuthorization();
            if (!authorized || requestingPlayerId == null)
            {
                return Unauthorized(new { error = "Unauthorized" });
            }

            // Verify requesting user owns this inventory
            if (!Guid.TryParse(playerId, out var playerIdGuid))
            {
                return BadRequest(new { error = "Invalid player ID format" });
            }

            if (requestingPlayerId.Value != playerIdGuid)
            {
                return Unauthorized(new { error = "Unauthorized - cannot modify another player's inventory" });
            }

            // Validate inventory data
            if (request.Inventory == null)
            {
                return BadRequest(new { error = "Inventory data is required" });
            }

            // Save inventory with optimistic locking
            var (success, newVersion) = await _database.SavePlayerInventoryAsync(
                playerIdGuid,
                request.Inventory,
                request.Version
            );

            if (!success)
            {
                // Version conflict - concurrent modification detected
                _logger.LogWarning("Version conflict saving inventory for player {PlayerId} - version {Version}", playerId, request.Version);
                return Conflict(new SaveInventoryResponse
                {
                    Success = false,
                    NewVersion = request.Version,
                    Timestamp = DateTime.UtcNow,
                    Error = "Version conflict - inventory was modified by another process"
                });
            }

            _logger.LogInformation("Saved inventory for player {PlayerId} (new version {Version})", playerId, newVersion);

            return Ok(new SaveInventoryResponse
            {
                Success = true,
                NewVersion = newVersion,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving inventory for player {PlayerId}", playerId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all item definitions
    /// GET /api/items/definitions
    /// </summary>
    [HttpGet("items/definitions")]
    public async Task<IActionResult> GetItemDefinitions()
    {
        try
        {
            var definitions = await _database.GetAllItemDefinitionsAsync();

            var response = new ItemDefinitionsResponse
            {
                Success = true,
                Items = definitions.Select(d => new ItemDefinitionResponse
                {
                    ItemId = d.ItemId,
                    ItemName = d.ItemName,
                    ItemType = d.ItemType,
                    GridSize = JsonSerializer.Deserialize<Size>(d.GridSizeJson) ?? new Size(),
                    Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(d.PropertiesJson) ?? new Dictionary<string, object>(),
                    MaxStack = d.MaxStack,
                    IsTradeable = d.IsTradeable,
                    IsConsumable = d.IsConsumable,
                    BaseValue = d.BaseValue,
                    Weight = d.Weight
                }).ToList()
            };

            _logger.LogInformation("Returned {Count} item definitions", response.Items.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading item definitions");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate authorization header and extract player ID
    /// </summary>
    private (bool Authorized, Guid? PlayerId) ValidateAuthorization()
    {
        // Get token from Authorization header
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return (false, null);
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Validate token
        var (valid, playerId, username) = _jwt.ValidateToken(token);

        if (!valid || playerId == null)
        {
            return (false, null);
        }

        return (true, playerId);
    }
}
