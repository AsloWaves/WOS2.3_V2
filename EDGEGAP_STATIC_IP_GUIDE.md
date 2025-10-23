# Edgegap Static IP Configuration Guide

Solutions for keeping Edgegap server IP consistent across deployments.

---

## Problem

By default, Edgegap assigns a new IP address each time you deploy your server. This creates issues:
- ❌ Must update `ServerConfig.asset` after each deployment
- ❌ Players need new client builds after server updates
- ❌ Manual step in update workflow
- ❌ Higher chance of errors

---

## Solution 1: Reserved IPs (Recommended)

Edgegap offers **Reserved IPs** that stay the same across deployments.

### How to Get Reserved IP

1. **Contact Edgegap Support**:
   - Email: support@edgegap.com
   - Or through Edgegap dashboard support chat

2. **Request Reserved IP**:
   ```
   Subject: Reserved IP Request for WOS Naval MMO

   Hi Edgegap team,

   I'd like to request a reserved IP for my game server deployment.

   Application: wos-naval-mmo
   Region: Seattle, Washington (us-west-2)

   This will help streamline our deployment workflow and avoid
   client updates when we redeploy the server.

   Thank you!
   ```

3. **Edgegap Will Provision**:
   - They'll assign you a static IP
   - Usually takes 1-2 business days
   - May have additional cost (check your plan)

4. **Update Your Deployment**:
   - Deploy server using reserved IP
   - Update `ServerConfig.asset` **one last time**
   - Future deployments use same IP

### Pricing Note

Reserved IPs may have additional costs:
- **Free Tier**: May not include reserved IPs
- **Paid Plans**: Often included or small fee ($5-10/month)
- **Contact Edgegap**: Confirm pricing for your plan

### Benefits

✅ IP never changes
✅ No client updates needed for server deployments
✅ Simpler workflow
✅ Reduced errors
✅ Better for production

---

## Solution 2: DNS with Subdomain (Alternative)

Use a domain name instead of IP address directly.

### How to Set Up

