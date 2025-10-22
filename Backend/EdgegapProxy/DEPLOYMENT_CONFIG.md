# Backend Deployment Configuration Guide

Complete guide for configuring and deploying the WOS Backend API to Render.com.

## Prerequisites

- Render.com account (free tier works)
- PostgreSQL database created on Render.com (see `Database/RENDER_POSTGRES_SETUP.md`)
- Backend code ready to deploy

---

## Step 1: PostgreSQL Database Setup

### 1.1 Create PostgreSQL Database on Render.com

Follow the complete setup guide in `Database/RENDER_POSTGRES_SETUP.md`. This will:
- Create a PostgreSQL 14+ instance on Render.com
- Run the `setup.sql` schema and seed data
- Provide you with the connection string

### 1.2 Get Connection String

After creating the database, Render.com provides connection details:

**Internal Connection String** (for Render.com services):
```
postgresql://username:password@hostname/database?sslmode=require
```

**External Connection String** (for local testing):
```
postgresql://username:password@hostname:port/database?sslmode=require
```

**Parse into appsettings.json format**:
```
Host=hostname;Database=database_name;Username=username;Password=password;SslMode=Require
```

---

## Step 2: Generate JWT Secret

The JWT secret must be a secure random string. **NEVER use a predictable value.**

### Option A: Generate with PowerShell (Recommended)

```powershell
# Generate cryptographically secure 64-character base64 string
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

**Example Output**:
```
Kj7mP9xQw2zRvT5nBcY8fDhU3sL4aE6gX1oI0yWpN7qMjV9uZtA8bH5cG2kF4rS6
```

### Option B: Generate with Bash/Linux

```bash
openssl rand -base64 48
```

### Option C: Online Generator (Use with Caution)

Visit: https://generate-secret.vercel.app/64

**⚠️ Security Note**: For production, use local generation (Option A or B) to ensure secret never leaves your machine.

---

## Step 3: Update appsettings.json (Local Development)

Edit `Backend/EdgegapProxy/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PostgreSQL": "Host=dpg-abc123xyz.oregon-postgres.render.com;Database=wos_accounts;Username=wos_user;Password=YOUR_DB_PASSWORD;SslMode=Require"
  },
  "Jwt": {
    "Secret": "Kj7mP9xQw2zRvT5nBcY8fDhU3sL4aE6gX1oI0yWpN7qMjV9uZtA8bH5cG2kF4rS6",
    "Issuer": "wos-game-server",
    "Audience": "wos-game-client",
    "ExpirationDays": 7
  },
  "Edgegap": {
    "ApiBaseUrl": "https://api.edgegap.com/v1",
    "HealthCheckPort": 8080,
    "HealthCheckTimeout": 5,
    "RequiredTags": ["production"]
  }
}
```

**Replace**:
- `Host`: Your Render PostgreSQL hostname
- `Database`: `wos_accounts` (from setup.sql)
- `Username`: Your database username
- `Password`: Your database password
- `Secret`: Your generated JWT secret (64+ characters)

---

## Step 4: Update appsettings.Production.json (Production Deployment)

For production deployment on Render.com, create `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "AllowedHosts": "*"
}
```

**Why minimal?** Production secrets will be set via **Environment Variables** on Render.com (more secure).

---

## Step 5: Configure Render.com Environment Variables

### 5.1 Navigate to Your Web Service

1. Go to Render.com Dashboard
2. Select your existing web service (wos-edgegap-proxy)
3. Click **Environment** tab

### 5.2 Add Environment Variables

**Format**: `SectionName__SettingName=value` (double underscore separates sections)

**Required Variables**:

| Key | Value | Example |
|-----|-------|---------|
| `ConnectionStrings__PostgreSQL` | Your PostgreSQL connection string | `Host=dpg-abc123.oregon-postgres.render.com;Database=wos_accounts;Username=wos_user;Password=xyz789;SslMode=Require` |
| `Jwt__Secret` | Your generated JWT secret | `Kj7mP9xQw2zRvT5nBcY8fDhU3sL4aE6gX1oI0yWpN7qMjV9uZtA8bH5cG2kF4rS6` |
| `Jwt__Issuer` | `wos-game-server` | `wos-game-server` |
| `Jwt__Audience` | `wos-game-client` | `wos-game-client` |
| `Jwt__ExpirationDays` | `7` | `7` |

**Edgegap Variables** (already configured):

| Key | Value |
|-----|-------|
| `Edgegap__ApiBaseUrl` | `https://api.edgegap.com/v1` |
| `Edgegap__HealthCheckPort` | `8080` |
| `Edgegap__HealthCheckTimeout` | `5` |

