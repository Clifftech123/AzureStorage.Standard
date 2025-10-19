# Release Process for v1.0.0

This document provides step-by-step instructions for releasing v1.0.0 of AzureStorage.Standard.

## Pre-Release Checklist

Before releasing v1.0.0, ensure:

- [ ] All tests pass (unit and integration)
- [ ] CI/CD pipeline is green on develop branch
- [ ] Alpha packages tested following [TESTING.md](TESTING.md)
- [ ] [CHANGELOG.md](CHANGELOG.md) is updated with v1.0.0 release notes
- [ ] All documentation is up to date
- [ ] No critical bugs or issues
- [ ] Branch protection rules configured for main branch

## Release Steps

### Step 1: Verify Current State

```bash
# Check current branch
git branch

# Ensure you're on develop
git checkout develop

# Pull latest changes
git pull origin develop

# Verify CI/CD passed
# Visit: https://github.com/Clifftech123/AzureStorage.Standard/actions
```

### Step 2: Update Version References (if needed)

Check that any hardcoded version references in documentation are correct:

- [ ] README files mention v1.0.0
- [ ] CHANGELOG.md has v1.0.0 entry
- [ ] Package release notes are accurate

### Step 3: Merge to Main Branch

```bash
# Switch to main branch
git checkout main

# Pull latest main
git pull origin main

# Merge develop into main
git merge develop

# Push to main
git push origin main
```

**⚠️ Note**: This push will trigger CI/CD and publish alpha packages to NuGet.org. The tag (next step) will republish with stable version.

### Step 4: Create v1.0.0 Tag

```bash
# Create annotated tag for v1.0.0
git tag -a v1.0.0 -m "Release v1.0.0: Initial stable release

AzureStorage.Standard v1.0.0 - Simplified Azure Storage for .NET

This is the first stable release of AzureStorage.Standard, a modern .NET client library
for Azure Blob Storage, Queue Storage, Table Storage, and File Share Storage.

Features:
- Simplified APIs for all Azure Storage services
- Built-in retry policies and error handling
- Multi-targeting (.NET Standard 2.1, .NET 8, .NET 9)
- Comprehensive documentation and examples
- Full test coverage

Packages included:
- AzureStorage.Standard.Blobs v1.0.0
- AzureStorage.Standard.Queues v1.0.0
- AzureStorage.Standard.Tables v1.0.0
- AzureStorage.Standard.Files v1.0.0
- AzureStorage.Standard.Core v1.0.0

For full release notes, see CHANGELOG.md"

# Verify tag was created
git tag -l -n9 v1.0.0

# Push tag to trigger release
git push origin v1.0.0
```

### Step 5: Monitor CI/CD Pipeline

After pushing the tag, the CI/CD pipeline will automatically:

1. **Build** all projects
2. **Run** unit and integration tests
3. **Pack** NuGet packages with version 1.0.0
4. **Publish** to NuGet.org
5. **Create** GitHub Release with release notes

Monitor progress at:
- GitHub Actions: https://github.com/Clifftech123/AzureStorage.Standard/actions

Expected workflow jobs:
- ✅ Build and Test
- ✅ Publish to NuGet.org (Release)
- ✅ Create GitHub Release

### Step 6: Verify Release

#### Check NuGet.org

Visit each package page and verify version 1.0.0 is published:

- https://www.nuget.org/packages/AzureStorage.Standard.Blobs
- https://www.nuget.org/packages/AzureStorage.Standard.Queues
- https://www.nuget.org/packages/AzureStorage.Standard.Tables
- https://www.nuget.org/packages/AzureStorage.Standard.Files
- https://www.nuget.org/packages/AzureStorage.Standard.Core

Verify:
- [ ] Version shows as 1.0.0 (not prerelease)
- [ ] Package description is correct
- [ ] All frameworks are listed (.NET Standard 2.1, .NET 8.0, .NET 9.0)
- [ ] Dependencies show correctly
- [ ] README is visible

#### Check GitHub Release

Visit: https://github.com/Clifftech123/AzureStorage.Standard/releases

Verify:
- [ ] Release v1.0.0 is created
- [ ] Release notes from CHANGELOG are included
- [ ] All .nupkg files are attached
- [ ] Release is marked as stable (not pre-release)

### Step 7: Test Installation

Test that users can install the packages:

```bash
# Create a test project
mkdir test-v1-install
cd test-v1-install
dotnet new console

# Install packages
dotnet add package AzureStorage.Standard.Blobs --version 1.0.0
dotnet add package AzureStorage.Standard.Queues --version 1.0.0

# Verify installation
dotnet restore
dotnet build
```

### Step 8: Update Develop Branch

After successful release, update develop branch:

```bash
# Switch back to develop
git checkout develop

# Merge main to keep in sync
git merge main

# Push develop
git push origin develop
```

### Step 9: Announce Release (Optional)

Consider announcing the release:

- [ ] GitHub Discussions: Create announcement post
- [ ] Twitter/LinkedIn: Share release announcement
- [ ] Dev.to/Medium: Write blog post about the library
- [ ] .NET Community: Share in relevant forums/channels

## Post-Release Checklist

After v1.0.0 is released:

- [ ] All packages show version 1.0.0 on NuGet.org
- [ ] GitHub Release is created and public
- [ ] Documentation links are working
- [ ] Package installation works correctly
- [ ] Update project board/issues with v1.0.0 milestone

## Troubleshooting

### Build Failed

If CI/CD build fails:
1. Check GitHub Actions logs
2. Fix the issue on develop branch
3. Merge fix to main
4. Delete and recreate the tag:
   ```bash
   git tag -d v1.0.0
   git push origin :refs/tags/v1.0.0
   # Fix issues, then recreate tag
   git tag -a v1.0.0 -m "Release v1.0.0"
   git push origin v1.0.0
   ```

### NuGet Publish Failed

If NuGet.org publish fails:
1. Check if API key is valid: https://www.nuget.org/account/apikeys
2. Verify `NUGET_API_KEY` secret in GitHub: Settings → Secrets → Actions
3. Re-run the failed job in GitHub Actions

### Package Not Appearing on NuGet.org

- Packages may take 5-15 minutes to index
- Refresh the package page
- Clear NuGet cache: `dotnet nuget locals all --clear`

## Future Releases

For future releases (v1.0.1, v1.1.0, v2.0.0):

1. Update [CHANGELOG.md](CHANGELOG.md) with new version
2. Follow steps 1-9 above with the new version number
3. Use semantic versioning:
   - **Patch** (v1.0.x): Bug fixes
   - **Minor** (v1.x.0): New features (backward compatible)
   - **Major** (vx.0.0): Breaking changes

## Quick Reference

```bash
# Complete release in one command block (after testing)
git checkout main && \
git pull origin main && \
git merge develop && \
git push origin main && \
git tag -a v1.0.0 -m "Release v1.0.0: Initial stable release" && \
git push origin v1.0.0 && \
git checkout develop && \
git merge main && \
git push origin develop
```

## Need Help?

- Review GitHub Actions logs: https://github.com/Clifftech123/AzureStorage.Standard/actions
- Check NuGet.org package status
- Review this documentation: [BRANCHING_STRATEGY.md](.github/BRANCHING_STRATEGY.md)

---

**Ready to release? Start with Step 1!** 🚀
