#!/bin/bash

# Script to clean repository from compiled files before pushing to Railway
# Run this before committing to ensure clean deployment

echo "🧹 Cleaning repository for Railway deployment..."

# Remove all compiled files and directories
echo "Removing compiled files..."
find . -name "*.dll" -type f -delete
find . -name "*.exe" -type f -delete
find . -name "*.pdb" -type f -delete
find . -name "*.deps.json" -type f -delete
find . -name "*.runtimeconfig.json" -type f -delete

# Remove build directories
echo "Removing build directories..."
rm -rf bin/
rm -rf obj/
rm -rf publish/
rm -rf out/
rm -rf .vs/

# Remove logs
echo "Removing log files..."
rm -rf logs/
find . -name "*.log" -type f -delete

# Remove websitebuilder-admin if it exists (should be separate repo)
if [ -d "websitebuilder-admin" ]; then
    echo "⚠️  Found websitebuilder-admin directory"
    echo "This should be in a separate repository for Vercel"
    read -p "Remove websitebuilder-admin directory? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        rm -rf websitebuilder-admin/
        echo "✅ Removed websitebuilder-admin"
    fi
fi

# Git operations
echo "Updating git..."
git rm -r --cached bin/ obj/ publish/ out/ .vs/ logs/ 2>/dev/null || true
git rm --cached *.dll *.exe *.pdb *.deps.json *.runtimeconfig.json 2>/dev/null || true

echo "✅ Repository cleaned!"
echo ""
echo "📝 Next steps:"
echo "1. Review changes: git status"
echo "2. Commit changes: git add . && git commit -m 'Clean repository for Railway deployment'"
echo "3. Push to GitHub: git push"
echo "4. Railway will automatically deploy from GitHub"