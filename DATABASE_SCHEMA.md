# WOS2.3 Database Schema Design
**Comprehensive Player Account and Persistence System**

Generated: 2025-01-21
Based on: GDD_Updated-1.md, Unity Project Analysis

---

## Overview

This schema supports a naval MMO with:
- **300+ concurrent players** per server
- **Extraction-based gameplay** with permadeath mechanics (Tiers 6-10)
- **Player-driven economy** (Tarkov-style marketplace)
- **Multi-ship ownership** with Tetris-style cargo
- **75+ crew specializations** with individual progression
- **7 nation reputation systems** with diplomatic consequences
- **Risk mitigation** via insurance badges and emergency beacons

---

## Schema Migration Strategy

**Yes, you can change the schema later!** We'll use **Entity Framework Core Migrations**:

```bash
# Add new migration after schema changes
dotnet ef migrations add AddNewFeature

# Apply migration to database
dotnet ef database update

# Rollback if needed
dotnet ef database update PreviousMigrationName
```

**Best Practices**:
- Never delete columns (mark as deprecated, hide in queries)
- Add new columns as nullable or with default values
- Use separate tables for new features when possible
- Keep audit trail with `created_at` and `updated_at` timestamps

---

## Core Tables

### 1. **users** - Authentication & Account Info
```sql
CREATE TABLE users (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Authentication
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,  -- BCrypt hashed

    -- Account Status
    account_status VARCHAR(20) DEFAULT 'active',  -- active, banned, suspended
    ban_reason TEXT,
    ban_expires_at TIMESTAMP,

    -- Security
    email_verified BOOLEAN DEFAULT false,
    two_factor_enabled BOOLEAN DEFAULT false,
    two_factor_secret VARCHAR(255),

    -- Timestamps
    created_at TIMESTAMP DEFAULT NOW(),
    last_login TIMESTAMP,
    last_ip_address INET,

    -- Soft Delete
    deleted_at TIMESTAMP,

    CONSTRAINT valid_email CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$')
);

CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_last_login ON users(last_login DESC);
```

### 2. **player_profiles** - Core Player Data
```sql
CREATE TABLE player_profiles (
    -- Primary Key
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,

    -- Identity
    display_name VARCHAR(50) NOT NULL,
    captain_title VARCHAR(100),  -- "Legendary Captain", "Fleet Admiral"

    -- Progression
    player_level INT DEFAULT 1 CHECK (player_level >= 1 AND player_level <= 200),
    experience BIGINT DEFAULT 0,
    progression_tier VARCHAR(50) DEFAULT 'Novice Captain',
    -- Tiers: Novice Captain, Veteran Commander, Fleet Admiral, Legendary Captain

    -- Economy
    credits BIGINT DEFAULT 150000,  -- Starting: ₡150,000

    -- Statistics
    total_playtime_minutes INT DEFAULT 0,
    ships_sunk INT DEFAULT 0,
    ships_lost INT DEFAULT 0,
    pvp_kills INT DEFAULT 0,
    pve_kills INT DEFAULT 0,
    successful_extractions INT DEFAULT 0,
    failed_extractions INT DEFAULT 0,
    total_cargo_delivered_tons INT DEFAULT 0,
    total_distance_sailed_km DECIMAL(12,2) DEFAULT 0,

    -- Preferences
    preferred_nation VARCHAR(50) DEFAULT 'USA',
    -- Nations: USA, UK, Germany, Japan, France, Italy, Soviet Union
    ui_language VARCHAR(10) DEFAULT 'en',
    tutorial_completed BOOLEAN DEFAULT false,

    -- Timestamps
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT valid_credits CHECK (credits >= 0)
);

CREATE INDEX idx_player_level ON player_profiles(player_level DESC);
CREATE INDEX idx_player_credits ON player_profiles(credits DESC);
```

