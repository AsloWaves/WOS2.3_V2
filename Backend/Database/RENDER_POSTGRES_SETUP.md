# PostgreSQL Setup on Render.com

**Purpose**: Create PostgreSQL database for WOS player accounts and inventory system

**Estimated Time**: 10-15 minutes

---

## Step 1: Create PostgreSQL Database on Render.com

1. **Login to Render.com**:
   - Go to https://render.com
   - Sign in with your account

2. **Create New PostgreSQL Database**:
   - Click "New +" in top right
   - Select "PostgreSQL"

3. **Configure Database**:
   ```
   Name: wos-accounts-db
   Database: wos_accounts
   User: wos_admin (auto-generated)
   Region: Oregon (US West) - Same as backend API
   PostgreSQL Version: 16
   Plan: Free (0.25GB RAM, 1GB storage)
   ```

4. **Create Database**:
   - Click "Create Database"
   - Wait 2-3 minutes for provisioning

5. **Get Connection String**:
   - Once created, go to "Connect" tab
   - Copy "External Connection String":
     ```
     postgresql://wos_admin:password@hostname.oregon-postgres.render.com/wos_accounts
     ```

---

## Step 2: Connect to Database

### Option A: Using pgAdmin (Recommended for Windows)

1. **Download pgAdmin**:
   - https://www.pgadmin.org/download/
   - Install pgAdmin 4

2. **Add Server**:
   - Open pgAdmin
   - Right-click "Servers" → Create → Server

3. **Configure Connection**:
   - **General Tab**:
     - Name: `WOS Render DB`

   - **Connection Tab**:
     - Host: `hostname.oregon-postgres.render.com` (from connection string)
     - Port: `5432`
     - Maintenance database: `wos_accounts`
     - Username: `wos_admin` (from connection string)
     - Password: [your password from connection string]
     - Save password: ✅ Yes

   - **SSL Tab**:
     - SSL mode: `Require`

4. **Test Connection**:
   - Click "Save"
   - Should see "WOS Render DB" in server list

### Option B: Using psql (Command Line)

```bash
# Connect to database
psql "postgresql://wos_admin:password@hostname.oregon-postgres.render.com/wos_accounts?sslmode=require"

# Should see:
# wos_accounts=>
```

---

## Step 3: Run Database Setup Script

### Using pgAdmin:

1. **Open Query Tool**:
   - In pgAdmin, expand "WOS Render DB" → Databases → wos_accounts
   - Right-click "wos_accounts" → Query Tool

2. **Load Setup Script**:
   - File → Open → Select `Backend/Database/setup.sql`

3. **Execute Script**:
   - Click ⚡ "Execute" button (or press F5)
   - Should see messages:
     ```
     NOTICE: Database setup complete!
     NOTICE: Tables created:
     NOTICE:   - player_accounts
     NOTICE:   - player_inventories
     ...
     ```

4. **Verify Tables**:
   - In left panel, expand Tables
   - Should see 5 tables:
     - player_accounts
     - player_inventories
     - item_definitions
     - trade_log
     - port_storage

### Using psql:

```bash
# Connect to database
psql "postgresql://wos_admin:password@hostname.oregon-postgres.render.com/wos_accounts?sslmode=require"

# Run setup script
\i Backend/Database/setup.sql

# Verify tables
\dt

# Should see 5 tables listed
```

---

## Step 4: Verify Setup

### Check Item Definitions:

```sql
SELECT item_id, item_name, item_type, max_stack
FROM item_definitions
ORDER BY item_name;
```

**Expected Output**:
```
     item_id     |     item_name      |  item_type | max_stack
-----------------+--------------------+------------+-----------
 ammo_crate      | Ammunition Crate   | consumable |         5
 rum_barrel      | Barrel of Rum      | cargo      |        10
 health_potion   | Health Potion      | consumable |        10
 iron_ore        | Iron Ore           | cargo      |        50
 repair_kit      | Repair Kit         | consumable |         5
 sail            | Replacement Sail   | equipment  |         1
 cannon          | Ship Cannon        | equipment  |         1
 timber          | Timber             | cargo      |        20
```

### Check Table Structure:

```sql
-- Check player_accounts columns
\d player_accounts

-- Expected columns: player_id, username, password_hash, email, created_at, last_login
```

---

## Step 5: Configure Backend API

### Update Connection String

1. **Open Backend Project**:
   - File: `Backend/EdgegapProxy/appsettings.json`

2. **Add Connection String**:
   ```json
   {
     "ConnectionStrings": {
       "PostgreSQL": "Host=hostname.oregon-postgres.render.com;Database=wos_accounts;Username=wos_admin;Password=your_password;SSL Mode=Require;Trust Server Certificate=true"
     },
     "Jwt": {
       "Secret": "your-256-bit-secret-key-change-this-in-production",
       "Issuer": "wos-game-server",
       "Audience": "wos-game-client",
       "ExpirationDays": 7
     }
   }
   ```

3. **Generate JWT Secret**:
   ```bash
   # PowerShell - Generate random 256-bit key
   -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})
   ```

4. **Update appsettings.Production.json**:
   - Use environment variables for production secrets
   - Never commit real passwords to Git

