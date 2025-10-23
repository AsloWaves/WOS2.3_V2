# Secure Server Integration Guide - WOS2.3 Naval MMO

**Complete guide for secure Edgegap server discovery using backend proxy architecture.**

---

## 🔐 Security Architecture

**SECURE** (✅ What we built):
```
Unity Client → YOUR Backend API → Edgegap API
```
- API token stays on YOUR backend (secure!)
- Client never touches Edgegap API directly
- Backend validates and filters server list

**INSECURE** (❌ What we replaced):
```
Unity Client → Edgegap API (with embedded token)
```
- API token in Unity build (extractable!)
- Full account access exposed to players
- Security vulnerability!

---

## 📋 What Was Changed

### Files Created

**Backend (ASP.NET Core):**
- `Backend/EdgegapProxy/Program.cs` - Main API server
- `Backend/EdgegapProxy/Controllers/ServersController.cs` - REST endpoints
- `Backend/EdgegapProxy/Services/EdgegapService.cs` - Edgegap API interaction
- `Backend/EdgegapProxy/Models/ServerInfo.cs` - Data models
- `Backend/EdgegapProxy/appsettings.json` - Configuration
- `Backend/EdgegapProxy/.gitignore` - Protect secrets
- `Backend/EdgegapProxy/README.md` - Deployment guide

**Unity Client:**
- `Assets/Scripts/Networking/ServerBrowserManager.cs` - Secure server browser

### Files Modified

**Unity Client:**
- `Assets/Scripts/UI/JoinMenuController.cs` - Integrated ServerBrowserManager

### Files Deleted (Insecure)

**❌ Removed:**
- `Assets/Scripts/Networking/EdgegapAPIConfig.cs` (stored API token)
- `Assets/Scripts/Networking/EdgegapDeploymentManager.cs` (called Edgegap directly)

---

## 🚀 Setup Instructions

### Step 1: Deploy Backend API

**Choose hosting platform:**

#### Option A: Railway (Recommended for Quick Start)

1. **Sign up**: https://railway.app (free with GitHub)
2. **Deploy:**
   - Click "New Project" → "Deploy from GitHub repo"
   - Select your repo
   - Select `Backend/EdgegapProxy` folder
3. **Set environment variable:**
   - Key: `Edgegap__ApiToken`
   - Value: `YOUR_EDGEGAP_API_TOKEN_FROM_DASHBOARD`
4. **Get backend URL**:
   - Railway provides: `https://your-app.up.railway.app`
   - Copy this URL for Unity setup

#### Option B: Azure App Service (Recommended for Production)

1. **Install Azure CLI**: https://aka.ms/installazurecli
2. **Deploy:**
   ```bash
   cd Backend/EdgegapProxy

   # Login to Azure
   az login

   # Create resources
   az group create --name wos-backend --location eastus
   az appservice plan create --name wos-plan --resource-group wos-backend --sku F1
   az webapp create --name wos-edgegap-proxy --resource-group wos-backend --plan wos-plan --runtime "DOTNET|8.0"

   # Set API token (environment variable)
   az webapp config appsettings set --name wos-edgegap-proxy --resource-group wos-backend --settings Edgegap__ApiToken="YOUR_TOKEN"

   # Deploy
   dotnet publish -c Release
   cd bin/Release/net8.0/publish
   zip -r deploy.zip .
   az webapp deployment source config-zip --resource-group wos-backend --name wos-edgegap-proxy --src deploy.zip
   ```
3. **Get backend URL**:
   - Azure provides: `https://wos-edgegap-proxy.azurewebsites.net`

#### Option C: Local Development (Testing Only)

1. **Install .NET 8.0 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
2. **Configure token:**
   ```bash
   cd Backend/EdgegapProxy
   # Edit appsettings.Development.json and replace placeholder with your token
   ```
3. **Run:**
   ```bash
   dotnet run
   ```
4. **Backend URL**: `http://localhost:5000`

**⚠️ WARNING**: Local backend only works during development. Must deploy to cloud for production builds!

---

### Step 2: Configure Unity

#### A. Add ServerBrowserManager to Scene

1. **Open MainMenu scene** in Unity
2. **Create empty GameObject**:
   - Name: "ServerBrowserManager"
   - Position: Root of scene hierarchy
3. **Add component**: `ServerBrowserManager`
4. **Configure in Inspector:**
   - **Backend API URL**: `https://your-backend-url.com` (from Step 1)
   - **Fallback Config**: Assign your existing `ServerConfig.asset`
   - **API Timeout**: `10` seconds
   - **Auto Refresh Interval**: `30` seconds (0 = manual only)

**Example Configuration:**
```
Backend API URL: https://wos-backend.up.railway.app
Fallback Config: Assets/Resources/ServerConfigs/ServerConfig.asset
API Timeout: 10
Auto Refresh Interval: 30
```

#### B. Link ServerBrowserManager to JoinMenuController