### 3. **player_ships** - Ship Ownership & State
```sql
CREATE TABLE player_ships (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Ship Identity
    ship_name VARCHAR(100) NOT NULL,  -- Custom player-assigned name
    ship_class VARCHAR(100) NOT NULL,  -- "Königsberg", "Fletcher", "Yamato"
    ship_tier INT NOT NULL CHECK (ship_tier >= 1 AND ship_tier <= 10),
    nation VARCHAR(50) NOT NULL,
    ship_type VARCHAR(50) NOT NULL,  -- Destroyer, Cruiser, Battleship, Carrier, Submarine

    -- Ship Status
    status VARCHAR(50) DEFAULT 'docked',
    -- Status: docked, at_sea, in_combat, destroyed, repairing

    -- Location
    current_port VARCHAR(100),  -- "Pearl Harbor", "Kiel", NULL if at sea
    current_position_x DECIMAL(10,2),
    current_position_y DECIMAL(10,2),
    current_heading DECIMAL(5,2),  -- 0-360 degrees

    -- Condition
    hull_integrity DECIMAL(5,2) DEFAULT 100.00 CHECK (hull_integrity >= 0 AND hull_integrity <= 100),
    fuel_remaining DECIMAL(10,2) DEFAULT 0,
    fuel_capacity DECIMAL(10,2) NOT NULL,
    ammunition_remaining INT DEFAULT 0,

    -- Damage State
    engine_damage_percent DECIMAL(5,2) DEFAULT 0,
    rudder_damage_percent DECIMAL(5,2) DEFAULT 0,
    fire_control_damage_percent DECIMAL(5,2) DEFAULT 0,
    is_on_fire BOOLEAN DEFAULT false,
    is_flooding BOOLEAN DEFAULT false,

    -- Insurance
    insured BOOLEAN DEFAULT false,
    insurance_type VARCHAR(50),  -- Hull, Crew, Module, Premium
    insurance_nation VARCHAR(50),  -- Which nation provides insurance
    insurance_expires_at TIMESTAMP,

    -- Economy
    purchase_price BIGINT NOT NULL,
    total_repair_cost BIGINT DEFAULT 0,
    resale_value BIGINT,

    -- Timestamps
    acquired_at TIMESTAMP DEFAULT NOW(),
    last_sortie_at TIMESTAMP,
    destroyed_at TIMESTAMP,
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT valid_position CHECK (
        (current_port IS NOT NULL AND current_position_x IS NULL) OR
        (current_port IS NULL AND current_position_x IS NOT NULL)
    )
);

CREATE INDEX idx_player_ships_user ON player_ships(user_id);
CREATE INDEX idx_player_ships_status ON player_ships(status);
CREATE INDEX idx_player_ships_tier ON player_ships(ship_tier);
CREATE INDEX idx_player_ships_location ON player_ships(current_port);
```

### 4. **ship_modules** - Installed Equipment
```sql
CREATE TABLE ship_modules (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ship_id UUID NOT NULL REFERENCES player_ships(id) ON DELETE CASCADE,

    -- Module Identity
    module_type VARCHAR(100) NOT NULL,
    -- Types: Main_Battery, Secondary_Battery, AA_Battery, Torpedo_Launcher,
    --        Fire_Control, Engine, Rudder, Radar, Sonar, Armor_Plating
    module_name VARCHAR(200) NOT NULL,
    module_tier INT NOT NULL CHECK (module_tier >= 1 AND module_tier <= 10),

    -- Module Slot
    slot_number INT NOT NULL,
    is_primary BOOLEAN DEFAULT false,  -- Primary weapon system

    -- Module Condition
    condition_percent DECIMAL(5,2) DEFAULT 100.00 CHECK (condition_percent >= 0 AND condition_percent <= 100),
    -- Condition: 100%=New, 75-99%=Used, 50-74%=Damaged, <50%=Broken

    -- Module Stats (varies by type, store as JSONB for flexibility)
    stats JSONB NOT NULL,
    -- Example for Main_Battery: {"caliber_mm": 203, "reload_time_sec": 12, "range_km": 18}
    -- Example for Engine: {"max_speed_knots": 35, "acceleration": 2.5, "fuel_consumption": 150}

    -- Economy
    purchase_price BIGINT NOT NULL,
    resale_value BIGINT,

    -- Timestamps
    installed_at TIMESTAMP DEFAULT NOW(),
    last_repaired_at TIMESTAMP,
    updated_at TIMESTAMP DEFAULT NOW(),

    UNIQUE(ship_id, slot_number)
);

CREATE INDEX idx_ship_modules_ship ON ship_modules(ship_id);
CREATE INDEX idx_ship_modules_type ON ship_modules(module_type);
CREATE INDEX idx_ship_modules_tier ON ship_modules(module_tier);
```

