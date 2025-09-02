# 🔧 General Troubleshooting Index

[← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [Database Issues](#database-issues)
- [Development Environment](#development-environment)
- [Build & Deployment](#build--deployment)
- [Package Management](#package-management)
- [Performance Issues](#performance-issues)

---

## 📋 Problem Files in This Category

### [General-01: Database Issues](./general-01-database-issues.md)
- Migration errors
- Multiple DbContext problems
- Connection string issues
- PostgreSQL setup
- Seed data problems

### [General-02: Development Setup](./general-02-development-setup.md)
- Node.js version conflicts
- npm install errors
- Visual Studio configuration
- Port conflicts
- Environment variables

### [General-03: Hot Reload Issues](./general-03-hot-reload-issues.md)
- Next.js not updating
- ASP.NET Core reload problems
- File watcher limits
- Cache issues
- Browser cache problems

### [General-04: Build Errors](./general-04-build-errors.md)
- TypeScript compilation errors
- C# build failures
- Missing dependencies
- Version conflicts
- Production build issues

### [General-05: Performance Problems](./general-05-performance-problems.md)
- Slow API responses
- Frontend bundle size
- Database query optimization
- Memory leaks
- Cache configuration

---

## 🔍 Quick Diagnosis

### Symptoms → Likely Problem

| Symptom | Check This First | Detailed Solution |
|---------|------------------|-------------------|
| "More than one DbContext" | Migration command | [General-01](./general-01-database-issues.md) |
| npm ERR! | Node version, cache | [General-02](./general-02-development-setup.md) |
| Changes not appearing | Hot reload, cache | [General-03](./general-03-hot-reload-issues.md) |
| Build failed | Dependencies, versions | [General-04](./general-04-build-errors.md) |
| Slow loading | Bundle size, queries | [General-05](./general-05-performance-problems.md) |

---

## 🏷️ Common Search Terms

**Database**: `Add-Migration`, `Update-Database`, `DbContext`, `PostgreSQL`
**Environment**: `npm`, `node`, `dotnet`, `Visual Studio`, `VS Code`
**Build**: `TypeScript`, `webpack`, `build error`, `compilation`
**Performance**: `slow`, `memory`, `cache`, `optimization`

---

## 💡 Prevention Tips

1. **Keep dependencies updated** but test before major upgrades
2. **Use consistent Node.js version** across team
3. **Document environment setup** for new developers
4. **Regular database backups** before migrations
5. **Monitor performance** from the start

---

## 🛠️ Common Commands Reference

### Database Commands
```powershell
# Add migration
Add-Migration MigrationName -Context ApplicationDbContext

# Update database
Update-Database -Context ApplicationDbContext

# Remove last migration
Remove-Migration -Context ApplicationDbContext
```

### NPM Commands
```bash
# Clear cache
npm cache clean --force

# Fresh install
rm -rf node_modules package-lock.json
npm install

# Check outdated
npm outdated
```

### Build Commands
```bash
# Next.js
npm run build
npm run dev

# ASP.NET Core
dotnet build
dotnet run
```

---

## 📚 Related Documentation

- [API Integration Issues](../api/api-00-index.md)
- [Authentication Setup](../auth/auth-00-index.md)
- [CLAUDE.md Project Rules](../../../CLAUDE.md)

---

**Need to add a new general problem?**
1. Create file: `general-##-descriptive-name.md`
2. Use the template structure
3. Update this index
4. Add cross-references