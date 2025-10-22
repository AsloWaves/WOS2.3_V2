using Npgsql;
using System.Text.Json;
using EdgegapProxy.Models.Auth;
using EdgegapProxy.Models.Inventory;

namespace EdgegapProxy.Services;

/// <summary>
/// Database service for PostgreSQL operations
/// Handles player accounts, inventories, and item definitions
/// </summary>
public class DatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
        _logger = logger;
    }

    #region Authentication

    /// <summary>
    /// Create new player account
    /// </summary>
    public async Task<PlayerAccount?> CreatePlayerAccountAsync(string username, string passwordHash, string? email = null)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO player_accounts (username, password_hash, email)
                VALUES (@username, @password_hash, @email)
                RETURNING player_id, username, password_hash, email, created_at, last_login";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", username.ToLower());
            cmd.Parameters.AddWithValue("password_hash", passwordHash);
            cmd.Parameters.AddWithValue("email", (object?)email ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var account = new PlayerAccount
                {
                    PlayerId = reader.GetGuid(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    LastLogin = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                };

                // Initialize empty inventory
                await InitializePlayerInventoryAsync(account.PlayerId);

                return account;
            }

            return null;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
        {
            _logger.LogWarning("Username already exists: {Username}", username);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating player account: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Get player account by username
    /// </summary>
    public async Task<PlayerAccount?> GetPlayerAccountByUsernameAsync(string username)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT player_id, username, password_hash, email, created_at, last_login
                FROM player_accounts
                WHERE username = @username";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("username", username.ToLower());

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PlayerAccount
                {
                    PlayerId = reader.GetGuid(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    LastLogin = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player account: {Username}", username);
            throw;
        }
    }

    /// <summary>
    /// Update last login timestamp
    /// </summary>
    public async Task UpdateLastLoginAsync(Guid playerId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "UPDATE player_accounts SET last_login = NOW() WHERE player_id = @player_id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("player_id", playerId);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for player: {PlayerId}", playerId);
            throw;
        }
    }

    #endregion

    #region Inventory

    /// <summary>
    /// Initialize empty inventory for new player
    /// </summary>
    private async Task InitializePlayerInventoryAsync(Guid playerId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Create empty cargo grid
            var cargoGrid = new CargoGrid
            {
                Width = 10,
                Height = 10,
                Cells = new List<List<string?>>(),
                Items = new Dictionary<string, ItemData>()
            };

            // Initialize empty grid cells
            for (int y = 0; y < 10; y++)
            {
                var row = new List<string?>();
                for (int x = 0; x < 10; x++)
                {
                    row.Add(null);
                }
                cargoGrid.Cells.Add(row);
            }

            var gridJson = JsonSerializer.Serialize(cargoGrid);

            var sql = "INSERT INTO player_inventories (player_id, cargo_grid) VALUES (@player_id, @cargo_grid::jsonb)";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("player_id", playerId);
            cmd.Parameters.AddWithValue("cargo_grid", gridJson);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing inventory for player: {PlayerId}", playerId);
            throw;
        }
    }

    /// <summary>
    /// Load player inventory
    /// </summary>
    public async Task<(CargoGrid? Grid, DateTime LastUpdated, int Version)> LoadPlayerInventoryAsync(Guid playerId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT cargo_grid, last_updated, version
                FROM player_inventories
                WHERE player_id = @player_id";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("player_id", playerId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var gridJson = reader.GetString(0);
                var lastUpdated = reader.GetDateTime(1);
                var version = reader.GetInt32(2);

                var grid = JsonSerializer.Deserialize<CargoGrid>(gridJson);

                return (grid, lastUpdated, version);
            }

            return (null, DateTime.UtcNow, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory for player: {PlayerId}", playerId);
            throw;
        }
    }

    /// <summary>
    /// Save player inventory with optimistic locking
    /// </summary>
    public async Task<(bool Success, int NewVersion)> SavePlayerInventoryAsync(Guid playerId, CargoGrid inventory, int version)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var gridJson = JsonSerializer.Serialize(inventory);

            var sql = @"
                UPDATE player_inventories
                SET cargo_grid = @cargo_grid::jsonb,
                    last_updated = NOW(),
                    version = version + 1
                WHERE player_id = @player_id AND version = @version
                RETURNING version";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("player_id", playerId);
            cmd.Parameters.AddWithValue("cargo_grid", gridJson);
            cmd.Parameters.AddWithValue("version", version);

            var result = await cmd.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
            {
                var newVersion = Convert.ToInt32(result);
                return (true, newVersion);
            }

            // Version conflict - concurrent modification
            return (false, version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving inventory for player: {PlayerId}", playerId);
            throw;
        }
    }

    #endregion

    #region Item Definitions

    /// <summary>
    /// Get all item definitions
    /// </summary>
    public async Task<List<ItemDefinition>> GetAllItemDefinitionsAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT item_id, item_name, item_type, grid_size, properties,
                       max_stack, is_tradeable, is_consumable, base_value, weight
                FROM item_definitions
                ORDER BY item_name";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var items = new List<ItemDefinition>();

            while (await reader.ReadAsync())
            {
                items.Add(new ItemDefinition
                {
                    ItemId = reader.GetString(0),
                    ItemName = reader.GetString(1),
                    ItemType = reader.GetString(2),
                    GridSizeJson = reader.GetString(3),
                    PropertiesJson = reader.GetString(4),
                    MaxStack = reader.GetInt32(5),
                    IsTradeable = reader.GetBoolean(6),
                    IsConsumable = reader.GetBoolean(7),
                    BaseValue = reader.GetInt32(8),
                    Weight = reader.GetFloat(9)
                });
            }

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading item definitions");
            throw;
        }
    }

    #endregion

    #region Testing

    /// <summary>
    /// Test database connection
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return false;
        }
    }

    #endregion
}