### 5. **crew_members** - Individual Crew with Permadeath
```sql
CREATE TABLE crew_members (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ship_id UUID REFERENCES player_ships(id) ON DELETE SET NULL,

    -- Crew Identity
    crew_name VARCHAR(100) NOT NULL,
    nationality VARCHAR(50) NOT NULL,
    portrait_id VARCHAR(100),  -- Reference to portrait sprite

    -- Crew Position (75+ specializations)
    crew_position VARCHAR(100) NOT NULL,
    -- Command: Captain, Executive_Officer, Navigator
    -- Engineering: Chief_Engineer, Damage_Control, Mechanics
    -- Weapons: Gunnery_Officer, Torpedo_Crew, AA_Gunners, Fire_Control_Officer
    -- Aviation: Pilots, Aircraft_Mechanics, Flight_Control
    -- Support: Radio_Operator, Medic, Supply_Officer, Quartermaster

    -- Progression
    experience INT DEFAULT 0,
    skill_level INT DEFAULT 1 CHECK (skill_level >= 1 AND skill_level <= 200),
    specialization_bonus DECIMAL(5,2) DEFAULT 0,  -- 0-50% bonus

    -- Cross-Training (secondary skills)
    secondary_skills JSONB DEFAULT '[]',
    -- Example: ["Damage_Control", "AA_Gunners"]

    -- Morale & Status
    morale INT DEFAULT 100 CHECK (morale >= 0 AND morale <= 100),
    status VARCHAR(50) DEFAULT 'active',
    -- Status: active, injured, recovering, killed, shore_leave, unassigned

    -- Injuries & Recovery
    injury_severity VARCHAR(50),  -- minor, moderate, severe, critical
    recovery_time_hours INT DEFAULT 0,
    total_battles_survived INT DEFAULT 0,

    -- Economy
    hiring_cost BIGINT DEFAULT 5000,
    monthly_salary BIGINT DEFAULT 1000,

    -- Timestamps
    hired_at TIMESTAMP DEFAULT NOW(),
    assigned_to_ship_at TIMESTAMP,
    last_battle_at TIMESTAMP,
    killed_at TIMESTAMP,  -- Permadeath tracking
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT crew_assigned_or_unassigned CHECK (
        (ship_id IS NULL AND status = 'unassigned') OR
        (ship_id IS NOT NULL AND status != 'unassigned')
    )
);

CREATE INDEX idx_crew_user ON crew_members(user_id);
CREATE INDEX idx_crew_ship ON crew_members(ship_id);
CREATE INDEX idx_crew_position ON crew_members(crew_position);
CREATE INDEX idx_crew_status ON crew_members(status);
CREATE INDEX idx_crew_skill_level ON crew_members(skill_level DESC);
```

### 6. **ship_inventory** - Tetris-Style Cargo Storage
```sql
CREATE TABLE ship_inventory (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ship_id UUID NOT NULL REFERENCES player_ships(id) ON DELETE CASCADE,

    -- Item Identity
    item_type VARCHAR(100) NOT NULL,
    -- Types: Module, Ammunition, Resource, Fuel, Supplies, Trade_Goods, Blueprint, Intelligence
    item_name VARCHAR(200) NOT NULL,
    item_tier INT CHECK (item_tier >= 1 AND item_tier <= 10),

    -- Grid Position (Tetris-style)
    grid_position_x INT NOT NULL CHECK (grid_position_x >= 0),
    grid_position_y INT NOT NULL CHECK (grid_position_y >= 0),
    grid_width INT NOT NULL CHECK (grid_width > 0),
    grid_height INT NOT NULL CHECK (grid_height > 0),
    rotation_degrees INT DEFAULT 0 CHECK (rotation_degrees IN (0, 90, 180, 270)),

    -- Quantity & Stacking
    quantity INT DEFAULT 1 CHECK (quantity > 0),
    is_stackable BOOLEAN DEFAULT false,
    max_stack_size INT,

    -- Item Condition
    condition_percent DECIMAL(5,2) DEFAULT 100.00 CHECK (condition_percent >= 0 AND condition_percent <= 100),

    -- Item Data (flexible storage for item-specific properties)
    item_data JSONB NOT NULL,
    -- Example for Resource: {"resource_type": "Steel", "weight_tons": 50}
    -- Example for Module: {"caliber_mm": 203, "damaged": false, "operational": true}
    -- Example for Ammunition: {"shell_type": "AP", "rounds": 500}

    -- Economy
    purchase_price BIGINT,
    market_value BIGINT,

    -- Timestamps
    acquired_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    UNIQUE(ship_id, grid_position_x, grid_position_y)  -- No overlapping items
);

CREATE INDEX idx_ship_inventory_ship ON ship_inventory(ship_id);
CREATE INDEX idx_ship_inventory_type ON ship_inventory(item_type);
CREATE INDEX idx_ship_inventory_position ON ship_inventory(ship_id, grid_position_x, grid_position_y);
```

