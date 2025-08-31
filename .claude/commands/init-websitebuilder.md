# Initialize Website Builder Session Command

Resume Website Builder implementation by loading context and checking progress: $ARGUMENTS

## 📊 Load Progress and Context

### Step 1: Load Core Documentation
First, load all essential Website Builder documentation in this specific order:

1. Read `websitebuilderprogress.md` to check current implementation status
2. Read `blueprintwebsite.md` for architecture and technical details
3. Read `CLAUDEBK1.md` for project base rules and patterns
4. Read `CLAUDEBK2.md` for UI patterns and troubleshooting
5. Read `PROJECT-PROGRESS.md` to understand overall project context
6. Read `Test Images/prompt.pdf` for theme configuration specifications
7. Check for additional specs in `Test Images/` (header.pdf, footer.pdf, etc.)
8. Check for any related implementation docs in `/docs/implementations/websitebuilder/`

### Step 2: Analyze Current Progress

Parse the progress document to identify:
- Current phase and percentage complete
- Completed tasks (✅)
- In-progress tasks (🔄)
- Blocked items (🔴)
- Next logical tasks to implement

### Step 3: Check Implementation Status

#### TypeScript Types (Phase 1)
Check if theme configuration types exist:
```bash
ls websitebuilder-admin/src/types/theme/
```
If exists, verify which files are implemented:
- appearance.ts
- typography.ts
- colorSchemes.ts
- productCards.ts
- productBadges.ts
- cart.ts
- favicon.ts
- navigation.ts
- socialMedia.ts
- swatches.ts
- index.ts

#### Backend Models (Phase 2)
Check for Website Builder models:
```bash
grep -l "ThemeConfiguration\|WebsitePage\|PageSection" Models/*.cs
```

#### Database Migrations (Phase 2)
Check for Website Builder migrations:
```bash
ls Migrations/ | grep -i "website\|theme\|section"
```

#### APIs (Phase 3)
Check for Website Builder controllers:
```bash
ls Controllers/ | grep -i "theme\|website\|section\|preview"
```

#### Frontend Components (Phase 4-7)
Check for editor components:
```bash
ls websitebuilder-admin/src/components/websiteBuilder/
```

### Step 4: Verify Environment

1. **Check for uncommitted changes**:
```bash
git status --short | grep -E "theme|website|section"
```

2. **Check current branch**:
```bash
git branch --show-current
```

3. **Database connection**:
Verify PostgreSQL is accessible and has Website Builder tables

### Step 5: Verify Critical System Context

#### Critical Reminders to Display:
1. **Configuration Hierarchy**:
   - Global configs are BASE/DEFAULT values
   - Sections INHERIT from global
   - Overrides are OPTIONAL (not common - max 3 colors per page)
   - One company = One configuration (no theme switching)

2. **System Separation**:
   - Website Builder is for PRODUCTS/E-COMMERCE only
   - Rooms/Reservations are SEPARATE modules (already implemented)
   - RoomReservationCart ≠ ProductShoppingCart (keep separated)

3. **Technical Constraints**:
   - Max 300 lines per file
   - Use JSONB in PostgreSQL for configs
   - Use Zustand for state management (already in project)
   - NEVER run frontend/backend from WSL

4. **Already Implemented Modules**:
   - ✅ Customers, Collections, Products, Orders
   - ✅ Rooms, Reservations, Availability System
   - ✅ Payment Gateway, Newsletter, Policies
   - ⏳ Website Builder (current focus)

### Step 6: Generate Session Summary

## 📋 Output Format

Present findings in this structure:

```
🚀 WEBSITE BUILDER SESSION INITIALIZED

📊 Overall Progress: [X]% Complete
Current Phase: [Phase Name] ([Y]% complete)
Total Tasks: [Completed]/[Total]

✅ Recently Completed:
- [Last 3-5 completed tasks with timestamps if available]

🔄 Currently In Progress:
- [Active task from progress tracker]
- Files modified: [List any WIP files]

📋 Next Recommended Tasks:
1. [Most logical next task based on dependencies]
2. [Second priority task]
3. [Third priority task]

⚠️ Attention Required:
- [Any blockers or issues]
- [Missing dependencies]
- [Decisions pending]

📁 Implementation Status:
├── TypeScript Types: [X/11] files created
├── Backend Models: [X/3] implemented
├── Database: [Tables created: Yes/No]
├── APIs: [X/4] controllers ready
├── Frontend Editor: [X/10] components built
└── Sections: [X/11] implemented

🎯 Today's Focus:
Based on the current state, you should focus on:
[Specific recommendation with rationale]

💡 Quick Commands:
- Continue with: [Suggested command or file to edit]
- Test current work: [Test command if applicable]
- Check types: npm run type-check
- Run migrations: [Migration command if needed]

📚 Context Loaded:
- websitebuilderprogress.md ✓
- blueprintwebsite.md ✓
- CLAUDEBK1.md & CLAUDEBK2.md ✓
- PROJECT-PROGRESS.md ✓
- Theme specs (prompt.pdf) ✓
- Current branch: [branch]
- Uncommitted changes: [Yes/No]

⚠️ CRITICAL REMINDERS:
- Configuration Inheritance: Global → Section → Override (optional)
- One Company = One Config (no theme switching)
- Products/E-commerce ONLY (Rooms are separate)
- Max 300 lines per file
- Use JSONB for storage, Zustand for state
- NEVER execute from WSL

Ready to continue Website Builder implementation.
Type 'show next task details' for detailed instructions on the next task.
```