**Step 1: Get a Domain** (if you don't have one)
- Purchase from: Namecheap, GoDaddy, CloudFlare, etc.
- Example: `wavesofsteel.com`
- Cost: ~$10-15/year

**Step 2: Create A Record**
1. Go to your domain's DNS settings
2. Create new **A Record**:
   - **Name**: `server` (or `game`)
   - **Type**: A
   - **Value**: Current Edgegap IP (e.g., 172.232.162.171)
   - **TTL**: 300 (5 minutes)

3. Result: `server.wavesofsteel.com` points to your Edgegap IP

**Step 3: Update ServerConfig**
```csharp
// In ServerConfig.asset Inspector:
Server Address: "server.wavesofsteel.com:30509"
```

**Step 4: When Edgegap IP Changes**
1. Deploy new server to Edgegap
2. Get new IP from dashboard
3. Update DNS A Record to new IP
4. Wait 5 minutes for DNS propagation
5. **No client rebuild needed!**

### Benefits

✅ Client connects to domain name, not IP
✅ Change IP without client updates
✅ Professional appearance
✅ Can add multiple regions (us-server, eu-server, etc.)

### Drawbacks

⚠️ DNS propagation delay (5-60 minutes)
⚠️ Requires domain purchase
⚠️ Manual DNS update after each deployment

---

## Solution 3: Edgegap Load Balancer (Enterprise)

For large-scale deployments, Edgegap offers load balancers with static endpoints.

**Features**:
- Single static endpoint
- Automatic failover
- Multiple server instances
- Geographic load balancing

**Cost**: Enterprise plan required

**Contact**: Edgegap sales team

---

## Recommended Approach

### For Your Current Setup (Testing/Development)

**Use Solution 1: Reserved IP**
- Request from Edgegap support
- One-time setup
- No ongoing maintenance
- Cleanest solution

### If Reserved IP Not Available

**Use Solution 2: DNS Subdomain**
- Purchase cheap domain (~$10/year)
- Set up A record
- Update DNS when IP changes
- Still better than updating clients

---

## Implementation Steps

### Option A: Reserved IP Setup

```powershell
# 1. Request reserved IP from Edgegap (email/support)

# 2. Once provisioned, deploy server
Unity → Tools → Edgegap → Server Hosting → Deploy

# 3. Verify using reserved IP
# Check Edgegap dashboard

# 4. Update ServerConfig.asset ONE LAST TIME
# In Unity Inspector:
# Server Address: [RESERVED_IP]:[PORT]

# 5. Build and deploy client
.\update_client.ps1 -NewVersion "1.0.X" -Description "Updated for reserved IP"

# 6. Future server updates: NO client rebuild needed
# Just deploy new server version to same reserved IP
```

### Option B: DNS Setup

```powershell
# 1. Purchase domain (e.g., wavesofsteel.com)

# 2. Create A Record in DNS settings:
# Name: server
# Type: A
# Value: [CURRENT_EDGEGAP_IP]
# TTL: 300

# 3. Update ServerConfig.asset
# Server Address: "server.wavesofsteel.com:[PORT]"

# 4. Build and deploy client
.\update_client.ps1 -NewVersion "1.0.X" -Description "Using DNS endpoint"

# 5. When Edgegap IP changes:
# - Deploy new server
# - Update DNS A Record
# - Wait 5-10 minutes
# - Clients reconnect automatically
```

---

## Current Temporary Solution (Already Implemented)

### Automatic Localhost in Editor

I've updated `ServerConfig.cs` to automatically use localhost in Unity Editor:

**How It Works**:
- ✅ **In Unity Editor**: Uses `localhost:7777` (for testing)
- ✅ **In Builds**: Uses Edgegap IP (for production)
- ✅ **Automatic**: No manual switching needed

**Configuration** (in Unity Inspector):
1. Select `Assets/Resources/ServerConfigs/ServerConfig.asset`
2. **Production Server** section:
   - Server Address: `172.232.162.171:30509` (your Edgegap IP)
3. **Local Testing** section:
   - Use Localhost In Editor: ✓ (checked)
   - Local Server Address: `localhost:7777`

**Testing Workflow**:
```
Editor Play Mode:
- Automatically uses localhost:7777
- Tests against Mirror's "Host (Server + Client)" mode
- No Edgegap connection

Builds:
- Automatically uses Edgegap IP
- Connects to production server
- No code changes needed
```

---

## Quick Reference

| Method | Cost | Setup Time | Maintenance | Reliability |
|--------|------|------------|-------------|-------------|
| **Reserved IP** | $0-10/month | 1-2 days | None | ⭐⭐⭐⭐⭐ |
| **DNS Subdomain** | $10/year | 30 minutes | 5 min per deploy | ⭐⭐⭐⭐ |
| **Load Balancer** | $$$$ | Enterprise setup | Managed | ⭐⭐⭐⭐⭐ |
| **Manual IP Updates** | $0 | None | 30 min per deploy | ⭐⭐ |

---

## Verification

### Test Reserved IP Setup

```powershell
# After getting reserved IP:

# 1. Deploy server to Edgegap
# 2. Check IP in dashboard - should match reserved IP
# 3. Test connection from client
# 4. Redeploy server (new version)
# 5. Verify IP stayed the same
# 6. Client reconnects without rebuild
```

### Test DNS Setup

```powershell
# After DNS setup:

# 1. Check DNS propagation
nslookup server.wavesofsteel.com

# Should return your Edgegap IP

# 2. Test connection from client
# 3. Deploy new server with different IP
# 4. Update DNS A Record
# 5. Wait 5-10 minutes
# 6. Test - nslookup should show new IP
# 7. Client reconnects without rebuild
```

---

## FAQ

**Q: Do I need Reserved IP for testing?**
A: No. Use the automatic localhost feature in Editor for local testing.

**Q: How much does Reserved IP cost?**
A: Depends on your Edgegap plan. Contact support for pricing. Often free or ~$5-10/month.

**Q: Can I use both Reserved IP and DNS?**
A: Yes! Point DNS A Record to your reserved IP for maximum reliability.

**Q: What happens if I change Edgegap region?**
A: Reserved IP is region-specific. Changing regions may require new reserved IP.

**Q: Is Reserved IP worth it?**
A: Yes, if you deploy frequently. Saves time and prevents client update headaches.

**Recommended**: Start with Reserved IP request. While waiting, use automatic localhost in Editor for testing.

---

## Contact Edgegap

- **Email**: support@edgegap.com
- **Dashboard**: https://app.edgegap.com
- **Documentation**: https://docs.edgegap.com
- **Discord**: https://discord.gg/edgegap
