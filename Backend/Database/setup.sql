-- WOS Player Account System Database Setup
-- PostgreSQL 14+
-- Run this script to create all necessary tables

-- ============================================
-- Table: player_accounts
-- Purpose: Store player authentication and account data
-- ============================================
CREATE TABLE IF NOT EXISTS player_accounts (
    player_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW(),
    last_login TIMESTAMP,

    -- Constraints
    CONSTRAINT username_lowercase CHECK (username = LOWER(username)),
    CONSTRAINT username_length CHECK (LENGTH(username) >= 3 AND LENGTH(username) <= 50)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_username ON player_accounts(username);
CREATE INDEX IF NOT EXISTS idx_last_login ON player_accounts(last_login);

-- ============================================
-- Table: player_inventories
-- Purpose: Store player Tetris-style cargo grids
-- ============================================
CREATE TABLE IF NOT EXISTS player_inventories (
    player_id UUID PRIMARY KEY REFERENCES player_accounts(player_id) ON DELETE CASCADE,
    cargo_grid JSONB NOT NULL DEFAULT '{"width":10,"height":10,"cells":[],"items":{}}',
    last_updated TIMESTAMP DEFAULT NOW(),
    version INT DEFAULT 1,  -- Optimistic locking

    -- Constraints
    CONSTRAINT valid_cargo_grid CHECK (jsonb_typeof(cargo_grid) = 'object')
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_inventory_last_updated ON player_inventories(last_updated);

-- ============================================
-- Table: item_definitions
-- Purpose: Define all item types available in the game
-- ============================================
CREATE TABLE IF NOT EXISTS item_definitions (
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

-- Indexes
CREATE INDEX IF NOT EXISTS idx_item_type ON item_definitions(item_type);

-- ============================================
-- Table: trade_log
-- Purpose: Audit trail for all player trades
-- ============================================
CREATE TABLE IF NOT EXISTS trade_log (
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

-- Indexes
CREATE INDEX IF NOT EXISTS idx_player1_trades ON trade_log(player1_id, trade_timestamp);
CREATE INDEX IF NOT EXISTS idx_player2_trades ON trade_log(player2_id, trade_timestamp);

-- ============================================
-- Table: port_storage
-- Purpose: Store player cargo at ports (20x20 grid)
-- ============================================
CREATE TABLE IF NOT EXISTS port_storage (
    storage_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID REFERENCES player_accounts(player_id) ON DELETE CASCADE,
    port_name VARCHAR(100) NOT NULL,
    cargo_grid JSONB NOT NULL DEFAULT '{"width":20,"height":20,"cells":[],"items":{}}',
    last_accessed TIMESTAMP DEFAULT NOW(),

    UNIQUE(player_id, port_name)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_player_port ON port_storage(player_id, port_name);

-- ============================================
-- Seed Data: Item Definitions
-- ============================================
INSERT INTO item_definitions (item_id, item_name, item_type, grid_size, properties, max_stack, is_consumable, base_value, weight) VALUES
('health_potion', 'Health Potion', 'consumable', '{"width":1,"height":1}', '{"healAmount":50}', 10, true, 25, 0.5),
('repair_kit', 'Repair Kit', 'consumable', '{"width":2,"height":1}', '{"repairAmount":25}', 5, true, 50, 1.0),
('ammo_crate', 'Ammunition Crate', 'consumable', '{"width":2,"height":2}', '{"ammoAmount":100}', 5, true, 75, 5.0),
('iron_ore', 'Iron Ore', 'cargo', '{"width":2,"height":2}', '{}', 50, false, 10, 10.0),
('timber', 'Timber', 'cargo', '{"width":3,"height":1}', '{}', 20, false, 15, 8.0),
('rum_barrel', 'Barrel of Rum', 'cargo', '{"width":1,"height":2}', '{}', 10, false, 30, 12.0),
('cannon', 'Ship Cannon', 'equipment', '{"width":3,"height":2}', '{"damage":100,"range":500}', 1, false, 500, 150.0),
('sail', 'Replacement Sail', 'equipment', '{"width":2,"height":3}', '{"speedBonus":0.1}', 1, false, 200, 25.0)
ON CONFLICT (item_id) DO NOTHING;

-- ============================================
-- Success Message
-- ============================================
DO $$
BEGIN
    RAISE NOTICE 'Database setup complete!';
    RAISE NOTICE 'Tables created:';
    RAISE NOTICE '  - player_accounts';
    RAISE NOTICE '  - player_inventories';
    RAISE NOTICE '  - item_definitions';
    RAISE NOTICE '  - trade_log';
    RAISE NOTICE '  - port_storage';
    RAISE NOTICE 'Item definitions seeded: 8 items';
END $$;
