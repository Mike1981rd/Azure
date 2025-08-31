# Database Backup Command

Create a backup of the project database: $ARGUMENTS

## Backup Process

### 1. Detect Database Configuration
Read configuration files to determine:
- Database type (PostgreSQL, MySQL, SQL Server, etc.)
- Connection string from appsettings.json
- Parse database name, host, username
- Verify database accessibility

### 2. Generate Backup Filename
Format: `[database_name]_backup_[YYYYMMDD]_[HHMMSS].[ext]`
Example: `websitebuilder_backup_20250811_143022.sql`

### 3. Execute Backup Command

#### For PostgreSQL:
```bash
pg_dump -h [host] -U [username] -d [database] -f [backup_file]
```

#### For SQL Server:
```bash
sqlcmd -S [server] -d [database] -Q "BACKUP DATABASE [database] TO DISK='[backup_file]'"
```

#### For MySQL:
```bash
mysqldump -h [host] -u [username] -p[password] [database] > [backup_file]
```

### 4. Verify Backup
- Check if backup file was created
- Verify file size is reasonable (not empty)
- Calculate file hash for integrity
- Test backup header if possible

### 5. Document Backup
Update PROJECT-PROGRESS.md with:
- Backup timestamp
- File location and size
- Database state summary
- Any warnings or notes

## Safety Protocol

### Pre-Backup Checks
1. Ensure no active migrations pending
2. Check disk space availability
3. Verify database is accessible
4. Warn if database is large (>1GB)

### Backup Execution
```
🔄 DATABASE BACKUP PROCESS

Database: [name]
Type: [PostgreSQL/MySQL/SQLServer]
Size estimate: [size]

Creating backup...
Filename: [backup_filename]
Location: [full_path]

⚠️ This will create a database snapshot. Continue? [Y/N]
```

### Post-Backup Actions
1. Verify backup completed successfully
2. Show backup file details:
   - Full path
   - File size
   - Creation time
   - MD5/SHA checksum
3. Suggest backup storage/archival

### Migration Safety
If user wants to proceed with migrations after backup:
```
✅ Backup completed successfully
File: [backup_file] ([size])

Ready to apply database migration? [Y/N]
⚠️ Warning: This will modify your database structure
```

## Error Handling

### Common Issues:
- **Access denied**: Check database credentials
- **Disk space**: Ensure sufficient space
- **Connection failed**: Verify database is running
- **Command not found**: Install database tools

### Recovery Options:
- Provide alternative backup methods
- Suggest manual backup instructions
- Offer to create backup script for future use

## Backup Documentation Template
Add to PROJECT-PROGRESS.md:
```markdown
### Database Backup Log
- **Date**: [timestamp]
- **File**: [backup_filename]
- **Size**: [file_size]
- **Hash**: [checksum]
- **Pre-migration state**: [description]
- **Notes**: [any important notes]
```