### 7. **port_storage** - Expandable Port Warehouses
```sql
CREATE TABLE port_storage (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Location
    port_name VARCHAR(100) NOT NULL,  -- "Pearl Harbor", "Kiel", etc.
    storage_tier INT DEFAULT 1 CHECK (storage_tier >= 1 AND storage_tier <= 5),
    -- Higher tier = more grid space

    -- Storage Capacity
    grid_width INT DEFAULT 10,
    grid_height INT DEFAULT 10,
    -- Upgradeable: T1=10x10, T2=15x15, T3=20x20, T4=25x25, T5=30x30

    -- Economy
    monthly_rent BIGINT DEFAULT 5000,
    upgrade_cost BIGINT,
    rent_paid_until TIMESTAMP,

    -- Timestamps
    rented_at TIMESTAMP DEFAULT NOW(),
    last_upgraded_at TIMESTAMP,
    updated_at TIMESTAMP DEFAULT NOW(),

    UNIQUE(user_id, port_name)
);

CREATE INDEX idx_port_storage_user ON port_storage(user_id);
CREATE INDEX idx_port_storage_location ON port_storage(port_name);
```

### 8. **port_storage_items** - Items in Port Warehouses
```sql
CREATE TABLE port_storage_items (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    storage_id UUID NOT NULL REFERENCES port_storage(id) ON DELETE CASCADE,

    -- Item Identity (same structure as ship_inventory)
    item_type VARCHAR(100) NOT NULL,
    item_name VARCHAR(200) NOT NULL,
    item_tier INT CHECK (item_tier >= 1 AND item_tier <= 10),

    -- Grid Position
    grid_position_x INT NOT NULL CHECK (grid_position_x >= 0),
    grid_position_y INT NOT NULL CHECK (grid_position_y >= 0),
    grid_width INT NOT NULL CHECK (grid_width > 0),
    grid_height INT NOT NULL CHECK (grid_height > 0),
    rotation_degrees INT DEFAULT 0 CHECK (rotation_degrees IN (0, 90, 180, 270)),

    -- Quantity & Stacking
    quantity INT DEFAULT 1 CHECK (quantity > 0),
    is_stackable BOOLEAN DEFAULT false,
    max_stack_size INT,

    -- Item Condition
    condition_percent DECIMAL(5,2) DEFAULT 100.00,

    -- Item Data
    item_data JSONB NOT NULL,

    -- Economy
    purchase_price BIGINT,
    market_value BIGINT,

    -- Timestamps
    stored_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    UNIQUE(storage_id, grid_position_x, grid_position_y)
);

CREATE INDEX idx_port_items_storage ON port_storage_items(storage_id);
CREATE INDEX idx_port_items_type ON port_storage_items(item_type);
```

### 9. **nation_reputation** - Per-Nation Diplomacy
```sql
CREATE TABLE nation_reputation (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Nation
    nation VARCHAR(50) NOT NULL,
    -- Nations: USA, UK, Germany, Japan, France, Italy, Soviet_Union, Pirates

    -- Reputation Score (-100 to +100)
    reputation INT DEFAULT 0 CHECK (reputation >= -100 AND reputation <= 100),
    -- Levels: Allied(+75 to +100), Friendly(+50 to +74), Neutral(0 to +49),
    --         Unfriendly(-1 to -49), Hostile(-50 to -100)

    -- Diplomatic Status
    access_level VARCHAR(50) DEFAULT 'Neutral',
    port_discount_percent DECIMAL(5,2) DEFAULT 0,

    -- Statistics
    friendly_ships_sunk INT DEFAULT 0,
    enemy_ships_sunk INT DEFAULT 0,
    missions_completed INT DEFAULT 0,
    missions_failed INT DEFAULT 0,
    cargo_contracts_completed INT DEFAULT 0,

    -- Timestamps
    last_reputation_change TIMESTAMP,
    updated_at TIMESTAMP DEFAULT NOW(),

    UNIQUE(user_id, nation)
);

CREATE INDEX idx_reputation_user ON nation_reputation(user_id);
CREATE INDEX idx_reputation_nation ON nation_reputation(nation);
CREATE INDEX idx_reputation_score ON nation_reputation(reputation DESC);
```

