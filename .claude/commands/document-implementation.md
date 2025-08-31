# /document-implementation Command

## Purpose
Automatically create comprehensive implementation and troubleshooting documentation following project standards.

## Usage
```
/document-implementation [feature-name]
```

## Examples
- `/document-implementation login` - Documents login/auth implementation
- `/document-implementation multi-tenant` - Documents multi-tenancy implementation
- `/document-implementation product-variants` - Documents product variants feature

## Workflow

### 1. Initial Questions
When invoked, ask the user:
- **Implementation Type**: auth / api / feature / infrastructure?
- **Problems Encountered**: Were there troubleshooting issues? (Y/N)
- **Time Spent**: How long did implementation take?
- **Key Decisions**: What architecture decisions were made?

### 2. File Creation

#### Implementation Documentation
Create in `/docs/implementations/[category]/YYYY-MM-feature-name.md`:
- Use implementation template structure
- Include all code snippets and configuration
- Document architecture decisions
- Link to related troubleshooting docs

#### Troubleshooting Documentation (if problems occurred)
For each problem, create in `/docs/troubleshooting/[category]/`:
- Follow troubleshooting template
- Use sequential numbering (e.g., auth-05, auth-06)
- Include exact error messages
- Document root causes and solutions

### 3. Index Updates

#### Update Master Indices
- `/docs/implementations/00-implementations-index.md`
- `/docs/troubleshooting/00-troubleshooting-index.md`

#### Update Category Indices
- `/docs/implementations/[category]/[category]-00-index.md`
- `/docs/troubleshooting/[category]/[category]-00-index.md`

### 4. PROJECT-PROGRESS.md Update
Add documentation references:
```markdown
### Documentation
- ✅ Login Implementation: `/docs/implementations/auth/2025-08-login-implementation.md`
- ✅ Login Troubleshooting: 
  - Network errors: `/docs/troubleshooting/auth/auth-04-login-problems.md`
  - Next.js integration: `/docs/troubleshooting/auth/auth-03-nextjs-integration.md`
```

## Implementation Categories

### auth/
- Login/logout
- JWT tokens
- Role-based access
- Password reset
- Session management

### api/
- REST endpoints
- GraphQL resolvers
- WebSocket connections
- Third-party integrations
- Rate limiting

### features/
- Business logic implementations
- Domain-specific features
- Complex workflows
- Data processing

### infrastructure/
- Database setup
- Docker configuration
- CI/CD pipelines
- Monitoring setup
- Performance optimization

## Automation Rules

### File Naming
- Implementation: `YYYY-MM-feature-name.md`
- Troubleshooting: `category-##-descriptive-name.md`

### Cross-References
- Always link implementation → troubleshooting
- Always link troubleshooting → implementation
- Update "See Also" sections

### Code Examples
- Include actual code from implementation
- Show before/after for fixes
- Include configuration snippets

### Quality Checks
- Verify file < 800 lines
- Check all sections completed
- Ensure navigation links work
- Confirm search keywords added

## Template Selection

### For Implementation Docs
Use `/docs/documentation-templates/implementation-template.md`

### For Troubleshooting Docs
Use `/docs/documentation-templates/troubleshooting-template.md`

### For Quick Reference
Check `/docs/documentation-templates/documentation-checklist.md`

## Error Handling
If user cancels or provides incomplete info:
- Save partial documentation
- Mark as DRAFT in file
- Add TODO comments for missing sections
- Note in PROJECT-PROGRESS.md