# Project Status Check Command

Generate comprehensive project status report: $ARGUMENTS

## Status Report Generation

### 1. Progress Calculation
Read PROJECT-PROGRESS.md and calculate:
- Total tasks across all phases
- Completed tasks (✅)
- In-progress tasks (🔄)
- Pending tasks (⏳)
- Blocked tasks (❌)
- Overall completion percentage

### 2. Phase Analysis
For each development phase:
- Phase name and target completion
- Tasks completed vs total
- Percentage complete
- Estimated time remaining

### 3. Recent Activity
Show last 5 completed tasks:
- Task name
- Completion timestamp
- Files affected
- Any notes or issues

### 4. Current Focus
- Active tasks (if any)
- Next 3-5 upcoming tasks
- Dependencies between tasks
- Critical path items

### 5. Pending Operations
Check for:
- Uncommitted git changes (run git status)
- Unapplied database migrations
- Unpushed commits
- Files awaiting creation

### 6. Blockers & Issues
Identify:
- Tasks marked as blocked
- Missing dependencies
- Configuration issues
- Unresolved TODOs

## Output Format

```
📊 PROJECT STATUS REPORT
Generated: [timestamp]

🎯 Overall Progress: [X]% Complete
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[Progress bar visualization]

📈 Phase Breakdown:
┌─────────────────────────────────┬──────────┬────────────┐
│ Phase                           │ Progress │ Status     │
├─────────────────────────────────┼──────────┼────────────┤
│ 1. Setup & Structure           │ 85%      │ Active     │
│ 2. Core Modules                │ 0%       │ Pending    │
│ 3. Website Builder             │ 0%       │ Pending    │
└─────────────────────────────────┴──────────┴────────────┘

✅ Recently Completed (Last 5):
• [Task name] - [time ago]
• [Task name] - [time ago]

🔄 Currently Active:
• [Task name] - Started [time]

📋 Upcoming Tasks:
1. [Next task] - [Dependencies]
2. [Following task]
3. [Another task]

⚠️ Pending Actions Required:
• [X] files with uncommitted changes
• [Y] commits not pushed to remote
• Database migration pending

🚧 Blockers:
• [Any blocked items]

💡 Recommended Next Action:
[Specific recommendation based on current state]
```

## Additional Checks

### Git Repository Status
```bash
git status --short
git log --oneline -5
git branch --show-current
```

### File System Checks
- Verify critical files exist
- Check for TODO comments
- Look for incomplete implementations

### Configuration Verification
- Database connection string present
- JWT configuration set
- CORS properly configured

## Smart Recommendations
Based on status, suggest:
- If many uncommitted changes: "Consider committing completed work"
- If phase nearly complete: "Finish remaining tasks in current phase"
- If blocked: "Resolve blockers before continuing"
- If all clear: "Ready to continue with: [specific task]"