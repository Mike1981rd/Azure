# 🔐 Auth-04: Login Network Connection Errors

[← Back to Auth Index](./auth-00-index.md) | [← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [Problem Summary](#problem-summary)
- [Symptoms](#symptoms)
- [Root Causes](#root-causes)
- [Solutions](#solutions)
- [Prevention](#prevention)
- [Related Issues](#related-issues)

---

## Problem Summary

**Login attempts fail with network errors despite API working correctly in Swagger, typically due to incorrect port configuration or axios setup.**

**Affects**: Login functionality, Authentication flow
**Frequency**: Very Common in initial setup
**Severity**: High (blocks all authenticated features)
**First seen**: Initial login implementation

---

## Symptoms

### Primary Symptoms
- [ ] Login works in Swagger but fails from Next.js
- [ ] "Network Error" shown in UI after login attempt
- [ ] Console shows `ERR_CONNECTION_REFUSED`
- [ ] API is running and accessible via browser

### Error Messages
```
Error en login: AxiosError
Failed to load resource: net::ERR_CONNECTION_REFUSED
:7224/api/auth/login:1  Failed to load resource: net::ERR_CONNECTION_REFUSED
```

### When It Occurs
- First login attempt from Next.js
- After changing API ports
- When API URL is misconfigured

---

## Root Causes

### Cause 1: Wrong Port in Frontend Configuration
**Description**: Frontend configured with port 7224 (HTTPS) but API running on 5266 (HTTP)
**How to verify**: 
```bash
# Check actual API port in launchSettings.json
cat Properties/launchSettings.json | grep applicationUrl
# Output: "applicationUrl": "http://localhost:5266"
```

### Cause 2: Hardcoded URLs Instead of Environment Variables
**Description**: API URL hardcoded in multiple places with wrong port
**How to verify**: Search for "7224" or "localhost" in frontend code

### Cause 3: axios Not Using Configured Base URL
**Description**: Auth service using raw axios instead of configured instance
**How to verify**: Check if `import axios from 'axios'` instead of `import { api }`

---

## Solutions

### 🚀 Quick Fix
**Time**: < 5 minutes

1. Create `.env.local` with correct port:
```bash
# websitebuilder-admin/.env.local
NEXT_PUBLIC_API_URL=http://localhost:5266/api
```

2. Restart Next.js (REQUIRED for env changes):
```bash
# Ctrl+C then
npm run dev
```

### 📋 Step-by-Step Solution

#### Step 1: Find Correct API Port
```json
// Properties/launchSettings.json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5266"  // ← Your actual port
    }
  }
}
```

#### Step 2: Configure Environment Variable
```bash
# websitebuilder-admin/.env.local
NEXT_PUBLIC_API_URL=http://localhost:5266/api
```

#### Step 3: Update Auth Service to Use Configured API
```typescript
// src/services/auth.service.ts
import { api } from '@/lib/api';  // NOT import axios from 'axios'

class AuthService {
  async login(credentials: LoginDto): Promise<AuthResponse> {
    // Use 'api' not 'axios'
    const response = await api.post<AuthResponse>(`${this.baseUrl}/login`, credentials);
    return response.data;
  }
}
```

#### Step 4: Verify API Configuration
```typescript
// src/lib/api.ts
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5266/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  }
});
```

### 🔧 Alternative Solutions

**If you need HTTPS**:
1. Use the HTTPS profile in Visual Studio
2. Update .env.local to use https://localhost:7xxx/api
3. Handle certificate warnings in development

---

## Prevention

### Best Practices
1. **Always use environment variables** for API URLs
2. **Check launchSettings.json first** before configuring frontend
3. **Use the configured axios instance** not raw axios
4. **Test with Swagger first** to verify API is working
5. **Remember to restart Next.js** after changing .env files

### Debugging Checklist
- [ ] Can access Swagger at http://localhost:PORT/swagger
- [ ] .env.local has correct NEXT_PUBLIC_API_URL
- [ ] Restarted Next.js after env changes
- [ ] Using `api` instance not raw `axios`
- [ ] No hardcoded URLs in code

---

## Related Issues

### See Also
- [API-02: ASP.NET + Next.js Integration](../api/api-02-aspnet-nextjs-integration.md) - General port configuration
- [API-01: CORS Problems](../api/api-01-cors-problems.md) - If login blocked by CORS
- [Auth-03: Next.js Integration](./auth-03-nextjs-integration.md) - Frontend auth setup

### Often Occurs With
- Initial project setup
- Cloning project on new machine
- Switching between HTTP/HTTPS

---

## 🏷️ Search Keywords

`login` `network error` `ERR_CONNECTION_REFUSED` `axios` `port` `5266` `7224` `env.local` `NEXT_PUBLIC_API_URL`

---

**Last Updated**: August 2025
**Contributors**: WebsiteBuilder Team
**Verified On**: ASP.NET Core 8, Next.js 14, axios 1.x