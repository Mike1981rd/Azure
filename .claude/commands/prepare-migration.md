# Prepare Entity Framework Migration Command

Analyze model changes and prepare migration files: $ARGUMENTS

## Migration Preparation Process

### 1. Analyze Current State
- Check for existing migrations in /Migrations folder
- Identify last applied migration (if any)
- Compare current models against database schema
- Detect all changes requiring migration

### 2. Generate Migration Name
Based on changes detected, create descriptive name:
- For initial: `InitialCreate`
- For new entities: `Add[EntityName]`
- For modifications: `Update[EntityName][Change]`
- For multiple changes: `Add[Entity1]And[Entity2]`
- Include date if needed: `AddProducts_20250811`

### 3. Create Migration Files
**IMPORTANT**: Only prepare files, do NOT execute database update

Execute command to generate migration:
```bash
dotnet ef migrations add [MigrationName]
```

This will create:
- `[Timestamp]_[MigrationName].cs` - Up/Down methods
- `[Timestamp]_[MigrationName].Designer.cs` - Model snapshot
- Update `ApplicationDbContextModelSnapshot.cs`

### 4. Analyze Generated Migration
Read and display the generated Up() method to show:
- Tables to be created
- Columns to be added/modified
- Indexes to be created
- Foreign keys to be established
- Any data migrations

### 5. Safety Validation
Check for potentially dangerous operations:
- Dropping tables or columns
- Renaming operations that might lose data
- Type changes that could truncate data
- Required columns without defaults

## Required Output Format

```
📋 MIGRATION PREPARED

Migration Name: [ExactMigrationName]
Generated Files:
- Migrations/[Timestamp]_[MigrationName].cs
- Migrations/[Timestamp]_[MigrationName].Designer.cs

🔄 Changes to be Applied:
✅ Tables to Create:
   - [TableName] ([X] columns)
   
📝 Columns to Add:
   - [Table].[Column] ([Type])
   
🔗 Foreign Keys:
   - [FK_Name]: [Table1] -> [Table2]
   
📊 Indexes:
   - [IX_Name] on [Table]([Columns])

⚠️ Warnings:
   - [Any potential issues]

📌 TO APPLY THIS MIGRATION:

Option 1 - Visual Studio Package Manager Console:
```
Update-Database
```

Option 2 - Command Line:
```
dotnet ef database update
```

Option 3 - Specific Migration:
```
Update-Database -Migration [ExactMigrationName]
```

❗ IMPORTANT: Migration files created but NOT applied to database.
Execute the commands above in Visual Studio or terminal to apply changes.

Confirm when migration has been executed? [Y/N]
```

## Error Handling

### Common Issues:
- **No DbContext found**: Ensure ApplicationDbContext exists
- **Build errors**: Project must compile before migration
- **No changes detected**: Models haven't changed since last migration
- **Connection issues**: Can't connect to database for comparison

### Recovery Actions:
- If migration fails, suggest removing it with:
  ```
  dotnet ef migrations remove
  ```
- Check for compilation errors
- Verify connection string
- Review model changes

## Documentation Update
After human confirms execution, update PROJECT-PROGRESS.md:
```markdown
### Database Migrations Log
- **Date**: [timestamp]
- **Migration**: [MigrationName]
- **Status**: Prepared ✓ | Applied ✓
- **Changes**: [summary of changes]
- **Tables Affected**: [list]
- **Warnings**: [any issues]
```

## Important Notes
- NEVER run `dotnet ef database update` automatically
- Always wait for human confirmation
- If in WSL/Linux environment without dotnet, provide files content for manual creation
- Keep migration names clear and descriptive
- Document all changes thoroughly

## Integration with PROJECT-PROGRESS.md

### Before Migration:
Update progress tracker to show migration preparation:
```markdown
#### 🗄️ Database Migrations
- 🔄 Preparing migration: [MigrationName]
  - Status: Files generated, awaiting execution
  - Command: `Update-Database -Migration [MigrationName]`
```

### After Human Confirmation:
Update to show completion:
```markdown
#### 🗄️ Database Migrations
- ✅ [MigrationName] - [timestamp]
  - Status: Applied successfully
  - Changes: [X] tables created/modified
  - Command used: `Update-Database -Migration [MigrationName]`
```

### Safety Compliance
This command follows all CLAUDE.md rules:
- ✅ Prepares migrations only (no execution)
- ✅ Requires explicit human action
- ✅ Provides exact commands
- ✅ Documents in progress tracker
- ✅ Waits for confirmation