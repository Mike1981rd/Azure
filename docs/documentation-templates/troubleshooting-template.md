# 📋 [Category-##: Problem Title]

[← Back to Category Index](./category-00-index.md) | [← Back to Master Index](../00-troubleshooting-index.md)

## Quick Navigation
- [Problem Summary](#problem-summary)
- [Symptoms](#symptoms)
- [Root Causes](#root-causes)
- [Solutions](#solutions)
- [Prevention](#prevention)
- [Related Issues](#related-issues)

---

## Problem Summary

**[Brief description of the problem in 1-2 sentences explaining what goes wrong and why it matters.]**

**Affects**: [Component/Service names that are impacted]
**Frequency**: [Common/Uncommon/Rare - how often this occurs]
**Severity**: [High/Medium/Low - impact on functionality]
**First seen**: [Version/Date when first encountered]

---

## Symptoms

### Primary Symptoms
- [ ] [Symptom 1 - specific behavior or error user sees]
- [ ] [Symptom 2 - what stops working]
- [ ] [Symptom 3 - console/log output]
- [ ] [Symptom 4 - UI behavior]

### Error Messages
```
[Exact error message from console/logs]
[Include full stack trace if relevant]
[Multiple error examples if they vary]
```

### When It Occurs
- During [specific action - e.g., login attempt]
- After [specific event - e.g., deploying new version]
- When [specific condition - e.g., using HTTPS]
- In [specific environment - e.g., production only]

### Browser/Environment Specific
- **All browsers**: [Yes/No]
- **Specific to**: [Chrome/Firefox/Safari/Edge]
- **OS specific**: [Windows/Mac/Linux]
- **Mobile**: [Affects mobile?]

---

## Root Causes

### Cause 1: [Brief Title - e.g., "Wrong Port Configuration"]
**Description**: [Detailed explanation of why this happens]
**How to verify**: 
1. [Step to confirm - e.g., Check launchSettings.json]
2. [What to look for - e.g., Compare port in frontend config]
3. [Expected vs actual - e.g., Should be 5266 not 7224]

### Cause 2: [Brief Title - e.g., "Missing Environment Variable"]
**Description**: [Detailed explanation]
**How to verify**: 
1. [Verification step]
2. [What indicates this is the issue]

### Cause 3: [Brief Title]
**Description**: [Detailed explanation]
**How to verify**: [Steps to confirm]

---

## Solutions

### 🚀 Quick Fix
**Time**: < 5 minutes
**Success Rate**: [90%/75%/50%]

```bash
# Quick command or code change that resolves most cases
[command here]
```

**Why this works**: [Brief explanation]

### 📋 Step-by-Step Solution

#### Step 1: [Action - e.g., "Verify Current Configuration"]
```bash
# Command to run
[command with explanation]
```
Expected output:
```
[what you should see]
```

#### Step 2: [Action - e.g., "Update Configuration"]
```typescript
// File: [path/to/file]
// Change this:
[old code]

// To this:
[new code]
```

#### Step 3: [Action - e.g., "Restart Services"]
```bash
# Stop services
[stop command]

# Start services
[start command]
```

#### Step 4: [Verify Fix]
- Test by: [specific test action]
- Check that: [expected result]
- Confirm in logs: [what to look for]

### 🔧 Alternative Solutions

**If Quick Fix doesn't work**:

#### Alternative 1: [Title]
```bash
[alternative commands/code]
```
When to use: [Specific scenario]

#### Alternative 2: [Title]
1. [Step 1]
2. [Step 2]
3. [Step 3]

#### Nuclear Option: [Complete Reset]
⚠️ **Warning**: This will [consequences]
```bash
[reset commands]
```

---

## Prevention

### Best Practices
1. **Always [action]** before [event]
   - Example: Always check port configuration before first run
   - Why: Prevents connection refused errors

2. **Configure [item]** in development
   - Example: Set up .env.local with correct values
   - Template: See configuration below

3. **Test with [method]** regularly
   - Example: Test in incognito mode
   - Catches: Authentication state issues

4. **Document [what]** for team
   - Example: Document actual ports in README
   - Prevents: Configuration mismatches

5. **Use [tool/process]** to validate
   - Example: Pre-commit hooks for config
   - Benefit: Catches issues before deployment

### Configuration Template
```javascript
// Recommended [config file name]
{
  "[setting1]": "[value1]",  // [Why this value]
  "[setting2]": "[value2]",  // [Important note]
  "[setting3]": {
    "[nested]": "[value]"    // [Configuration tip]
  }
}
```

### Validation Checklist
Before deployment, verify:
- [ ] [Configuration item 1] is set correctly
- [ ] [Service A] can connect to [Service B]
- [ ] [Security setting] is enabled
- [ ] [Performance setting] is optimized
- [ ] [Logs] are accessible

---

## Related Issues

### See Also
- [[Related-01: Issue Name]](../path/to/file.md) - When you also have [symptom]
- [[Related-02: Issue Name]](../path/to/file.md) - Similar but for [different component]
- [[Related-03: Issue Name]](../path/to/file.md) - Root cause often same

### Often Occurs With
- [Problem A] - [Why they're related]
- [Problem B] - [Common trigger]
- [Problem C] - [Shared dependency]

### Prerequisites
Check these first:
- [[Prerequisite Issue]](../path/to/file.md) - Must be resolved first
- [[Configuration Guide]](../path/to/file.md) - Ensure proper setup

### Leads To
If not fixed, may cause:
- [Subsequent Problem 1]
- [Subsequent Problem 2]

---

## 🏷️ Search Keywords

`[primary-error]` `[component-name]` `[specific-term]` `[error-code]` `[symptom]` `[technology]` `[configuration-item]`

---

## 📝 Notes

### Version-Specific Notes
- **v1.0-1.5**: Use original solution
- **v2.0+**: Use alternative solution
- **v3.0+**: Issue resolved in framework

### Environment-Specific Notes
- **Development**: [Specific consideration]
- **Staging**: [Different approach needed]
- **Production**: [Critical warning]
- **Docker**: [Container-specific fix]
- **Windows**: [OS-specific step]
- **Mac/Linux**: [Different command]

### Historical Context
- First reported: [Date/Version]
- Major fix in: [Version]
- Regression in: [Version] 
- Final resolution: [Version/Date]

---

## 🔍 Debugging Deep Dive

### Enable Debug Logging
```bash
# Set environment variable
export DEBUG_LEVEL=verbose

# Or in config file
"logging": {
  "level": "debug",
  "components": ["[component]"]
}
```

### What to Look For
In logs, search for:
- `[ERROR]` followed by [pattern]
- Timestamp around [event]
- Stack trace containing [method]

### Tools for Investigation
- **[Tool 1]**: For [purpose]
  ```bash
  [tool command]
  ```
- **[Tool 2]**: To check [what]
- **Browser DevTools**: Network tab for [what to examine]

---

**Last Updated**: [YYYY-MM-DD]
**Contributors**: [Names/Teams]
**Verified On**: [Technology Stack Versions]

<!-- Internal notes - not visible in rendered markdown
TODO: Add screenshot of error
TODO: Link to video walkthrough
TODO: Add performance metrics
-->