### 10. **marketplace_listings** - Player & NPC Trading
```sql
CREATE TABLE marketplace_listings (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Seller
    seller_user_id UUID REFERENCES users(id) ON DELETE CASCADE,  -- NULL for NPC listings
    seller_type VARCHAR(20) DEFAULT 'player',  -- player, npc
    seller_nation VARCHAR(50),  -- For NPC vendors

    -- Item
    item_type VARCHAR(100) NOT NULL,
    item_name VARCHAR(200) NOT NULL,
    item_tier INT CHECK (item_tier >= 1 AND item_tier <= 10),
    quantity INT DEFAULT 1 CHECK (quantity > 0),
    condition_percent DECIMAL(5,2) DEFAULT 100.00,

    -- Item Data
    item_data JSONB NOT NULL,

    -- Pricing
    asking_price BIGINT NOT NULL CHECK (asking_price > 0),
    unit_price BIGINT GENERATED ALWAYS AS (asking_price / quantity) STORED,

    -- Listing Status
    status VARCHAR(50) DEFAULT 'active',  -- active, sold, expired, cancelled

    -- Location
    listed_at_port VARCHAR(100),

    -- NPC Dynamic Pricing (event-based modifiers)
    price_modifier DECIMAL(5,2) DEFAULT 1.00,  -- 0.5-2.0 based on events
    -- War: +30-50% ammo, Resource scarcity: +20-80%, Victory: -10-30% modules

    -- Timestamps
    listed_at TIMESTAMP DEFAULT NOW(),
    expires_at TIMESTAMP DEFAULT (NOW() + INTERVAL '48 hours'),
    sold_at TIMESTAMP,
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT valid_seller CHECK (
        (seller_type = 'player' AND seller_user_id IS NOT NULL) OR
        (seller_type = 'npc' AND seller_nation IS NOT NULL)
    )
);

CREATE INDEX idx_marketplace_seller ON marketplace_listings(seller_user_id);
CREATE INDEX idx_marketplace_item_type ON marketplace_listings(item_type);
CREATE INDEX idx_marketplace_tier ON marketplace_listings(item_tier);
CREATE INDEX idx_marketplace_price ON marketplace_listings(unit_price);
CREATE INDEX idx_marketplace_status ON marketplace_listings(status);
CREATE INDEX idx_marketplace_location ON marketplace_listings(listed_at_port);
CREATE INDEX idx_marketplace_expires ON marketplace_listings(expires_at);
```

### 11. **trade_history** - Transaction Audit Trail
```sql
CREATE TABLE trade_history (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Transaction Parties
    buyer_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    seller_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    seller_type VARCHAR(20) NOT NULL,  -- player, npc

    -- Item Traded
    item_type VARCHAR(100) NOT NULL,
    item_name VARCHAR(200) NOT NULL,
    item_tier INT,
    quantity INT NOT NULL,
    condition_percent DECIMAL(5,2),

    -- Transaction Details
    sale_price BIGINT NOT NULL,
    unit_price BIGINT NOT NULL,
    location VARCHAR(100),  -- Port where trade occurred

    -- Timestamps
    traded_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT buyer_or_seller_exists CHECK (
        buyer_user_id IS NOT NULL OR seller_user_id IS NOT NULL
    )
);

CREATE INDEX idx_trade_buyer ON trade_history(buyer_user_id);
CREATE INDEX idx_trade_seller ON trade_history(seller_user_id);
CREATE INDEX idx_trade_date ON trade_history(traded_at DESC);
CREATE INDEX idx_trade_item_type ON trade_history(item_type);
```

