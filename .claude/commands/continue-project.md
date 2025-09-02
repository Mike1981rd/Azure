# Continue Project Development Command

Continue with the next logical task from PROJECT-PROGRESS.md: $ARGUMENTS

## Workflow Steps

### 1. Load Current Status
- Read PROJECT-PROGRESS.md to understand current position
- Read relevant blueprint files for task requirements
- Check CLAUDE.md for workflow rules
- Verify no tasks are currently marked as "In Progress"

### 2. Task Selection
- Find the next pending (⏳) task in logical order
- Consider task dependencies and prerequisites
- Prefer tasks in current phase before moving to next phase
- If arguments specify a particular area, prioritize those tasks

### 3. Pre-Task Updates
- Update PROJECT-PROGRESS.md marking selected task as "🔄 In Progress"
- Document task start time
- Note any specific requirements or constraints

### 4. Task Execution
Follow these rules strictly:
- Work on ONLY ONE task at a time
- Follow blueprint specifications exactly
- Implement according to CLAUDE.md patterns
- Create/modify files as needed
- Follow naming conventions and file size limits

### 5. Safety Checks
Before any critical operations:
- Git commits: Ask "Ready to commit these changes to GitHub? [Y/N]"
- Database migrations: Follow workflow:
  1. Prepare migration with descriptive name
  2. Show EXACT migration name generated
  3. Provide command: `Update-Database -Migration [EXACT_NAME]`
  4. Ask: "Migration [EXACT_NAME] prepared. Execute in Visual Studio: `Update-Database -Migration [EXACT_NAME]`. Confirm when completed? [Y/N]"
  5. Wait for confirmation before continuing
- System changes: Ask for explicit permission

### 6. Task Completion
- Update PROJECT-PROGRESS.md marking task as "✅ Completed"
- Document what was accomplished:
  - Files created/modified
  - Key decisions made
  - Any issues encountered
  - For database tasks: Include exact migration name and status
- Note any follow-up tasks discovered
- If migration was involved:
  ```markdown
  - ✅ [Task Name]
    - Migration: [EXACT_MIGRATION_NAME] 
    - Status: Applied successfully
    - Command used: Update-Database -Migration [EXACT_MIGRATION_NAME]
  ```

### 7. Stop Point
After completing ONE task:
- Save all work
- Update progress tracker
- Output: "TASK COMPLETED - Ready for next instruction"
- DO NOT continue to next task

## Task Execution Guidelines

### For Controllers/Services/Repositories:
1. Create interface first
2. Implement with proper dependency injection
3. Follow async/await patterns
4. Include error handling
5. Add XML documentation comments

### For Frontend Components:
1. Use TypeScript with strict typing
2. Follow existing component patterns
3. Include proper props interfaces
4. Make components reusable

### For Database Changes:
1. Update models as needed
2. Modify DbContext if required
3. Prepare migration files using:
   ```bash
   dotnet ef migrations add [DescriptiveName]
   ```
4. Show migration summary with tables/columns affected
5. Provide EXACT migration name and command:
   ```
   Update-Database -Migration [EXACT_MIGRATION_NAME]
   ```
6. Ask for confirmation:
   ```
   Migration [EXACT_MIGRATION_NAME] prepared. 
   Execute in Visual Studio: `Update-Database -Migration [EXACT_MIGRATION_NAME]`
   Confirm when completed? [Y/N]
   ```
7. Wait for human confirmation before proceeding
8. Update PROJECT-PROGRESS.md with migration details

## Error Recovery
If task fails:
- Document the error in progress tracker
- Suggest solutions
- Mark task as blocked if unresolvable
- Recommend alternative tasks