### 5.3 Save and Redeploy

After adding environment variables, Render.com will automatically redeploy your service.

---

## Step 6: Verify Deployment

### 6.1 Check Startup Logs

In Render.com dashboard, view **Logs** tab. Look for:

```
[Database] ✅ PostgreSQL connection successful
[Server] 🚀 WOS Edgegap Proxy started
[Server] 📡 Server discovery: /api/servers
[Server] 🔐 Authentication: /api/auth/*
[Server] 🎒 Inventory: /api/inventory/*
[Server] 📦 Items: /api/items/definitions
```

**If you see**:
```
[Database] ❌ PostgreSQL connection failed - check configuration
```

**Troubleshooting**:
- Verify `ConnectionStrings__PostgreSQL` environment variable is correct
- Check database hostname, username, password
- Ensure database is running on Render.com
- Verify `SslMode=Require` is included

### 6.2 Test Endpoints with curl

**Test health endpoint**:
```bash
curl https://wos-edgegap-proxy.onrender.com/health
```

**Expected Response**:
```json
{"status":"healthy","timestamp":1234567890}
```

**Test item definitions endpoint** (no auth required):
```bash
curl https://wos-edgegap-proxy.onrender.com/api/items/definitions
```

**Expected Response**:
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
      "isConsumable": true,
      "baseValue": 25,
      "weight": 0.5
    },
    ...
  ]
}
```

**Test registration endpoint**:
```bash
curl -X POST https://wos-edgegap-proxy.onrender.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testplayer",
    "password": "TestPassword123",
    "email": "test@example.com"
  }'
```

**Expected Response**:
```json
{
  "success": true,
  "playerId": "123e4567-e89b-12d3-a456-426614174000",
  "username": "testplayer",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "lastLogin": null
}
```

**Test login endpoint**:
```bash
curl -X POST https://wos-edgegap-proxy.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testplayer",
    "password": "TestPassword123"
  }'
```

**Test inventory endpoint** (requires JWT token):
```bash
# Save token from register/login response
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
PLAYER_ID="123e4567-e89b-12d3-a456-426614174000"

curl https://wos-edgegap-proxy.onrender.com/api/inventory/$PLAYER_ID \
  -H "Authorization: Bearer $TOKEN"