### 12. **player_achievements** - Unlocks & Progression
```sql
CREATE TABLE player_achievements (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Achievement Identity
    achievement_id VARCHAR(100) NOT NULL,
    achievement_category VARCHAR(50) NOT NULL,
    -- Categories: Combat, Economic, Exploration, Diplomatic, Collection

    -- Progress
    progress INT DEFAULT 0,
    required_progress INT NOT NULL,
    completed BOOLEAN DEFAULT false,

    -- Rewards
    credits_reward BIGINT DEFAULT 0,
    experience_reward INT DEFAULT 0,
    unlocks_content VARCHAR(200),  -- Blueprint, Title, Paint Scheme

    -- Timestamps
    started_at TIMESTAMP DEFAULT NOW(),
    completed_at TIMESTAMP,
    updated_at TIMESTAMP DEFAULT NOW(),

    UNIQUE(user_id, achievement_id)
);

CREATE INDEX idx_achievements_user ON player_achievements(user_id);
CREATE INDEX idx_achievements_category ON player_achievements(achievement_category);
CREATE INDEX idx_achievements_completed ON player_achievements(completed, completed_at DESC);
```

### 13. **player_blueprints** - Crafting Unlocks
```sql
CREATE TABLE player_blueprints (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Blueprint Identity
    blueprint_id VARCHAR(100) NOT NULL,
    blueprint_name VARCHAR(200) NOT NULL,
    blueprint_tier INT NOT NULL CHECK (blueprint_tier >= 1 AND blueprint_tier <= 10),
    blueprint_category VARCHAR(50) NOT NULL,
    -- Categories: Ship, Module, Ammunition, Equipment

    -- Crafting Recipe (JSONB for flexibility)
    required_materials JSONB NOT NULL,
    -- Example: [{"material": "Steel", "quantity": 100}, {"material": "Chromium", "quantity": 20}]

    crafting_time_hours INT DEFAULT 1,
    crafting_cost BIGINT DEFAULT 10000,

    -- Economy
    purchase_price BIGINT NOT NULL,

    -- Timestamps
    unlocked_at TIMESTAMP DEFAULT NOW(),
    last_crafted_at TIMESTAMP,
    total_crafts INT DEFAULT 0,

    UNIQUE(user_id, blueprint_id)
);

CREATE INDEX idx_blueprints_user ON player_blueprints(user_id);
CREATE INDEX idx_blueprints_category ON player_blueprints(blueprint_category);
CREATE INDEX idx_blueprints_tier ON player_blueprints(blueprint_tier);
```

### 14. **session_tokens** - JWT Authentication
```sql
CREATE TABLE session_tokens (
    -- Primary Key
    token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Token
    token_hash VARCHAR(500) NOT NULL,  -- SHA-256 hash of JWT
    refresh_token_hash VARCHAR(500),

    -- Device Info
    device_type VARCHAR(50),  -- PC, Mobile
    device_id VARCHAR(200),
    ip_address INET,
    user_agent TEXT,

    -- Token Lifecycle
    expires_at TIMESTAMP NOT NULL,
    refresh_expires_at TIMESTAMP,
    revoked BOOLEAN DEFAULT false,
    revoked_at TIMESTAMP,
    revoke_reason VARCHAR(200),

    -- Timestamps
    created_at TIMESTAMP DEFAULT NOW(),
    last_used_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT valid_expiry CHECK (expires_at > created_at)
);

CREATE INDEX idx_tokens_user ON session_tokens(user_id);
CREATE INDEX idx_tokens_hash ON session_tokens(token_hash);
CREATE INDEX idx_tokens_expires ON session_tokens(expires_at);
CREATE INDEX idx_tokens_revoked ON session_tokens(revoked);
```

