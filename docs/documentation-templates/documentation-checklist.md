# 📋 Documentation Quality Checklist

Use this checklist to ensure documentation meets project standards before finalizing.

## 🔍 Pre-Documentation Checklist

### Before You Start
- [ ] **Implementation complete** - Don't document work in progress
- [ ] **Problems noted** - List all errors/issues encountered
- [ ] **Screenshots taken** - For UI changes or errors
- [ ] **Code committed** - Document reflects actual code
- [ ] **Time tracked** - Know how long implementation took

### Information to Gather
- [ ] Exact error messages (copy/paste)
- [ ] Configuration changes made
- [ ] Packages installed (with versions)
- [ ] Architecture decisions and rationale
- [ ] Performance metrics (if relevant)
- [ ] Security considerations
- [ ] Test results

---

## 📄 Implementation Documentation Checklist

### Required Sections
- [ ] **Overview** - Purpose clearly stated
- [ ] **Architecture Decisions** - Pattern and tech choices explained
- [ ] **Implementation Details** - Both backend and frontend covered
- [ ] **Configuration** - All env vars and settings documented
- [ ] **Testing** - Test approach and coverage noted
- [ ] **Known Issues** - Limitations honestly documented
- [ ] **Troubleshooting** - Links to related problem docs
- [ ] **References** - External resources linked

### Code Examples
- [ ] **Syntax highlighted** - Using proper markdown code blocks
- [ ] **Runnable** - Examples actually work
- [ ] **Commented** - Complex parts explained
- [ ] **Before/After** - Shows what changed
- [ ] **Real paths** - File paths are accurate

### Quality Checks
- [ ] **File size** - Under 800 lines
- [ ] **Navigation works** - All links tested
- [ ] **No sensitive data** - No passwords, keys, or PII
- [ ] **Spell checked** - No obvious typos
- [ ] **Formatted consistently** - Follows template

---

## 🔧 Troubleshooting Documentation Checklist

### Required Sections
- [ ] **Problem Summary** - Clear one-liner description
- [ ] **Symptoms** - Checklist format with exact errors
- [ ] **Root Causes** - All causes with verification steps
- [ ] **Solutions** - Quick fix + detailed steps
- [ ] **Prevention** - Best practices to avoid issue
- [ ] **Related Issues** - Cross-references added

### Solution Quality
- [ ] **Quick Fix** - Under 5 minutes, high success rate
- [ ] **Step-by-step** - Each step clearly numbered
- [ ] **Alternative solutions** - For when quick fix fails
- [ ] **Verification steps** - How to confirm fix worked
- [ ] **Time estimates** - Realistic for each solution

### Error Documentation
- [ ] **Exact error text** - Copy/pasted, not paraphrased
- [ ] **Stack traces** - Included when relevant
- [ ] **Screenshots** - For UI issues
- [ ] **Environment details** - Browser, OS, versions
- [ ] **Reproduction steps** - How to trigger the error

---

## 🔗 Cross-Reference Checklist

### Index Updates
- [ ] **Master index** - Entry added with description
- [ ] **Category index** - Listed in correct section
- [ ] **Related docs** - "See Also" sections updated
- [ ] **Search keywords** - Relevant terms included

### Link Validation
- [ ] **Internal links** - All work and use relative paths
- [ ] **External links** - All return 200 OK
- [ ] **Breadcrumbs** - Navigation path correct
- [ ] **Cross-references** - Bidirectional where needed

---

## 📊 Documentation Metrics

### Completeness Score
Rate each section 0-10:
- [ ] Accuracy: ___/10
- [ ] Clarity: ___/10
- [ ] Completeness: ___/10
- [ ] Usefulness: ___/10
- [ ] Maintainability: ___/10

### Time Investment
- [ ] Documentation time: ___ minutes
- [ ] Review time: ___ minutes
- [ ] Update frequency expected: ___________

---

## 🚀 Quick Quality Checks

### The 5-Minute Test
Can a new developer:
- [ ] Understand the problem/feature in 1 minute?
- [ ] Find the solution/implementation in 2 minutes?
- [ ] Start implementing/fixing in 5 minutes?

### The Search Test
Search for the primary keyword:
- [ ] Does this doc appear in results?
- [ ] Is it the most relevant result?
- [ ] Are search keywords comprehensive?

### The Freshness Test
- [ ] Versions mentioned are current?
- [ ] Code examples match current codebase?
- [ ] Links still work?
- [ ] Solutions still apply?

---

## ✅ Final Approval

### Sign-off Checklist
- [ ] **Technical accuracy** - Implementation/fix works
- [ ] **Documentation standards** - Follows all templates
- [ ] **File organization** - In correct directory
- [ ] **Naming convention** - Follows standards
- [ ] **PROJECT-PROGRESS.md** - Updated with doc references

### Ready to Publish When
- [ ] All sections complete (no TODOs)
- [ ] Peer reviewed (if applicable)
- [ ] Tested on fresh environment
- [ ] Metrics tracked in progress doc

---

## 📝 Common Pitfalls to Avoid

### Don't Do This
- ❌ Document before implementation is stable
- ❌ Use absolute paths in examples
- ❌ Include environment-specific values
- ❌ Leave placeholder text
- ❌ Forget to update indices
- ❌ Use outdated screenshots
- ❌ Copy-paste without adapting
- ❌ Skip the verification steps

### Always Do This
- ✅ Test every code example
- ✅ Include actual error messages
- ✅ Explain the "why" not just "how"
- ✅ Consider multiple scenarios
- ✅ Update when codebase changes
- ✅ Think about future readers
- ✅ Be honest about limitations
- ✅ Provide escape hatches

---

## 🎯 Documentation Goals

Remember, good documentation should:
1. **Save time** - Faster than figuring it out alone
2. **Prevent errors** - Stop mistakes before they happen
3. **Share knowledge** - Spread expertise across team
4. **Build confidence** - Developer knows they're on right track
5. **Enable independence** - Self-service problem solving

---

**Template Version**: 1.0
**Last Updated**: 2025-08
**Applies To**: All project documentation