```

**Expected Response**:
```json
{
  "success": true,
  "inventory": {
    "width": 10,
    "height": 10,
    "cells": [[null, null, ...], ...],
    "items": {}
  },
  "lastUpdated": "2025-01-21T12:00:00Z",
  "version": 1
}
```

---

## Step 7: Security Checklist

### Production Security Requirements

- ✅ **JWT Secret**: 64+ character random string, never committed to Git
- ✅ **Database Password**: Strong password (Render.com generates automatically)
- ✅ **SSL/TLS**: SslMode=Require for database connections
- ✅ **Environment Variables**: All secrets stored in Render.com environment variables, NOT in appsettings.json
- ✅ **Password Hashing**: BCrypt with cost factor 10 (implemented in AuthController)
- ✅ **Token Expiration**: 7-day JWT expiration (configurable)
- ✅ **HTTPS**: Render.com provides automatic HTTPS for all services

### Development vs Production Configuration

**Development (appsettings.json)**:
- Contains actual secrets (for local testing only)
- File is `.gitignore`d (should NOT be committed)
- Uses external database connection string

**Production (Render.com Environment Variables)**:
- Secrets stored securely in Render.com
- Uses internal database connection string (faster, more secure)
- Automatic HTTPS and SSL certificate management

---

## Step 8: Common Issues

### Issue: "JWT Secret not configured"

**Cause**: Missing `Jwt__Secret` environment variable

**Solution**:
1. Generate JWT secret (see Step 2)
2. Add `Jwt__Secret` environment variable on Render.com
3. Redeploy service

---

### Issue: "PostgreSQL connection failed"

**Cause**: Incorrect connection string or database not running

**Solutions**:
1. Verify database is running on Render.com
2. Check `ConnectionStrings__PostgreSQL` environment variable
3. Ensure `SslMode=Require` is included
4. Use **Internal Connection String** (not external) on Render.com
5. Verify database schema was created (run `setup.sql`)

---

### Issue: "Unauthorized" when accessing inventory

**Cause**: Invalid or missing JWT token

**Solutions**:
1. Register or login to get a valid token
2. Include token in `Authorization: Bearer <token>` header
3. Verify token hasn't expired (7-day default)
4. Check `Jwt__Secret` matches between token generation and validation

---

### Issue: "Username already exists" when testing

**Cause**: Test user already registered in database

**Solutions**:
1. Use a different username for testing
2. Delete test user from database:
   ```sql
   DELETE FROM player_accounts WHERE username = 'testplayer';
   ```

---

## Step 9: Next Steps

After successful deployment:

1. ✅ **Document API Base URL**: Update Unity client configuration with production URL
2. ✅ **Test All Endpoints**: Use Postman or curl to verify all endpoints work
3. ✅ **Monitor Logs**: Check Render.com logs for errors or warnings
4. ✅ **Proceed to Phase 2**: Unity Server-Side Implementation (AccountManager, ServerInventoryManager)

---

## Configuration Files Reference

### Local Development
- `appsettings.json` - Contains all configuration including secrets (DO NOT COMMIT)
- `appsettings.Development.json` - Development-specific overrides

### Production Deployment
- `appsettings.Production.json` - Minimal production config
- **Render.com Environment Variables** - All secrets and configuration

### Database
- `Database/setup.sql` - Schema and seed data
- `Database/RENDER_POSTGRES_SETUP.md` - Database setup guide

---

## Environment Variable Template

For quick reference, here's the complete set of environment variables for Render.com:

```bash
# PostgreSQL Database
ConnectionStrings__PostgreSQL=Host=YOUR_HOST;Database=wos_accounts;Username=YOUR_USER;Password=YOUR_PASSWORD;SslMode=Require

# JWT Authentication
Jwt__Secret=YOUR_64_CHAR_SECRET_HERE
Jwt__Issuer=wos-game-server
Jwt__Audience=wos-game-client
Jwt__ExpirationDays=7

# Edgegap Configuration (already configured)
Edgegap__ApiBaseUrl=https://api.edgegap.com/v1
Edgegap__HealthCheckPort=8080
Edgegap__HealthCheckTimeout=5
```

---

## Testing Checklist

Before moving to Phase 2, verify:

- [ ] Database connection successful on startup
- [ ] `/health` endpoint returns 200 OK
- [ ] `/api/items/definitions` returns item catalog
- [ ] `/api/auth/register` creates new account and returns JWT token
- [ ] `/api/auth/login` authenticates existing user and returns JWT token
- [ ] `/api/auth/validate` validates JWT token correctly
- [ ] `/api/inventory/{playerId}` returns empty inventory for new user
- [ ] `/api/inventory/{playerId}` requires valid JWT token (401 without)
- [ ] Inventory save/load with optimistic locking works correctly

---

**Phase 1: Backend Foundation - Complete ✅**

Next: **Phase 2: Unity Server-Side Implementation**
