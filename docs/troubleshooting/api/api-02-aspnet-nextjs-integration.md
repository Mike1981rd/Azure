# 🌐 API-02: ASP.NET Core + Next.js Integration Issues

[← Back to API Index](./api-00-index.md) | [← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [Problem Summary](#problem-summary)
- [Symptoms](#symptoms)
- [Root Causes](#root-causes)
- [Solutions](#solutions)
- [Prevention](#prevention)
- [Related Issues](#related-issues)

---

## Problem Summary

**Connection and integration problems between ASP.NET Core API backend and Next.js frontend, including port mismatches, protocol issues, and environment configuration.**

**Affects**: API calls, Authentication, All frontend-backend communication
**Frequency**: Very Common (especially in initial setup)
**Severity**: High (blocks all functionality)
**First seen**: Initial project setup

---

## Symptoms

### Primary Symptoms
- [ ] `net::ERR_CONNECTION_REFUSED` in browser console
- [ ] `ECONNREFUSED` errors in terminal
- [ ] API calls returning 404 even though endpoints exist
- [ ] Login works in Swagger but not from Next.js
- [ ] Different behavior between HTTP and HTTPS

### Error Messages
```
Failed to load resource: net::ERR_CONNECTION_REFUSED
GET http://localhost:7224/api/auth/login net::ERR_CONNECTION_REFUSED
AxiosError: connect ECONNREFUSED ::1:7224
```

### When It Occurs
- First time running the project
- After changing ports
- When switching between HTTP/HTTPS
- After restarting only one service

---

## Root Causes

### Cause 1: Wrong Port Configuration
**Description**: Frontend is trying to connect to wrong API port
**How to verify**: 
1. Check `launchSettings.json` for actual port
2. Check `.env.local` for configured port
3. Compare browser network tab URL with Swagger URL

### Cause 2: API Not Running
**Description**: Forgot to start the ASP.NET Core API
**How to verify**: 
- Try accessing http://localhost:[port]/swagger
- Check if Visual Studio is running
- Look for console window with API logs

### Cause 3: HTTP vs HTTPS Mismatch
**Description**: Frontend using HTTP but API expects HTTPS or vice versa
**How to verify**: 
- Check if Swagger URL uses http:// or https://
- Check NEXT_PUBLIC_API_URL protocol

---

## Solutions

### 🚀 Quick Fix
**Time**: < 5 minutes

1. Find correct API port:
```bash
# Check launchSettings.json
cat Properties/launchSettings.json | grep applicationUrl
# Usually: http://localhost:5266
```

2. Update Next.js config:
```bash
# In websitebuilder-admin/.env.local
NEXT_PUBLIC_API_URL=http://localhost:5266/api
```

3. Restart Next.js:
```bash
# Ctrl+C to stop, then:
npm run dev
```

### 📋 Step-by-Step Solution

#### Step 1: Verify API Port
Open `/Properties/launchSettings.json`:
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5266",  // ← This is your port
    }
  }
}
```

#### Step 2: Configure Next.js
Create/update `websitebuilder-admin/.env.local`:
```env
# Use the exact port from launchSettings.json
NEXT_PUBLIC_API_URL=http://localhost:5266/api
```

#### Step 3: Update Constants (if needed)
Check `src/lib/constants.ts`:
```typescript
export const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5266/api';
```

#### Step 4: Restart Both Services
1. Stop Next.js (Ctrl+C)
2. Stop API (Shift+F5 in Visual Studio)
3. Start API (F5 in Visual Studio)
4. Start Next.js (`npm run dev`)

### 🔧 Alternative Solutions

**If using HTTPS**:
```env
NEXT_PUBLIC_API_URL=https://localhost:7224/api
```

**If using multiple profiles**:
```bash
# Run specific profile
dotnet run --launch-profile "https"
```

---

## Prevention

### Best Practices
1. **Always check launchSettings.json** before configuring frontend
2. **Use consistent protocol** (all HTTP or all HTTPS)
3. **Document the ports** in README
4. **Create a startup script** that launches both services

### Recommended Setup Script
```bash
# create start-dev.bat (Windows)
start "API" cmd /k "cd WebsiteBuilderAPI && dotnet run"
start "Frontend" cmd /k "cd websitebuilder-admin && npm run dev"
```

---

## Related Issues

### See Also
- [API-01: CORS Problems](./api-01-cors-problems.md) - If connection works but CORS blocks
- [Auth-04: Login Problems](../auth/auth-04-login-problems.md) - If only auth endpoints fail
- [General-02: Development Setup](../general/general-02-development-setup.md) - For environment issues

### Often Occurs With
- Fresh project clone
- Team member onboarding
- Development machine changes

---

## 🏷️ Search Keywords

`ERR_CONNECTION_REFUSED` `port` `5266` `7224` `localhost` `ECONNREFUSED` `launchSettings.json` `.env.local` `NEXT_PUBLIC_API_URL`

---

**Last Updated**: August 2025
**Contributors**: WebsiteBuilder Team
**Verified On**: ASP.NET Core 8, Next.js 14