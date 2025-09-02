# 🌐 API Integration Troubleshooting Index

[← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [CORS Issues](#cors-issues)
- [Connection Problems](#connection-problems)
- [Request/Response Errors](#requestresponse-errors)
- [Port Configuration](#port-configuration)
- [Middleware Conflicts](#middleware-conflicts)

---

## 📋 Problem Files in This Category

### [API-01: CORS Problems](./api-01-cors-problems.md)
- Access-Control-Allow-Origin errors
- Preflight request failures
- Credentials not included
- Wildcard origin issues
- Headers not allowed

### [API-02: ASP.NET + Next.js Integration](./api-02-aspnet-nextjs-integration.md)
- Port mismatch errors
- HTTPS vs HTTP issues
- Environment variable configuration
- API URL not found
- Connection refused errors

### [API-03: Request Configuration](./api-03-request-configuration.md)
- Axios configuration issues
- Missing headers
- Content-Type problems
- Authorization header setup
- Request interceptors

### [API-04: Response Handling](./api-04-response-handling.md)
- 404 Not Found errors
- 500 Internal Server errors
- JSON parsing errors
- Response interceptors
- Error handling patterns

### [API-05: Middleware Ordering](./api-05-middleware-ordering.md)
- Middleware execution order
- Authentication before CORS
- Multi-tenant middleware issues
- Custom middleware conflicts
- Request pipeline problems

---

## 🔍 Quick Diagnosis

### Symptoms → Likely Problem

| Symptom | Check This First | Detailed Solution |
|---------|------------------|-------------------|
| "ERR_CONNECTION_REFUSED" | Is API running? Port? | [API-02](./api-02-aspnet-nextjs-integration.md) |
| "CORS policy" error | CORS configuration | [API-01](./api-01-cors-problems.md) |
| "404 Not Found" on valid endpoint | API route, middleware | [API-04](./api-04-response-handling.md) |
| Headers missing in request | Axios configuration | [API-03](./api-03-request-configuration.md) |
| Middleware not executing | Pipeline order | [API-05](./api-05-middleware-ordering.md) |

---

## 🏷️ Common Search Terms

**Configuration**: `AddCors`, `UseCors`, `axios.create`, `baseURL`
**Errors**: `CORS`, `404`, `500`, `ERR_CONNECTION_REFUSED`, `ERR_NETWORK`
**Middleware**: `UseAuthentication`, `UseAuthorization`, `UseMultiTenant`
**Ports**: `5266`, `7224`, `3000`, `launchSettings.json`

---

## 💡 Prevention Tips

1. **Always check launchSettings.json** for correct ports
2. **Configure CORS before authentication** in middleware pipeline
3. **Use .env.local** for API URLs in Next.js
4. **Test endpoints in Swagger** before frontend integration
5. **Check both HTTP and HTTPS** when debugging

---

## 🛠️ Common Fixes Checklist

### For Connection Issues:
- [ ] API is running in Visual Studio
- [ ] Correct port in frontend config
- [ ] No firewall blocking
- [ ] HTTP vs HTTPS match

### For CORS Issues:
- [ ] Origins include frontend URL
- [ ] Credentials allowed if needed
- [ ] Headers properly configured
- [ ] Preflight handling enabled

### For 404 Errors:
- [ ] Route exists in controller
- [ ] HTTP method matches
- [ ] No typos in URL
- [ ] Middleware not blocking

---

## 📚 Related Documentation

- [Authentication Issues](../auth/auth-00-index.md)
- [General Environment Setup](../general/general-00-index.md)
- [JWT Token Issues](../auth/auth-01-jwt-setup-issues.md)

---

**Need to add a new API problem?**
1. Create file: `api-##-descriptive-name.md`
2. Use the template structure
3. Update this index
4. Add cross-references