## 🔄 Auto-Actions

After loading context, automatically:

1. **Update TodoWrite** with next 3-5 tasks from the progress tracker
2. **Set working directory** to appropriate location based on current phase
3. **Open relevant files** if currently editing (based on git status)
4. **Suggest specific commands** to continue work

## 📝 Task Detail Command

If user asks for "show next task details", provide:

```
📋 NEXT TASK DETAILS

Task: [Task name from progress tracker]
Phase: [Phase number and name]
Estimated Time: [From progress doc]

Files to Create/Modify:
- [File 1 with full path]
- [File 2 with full path]

Implementation Steps:
1. [Specific step with code example if applicable]
2. [Next step]
3. [Continue...]

Validation Checklist:
- [ ] File under 300 lines
- [ ] TypeScript types correct
- [ ] Follows established patterns
- [ ] Includes default values

Reference:
- See blueprintwebsite.md section [X.X]
- Original spec: prompt.pdf page [N]

Ready to implement? (Y/n)
```

## 🚨 Error Handling

If files are missing or corrupted:
1. Alert which files are missing
2. Offer to recreate from blueprint
3. Check git history for lost work
4. Suggest recovery steps

If progress tracker is invalid:
1. Attempt to reconstruct from git history
2. Scan implementation to determine actual progress
3. Offer to rebuild progress tracker

## 🎯 Smart Recommendations

Based on the current state, provide intelligent suggestions:

### If Phase 1 (Types) not started:
"Let's begin with creating the TypeScript types structure. This is the foundation for everything else."

### If Phase 1 complete, Phase 2 not started:
"Types are ready! Time to create the backend models and database structure."

### If working on Frontend:
"Remember to test in both editor and preview modes as you implement."

### If near phase completion:
"You're 90% done with [Phase]. Let's finish the remaining [X] tasks before moving on."

## 📊 Progress Tracking

After session initialization, always:
1. Update the `websitebuilderprogress.md` with current timestamp
2. Mark any completed tasks discovered during scan
3. Note any new blockers or issues found
4. Commit progress if requested

## 🔧 Utility Functions

### Check specific phase completion:
```bash
grep -A 50 "FASE [N]:" websitebuilderprogress.md | grep -c "✅"
```

### Find TODO comments in code:
```bash
grep -r "TODO.*[Ww]ebsite\|TODO.*[Bb]uilder" --include="*.ts" --include="*.tsx" --include="*.cs"
```

### List recent Website Builder commits:
```bash
git log --oneline --grep="website\|builder\|theme\|section" -10
```

## 💾 Session State

Remember to maintain session context:
- Current working phase
- Active task being implemented
- Any decisions made during session
- Files modified but not committed

This ensures continuity even if the session is interrupted.

## ⚠️ CRITICAL VALIDATION CHECKLIST

Before starting any Website Builder implementation, ALWAYS verify:

### Architecture Understanding:
- [ ] I understand Global Config → Section → Override hierarchy
- [ ] I know this is for PRODUCTS only (not rooms)
- [ ] I understand one company = one config (no themes)
- [ ] I know configs are stored as JSONB in PostgreSQL

### Technical Requirements:
- [ ] Files will be < 300 lines
- [ ] Will use TypeScript strict mode
- [ ] Will use Zustand for state (not Redux)
- [ ] Will NOT execute from WSL

### System Context:
- [ ] I know what modules are already implemented
- [ ] I understand the 9 problems we're solving from v1
- [ ] I've read the separation between rooms/products
- [ ] I know about RoomReservationCart vs ProductShoppingCart

### Implementation Approach:
- [ ] Start with TypeScript types (Phase 1)
- [ ] Then backend models (Phase 2)
- [ ] Then APIs (Phase 3)
- [ ] Finally UI components (Phase 4+)

If ANY checkbox is unclear, re-read the relevant documentation before proceeding.