### 15. **player_settings** - Game Preferences
```sql
CREATE TABLE player_settings (
    -- Primary Key
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,

    -- Graphics
    graphics_quality VARCHAR(20) DEFAULT 'medium',  -- low, medium, high, ultra
    resolution_width INT DEFAULT 1920,
    resolution_height INT DEFAULT 1080,
    fullscreen BOOLEAN DEFAULT true,
    vsync BOOLEAN DEFAULT true,
    frame_rate_limit INT DEFAULT 60,

    -- Audio
    master_volume DECIMAL(3,2) DEFAULT 1.00 CHECK (master_volume >= 0 AND master_volume <= 1),
    music_volume DECIMAL(3,2) DEFAULT 0.70,
    sfx_volume DECIMAL(3,2) DEFAULT 0.85,
    voice_volume DECIMAL(3,2) DEFAULT 1.00,

    -- Controls
    mouse_sensitivity DECIMAL(3,2) DEFAULT 1.00,
    invert_y_axis BOOLEAN DEFAULT false,
    camera_zoom_speed DECIMAL(3,2) DEFAULT 1.00,

    -- UI
    ui_scale DECIMAL(3,2) DEFAULT 1.00,
    show_minimap BOOLEAN DEFAULT true,
    show_fps_counter BOOLEAN DEFAULT false,
    colorblind_mode VARCHAR(20) DEFAULT 'none',  -- none, protanopia, deuteranopia, tritanopia

    -- Gameplay
    auto_save_interval_minutes INT DEFAULT 5,
    show_tutorial_hints BOOLEAN DEFAULT true,
    enable_profanity_filter BOOLEAN DEFAULT true,

    -- Keybindings (JSONB for flexibility)
    keybindings JSONB DEFAULT '{}',

    -- Timestamps
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);
```

---

## Relationships Summary

```
users (1) → (1) player_profiles
users (1) → (N) player_ships
users (1) → (N) crew_members
users (1) → (N) nation_reputation (one per nation)
users (1) → (N) port_storage (one per port)
users (1) → (N) marketplace_listings
users (1) → (N) player_achievements
users (1) → (N) player_blueprints
users (1) → (N) session_tokens
users (1) → (1) player_settings

player_ships (1) → (N) ship_modules
player_ships (1) → (N) ship_inventory
player_ships (1) → (N) crew_members

port_storage (1) → (N) port_storage_items
```

---

## Indexes Strategy

**Performance Targets**:
- User login: <100ms
- Player data load: <200ms
- Inventory queries: <150ms
- Marketplace searches: <300ms

**Key Indexes**:
1. **Authentication**: username, email, token_hash
2. **Ship Queries**: user_id, status, tier, location
3. **Inventory**: ship_id + grid_position (spatial queries)
4. **Marketplace**: item_type + tier + price (filtered searches)
5. **Reputation**: user_id + nation (diplomatic checks)

---

## Data Size Estimates (per player)

| Table | Rows/Player | Size/Player | 1000 Players |
|-------|-------------|-------------|--------------|
| users | 1 | 1 KB | 1 MB |
| player_profiles | 1 | 2 KB | 2 MB |
| player_ships | 5 avg | 10 KB | 10 MB |
| ship_modules | 30 avg | 15 KB | 15 MB |
| crew_members | 50 avg | 25 KB | 25 MB |
| ship_inventory | 100 avg | 50 KB | 50 MB |
| port_storage | 3 avg | 1 KB | 1 MB |
| port_storage_items | 500 avg | 250 KB | 250 MB |
| nation_reputation | 8 (fixed) | 2 KB | 2 MB |
| marketplace_listings | 10 avg | 5 KB | 5 MB |
| player_achievements | 50 avg | 10 KB | 10 MB |
| player_blueprints | 20 avg | 5 KB | 5 MB |
| session_tokens | 2 avg | 1 KB | 1 MB |
| player_settings | 1 | 2 KB | 2 MB |
| **TOTAL** | - | **~380 KB** | **~380 MB** |

**Render Free Tier**: 256MB PostgreSQL (supports ~650 players with full data)
**Recommendation**: Upgrade to Starter ($7/month, 256MB-1GB) for 1000-2500 players

---

## Security Considerations

1. **Password Hashing**: Use BCrypt with cost factor 12
2. **SQL Injection**: Use parameterized queries (EF Core handles this)
3. **Rate Limiting**: Limit login attempts (5/hour per IP)
4. **JWT Expiry**: Access tokens expire in 15 minutes, refresh tokens in 7 days
5. **Sensitive Data**: Never log passwords, tokens, or IP addresses in plain text
6. **GDPR Compliance**: Implement soft deletes (`deleted_at` timestamp)

---

## Next Steps

1. ✅ Create SQL migration files
2. ✅ Extend ASP.NET Core backend with auth endpoints
3. ✅ Add Entity Framework models
4. ✅ Implement JWT authentication
5. ✅ Create player data API endpoints
6. ✅ Test with Render PostgreSQL free tier
7. ✅ Unity client integration (login, data sync, inventory)

---

**Generated by Claude Code for WOS2.3 Naval MMO Project**
