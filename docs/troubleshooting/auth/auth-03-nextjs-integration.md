# 🔐 Auth-03: Next.js Authentication Integration Issues

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

**Problems with Next.js middleware, AuthContext, and maintaining authentication state between page navigations and refreshes.**

**Affects**: Protected routes, Auth persistence, User session
**Frequency**: Common
**Severity**: Medium-High
**First seen**: When implementing protected routes

---

## Symptoms

### Primary Symptoms
- [ ] Successfully login but immediately redirected back to login
- [ ] Lost authentication state on page refresh
- [ ] Middleware blocking authenticated users
- [ ] `/api/auth/me` returns 404 on page load
- [ ] Infinite redirect loops

### Error Messages
```
GET http://localhost:5266/api/auth/me 404 (Not Found)
Warning: Extra attributes from the server: cz-shortcut-listen
Redirecting to /login?redirect=/dashboard
```

### When It Occurs
- After successful login
- On page refresh
- When navigating to protected routes
- When middleware checks cookies but auth uses localStorage

---

## Root Causes

### Cause 1: Middleware Using Cookies, Auth Using localStorage
**Description**: Next.js middleware checking for token in cookies but auth system stores in localStorage
**How to verify**: 
- Check middleware.ts for `request.cookies.get('token')`
- Check auth service for `localStorage.setItem('token')`

### Cause 2: 404 on /api/auth/me Treated as Error
**Description**: AuthContext treating normal 404 (when not logged in) as error
**How to verify**: Console shows 404 errors on page load even when not logged in

### Cause 3: Overly Aggressive Middleware Protection
**Description**: Middleware redirecting all routes without proper token check
**How to verify**: Even public pages redirect to login

---

## Solutions

### 🚀 Quick Fix
**Time**: < 5 minutes

Simplify middleware to only handle basic redirects:
```typescript
// src/middleware.ts
export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  
  // Only redirect root to login
  if (pathname === '/') {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  // Let client-side handle auth
  return NextResponse.next();
}
```

### 📋 Step-by-Step Solution

#### Step 1: Fix AuthContext 404 Handling
```typescript
// src/contexts/AuthContext.tsx
const checkAuth = async () => {
  try {
    const token = authService.getToken();
    
    if (!token || !authService.isAuthenticated()) {
      setUser(null);
      setLoading(false);
      return;
    }

    const currentUser = await authService.getCurrentUser(token);
    setUser(currentUser);
  } catch (error: any) {
    // Don't log 404/401 - normal when not authenticated
    if (error.response?.status === 404 || error.response?.status === 401) {
      authService.clearAuth();
    } else {
      console.error('Error al verificar autenticación:', error);
    }
    setUser(null);
  } finally {
    setLoading(false);
  }
};
```

#### Step 2: Create Client-Side Route Protection
```typescript
// src/components/ProtectedRoute.tsx
import { useAuth } from '@/contexts/AuthContext';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!loading && !isAuthenticated) {
      router.push('/login');
    }
  }, [loading, isAuthenticated, router]);

  if (loading) return <div>Loading...</div>;
  if (!isAuthenticated) return null;

  return <>{children}</>;
}
```

#### Step 3: Handle Browser Extension Warnings
```typescript
// src/app/layout.tsx
<html lang="es" suppressHydrationWarning>
  <body suppressHydrationWarning>
    {children}
  </body>
</html>
```

#### Step 4: Consistent Token Storage
```typescript
// src/services/auth.service.ts
saveAuth(authResponse: AuthResponse): void {
  localStorage.setItem('token', authResponse.token);
  localStorage.setItem('user', JSON.stringify(authResponse.user));
  localStorage.setItem('expiresAt', authResponse.expiresAt);
  
  // If you need cookies for SSR:
  // document.cookie = `token=${authResponse.token}; path=/; max-age=604800`;
}
```

### 🔧 Alternative Solutions

**For SSR Authentication**:
```typescript
// Use cookies instead of localStorage
import Cookies from 'js-cookie';

saveAuth(authResponse: AuthResponse): void {
  Cookies.set('token', authResponse.token, { expires: 7 });
  Cookies.set('user', JSON.stringify(authResponse.user), { expires: 7 });
}
```

---

## Prevention

### Best Practices
1. **Choose one storage method** (localStorage OR cookies, not both)
2. **Handle 404/401 gracefully** in auth checks
3. **Use client-side protection** for SPA behavior
4. **Keep middleware simple** for Next.js App Router
5. **Test auth flow** in incognito mode

### Recommended Auth Flow
1. User logs in → Save token to localStorage
2. AuthContext checks token on mount
3. Protected pages use `useAuth` hook
4. API calls include token via interceptor
5. 401 response → Clear auth → Redirect to login

---

## Related Issues

### See Also
- [Auth-04: Login Problems](./auth-04-login-problems.md) - Network connection issues
- [Auth-02: Token Validation](./auth-02-token-validation.md) - Token not being sent
- [API-05: Middleware Ordering](../api/api-05-middleware-ordering.md) - Middleware conflicts

### Often Occurs With
- First time implementing auth
- Switching from Pages to App Router
- Mixing SSR and CSR patterns

---

## 🏷️ Search Keywords

`middleware` `localStorage` `cookies` `404` `auth/me` `suppressHydrationWarning` `AuthContext` `protected routes` `redirect loop`

---

**Last Updated**: August 2025
**Contributors**: WebsiteBuilder Team
**Verified On**: Next.js 14 App Router, React 18