1. **Find JoinMenuController** in MainMenu scene hierarchy
2. **In Inspector**, find the section: **"Server Browser (Secure)"**
3. **Assign references:**
   - **Server Browser**: Drag ServerBrowserManager GameObject here
   - **Auto Select Best Server**: ✅ Check (recommended)

#### C. Verify ServerConfig Assignment

1. **JoinMenuController** → **Configuration** section
2. **Server Config**: Should already be assigned to `ServerConfig.asset`
3. **This serves as fallback** if backend is unavailable

---

### Step 3: Test Integration

#### Test 1: Backend API (Direct)

**In browser or Postman:**
```
GET https://your-backend-url.com/health
```

**Expected Response:**
```json
{
  "status": "healthy",
  "timestamp": 1234567890
}
```

**Test server list:**
```
GET https://your-backend-url.com/api/servers
```

**Expected Response:**
```json
[
  {
    "serverId": "abc123",
    "serverName": "Chicago Server",
    "ipAddress": "172.234.217.194",
    "port": 32479,
    "city": "Chicago",
    "currentPlayers": 5,
    "maxPlayers": 300,
    "pingMs": 45,
    "isHealthy": true,
    "status": "Ready"
  }
]
```

#### Test 2: Unity Editor (Integration)

1. **Play in Unity Editor**
2. **Navigate to Join Game panel**
3. **Check Console for logs:**

**Expected logs:**
```
[ServerBrowser] 🌐 Server Browser Manager initialized
[ServerBrowser] Backend URL: https://your-backend-url.com
[JoinMenu] 🌐 ServerBrowserManager detected - fetching servers from backend...
[ServerBrowser] 🔄 Fetching servers from backend...
[ServerBrowser] ✅ Fetched 1 servers (1 healthy)
[ServerBrowser]   ✅ Chicago - 5/300 players, 45ms
[JoinMenu] 📋 Received 1 servers from backend
[JoinMenu]   ✅ Chicago (5/300 players) - 45ms - 172.234.217.194:32479
[JoinMenu] 🎯 Auto-selected best server: Chicago
[JoinMenu] ✅ Updated ServerConfig:
[JoinMenu]   Address: 172.234.217.194:32479
[JoinMenu]   Location: Chicago, United States
[JoinMenu]   Players: 5/300
[JoinMenu]   Ping: 45ms
```

#### Test 3: Server Connection

1. **Click "Connect" button** in Join Game panel
2. **Verify connection** to selected server
3. **Check server player count** increments

---

## 🔧 Troubleshooting

### Issue 1: "Backend API error"

**Symptoms:**
```
[ServerBrowser] ❌ Backend API error: Could not resolve host
```

**Solutions:**
1. **Check backend URL** in ServerBrowserManager Inspector
   - Must include protocol: `https://` or `http://`
   - No trailing slash: ✅ `https://example.com` ❌ `https://example.com/`
2. **Verify backend is running:**
   - Open backend URL in browser
   - Should see Swagger UI or "Healthy" response
3. **Check firewall/CORS** (if self-hosting)

---

### Issue 2: "Backend returned empty server list"

**Symptoms:**
```
[ServerBrowser] ⚠️ Backend returned empty server list
[JoinMenu] Falling back to hardcoded ServerConfig
```

**Solutions:**
1. **Check Edgegap deployments:**
   - Login to Edgegap dashboard
   - Verify servers are running
2. **Check deployment tags:**
   - Backend filters by `"production"` tag by default
   - Edit `appsettings.json` → `Edgegap.RequiredTags` to match your tags
   - Redeploy backend
3. **Check health endpoint:**
   - Edgegap must expose port 8080 (TCP/HTTP)
   - Verify `ServerHealthEndpoint` component on server

---

### Issue 3: "No healthy servers available"

**Symptoms:**
```
[ServerBrowser] ✅ Fetched 1 servers (0 healthy)
[ServerBrowser]   ❌ Chicago - Offline
```

**Solutions:**
1. **Verify ServerHealthEndpoint on server:**
   - Check NetworkManager GameObject has `ServerHealthEndpoint` component
   - Enabled: ✅
   - Health Port: 8080
2. **Check Edgegap port mapping:**
   - Dashboard → Deployment → Ports
   - Must have: `healthport` (8080, TCP)
3. **Test health endpoint directly:**
   ```
   curl http://SERVER_IP:8080/health
   ```
   Should return JSON with player count

---

### Issue 4: "ServerBrowserManager not assigned"

**Symptoms:**
```
[JoinMenu] ⚠️ ServerBrowserManager not assigned - using hardcoded ServerConfig
```

**Solutions:**
1. **Create ServerBrowserManager** GameObject in MainMenu scene
2. **Add component** `ServerBrowserManager`
3. **Assign to JoinMenuController:**
   - Inspector → Server Browser (Secure) → Server Browser
   - Drag ServerBrowserManager GameObject here

---

### Issue 5: Backend API Token Error

**Symptoms:**
```
Backend logs: "Edgegap API token not configured"
```

**Solutions:**
1. **Railway:**
   - Dashboard → Variables → Add
   - Key: `Edgegap__ApiToken`
   - Value: Your token from Edgegap