### Add to Render.com Environment Variables

1. **Go to Backend Service on Render.com**:
   - Dashboard → Your backend service (wos-edgegap-proxy)

2. **Add Environment Variables**:
   - Go to "Environment" tab
   - Add:
     ```
     Key: ConnectionStrings__PostgreSQL
     Value: Host=hostname.oregon-postgres.render.com;Database=wos_accounts;Username=wos_admin;Password=your_password;SSL Mode=Require;Trust Server Certificate=true

     Key: Jwt__Secret
     Value: [your generated secret]

     Key: Jwt__Issuer
     Value: wos-game-server

     Key: Jwt__Audience
     Value: wos-game-client

     Key: Jwt__ExpirationDays
     Value: 7
     ```

3. **Save Changes**:
   - Backend will automatically redeploy with new environment variables

---

## Step 6: Test Database Connection

### Create Test User

```sql
-- Insert test account
INSERT INTO player_accounts (username, password_hash, email)
VALUES ('testuser', '$2a$10$fakehashfortesingonly', 'test@example.com')
RETURNING player_id, username;

-- Should return:
--               player_id                | username
-- --------------------------------------+----------
--  xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx | testuser
```

### Create Test Inventory

```sql
-- Insert test inventory (auto-created when player registers)
INSERT INTO player_inventories (player_id)
VALUES ('xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx');  -- Use player_id from above

-- Verify
SELECT player_id, last_updated, version FROM player_inventories;
```

---

## Troubleshooting

### Connection Refused

**Error**: `could not connect to server: Connection refused`

**Fix**:
- Verify SSL mode is set to "Require"
- Check firewall allows outbound connections to Render.com
- Verify database is "Available" status on Render.com

---

### SSL Certificate Error

**Error**: `SSL error: certificate verify failed`

**Fix**:
- Add `Trust Server Certificate=true` to connection string
- Or download Render.com SSL certificate

---

### Authentication Failed

**Error**: `password authentication failed for user "wos_admin"`

**Fix**:
- Copy password exactly from Render.com connection string
- Password may contain special characters - use quotes in connection string

---

### Table Already Exists

**Error**: `ERROR: relation "player_accounts" already exists`

**Fix**:
- Script uses `CREATE TABLE IF NOT EXISTS` - safe to re-run
- To reset database:
  ```sql
  DROP TABLE IF EXISTS port_storage CASCADE;
  DROP TABLE IF EXISTS trade_log CASCADE;
  DROP TABLE IF EXISTS player_inventories CASCADE;
  DROP TABLE IF EXISTS item_definitions CASCADE;
  DROP TABLE IF EXISTS player_accounts CASCADE;
  ```
- Then re-run setup script

---

## Database Maintenance

### Backup Database

**Render.com provides**:
- Daily automated backups (retained for 7 days on free tier)
- Manual backups available in dashboard

**Manual Backup**:
```bash
# Export to SQL file
pg_dump "postgresql://wos_admin:password@hostname.oregon-postgres.render.com/wos_accounts" > backup.sql

# Restore from backup
psql "postgresql://wos_admin:password@hostname.oregon-postgres.render.com/wos_accounts" < backup.sql
```

---

### Monitor Database Size

```sql
-- Check database size
SELECT pg_size_pretty(pg_database_size('wos_accounts'));

-- Check table sizes
SELECT
    tablename,
    pg_size_pretty(pg_total_relation_size(tablename::text)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(tablename::text) DESC;
```

**Free Tier Limit**: 1GB total

---

### Clean Up Old Data (Optional)

```sql
-- Delete accounts inactive for 90+ days
DELETE FROM player_accounts
WHERE last_login < NOW() - INTERVAL '90 days';

-- Clean up old trade logs (keep last 30 days)
DELETE FROM trade_log
WHERE trade_timestamp < NOW() - INTERVAL '30 days';
```

---

## Next Steps

After database setup is complete:

1. ✅ Database created and running
2. ✅ Tables created and verified
3. ✅ Item definitions seeded
4. ⏳ **Next**: Extend backend API with auth and inventory endpoints
5. ⏳ **Then**: Test endpoints with Postman
6. ⏳ **Finally**: Implement Unity server-side inventory manager

---

## Connection Info (Save This!)

**Database Name**: `wos_accounts`

**Connection String** (for backend):
```
Host=hostname.oregon-postgres.render.com;Database=wos_accounts;Username=wos_admin;Password=your_password;SSL Mode=Require;Trust Server Certificate=true
```

**pgAdmin Connection**:
- Host: `hostname.oregon-postgres.render.com`
- Port: `5432`
- Database: `wos_accounts`
- Username: `wos_admin`
- Password: `[from Render.com]`
- SSL Mode: `Require`

**Tables Created**:
1. `player_accounts` - User authentication
2. `player_inventories` - Tetris cargo grids
3. `item_definitions` - Item catalog (8 items seeded)
4. `trade_log` - Trade audit trail
5. `port_storage` - Port-based storage

---

**Status**: ✅ Database setup guide complete
**Next**: Continue to Phase 1.2 - Extend Backend API
