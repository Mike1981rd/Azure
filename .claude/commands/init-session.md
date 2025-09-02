# Initialize Project Session Command

Load all project context and provide comprehensive session summary: $ARGUMENTS

## Load Context Files
First, let me load all the essential project files in this specific order:

1. Read CLAUDE.md for workflow rules and project constraints
2. Read CLAUDEBK1.md for base rules and architecture (lines 1-501)
3. Read CLAUDEBK2.md for UI patterns and troubleshooting (lines 502-895)
4. Read blueprint1.md for project overview and problems to solve
5. Read blueprint2.md for system architecture details
6. Read blueprint3.md for implementation details and modules
7. Read PROJECT-PROGRESS.md for current status
8. Check for any other relevant context files (SistemaDisponibilidad.md, etc.)

## Session Summary Generation

### 1. Project Overview
- Extract project name, tech stack, and main objectives
- Identify current development phase
- Show project repository and key dates

### 2. Progress Analysis
- Count total tasks across all phases
- Calculate completed tasks percentage
- Break down progress by phase/module
- Identify critical path items

### 3. Recent Activity
- Show last 3-5 completed tasks with timestamps
- Display current in-progress tasks
- List any blockers or issues noted

### 4. Next Steps
- Identify next logical task based on dependencies
- Check for any prerequisite tasks
- Recommend immediate action items

### 5. Pending Approvals
- Check git status for uncommitted changes
- Look for pending database migrations
- Identify any awaiting permissions

### 6. Environment Status
- Verify database connection details
- Check API configuration
- Confirm frontend setup status

## Output Format
Present findings in this structure:
```
🚀 PROJECT SESSION INITIALIZED

📊 Project: [Name]
Tech Stack: [Stack details]
Phase: [Current phase] ([X]% complete)

✅ Recent Completions:
- [Task 1] (timestamp)
- [Task 2] (timestamp)

🔄 Currently Active:
- [In-progress task]

📋 Next Recommended Task:
[Task details with rationale]

⚠️ Pending Actions:
- [Any uncommitted changes]
- [Any pending migrations]

🔗 Key Files Loaded:
- CLAUDE.md ✓
- blueprintX.md ✓
- PROJECT-PROGRESS.md ✓

Context loaded. Ready to continue project work.
```

## Error Handling
- If files are missing, note which ones and suggest recovery
- If progress tracker is corrupted, offer to rebuild from git history
- Handle file reading errors gracefully with clear messages