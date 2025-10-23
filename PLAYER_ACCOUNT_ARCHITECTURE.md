# Player Account & Inventory System Architecture

**Project**: Waves of Steel (WOS2.3_V2)
**Version**: 1.0
**Created**: 2025-10-21
**Status**: Design Phase

---

## Table of Contents

1. [Overview & Goals](#overview--goals)
2. [Database Schema (PostgreSQL)](#database-schema-postgresql)
3. [Backend API Endpoints](#backend-api-endpoints)
4. [Server-Side Architecture](#server-side-architecture)
5. [Client-Side Architecture](#client-side-architecture)
6. [Real-Time Item Usage Flow](#real-time-item-usage-flow)
7. [Synchronization Strategies](#synchronization-strategies)
8. [Performance Targets](#performance-targets)
9. [Security Considerations](#security-considerations)
10. [Testing Strategy](#testing-strategy)
11. [Implementation Phases](#implementation-phases)

---

## Overview & Goals

### System Purpose

The Player Account & Inventory System provides:
- **Authentication**: Secure login with JWT tokens
- **Persistent Storage**: Player data survives server restarts
- **Real-Time Inventory**: Tetris-style cargo management with <50ms operations
- **Item Usage During Gameplay**: Consumables, equipment, and cargo operations while at sea
- **Trade System**: Player-to-player item transfers with transaction safety
- **Port Interactions**: Transfer cargo between ship and port storage

### Design Principles

1. **Server-Authoritative**: Server validates ALL inventory operations (prevents cheating)
2. **In-Memory Performance**: Active inventories cached in RAM for instant access
3. **Optimistic UI**: Client updates immediately, server validates asynchronously
4. **Batched Persistence**: Dirty flag system reduces DB writes by 95%+
5. **Transaction Safety**: ACID compliance prevents item duplication
6. **Horizontal Scalability**: Stateless backend API for load balancing

### Key Requirements

**Real-Time Performance**:
- Item usage during combat: <50ms response time
- Inventory operations: <100ms validation
- Login authentication: <500ms total

**Reliability**:
- Zero item duplication (ACID transactions)
- No data loss on server crash (auto-save every 60s)
- Graceful degradation if backend API unavailable

**Gameplay Features**:
- ✅ Tetris-style cargo grid (10x10 default)
- ✅ Item usage from inventory (health potions, repair kits, ammo)
- ✅ Hotbar system (1-9 keys for quick access)
- ✅ Player-to-player trading
- ✅ Port storage transfers
- ✅ Ship-to-ship transfers (piracy/boarding)

---

## Database Schema (PostgreSQL)

### Why PostgreSQL?

- ✅ ACID compliance (prevents item duplication)
- ✅ Transaction support (critical for trades)
- ✅ JSONB columns (flexible item storage)
- ✅ Free tier on Render.com (same host as backend)
- ✅ Excellent C# support (Npgsql)

### Schema Design

#### **Table: player_accounts**
```sql
CREATE TABLE player_accounts (
    player_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW(),
    last_login TIMESTAMP,

    -- Indexes
    CONSTRAINT username_lowercase CHECK (username = LOWER(username))
);

CREATE INDEX idx_username ON player_accounts(username);
CREATE INDEX idx_last_login ON player_accounts(last_login);
```

**Purpose**: Store player authentication credentials

**Fields**:
- `player_id`: UUID primary key (globally unique)
- `username`: Lowercase, unique identifier (3-50 chars)
- `password_hash`: bcrypt hash (never store plain text)
- `email`: Optional, for password recovery
- `created_at`: Account creation timestamp
- `last_login`: Last successful login timestamp

---

#### **Table: player_inventories**
```sql
CREATE TABLE player_inventories (
    player_id UUID PRIMARY KEY REFERENCES player_accounts(player_id) ON DELETE CASCADE,
    cargo_grid JSONB NOT NULL DEFAULT '{"width":10,"height":10,"cells":[],"items":{}}',
    last_updated TIMESTAMP DEFAULT NOW(),
    version INT DEFAULT 1,  -- Optimistic locking

    -- Constraints
    CONSTRAINT valid_cargo_grid CHECK (jsonb_typeof(cargo_grid) = 'object')
);

CREATE INDEX idx_last_updated ON player_inventories(last_updated);
```

**Purpose**: Store player inventory state (Tetris cargo grid)

**Fields**:
- `player_id`: Foreign key to player_accounts
- `cargo_grid`: JSONB structure containing:
  ```json
  {
    "width": 10,
    "height": 10,
    "cells": [
      ["item_uuid_1", "item_uuid_1", null, null, ...],
      ["item_uuid_1", "item_uuid_1", null, null, ...],
      ...
    ],
    "items": {
      "item_uuid_1": {
        "itemType": "health_potion",
        "quantity": 5,
        "position": {"x": 0, "y": 0},
        "rotation": 0,
        "size": {"width": 2, "height": 2}
      }
    }
  }
  ```
- `last_updated`: Timestamp of last save
- `version`: Optimistic locking version (prevents concurrent modification conflicts)

**Grid-Based Storage Rationale**:
- ✅ Fast collision detection (O(1) grid lookup)
- ✅ Easy validation of item placement
- ✅ Efficient serialization (~1-2KB per inventory)
- ✅ Supports overlapping item detection

---

#### **Table: item_definitions**
```sql
CREATE TABLE item_definitions (
    item_id VARCHAR(50) PRIMARY KEY,
    item_name VARCHAR(100) NOT NULL,
    item_type VARCHAR(50) NOT NULL,  -- consumable, equipment, cargo, quest
    grid_size JSONB NOT NULL,  -- {"width": 2, "height": 3}
    properties JSONB DEFAULT '{}',
    max_stack INT DEFAULT 1,
    is_tradeable BOOLEAN DEFAULT true,
    is_consumable BOOLEAN DEFAULT false,

    -- Item value and weight
    base_value INT DEFAULT 0,
    weight FLOAT DEFAULT 0.0,

    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_item_type ON item_definitions(item_type);
```

**Purpose**: Define all item types available in the game

**Fields**:
- `item_id`: Unique item type identifier (e.g., "health_potion", "iron_ore")
- `item_name`: Display name (e.g., "Health Potion")
- `item_type`: Category (consumable, equipment, cargo, quest)
- `grid_size`: Tetris grid dimensions `{"width": 2, "height": 1}`
- `properties`: Custom item data (e.g., `{"healAmount": 50, "cooldown": 5}`)
- `max_stack`: Maximum stack size (1 for non-stackable)
- `is_tradeable`: Can be traded between players
- `is_consumable`: Can be used/consumed
- `base_value`: Default sell price
- `weight`: Weight in kg (affects ship speed)

**Example Item Definitions**:
```sql
INSERT INTO item_definitions (item_id, item_name, item_type, grid_size, properties, max_stack, is_consumable) VALUES
('health_potion', 'Health Potion', 'consumable', '{"width":1,"height":1}', '{"healAmount":50}', 10, true),
('repair_kit', 'Repair Kit', 'consumable', '{"width":2,"height":1}', '{"repairAmount":25}', 5, true),
('iron_ore', 'Iron Ore', 'cargo', '{"width":2,"height":2}', '{}', 50, false),
('cannon', 'Ship Cannon', 'equipment', '{"width":3,"height":2}', '{"damage":100,"range":500}', 1, false);
```

---

#### **Table: trade_log**
```sql
CREATE TABLE trade_log (
    trade_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player1_id UUID REFERENCES player_accounts(player_id),
    player2_id UUID REFERENCES player_accounts(player_id),
    player1_items JSONB NOT NULL,  -- Array of item IDs given by player1
    player2_items JSONB NOT NULL,  -- Array of item IDs given by player2
    trade_timestamp TIMESTAMP DEFAULT NOW(),

    -- Status tracking
    status VARCHAR(20) DEFAULT 'completed',  -- completed, rolled_back

    CONSTRAINT different_players CHECK (player1_id != player2_id)
);

CREATE INDEX idx_player1_trades ON trade_log(player1_id, trade_timestamp);
CREATE INDEX idx_player2_trades ON trade_log(player2_id, trade_timestamp);
```

**Purpose**: Audit trail for all player trades

**Fields**:
- `trade_id`: Unique trade identifier
- `player1_id`, `player2_id`: Participating players
- `player1_items`, `player2_items`: Items exchanged (JSON arrays)
- `trade_timestamp`: When trade occurred
- `status`: completed or rolled_back (for debugging)

**Use Cases**:
- Debugging item duplication issues
- Player support (restore lost items)
- Economy analytics (track item flow)

---

#### **Table: port_storage**
```sql
CREATE TABLE port_storage (
    storage_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID REFERENCES player_accounts(player_id) ON DELETE CASCADE,
    port_name VARCHAR(100) NOT NULL,
    cargo_grid JSONB NOT NULL DEFAULT '{"width":20,"height":20,"cells":[],"items":{}}',
    last_accessed TIMESTAMP DEFAULT NOW(),

    UNIQUE(player_id, port_name)
);

CREATE INDEX idx_player_port ON port_storage(player_id, port_name);
```

**Purpose**: Store player cargo at ports (larger than ship inventory)

**Fields**:
- `player_id`: Owner of the storage
- `port_name`: Port location (e.g., "Port_Nassau", "Port_Boston")
- `cargo_grid`: Same structure as player_inventories (but 20x20)
- `last_accessed`: Last time player accessed this storage

**Design Notes**:
- Each player gets unique storage per port
- Larger grid size (20x20) than ship inventory (10x10)
- Can transfer items between ship and port storage

---

### Database Deployment

**Host**: Render.com (same as backend API)
**Plan**: Free tier (PostgreSQL 0.25GB RAM, 1GB storage)
**Connection**: SSL required, connection pooling enabled
**Backup**: Daily automated backups (Render.com feature)

**Connection String** (stored in backend env vars):
```
DATABASE_URL=postgresql://user:password@hostname:5432/database?ssl=true
```

---

## Backend API Endpoints

### Overview

**Host**: `https://wos-edgegap-proxy.onrender.com` (extend existing backend)
**Framework**: Node.js + Express
**Database Client**: `pg` (node-postgres)
**Authentication**: JWT tokens (HS256)

### API Structure

```
Backend/
├── routes/
│   ├── auth.js          # Authentication endpoints
│   ├── inventory.js     # Inventory CRUD
│   ├── items.js         # Item definitions
│   └── trade.js         # Trading system
├── middleware/
│   ├── authenticate.js  # JWT validation
│   └── validate.js      # Input validation
├── models/
│   ├── Player.js        # Player account model
│   ├── Inventory.js     # Inventory model
│   └── Item.js          # Item definition model
└── utils/
    ├── jwt.js           # Token generation/validation
    └── bcrypt.js        # Password hashing
```

---

### Authentication Endpoints

#### **POST /api/auth/register**

**Purpose**: Create new player account

**Request**:
```json
{
  "username": "player123",
  "password": "securePassword123!",
  "email": "player@example.com"  // Optional
}
```

**Validation**:
- Username: 3-50 chars, alphanumeric + underscore, lowercase
- Password: 8-64 chars, must contain letter + number
- Email: Valid email format (if provided)

**Response** (201 Created):
```json
{
  "success": true,
  "playerId": "uuid-here",
  "username": "player123",
  "token": "jwt-token-here"
}
```

**Errors**:
- 400: Invalid input (username/password doesn't meet requirements)
- 409: Username already exists

**Implementation**:
```javascript
// Backend/routes/auth.js
router.post('/register', async (req, res) => {
  const { username, password, email } = req.body;

  // Validate input
  if (!isValidUsername(username)) {
    return res.status(400).json({ error: 'Invalid username' });
  }

  // Check if username exists
  const existing = await db.query('SELECT player_id FROM player_accounts WHERE username = $1', [username.toLowerCase()]);
  if (existing.rows.length > 0) {
    return res.status(409).json({ error: 'Username already exists' });
  }

  // Hash password
  const passwordHash = await bcrypt.hash(password, 10);

  // Create account
  const result = await db.query(
    'INSERT INTO player_accounts (username, password_hash, email) VALUES ($1, $2, $3) RETURNING player_id',
    [username.toLowerCase(), passwordHash, email]
  );

  const playerId = result.rows[0].player_id;

  // Initialize empty inventory
  await db.query(
    'INSERT INTO player_inventories (player_id) VALUES ($1)',
    [playerId]
  );

  // Generate JWT token
  const token = jwt.sign({ playerId, username }, JWT_SECRET, { expiresIn: '7d' });

  res.status(201).json({ success: true, playerId, username, token });
});
```

---

#### **POST /api/auth/login**

**Purpose**: Authenticate player and get JWT token

**Request**:
```json
{
  "username": "player123",
  "password": "securePassword123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "playerId": "uuid-here",
  "username": "player123",
  "token": "jwt-token-here",
  "lastLogin": "2025-10-21T12:00:00Z"
}
```

**Errors**:
- 400: Missing username or password
- 401: Invalid credentials

**Implementation**:
```javascript
router.post('/login', async (req, res) => {
  const { username, password } = req.body;

  // Find player
  const result = await db.query(
    'SELECT player_id, username, password_hash FROM player_accounts WHERE username = $1',
    [username.toLowerCase()]
  );

  if (result.rows.length === 0) {
    return res.status(401).json({ error: 'Invalid credentials' });
  }

  const player = result.rows[0];

  // Verify password
  const validPassword = await bcrypt.compare(password, player.password_hash);
  if (!validPassword) {
    return res.status(401).json({ error: 'Invalid credentials' });
  }

  // Update last login
  await db.query('UPDATE player_accounts SET last_login = NOW() WHERE player_id = $1', [player.player_id]);

  // Generate JWT
  const token = jwt.sign({ playerId: player.player_id, username: player.username }, JWT_SECRET, { expiresIn: '7d' });

  res.json({ success: true, playerId: player.player_id, username: player.username, token });
});
```

---

#### **POST /api/auth/validate**

**Purpose**: Validate JWT token (called by game server on client connect)

**Request Headers**:
```
Authorization: Bearer <jwt-token>
```

**Response** (200 OK):
```json
{
  "valid": true,
  "playerId": "uuid-here",
  "username": "player123"
}
```

**Errors**:
- 401: Invalid or expired token

**Implementation**:
```javascript
router.post('/validate', authenticate, (req, res) => {
  // If middleware passes, token is valid
  res.json({ valid: true, playerId: req.user.playerId, username: req.user.username });
});

// Middleware
function authenticate(req, res, next) {
  const token = req.headers.authorization?.split(' ')[1];
  if (!token) return res.status(401).json({ error: 'No token provided' });

  try {
    const decoded = jwt.verify(token, JWT_SECRET);
    req.user = decoded;
    next();
  } catch (err) {
    return res.status(401).json({ error: 'Invalid token' });
  }
}
```

---

### Inventory Endpoints

#### **GET /api/inventory/:playerId**

**Purpose**: Load player inventory (called on server connect)

**Request Headers**:
```
Authorization: Bearer <jwt-token>
```

**Response** (200 OK):
```json
{
  "success": true,
  "inventory": {
    "width": 10,
    "height": 10,
    "cells": [ ... ],
    "items": { ... }
  },
  "lastUpdated": "2025-10-21T12:00:00Z",
  "version": 5
}
```

**Errors**:
- 401: Unauthorized (invalid token or wrong playerId)
- 404: Inventory not found

**Implementation**:
```javascript
router.get('/:playerId', authenticate, async (req, res) => {
  const { playerId } = req.params;

  // Verify requesting user owns this inventory
  if (req.user.playerId !== playerId) {
    return res.status(401).json({ error: 'Unauthorized' });
  }

  const result = await db.query(
    'SELECT cargo_grid, last_updated, version FROM player_inventories WHERE player_id = $1',
    [playerId]
  );

  if (result.rows.length === 0) {
    return res.status(404).json({ error: 'Inventory not found' });
  }

  res.json({
    success: true,
    inventory: result.rows[0].cargo_grid,
    lastUpdated: result.rows[0].last_updated,
    version: result.rows[0].version
  });
});
```

---

#### **POST /api/inventory/:playerId**

**Purpose**: Save player inventory (called by auto-save system)

**Request Headers**:
```
Authorization: Bearer <jwt-token>
```

**Request Body**:
```json
{
  "inventory": {
    "width": 10,
    "height": 10,
    "cells": [ ... ],
    "items": { ... }
  },
  "version": 5  // For optimistic locking
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "newVersion": 6,
  "timestamp": "2025-10-21T12:05:00Z"
}
```

**Errors**:
- 401: Unauthorized
- 409: Version conflict (concurrent modification)

**Implementation**:
```javascript
router.post('/:playerId', authenticate, async (req, res) => {
  const { playerId } = req.params;
  const { inventory, version } = req.body;

  // Verify ownership
  if (req.user.playerId !== playerId) {
    return res.status(401).json({ error: 'Unauthorized' });
  }

  // Optimistic locking: update only if version matches
  const result = await db.query(
    `UPDATE player_inventories
     SET cargo_grid = $1, last_updated = NOW(), version = version + 1
     WHERE player_id = $2 AND version = $3
     RETURNING version`,
    [JSON.stringify(inventory), playerId, version]
  );

  if (result.rows.length === 0) {
    // Version mismatch - concurrent modification detected
    return res.status(409).json({ error: 'Version conflict' });
  }

  res.json({ success: true, newVersion: result.rows[0].version, timestamp: new Date() });
});
```

---

### Item Definition Endpoints

#### **GET /api/items/definitions**

**Purpose**: Get all item type definitions (cached by client)

**Response** (200 OK):
```json
{
  "success": true,
  "items": [
    {
      "itemId": "health_potion",
      "itemName": "Health Potion",
      "itemType": "consumable",
      "gridSize": {"width": 1, "height": 1},
      "properties": {"healAmount": 50},
      "maxStack": 10,
      "isTradeable": true,
      "isConsumable": true
    },
    ...
  ]
}
```

**Implementation**:
```javascript
router.get('/definitions', async (req, res) => {
  const result = await db.query('SELECT * FROM item_definitions ORDER BY item_name');

  const items = result.rows.map(row => ({
    itemId: row.item_id,
    itemName: row.item_name,
    itemType: row.item_type,
    gridSize: row.grid_size,
    properties: row.properties,
    maxStack: row.max_stack,
    isTradeable: row.is_tradeable,
    isConsumable: row.is_consumable,
    baseValue: row.base_value,
    weight: row.weight
  }));

  res.json({ success: true, items });
});
```

**Client Caching**:
- Client caches item definitions on startup
- Reduces API calls (items don't change frequently)
- Include version hash to detect updates

---

### Trade Endpoints

#### **POST /api/trade/execute**

**Purpose**: Execute trade between two players (ACID transaction)

**Request Headers**:
```
Authorization: Bearer <jwt-token>
```

**Request Body**:
```json
{
  "player1Id": "uuid-1",
  "player2Id": "uuid-2",
  "player1Items": ["item-uuid-a", "item-uuid-b"],
  "player2Items": ["item-uuid-c"],
  "player1Version": 5,
  "player2Version": 12
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "tradeId": "trade-uuid",
  "player1NewVersion": 6,
  "player2NewVersion": 13
}
```

**Errors**:
- 400: Invalid trade (item not found, ownership mismatch)
- 409: Version conflict (inventory changed during trade)

**Implementation** (CRITICAL - uses PostgreSQL transaction):
```javascript
router.post('/execute', authenticate, async (req, res) => {
  const { player1Id, player2Id, player1Items, player2Items, player1Version, player2Version } = req.body;

  const client = await db.pool.connect();

  try {
    await client.query('BEGIN');

    // 1. Load both inventories with version check
    const inv1 = await client.query(
      'SELECT cargo_grid, version FROM player_inventories WHERE player_id = $1 AND version = $2 FOR UPDATE',
      [player1Id, player1Version]
    );
    const inv2 = await client.query(
      'SELECT cargo_grid, version FROM player_inventories WHERE player_id = $1 AND version = $2 FOR UPDATE',
      [player2Id, player2Version]
    );

    if (inv1.rows.length === 0 || inv2.rows.length === 0) {
      await client.query('ROLLBACK');
      return res.status(409).json({ error: 'Version conflict' });
    }

    // 2. Validate item ownership
    const grid1 = inv1.rows[0].cargo_grid;
    const grid2 = inv2.rows[0].cargo_grid;

    if (!validateOwnership(grid1, player1Items) || !validateOwnership(grid2, player2Items)) {
      await client.query('ROLLBACK');
      return res.status(400).json({ error: 'Invalid item ownership' });
    }

    // 3. Transfer items
    const newGrid1 = transferItems(grid1, grid2, player1Items, player2Items);
    const newGrid2 = transferItems(grid2, grid1, player2Items, player1Items);

    // 4. Update both inventories
    await client.query(
      'UPDATE player_inventories SET cargo_grid = $1, version = version + 1 WHERE player_id = $2',
      [JSON.stringify(newGrid1), player1Id]
    );
    await client.query(
      'UPDATE player_inventories SET cargo_grid = $1, version = version + 1 WHERE player_id = $2',
      [JSON.stringify(newGrid2), player2Id]
    );

    // 5. Log trade
    const tradeResult = await client.query(
      'INSERT INTO trade_log (player1_id, player2_id, player1_items, player2_items) VALUES ($1, $2, $3, $4) RETURNING trade_id',
      [player1Id, player2Id, JSON.stringify(player1Items), JSON.stringify(player2Items)]
    );

    await client.query('COMMIT');

    res.json({
      success: true,
      tradeId: tradeResult.rows[0].trade_id,
      player1NewVersion: player1Version + 1,
      player2NewVersion: player2Version + 1
    });

  } catch (err) {
    await client.query('ROLLBACK');
    console.error('Trade error:', err);
    res.status(500).json({ error: 'Trade failed' });
  } finally {
    client.release();
  }
});
```

---

## Server-Side Architecture

### Overview

**Platform**: Unity Mirror (KCP transport)
**Purpose**: In-memory inventory cache, real-time validation, auto-save system
**Performance**: <50ms item usage, <100ms inventory operations

### Component Structure

```
Assets/Scripts/
├── Networking/
│   ├── AccountManager.cs        # JWT validation, session management
│   └── AutoSaveSystem.cs        # Periodic dirty flag saves
├── Inventory/
│   ├── ServerInventoryManager.cs   # In-memory cache, validation
│   ├── ItemUsageHandler.cs         # Real-time item consumption
│   ├── InventorySerializer.cs      # JSON serialization
│   └── Models/
│       ├── CargoGrid.cs            # Grid data structure
│       ├── ItemData.cs             # Item instance data
│       └── ItemDefinition.cs       # Item type definition
└── Trading/
    └── TradeManager.cs             # Player-to-player trades
```

---

### AccountManager Component

**Purpose**: Validate JWT tokens on client connect, load player data

**File**: `Assets/Scripts/Networking/AccountManager.cs`

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Networking;

namespace WOS.Networking
{
    /// <summary>
    /// Manages player authentication and account session
    /// Validates JWT tokens with backend API
    /// </summary>
    public class AccountManager : NetworkBehaviour
    {
        [Header("Backend Configuration")]
        [SerializeField] private string backendApiUrl = "https://wos-edgegap-proxy.onrender.com";

        [Header("Player Data")]
        [SyncVar] public Guid playerId;
        [SyncVar] public string playerUsername;

        private string authToken;
        private bool isAuthenticated = false;

        #region Server-Side Authentication

        /// <summary>
        /// Called when client connects to server
        /// Validates JWT token with backend API
        /// </summary>
        [Server]
        public void ValidateAuthToken(string token)
        {
            authToken = token;
            StartCoroutine(ValidateTokenCoroutine());
        }

        [Server]
        private IEnumerator ValidateTokenCoroutine()
        {
            string url = $"{backendApiUrl}/api/auth/validate";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                ValidationResponse response = JsonUtility.FromJson<ValidationResponse>(json);

                if (response.valid)
                {
                    playerId = new Guid(response.playerId);
                    playerUsername = response.username;
                    isAuthenticated = true;

                    Debug.Log($"[AccountManager] ✅ Player authenticated: {playerUsername} ({playerId})");

                    // Load player inventory
                    GetComponent<ServerInventoryManager>().LoadInventoryFromBackend(playerId, authToken);
                }
                else
                {
                    Debug.LogError("[AccountManager] ❌ Token validation failed");
                    connectionToClient.Disconnect();
                }
            }
            else
            {
                Debug.LogError($"[AccountManager] ❌ Backend API error: {request.error}");
                connectionToClient.Disconnect();
            }
        }

        #endregion

        #region Client-Side

        /// <summary>
        /// Called by client to send auth token to server
        /// </summary>
        [Command]
        public void CmdAuthenticateWithToken(string token)
        {
            ValidateAuthToken(token);
        }

        #endregion

        [Serializable]
        private class ValidationResponse
        {
            public bool valid;
            public string playerId;
            public string username;
        }
    }
}
```

---

### ServerInventoryManager Component

**Purpose**: In-memory inventory cache, real-time validation, dirty flag system

**File**: `Assets/Scripts/Inventory/ServerInventoryManager.cs`

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Networking;

namespace WOS.Inventory
{
    /// <summary>
    /// Server-authoritative inventory manager
    /// In-memory cache for instant operations (<50ms)
    /// Dirty flag system for batched saves (every 60s)
    /// </summary>
    public class ServerInventoryManager : NetworkBehaviour
    {
        [Header("Backend Configuration")]
        [SerializeField] private string backendApiUrl = "https://wos-edgegap-proxy.onrender.com";

        [Header("Inventory State")]
        private CargoGrid cargoGrid;
        private int version = 1;
        private bool isDirty = false;
        private DateTime lastSaved;

        private string authToken;
        private Guid playerId;

        #region Server - Load from Backend

        /// <summary>
        /// Load inventory from backend API
        /// Called once when player connects
        /// </summary>
        [Server]
        public void LoadInventoryFromBackend(Guid playerId, string token)
        {
            this.playerId = playerId;
            this.authToken = token;
            StartCoroutine(LoadInventoryCoroutine());
        }

        [Server]
        private IEnumerator LoadInventoryCoroutine()
        {
            string url = $"{backendApiUrl}/api/inventory/{playerId}";

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                InventoryResponse response = JsonUtility.FromJson<InventoryResponse>(json);

                cargoGrid = JsonUtility.FromJson<CargoGrid>(JsonUtility.ToJson(response.inventory));
                version = response.version;
                lastSaved = DateTime.UtcNow;

                Debug.Log($"[ServerInventory] ✅ Loaded inventory for {playerId} (version {version})");

                // Sync to client
                RpcSyncFullInventory(JsonUtility.ToJson(cargoGrid));
            }
            else
            {
                Debug.LogError($"[ServerInventory] ❌ Failed to load inventory: {request.error}");

                // Initialize empty inventory
                cargoGrid = new CargoGrid(10, 10);
                RpcSyncFullInventory(JsonUtility.ToJson(cargoGrid));
            }
        }

        #endregion

        #region Server - Save to Backend

        /// <summary>
        /// Save inventory to backend (called by AutoSaveSystem)
        /// Only saves if isDirty = true
        /// </summary>
        [Server]
        public void SaveInventoryToBackend()
        {
            if (!isDirty) return;

            StartCoroutine(SaveInventoryCoroutine());
        }

        [Server]
        private IEnumerator SaveInventoryCoroutine()
        {
            string url = $"{backendApiUrl}/api/inventory/{playerId}";

            string json = JsonUtility.ToJson(new SaveRequest {
                inventory = cargoGrid,
                version = version
            });

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                SaveResponse response = JsonUtility.FromJson<SaveResponse>(request.downloadHandler.text);
                version = response.newVersion;
                isDirty = false;
                lastSaved = DateTime.UtcNow;

                Debug.Log($"[ServerInventory] ✅ Saved inventory (new version {version})");
            }
            else if (request.responseCode == 409)
            {
                Debug.LogError("[ServerInventory] ⚠️ Version conflict - reloading from backend");
                LoadInventoryFromBackend(playerId, authToken);
            }
            else
            {
                Debug.LogError($"[ServerInventory] ❌ Save failed: {request.error}");
            }
        }

        #endregion

        #region Server - Inventory Operations

        /// <summary>
        /// Use consumable item (health potion, repair kit, etc.)
        /// Real-time operation (<50ms) - NO database call
        /// </summary>
        [Server]
        public bool UseItem(Guid itemId, int quantity)
        {
            if (cargoGrid == null) return false;

            // Find item in inventory
            if (!cargoGrid.items.ContainsKey(itemId)) return false;

            ItemData item = cargoGrid.items[itemId];

            // Validate quantity
            if (item.quantity < quantity) return false;

            // Consume item
            item.quantity -= quantity;
            isDirty = true;

            if (item.quantity == 0)
            {
                // Remove item completely
                RemoveItemFromGrid(itemId);
                cargoGrid.items.Remove(itemId);
            }

            // Sync to client
            RpcItemUsed(itemId, quantity, item.quantity);

            Debug.Log($"[ServerInventory] 🔥 Used item {itemId} x{quantity} (remaining: {item.quantity})");
            return true;
        }

        /// <summary>
        /// Move item to new position (Tetris drag-and-drop)
        /// </summary>
        [Server]
        public bool MoveItem(Guid itemId, Vector2Int newPosition, int rotation)
        {
            if (cargoGrid == null) return false;
            if (!cargoGrid.items.ContainsKey(itemId)) return false;

            ItemData item = cargoGrid.items[itemId];
            Vector2Int oldPosition = item.position;
            int oldRotation = item.rotation;

            // Validate new position (collision check)
            if (!IsValidPlacement(itemId, newPosition, rotation))
            {
                RpcRejectMove(itemId);
                return false;
            }

            // Remove from old position
            RemoveItemFromGrid(itemId);

            // Update item data
            item.position = newPosition;
            item.rotation = rotation;

            // Place in new position
            PlaceItemOnGrid(itemId, item);

            isDirty = true;

            // Sync to client
            RpcItemMoved(itemId, newPosition, rotation);

            Debug.Log($"[ServerInventory] ✅ Moved item {itemId} from {oldPosition} to {newPosition}");
            return true;
        }

        /// <summary>
        /// Add new item to inventory (loot, purchase, etc.)
        /// </summary>
        [Server]
        public bool AddItem(string itemType, int quantity, Vector2Int position)
        {
            // Find item definition
            ItemDefinition definition = GetItemDefinition(itemType);
            if (definition == null) return false;

            // Check if item can stack with existing
            Guid existingItemId = FindStackableItem(itemType);
            if (existingItemId != Guid.Empty)
            {
                ItemData existing = cargoGrid.items[existingItemId];
                int remainingSpace = definition.maxStack - existing.quantity;

                if (remainingSpace >= quantity)
                {
                    // Stack completely
                    existing.quantity += quantity;
                    isDirty = true;
                    RpcItemQuantityChanged(existingItemId, existing.quantity);
                    return true;
                }
            }

            // Create new item instance
            Guid newItemId = Guid.NewGuid();
            ItemData newItem = new ItemData {
                itemId = newItemId,
                itemType = itemType,
                quantity = quantity,
                position = position,
                rotation = 0,
                size = definition.gridSize
            };

            // Validate placement
            if (!IsValidPlacement(newItemId, position, 0, newItem.size))
            {
                return false;
            }

            // Add to inventory
            cargoGrid.items[newItemId] = newItem;
            PlaceItemOnGrid(newItemId, newItem);
            isDirty = true;

            // Sync to client
            RpcItemAdded(newItemId, JsonUtility.ToJson(newItem));

            Debug.Log($"[ServerInventory] ✅ Added item {itemType} x{quantity} at {position}");
            return true;
        }

        #endregion

        #region Grid Management

        private bool IsValidPlacement(Guid itemId, Vector2Int position, int rotation, Vector2Int? customSize = null)
        {
            ItemData item = cargoGrid.items.ContainsKey(itemId) ? cargoGrid.items[itemId] : null;
            Vector2Int size = customSize ?? (item?.size ?? Vector2Int.zero);

            // Apply rotation to size
            if (rotation == 90 || rotation == 270)
            {
                size = new Vector2Int(size.y, size.x);
            }

            // Check bounds
            if (position.x < 0 || position.y < 0 ||
                position.x + size.x > cargoGrid.width ||
                position.y + size.y > cargoGrid.height)
            {
                return false;
            }

            // Check collisions
            for (int y = position.y; y < position.y + size.y; y++)
            {
                for (int x = position.x; x < position.x + size.x; x++)
                {
                    Guid cellItemId = cargoGrid.cells[y][x];
                    if (cellItemId != Guid.Empty && cellItemId != itemId)
                    {
                        return false;  // Collision
                    }
                }
            }

            return true;
        }

        private void PlaceItemOnGrid(Guid itemId, ItemData item)
        {
            Vector2Int size = item.size;
            if (item.rotation == 90 || item.rotation == 270)
            {
                size = new Vector2Int(size.y, size.x);
            }

            for (int y = item.position.y; y < item.position.y + size.y; y++)
            {
                for (int x = item.position.x; x < item.position.x + size.x; x++)
                {
                    cargoGrid.cells[y][x] = itemId;
                }
            }
        }

        private void RemoveItemFromGrid(Guid itemId)
        {
            for (int y = 0; y < cargoGrid.height; y++)
            {
                for (int x = 0; x < cargoGrid.width; x++)
                {
                    if (cargoGrid.cells[y][x] == itemId)
                    {
                        cargoGrid.cells[y][x] = Guid.Empty;
                    }
                }
            }
        }

        private Guid FindStackableItem(string itemType)
        {
            foreach (var kvp in cargoGrid.items)
            {
                if (kvp.Value.itemType == itemType)
                {
                    ItemDefinition definition = GetItemDefinition(itemType);
                    if (definition != null && kvp.Value.quantity < definition.maxStack)
                    {
                        return kvp.Key;
                    }
                }
            }
            return Guid.Empty;
        }

        private ItemDefinition GetItemDefinition(string itemType)
        {
            // TODO: Load from ItemDefinitionManager
            return null;
        }

        #endregion

        #region Client RPCs

        [ClientRpc]
        private void RpcSyncFullInventory(string json)
        {
            // Client receives full inventory state
        }

        [ClientRpc]
        private void RpcItemUsed(Guid itemId, int quantityUsed, int remaining)
        {
            // Client updates UI for item consumption
        }

        [ClientRpc]
        private void RpcItemMoved(Guid itemId, Vector2Int newPosition, int rotation)
        {
            // Client confirms item move
        }

        [ClientRpc]
        private void RpcRejectMove(Guid itemId)
        {
            // Client rolls back optimistic move
        }

        [ClientRpc]
        private void RpcItemAdded(Guid itemId, string itemJson)
        {
            // Client adds new item to UI
        }

        [ClientRpc]
        private void RpcItemQuantityChanged(Guid itemId, int newQuantity)
        {
            // Client updates item stack quantity
        }

        #endregion

        #region Data Models

        [Serializable]
        private class InventoryResponse
        {
            public bool success;
            public CargoGrid inventory;
            public int version;
        }

        [Serializable]
        private class SaveRequest
        {
            public CargoGrid inventory;
            public int version;
        }

        [Serializable]
        private class SaveResponse
        {
            public bool success;
            public int newVersion;
        }

        #endregion
    }
}
```

---

### AutoSaveSystem Component

**Purpose**: Periodic dirty flag saves (every 60s)

**File**: `Assets/Scripts/Networking/AutoSaveSystem.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace WOS.Networking
{
    /// <summary>
    /// Periodically saves dirty inventories to backend
    /// Reduces database writes by 95%+ (batched saves)
    /// </summary>
    public class AutoSaveSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float saveIntervalSeconds = 60f;

        private void Start()
        {
            if (NetworkServer.active)
            {
                StartCoroutine(AutoSaveCoroutine());
            }
        }

        private IEnumerator AutoSaveCoroutine()
        {
            while (NetworkServer.active)
            {
                yield return new WaitForSeconds(saveIntervalSeconds);

                Debug.Log("[AutoSave] 🔄 Starting auto-save...");

                int savedCount = 0;

                // Find all ServerInventoryManager components
                foreach (var inventory in FindObjectsOfType<Inventory.ServerInventoryManager>())
                {
                    if (inventory.isServer)
                    {
                        inventory.SaveInventoryToBackend();
                        savedCount++;
                    }
                }

                Debug.Log($"[AutoSave] ✅ Saved {savedCount} inventories");
            }
        }

        private void OnApplicationQuit()
        {
            if (NetworkServer.active)
            {
                Debug.Log("[AutoSave] 🔄 Server shutdown - saving all inventories...");

                foreach (var inventory in FindObjectsOfType<Inventory.ServerInventoryManager>())
                {
                    if (inventory.isServer)
                    {
                        inventory.SaveInventoryToBackend();
                    }
                }
            }
        }
    }
}
```

---

## Client-Side Architecture

### Overview

**Purpose**: Login UI, inventory visualization, optimistic updates
**Performance**: Instant UI feedback, server validation asynchronous

### Component Structure

```
Assets/Scripts/
├── UI/
│   ├── LoginPanel.cs               # Username/password login
│   ├── InventoryGridUI.cs          # Tetris grid visualization
│   ├── ItemSlotUI.cs               # Individual item display
│   ├── HotbarController.cs         # 1-9 hotkeys for items
│   └── ItemTooltip.cs              # Item info on hover
└── Inventory/
    └── ClientInventoryManager.cs   # Client-side inventory cache
```

---

### LoginPanel Component

**Purpose**: Authenticate with backend API, store JWT token

**File**: `Assets/Scripts/UI/LoginPanel.cs`

```csharp
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Mirror;

namespace WOS.UI
{
    /// <summary>
    /// Login screen - authenticate with backend API
    /// Stores JWT token in PlayerPrefs
    /// </summary>
    public class LoginPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private TMP_Text errorText;

        [Header("Backend Configuration")]
        [SerializeField] private string backendApiUrl = "https://wos-edgegap-proxy.onrender.com";

        private void Start()
        {
            loginButton.onClick.AddListener(OnLoginButtonClick);
            registerButton.onClick.AddListener(OnRegisterButtonClick);
            errorText.text = "";

            // Auto-fill if token exists
            string existingToken = PlayerPrefs.GetString("auth_token", "");
            if (!string.IsNullOrEmpty(existingToken))
            {
                Debug.Log("[Login] Found existing token - attempting auto-login");
                StartCoroutine(ValidateExistingToken(existingToken));
            }
        }

        private void OnLoginButtonClick()
        {
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter username and password");
                return;
            }

            StartCoroutine(LoginCoroutine(username, password));
        }

        private void OnRegisterButtonClick()
        {
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter username and password");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Password must be at least 8 characters");
                return;
            }

            StartCoroutine(RegisterCoroutine(username, password));
        }

        private IEnumerator LoginCoroutine(string username, string password)
        {
            loginButton.interactable = false;
            errorText.text = "Logging in...";

            string url = $"{backendApiUrl}/api/auth/login";
            string json = JsonUtility.ToJson(new LoginRequest { username = username, password = password });

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    // Store token
                    PlayerPrefs.SetString("auth_token", response.token);
                    PlayerPrefs.SetString("player_id", response.playerId);
                    PlayerPrefs.SetString("username", response.username);
                    PlayerPrefs.Save();

                    Debug.Log($"[Login] ✅ Logged in as {response.username}");

                    // Connect to game server
                    ConnectToGameServer(response.token);
                }
                else
                {
                    ShowError("Login failed");
                }
            }
            else
            {
                ShowError($"Login error: {request.error}");
            }

            loginButton.interactable = true;
        }

        private IEnumerator RegisterCoroutine(string username, string password)
        {
            registerButton.interactable = false;
            errorText.text = "Creating account...";

            string url = $"{backendApiUrl}/api/auth/register";
            string json = JsonUtility.ToJson(new RegisterRequest { username = username, password = password });

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);

                if (response.success)
                {
                    // Store token
                    PlayerPrefs.SetString("auth_token", response.token);
                    PlayerPrefs.SetString("player_id", response.playerId);
                    PlayerPrefs.SetString("username", response.username);
                    PlayerPrefs.Save();

                    Debug.Log($"[Login] ✅ Account created: {response.username}");

                    // Connect to game server
                    ConnectToGameServer(response.token);
                }
                else
                {
                    ShowError("Registration failed");
                }
            }
            else if (request.responseCode == 409)
            {
                ShowError("Username already exists");
            }
            else
            {
                ShowError($"Registration error: {request.error}");
            }

            registerButton.interactable = true;
        }

        private IEnumerator ValidateExistingToken(string token)
        {
            errorText.text = "Validating session...";

            string url = $"{backendApiUrl}/api/auth/validate";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ValidationResponse response = JsonUtility.FromJson<ValidationResponse>(request.downloadHandler.text);

                if (response.valid)
                {
                    Debug.Log($"[Login] ✅ Token valid - auto-login as {response.username}");
                    ConnectToGameServer(token);
                }
                else
                {
                    PlayerPrefs.DeleteKey("auth_token");
                    errorText.text = "Session expired - please login";
                }
            }
            else
            {
                PlayerPrefs.DeleteKey("auth_token");
                errorText.text = "Session expired - please login";
            }
        }

        private void ConnectToGameServer(string token)
        {
            // Get server IP from ServerConfig
            string serverAddress = "530c50ac1da5.pr.edgegap.net";  // Or use ServerBrowserManager

            // Store token for AccountManager to use
            PlayerPrefs.SetString("auth_token", token);

            // Connect to game server
            NetworkManager.singleton.networkAddress = serverAddress;
            NetworkManager.singleton.StartClient();

            // Hide login panel
            gameObject.SetActive(false);
        }

        private void ShowError(string message)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }

        [Serializable]
        private class LoginRequest
        {
            public string username;
            public string password;
        }

        [Serializable]
        private class LoginResponse
        {
            public bool success;
            public string playerId;
            public string username;
            public string token;
        }

        [Serializable]
        private class RegisterRequest
        {
            public string username;
            public string password;
        }

        [Serializable]
        private class RegisterResponse
        {
            public bool success;
            public string playerId;
            public string username;
            public string token;
        }

        [Serializable]
        private class ValidationResponse
        {
            public bool valid;
            public string playerId;
            public string username;
        }
    }
}
```

---

## Real-Time Item Usage Flow

### Use Case: Player Uses Health Potion During Combat

**Scenario**: Player is being attacked by another ship, health is low, presses "1" key to use health potion

**Flow**:

```
1. Client: Player presses "1" key
   ↓
2. Client: HotbarController detects key press
   ↓
3. Client: Optimistically updates UI (show cooldown, reduce quantity)
   ↓
4. Client: Send Command to server
   CmdUseItem(itemId: "health_potion_uuid")
   ↓
5. Server: Receive Command
   ↓
6. Server: ServerInventoryManager.UseItem()
   - Check if item exists: ✅
   - Check if quantity >= 1: ✅
   - Reduce quantity: 5 → 4
   - Mark isDirty = true
   ↓
7. Server: Apply item effect
   ShipHealth.Heal(50)
   ↓
8. Server: Broadcast RPC to all clients
   RpcItemUsed(itemId, quantityUsed: 1, remaining: 4)
   ↓
9. Client: Receive RPC
   - Update inventory UI (confirm quantity = 4)
   - Show heal effect (+50 HP visual)
   ↓
10. Server: AutoSaveSystem saves in 60s
    (if isDirty = true)
```

**Performance**:
- Client UI update: **Instant** (0ms - optimistic)
- Server validation: **<50ms** (in-memory operation)
- Total perceived latency: **~50ms** (network RTT + server processing)

---

### Use Case: Player Drags Item in Inventory

**Scenario**: Player opens inventory, drags a 2x2 cargo crate to new position

**Flow**:

```
1. Client: Player starts dragging item
   ↓
2. Client: InventoryGridUI shows drag preview
   ↓
3. Client: Player drops item at new position
   ↓
4. Client: Optimistically update UI (move item visually)
   ↓
5. Client: Send Command to server
   CmdMoveItem(itemId, newPosition: {x:5, y:3}, rotation: 0)
   ↓
6. Server: Receive Command
   ↓
7. Server: ServerInventoryManager.MoveItem()
   - Check if item exists: ✅
   - Remove from old grid position
   - Validate new position (collision check): ✅
   - Place at new grid position
   - Mark isDirty = true
   ↓
8. Server: Broadcast RPC
   RpcItemMoved(itemId, newPosition, rotation)
   ↓
9. Client: Receive RPC
   - Confirm item position (already optimistically updated)
```

**Collision Detected (Invalid Move)**:

```
7. Server: ServerInventoryManager.MoveItem()
   - Validate new position: ❌ (collision with another item)
   ↓
8. Server: Broadcast RPC
   RpcRejectMove(itemId)
   ↓
9. Client: Receive RPC
   - Rollback item to original position (undo optimistic update)
   - Show error message: "Cannot place item here"
```

---

## Synchronization Strategies

### Delta Synchronization

**Principle**: Only send what changed, not full state

**Full State Sync** (BAD - 1000+ bytes):
```csharp
[ClientRpc]
void RpcSyncFullInventory(ItemData[] allItems) {
    // Sends entire inventory every frame
}
```

**Delta Sync** (GOOD - 50-100 bytes):
```csharp
[ClientRpc]
void RpcItemMoved(Guid itemId, Vector2Int newPos, int rotation) {
    // Only sends changed item
}

[ClientRpc]
void RpcItemQuantityChanged(Guid itemId, int newQuantity) {
    // Only sends new quantity
}
```

---

### Optimistic UI Updates

**Principle**: Update client UI immediately, validate asynchronously

**Benefits**:
- ✅ Instant visual feedback (no perceived lag)
- ✅ Better player experience
- ✅ Handles 99% of cases (most operations valid)

**Rollback Strategy**:
```csharp
// Client-side
void OnItemDropped(Item item, Vector2Int targetPos) {
    // 1. Save original state
    Vector2Int originalPos = item.position;

    // 2. Update UI optimistically
    MoveItemVisual(item, targetPos);

    // 3. Send to server
    CmdMoveItem(item.id, targetPos);

    // 4. If server rejects, rollback
    // (handled in RpcRejectMove)
}

[ClientRpc]
void RpcRejectMove(Guid itemId) {
    // Rollback to original position
    Item item = FindItem(itemId);
    MoveItemVisual(item, item.serverPosition);
    ShowError("Invalid item placement");
}
```

---

### Batched Database Writes

**Principle**: Don't save to database on every operation

**Dirty Flag System**:
```csharp
// Mark inventory as dirty on any change
void OnItemMoved() {
    isDirty = true;  // Mark for save
}

void OnItemUsed() {
    isDirty = true;  // Mark for save
}

// Auto-save every 60 seconds (only if dirty)
IEnumerator AutoSave() {
    while (true) {
        yield return new WaitForSeconds(60f);

        if (isDirty) {
            SaveToBackend();  // Async save
            isDirty = false;
        }
    }
}
```

**Benefits**:
- ✅ Reduces DB writes by 95%+ (60s batching)
- ✅ In-memory operations remain <50ms
- ✅ Still saves on disconnect/shutdown

---

## Performance Targets

| Operation | Target | Strategy |
|-----------|--------|----------|
| **Login Authentication** | <500ms | JWT validation with backend API |
| **Inventory Load (on connect)** | <200ms | Single API call, load to memory |
| **Item Usage (combat)** | <50ms | In-memory operation, no DB call |
| **Item Move (drag-drop)** | <100ms | In-memory validation, dirty flag |
| **Item Add (loot)** | <100ms | In-memory placement, dirty flag |
| **Trade Execution** | <500ms | DB transaction + cache update |
| **Inventory Save (batched)** | <100ms | Async API call, non-blocking |
| **Full Inventory Sync** | <1s | Only on connect/reconnect |

---

## Security Considerations

### Authentication Security

**JWT Token Storage**:
- ✅ Store in PlayerPrefs (encrypted on device)
- ✅ 7-day expiration (require re-login)
- ❌ Never store plain password
- ❌ Never send password after initial auth

**Password Security**:
- ✅ bcrypt hashing (cost factor 10)
- ✅ Salted hashes (automatic with bcrypt)
- ✅ Minimum 8 characters
- ❌ No plain text storage EVER

---

### Server Authority

**Critical Principle**: NEVER trust client

**Bad Example** (Client can cheat):
```csharp
[Command]
void CmdSetGold(int amount) {
    gold = amount;  // ❌ Client can set any value
}
```

**Good Example** (Server validates):
```csharp
[Command]
void CmdBuyItem(string itemId) {
    ItemDefinition item = GetItemDefinition(itemId);

    if (gold < item.cost) {
        RpcShowError("Not enough gold");
        return;  // ❌ Reject invalid purchase
    }

    gold -= item.cost;  // ✅ Server controls gold
    AddItem(itemId);
}
```

---

### Item Duplication Prevention

**Transaction Safety**:
```csharp
// Use database transactions for trades
async Task ExecuteTrade() {
    using (var transaction = db.BeginTransaction()) {
        try {
            // 1. Verify both players have items
            VerifyOwnership();

            // 2. Transfer items (atomic)
            TransferItems();

            // 3. Commit (all or nothing)
            await transaction.CommitAsync();
        } catch {
            await transaction.RollbackAsync();  // Undo if error
        }
    }
}
```

**Optimistic Locking**:
```csharp
// Prevent concurrent modification
UPDATE player_inventories
SET cargo_grid = $1, version = version + 1
WHERE player_id = $2 AND version = $3
-- Only updates if version matches (prevents conflicts)
```

---

### Input Validation

**Backend API Validation**:
```javascript
// Validate all inputs
function validateUsername(username) {
    if (typeof username !== 'string') return false;
    if (username.length < 3 || username.length > 50) return false;
    if (!/^[a-z0-9_]+$/.test(username)) return false;  // alphanumeric + underscore
    return true;
}

// Sanitize inputs
const sanitized = username.toLowerCase().trim();
```

**Server-Side Validation**:
```csharp
[Command]
void CmdMoveItem(Guid itemId, Vector2Int position) {
    // Validate item exists
    if (!inventory.items.ContainsKey(itemId)) return;

    // Validate position bounds
    if (position.x < 0 || position.y < 0) return;
    if (position.x >= gridWidth || position.y >= gridHeight) return;

    // Validate collision
    if (!IsValidPlacement(itemId, position)) return;

    // Only NOW apply the move
    MoveItem(itemId, position);
}
```

---

## Testing Strategy

### 4-Tier Testing Approach

#### **Tier 1: Editor Play Mode** (Primary for client-side)
**Use for**: Login UI, inventory UI, item drag-and-drop

**Test**:
1. Open MainMenu scene
2. Click Play
3. Test login flow
4. Click "Host (Server + Client)"
5. Test inventory UI

**Time**: 30 seconds per iteration

---

#### **Tier 2: Local Build Testing**
**Use for**: Full client experience, performance testing

**Test**:
1. Build Windows client
2. Test login → game connection → inventory operations
3. Test item usage during gameplay

**Time**: 5-10 minutes per iteration

---

#### **Tier 3: Docker Desktop Testing** (Backend API + Server)
**Use for**: Backend API endpoints, server-side validation

**Test**:
```powershell
# Start PostgreSQL
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres

# Start backend API
cd Backend
npm install
npm start

# Build Linux server
Unity → Build Settings → Linux Server Build

# Run server
cd EdgegapServer
docker build -t wos-server:local .
docker run -d -p 7777:7777/udp wos-server:local
```

**Time**: 15-20 minutes per iteration

---

#### **Tier 4: Edgegap Cloud Testing** (Production environment)
**Use for**: Final validation, real network latency

**Test**:
1. Deploy backend API to Render.com
2. Deploy PostgreSQL to Render.com
3. Deploy game server to Edgegap
4. Test full flow with real network latency

**Time**: 30-60 minutes per iteration

---

### Unit Testing

**Backend API Tests** (Jest/Mocha):
```javascript
describe('Authentication', () => {
    test('Register creates new account', async () => {
        const response = await request(app)
            .post('/api/auth/register')
            .send({ username: 'testuser', password: 'password123' });

        expect(response.status).toBe(201);
        expect(response.body.success).toBe(true);
        expect(response.body.token).toBeDefined();
    });

    test('Login with valid credentials', async () => {
        const response = await request(app)
            .post('/api/auth/login')
            .send({ username: 'testuser', password: 'password123' });

        expect(response.status).toBe(200);
        expect(response.body.token).toBeDefined();
    });
});
```

**Unity Server Tests** (Unity Test Framework):
```csharp
[Test]
public void TestItemUsage()
{
    // Arrange
    ServerInventoryManager inventory = CreateTestInventory();
    Guid itemId = AddTestItem("health_potion", 5);

    // Act
    bool success = inventory.UseItem(itemId, 1);

    // Assert
    Assert.IsTrue(success);
    Assert.AreEqual(4, inventory.GetItemQuantity(itemId));
}
```

---

## Implementation Phases

### Phase 1: Backend Foundation (4-6 hours)

**Tasks**:
1. ✅ Setup PostgreSQL on Render.com
2. ✅ Create database schema (tables: player_accounts, player_inventories, item_definitions)
3. ✅ Extend backend API (`wos-edgegap-proxy.onrender.com`)
   - `/api/auth/register`
   - `/api/auth/login`
   - `/api/auth/validate`
   - `/api/inventory/:playerId` (GET, POST)
   - `/api/items/definitions`
4. ✅ Test endpoints with Postman/curl

**Deliverables**:
- PostgreSQL database live on Render.com
- Backend API endpoints working and tested
- Item definitions seeded (health potion, repair kit, etc.)

---

### Phase 2: Server-Side Inventory Manager (6-8 hours)

**Tasks**:
1. ✅ Create `AccountManager.cs` (JWT validation)
2. ✅ Create `ServerInventoryManager.cs` (in-memory cache)
3. ✅ Create `AutoSaveSystem.cs` (dirty flag saves)
4. ✅ Create data models (`CargoGrid.cs`, `ItemData.cs`, `ItemDefinition.cs`)
5. ✅ Implement inventory operations (UseItem, MoveItem, AddItem)
6. ✅ Test with Docker Desktop (Tier 3)

**Deliverables**:
- In-memory inventory system working
- Real-time item operations (<50ms)
- Auto-save system functional

---

### Phase 3: Client Login & UI (8-10 hours)

**Tasks**:
1. ✅ Create `LoginPanel.cs` (username/password UI)
2. ✅ Create `InventoryGridUI.cs` (Tetris grid visualization)
3. ✅ Create `ItemSlotUI.cs` (individual item display)
4. ✅ Create `ClientInventoryManager.cs` (client-side cache)
5. ✅ Implement optimistic updates
6. ✅ Test with Editor Play Mode (Tier 1)

**Deliverables**:
- Login screen functional
- Inventory UI working
- Drag-and-drop item movement
- Client-server synchronization

---

### Phase 4: Real-Time Item Usage (4-6 hours)

**Tasks**:
1. ✅ Create `HotbarController.cs` (1-9 hotkeys)
2. ✅ Create `ItemUsageHandler.cs` (item consumption)
3. ✅ Create `ItemEffectHandler.cs` (apply item effects)
4. ✅ Integrate with existing systems (ShipHealth, WeaponSystem)
5. ✅ Test during gameplay (combat, sailing)

**Deliverables**:
- Hotbar system working (1-9 keys)
- Item usage during combat (<50ms)
- Visual feedback (heal effects, etc.)

---

### Phase 5: Trading & Port Storage (6-8 hours)

**Tasks**:
1. ✅ Create `TradeManager.cs` (player-to-player trades)
2. ✅ Backend `/api/trade/execute` endpoint
3. ✅ Create `PortStorageManager.cs` (port inventory)
4. ✅ Port storage UI
5. ✅ Test trade transactions (ensure ACID compliance)

**Deliverables**:
- Player-to-player trading functional
- Port storage system working
- Transaction safety verified (no item duplication)

---

### Phase 6: Polish & Optimization (4-6 hours)

**Tasks**:
1. ✅ Performance profiling (ensure <50ms item usage)
2. ✅ Error handling and edge cases
3. ✅ UI polish (animations, tooltips)
4. ✅ Beta testing with real players
5. ✅ Bug fixes and refinements

**Deliverables**:
- Performance targets met
- All edge cases handled
- System ready for production

---

## Total Implementation Time

**Estimated**: 32-44 hours total

**Critical Path**:
1. Backend API (must complete first)
2. Server inventory manager (depends on backend)
3. Client UI (depends on server)
4. Real-time usage (depends on client)
5. Trading/storage (extends system)
6. Polish (final step)

---

## Next Steps

**Immediate Action**: Implement Phase 1 (Backend Foundation)

1. Setup PostgreSQL database on Render.com
2. Create database schema (run SQL scripts)
3. Extend backend API with auth and inventory endpoints
4. Test endpoints with Postman

**Decision Point**: After Phase 1 complete, review and proceed to Phase 2 (Server-Side Inventory Manager)

---

## References

- Backend API: `https://wos-edgegap-proxy.onrender.com`
- Database: PostgreSQL on Render.com (free tier)
- Unity Version: 6000.0.55f1
- Mirror Networking: Latest version
- Testing Strategy: `TESTING_STRATEGY.md`
- Update Workflows: `UPDATE_WORKFLOWS.md`
