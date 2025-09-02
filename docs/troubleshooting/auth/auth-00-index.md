# 🔐 Authentication Troubleshooting Index

[← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [JWT Issues](#jwt-issues)
- [Login/Logout Problems](#loginlogout-problems)
- [Token Management](#token-management)
- [Next.js Integration](#nextjs-integration)
- [Role & Permissions](#role--permissions)

---

## 📋 Problem Files in This Category

### [Auth-01: JWT Setup Issues](./auth-01-jwt-setup-issues.md)
- JWT configuration in Program.cs
- Token generation problems
- Secret key issues
- Token expiration configuration
- Missing claims in token

### [Auth-02: Token Validation](./auth-02-token-validation.md)
- 401 Unauthorized errors
- Token not being sent with requests
- Invalid token format
- Expired token handling
- Token refresh implementation

### [Auth-03: Next.js Integration](./auth-03-nextjs-integration.md)
- AuthContext setup issues
- Middleware vs client-side auth
- localStorage vs cookies debate
- Protected route implementation
- Session persistence problems

### [Auth-04: Login Problems](./auth-04-login-problems.md)
- Login endpoint returning errors
- Password hashing issues
- User not found errors
- CORS blocking login requests
- Network errors during login

### [Auth-05: DTO Naming Conflicts](./auth-05-dto-naming-conflicts.md)
- Swagger schema ID conflicts
- Multiple DTOs with same class name
- Internal server error in Swagger
- Namespace collision issues
- Swashbuckle configuration

### [Auth-06: Permissions Not Showing](./auth-06-permissions-not-showing.md)
- Sidebar appears empty after role assignment
- Permission name mismatch (.view vs .read)
- JWT token contains permissions but UI doesn't show
- Menu items not filtering correctly
- Token not refreshed after permission change

### [Auth-07: Role Update Fails](./auth-07-role-update-fails.md)
- Role changes not persisting to database
- Frontend/Backend DTO field mismatch
- permissions vs permissionIds confusion
- Silent failures with 200 response
- UpdateRoleDto validation issues

---

## 🔍 Quick Diagnosis

### Symptoms → Likely Problem

| Symptom | Check This First | Detailed Solution |
|---------|------------------|-------------------|
| "401 Unauthorized" | Token in request headers | [Auth-02](./auth-02-token-validation.md) |
| "Network error" on login | API running? Correct port? | [Auth-04](./auth-04-login-problems.md) |
| Logged out after refresh | Token storage method | [Auth-03](./auth-03-nextjs-integration.md) |
| Swagger fails to load | DTO naming conflicts | [Auth-05](./auth-05-dto-naming-conflicts.md) |
| JWT errors in console | JWT configuration | [Auth-01](./auth-01-jwt-setup-issues.md) |
| Empty sidebar menu | Permission naming mismatch | [Auth-06](./auth-06-permissions-not-showing.md) |
| Role changes not saving | DTO field mismatch | [Auth-07](./auth-07-role-update-fails.md) |

---

## 🏷️ Common Search Terms

**Configuration**: `AddAuthentication`, `AddJwtBearer`, `TokenValidationParameters`
**Services**: `IAuthService`, `AuthController`, `AuthContext`
**Errors**: `401`, `403`, `InvalidToken`, `TokenExpired`
**Next.js**: `useAuth`, `AuthProvider`, `middleware.ts`

---

## 💡 Prevention Tips

1. **Always check API is running** before testing auth
2. **Use consistent token storage** (localStorage OR cookies, not both)
3. **Configure CORS properly** for your frontend URL
4. **Test with Swagger first** to isolate frontend issues
5. **Check browser DevTools** Network tab for actual requests

---

## 📚 Related Documentation

- [API Integration Issues](../api/api-00-index.md)
- [CORS Problems](../api/api-01-cors-problems.md)
- [Database User/Role Setup](../general/general-01-database-issues.md)

---

**Need to add a new auth problem?**
1. Create file: `auth-##-descriptive-name.md`
2. Use the template structure
3. Update this index
4. Add cross-references