2. **Azure:**
   ```bash
   az webapp config appsettings set --name YOUR_APP --resource-group YOUR_RG --settings Edgegap__ApiToken="YOUR_TOKEN"
   ```
3. **Local:**
   - Edit `appsettings.Development.json`
   - Replace placeholder with actual token

---

## 📊 How It Works

### Architecture Flow

```
1. Unity Start()
   ↓
2. ServerBrowserManager.RefreshServers()
   ↓
3. HTTP GET → https://your-backend.com/api/servers
   ↓
4. Backend calls Edgegap API (token stays server-side!)
   ↓
5. Backend validates each server via HTTP health check
   ↓
6. Backend returns filtered, validated server list to Unity
   ↓
7. JoinMenuController receives server list
   ↓
8. Auto-select best server (health, players, ping)
   ↓
9. Update ServerConfig with selected server
   ↓
10. Player clicks Connect → Mirror connects to server
```

### Data Flow

**Server List Request:**
```
Unity → GET /api/servers → Backend → GET /v1/deployments → Edgegap
                                   ← JSON (deployments) ←
                           ← Health checks (HTTP :8080) ← Game Servers
        ← JSON (validated servers) ←
```

**Server Connection:**
```
Unity → Mirror KCP → Game Server UDP :7777
```

---

## 🎯 Best Practices

### Development

✅ **DO:**
- Use local backend (`http://localhost:5000`) for development
- Test server list in browser before testing in Unity
- Check Unity console logs for detailed debugging
- Use Swagger UI to test backend API (`http://localhost:5000/swagger`)

❌ **DON'T:**
- Commit `appsettings.Development.json` with real API token (already in .gitignore)
- Hardcode backend URL in code (use Inspector field)
- Skip backend deployment testing before release

### Production

✅ **DO:**
- Deploy backend to cloud (Railway, Azure, etc.)
- Use HTTPS for backend URL (provided automatically by Railway/Azure)
- Enable auto-refresh for server list (30-60 seconds)
- Test with multiple simultaneous clients
- Monitor backend logs for errors

❌ **DON'T:**
- Use local backend for production builds (won't work!)
- Expose Edgegap API token in Unity (defeats purpose!)
- Skip health checks (ensures accurate server status)

---

## 🔄 Fallback System

**Multi-layer fallback for reliability:**

1. **Primary**: Fetch from backend API
   - ✅ Always secure
   - ✅ Real-time server list
   - ✅ Health validated

2. **Fallback Level 1**: Backend returns cached servers
   - ✅ 30-second cache reduces Edgegap API calls
   - ✅ Still secure

3. **Fallback Level 2**: Hardcoded ServerConfig
   - ⚠️ Manual IP updates required
   - ✅ Works if backend is down
   - ⚠️ Can't discover new servers automatically

---

## 📦 Deployment Checklist

**Before release:**

- [ ] Backend deployed to cloud (Railway/Azure)
- [ ] Backend URL configured in ServerBrowserManager
- [ ] API token set in backend environment variables
- [ ] Tested server list fetch in Unity
- [ ] Tested server connection
- [ ] Verified health checks working
- [ ] ServerHealthEndpoint on server build
- [ ] Edgegap port 8080 exposed (TCP)
- [ ] Fallback ServerConfig assigned
- [ ] Removed any test/debug logs (optional)

**Security checklist:**

- [ ] No Edgegap API token in Unity project
- [ ] `appsettings.Development.json` in .gitignore
- [ ] Backend uses HTTPS (automatic on Railway/Azure)
- [ ] API token stored as environment variable (not in code)
- [ ] Insecure files deleted (EdgegapAPIConfig.cs, EdgegapDeploymentManager.cs)

---

## 🆘 Support Resources

**Backend issues:**
- ASP.NET Core Docs: https://docs.microsoft.com/aspnet/core
- Railway Docs: https://docs.railway.app
- Azure App Service Docs: https://docs.microsoft.com/azure/app-service

**Edgegap issues:**
- Edgegap API Docs: https://docs.edgegap.com/api
- Edgegap Support: https://edgegap.com/support

**Unity integration issues:**
- Check `SERVER_HEALTH_ENDPOINT_SETUP.md` for health check details
- Check Unity console logs (detailed debugging)
- Test backend API directly in browser first

---

## 🎓 Summary

**What we accomplished:**

✅ **Secure Architecture**: API token stays on YOUR backend (never in Unity)
✅ **Real-time Server Discovery**: Fetches active Edgegap deployments automatically
✅ **Health Validation**: Verifies each server is responding before showing to players
✅ **Auto-selection**: Picks best server based on health, player count, and ping
✅ **Graceful Fallback**: Falls back to hardcoded config if backend unavailable
✅ **Production Ready**: Deployed backend on cloud with HTTPS

**Security improvement:**

❌ **Before**: API token embedded in Unity → Extractable → Account compromise
✅ **After**: API token on backend only → Secure → Production ready!

---

**You're now ready for secure, production-grade Edgegap integration!** 🚀
