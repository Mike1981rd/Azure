# Rollback Last Task Command

Safely rollback the last completed task: $ARGUMENTS

## Rollback Process

### 1. Identify Last Completed Task
Read PROJECT-PROGRESS.md to find:
- Most recently completed task (✅)
- Associated timestamp
- Files created/modified
- Any database changes made

### 2. Analyze Changes
Determine what needs to be reverted:
- **Git changes**: Check commit history
- **File changes**: List modified/created files
- **Database changes**: Identify if migrations were applied
- **Configuration changes**: Check for setting modifications

### 3. Show Rollback Plan
Present clear summary:
```
🔄 ROLLBACK PLAN

Last Completed Task: [task name]
Completed: [timestamp]

Changes to be reverted:
📁 Files:
  - Created: [list of new files]
  - Modified: [list of changed files]
  
📊 Database:
  - Migrations: [any applied migrations]
  - Data changes: [any data modifications]
  
🔧 Configuration:
  - [any config changes]

⚠️ WARNING: This will undo the above changes.
Proceed with rollback? [Y/N]
```

### 4. Execute Rollback

#### Git Rollback (if committed):
```bash
# Show last commit
git log -1 --stat

# If confirmed, rollback
git reset --soft HEAD~1  # Keeps changes staged
# or
git reset --hard HEAD~1  # Discards changes completely
```

#### File Rollback (if not committed):
```bash
# For modified files
git checkout -- [file_path]

# For new files
rm [file_path]
```

#### Database Rollback:
```bash
# If migrations were applied
dotnet ef database update [previous_migration]

# Or restore from backup if available
psql -h [host] -U [user] -d [database] < [backup_file]
```

### 5. Update Progress Tracker
- Mark the task as ⏳ (pending) again
- Add rollback note with timestamp
- Document reason for rollback
- Update any dependent tasks

### 6. Verify Rollback
- Check git status
- Verify files are in previous state
- Confirm database state if applicable
- Run basic health checks

## Rollback Safety Rules

### Before Rollback:
1. Create safety backup if database involved
2. Stash any uncommitted work
3. Document current state
4. Check for dependent changes

### During Rollback:
1. Execute one type of rollback at a time
2. Verify each step before proceeding
3. Keep detailed log of actions
4. Stop if any errors occur

### After Rollback:
1. Verify system still functions
2. Update all documentation
3. Notify about any side effects
4. Suggest next steps

## Documentation Update
Add to PROJECT-PROGRESS.md:
```markdown
### Rollback Log
- **Date**: [timestamp]
- **Task Rolled Back**: [task name]
- **Reason**: $ARGUMENTS
- **Changes Reverted**:
  - Files: [list]
  - Commits: [list]
  - Database: [changes]
- **Current State**: [description]
- **Next Steps**: [recommendations]
```

## Error Recovery

### If Rollback Fails:
1. Stop immediately
2. Document exact error
3. Suggest manual recovery steps
4. Offer to restore from backup

### Common Issues:
- **Uncommitted changes**: Stash or commit first
- **Database in use**: Close connections
- **Missing backup**: Warn before database changes
- **Dependency conflicts**: Identify affected components

## Post-Rollback Actions
1. Suggest running tests
2. Recommend verifying key features
3. Update any external documentation
4. Plan re-implementation if needed