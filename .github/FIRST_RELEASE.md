# First Release - Step by Step Guide

This guide will walk you through releasing version 1.0.0 of AzureStorage.Standard.

## Current State:

- ✅ All code is ready
- ✅ Tests are passing
- ✅ CI/CD is configured
- ✅ Automatic versioning (MinVer) is set up
- ⏸️ No git tags exist yet
- ⏸️ No version released to NuGet.org

---

## Step 1: Commit Current Changes

You have uncommitted changes (MinVer setup, CI/CD, etc.). Let's commit them to `develop`:

```bash
# Check what needs to be committed
git status

# Add all changes
git add .

# Commit with a descriptive message
git commit -m "ci: Add automatic versioning with MinVer and complete CI/CD pipeline

- Add MinVer package for automatic semantic versioning
- Remove hardcoded versions from all .csproj files
- Add GitHub Actions workflows (CI/CD, CodeQL)
- Add Dependabot configuration
- Add comprehensive documentation (BRANCHING_STRATEGY, AUTOMATIC_VERSIONING, RELEASE_PROCESS)
- Configure NUGET_API_KEY secret for automated publishing"

# Push to develop branch
git push origin develop
```

**What happens**:
- ✅ GitHub Actions CI/CD runs automatically
- ✅ Builds and tests your code
- ✅ Publishes to GitHub Packages (pre-release)
- ✅ Version will be something like `0.0.0-alpha.0.X` (no tags yet)

---

## Step 2: Verify CI/CD is Working

1. Go to: https://github.com/Clifftech123/AzureStroage.Standard/actions
2. Watch the "CI/CD Pipeline" workflow run
3. Make sure all jobs pass:
   - ✅ Build and Test
   - ✅ Publish to GitHub Packages (develop branch)

**If it fails**:
- Read the error logs
- Fix the issue
- Commit and push again

**If it succeeds**:
- Your packages are on GitHub Packages with version `0.0.0-alpha.0.X`

---

## Step 3: Merge to Main Branch

Once `develop` is stable and tested, merge to `main`:

```bash
# Switch to main branch
git checkout main

# Pull latest changes (in case someone else pushed)
git pull origin main

# Merge develop into main
git merge develop

# Push to main
git push origin main
```

**What happens**:
- ✅ CI/CD runs again on `main`
- ✅ Builds and tests
- ⚠️ Still has version `0.0.0-alpha.0.X` (no tags yet)
- ⚠️ Publishes to NuGet.org with pre-release version

---

## Step 4: Create Your First Version Tag

Now you'll create the **v1.0.0** tag, which tells MinVer "this is version 1.0.0":

```bash
# Make sure you're on main branch
git checkout main

# Create an annotated tag for version 1.0.0
git tag -a v1.0.0 -m "Release version 1.0.0 - Initial release

Features:
- Azure Blob Storage client with full CRUD operations
- Azure Queue Storage client with message operations
- Azure Table Storage client with entity management
- Azure File Share client with file/directory operations
- Automatic retry policies using Polly
- Comprehensive error handling with AzureStorageException
- Full async/await support
- Extensive XML documentation
- 115+ unit and integration tests"

# Push the tag to GitHub
git push origin v1.0.0
```

**What happens**:
- 🚀 CI/CD detects the tag
- ✅ Builds with version **1.0.0** (MinVer calculates from tag)
- ✅ Publishes to **NuGet.org** with version **1.0.0**
- ✅ Creates **GitHub Release** with all `.nupkg` files attached

---

## Step 5: Verify Release on NuGet.org

1. Go to: https://www.nuget.org/profiles/Clifftech123
2. You should see 5 packages (all version 1.0.0):
   - AzureStorage.Standard.Blobs 1.0.0
   - AzureStorage.Standard.Queues 1.0.0
   - AzureStorage.Standard.Tables 1.0.0
   - AzureStorage.Standard.Files 1.0.0
   - AzureStorage.Standard.Core 1.0.0

3. Check GitHub Releases:
   - https://github.com/Clifftech123/AzureStroage.Standard/releases
   - You should see "Release 1.0.0" with attached `.nupkg` files

---

## Step 6: Test Installing Your Package

Open a new test project and install your package:

```bash
# Create a test console app
mkdir test-app
cd test-app
dotnet new console

# Install your package from NuGet.org
dotnet add package AzureStorage.Standard.Blobs --version 1.0.0

# It should also install the Core package automatically
```

**Expected output**:
```
info : Installing AzureStorage.Standard.Blobs 1.0.0.
info : Installing AzureStorage.Standard.Core 1.0.0.
```

---

## Future Releases

### For Patch Releases (Bug Fixes)

```bash
# After fixing a bug and merging to main
git checkout main
git tag -a v1.0.1 -m "Release 1.0.1 - Bug fixes"
git push origin v1.0.1

# MinVer will automatically version as 1.0.1
```

### For Minor Releases (New Features)

```bash
# After adding new features and merging to main
git checkout main
git tag -a v1.1.0 -m "Release 1.1.0 - Add blob leasing support"
git push origin v1.1.0

# MinVer will automatically version as 1.1.0
```

### For Major Releases (Breaking Changes)

```bash
# After breaking API changes and merging to main
git checkout main
git tag -a v2.0.0 -m "Release 2.0.0 - Major API redesign"
git push origin v2.0.0

# MinVer will automatically version as 2.0.0
```

---

## Summary of Commands (Quick Reference)

```bash
# 1. Commit changes to develop
git add .
git commit -m "ci: Add automatic versioning and CI/CD"
git push origin develop

# 2. Merge to main
git checkout main
git merge develop
git push origin main

# 3. Create version tag
git tag -a v1.0.0 -m "Release 1.0.0 - Initial release"
git push origin v1.0.0

# 4. Done! Check:
# - GitHub Actions: https://github.com/Clifftech123/AzureStroage.Standard/actions
# - NuGet.org: https://www.nuget.org/profiles/Clifftech123
# - GitHub Releases: https://github.com/Clifftech123/AzureStroage.Standard/releases
```

---

## Important Notes:

### ✅ You MUST Specify Version in Tag

MinVer calculates version from the tag name:

```bash
git tag -a v1.0.0 -m "Release 1.0.0"
              ↑
              This becomes the version number
```

**Valid tag formats**:
- ✅ `v1.0.0` → Version: 1.0.0
- ✅ `v1.2.3` → Version: 1.2.3
- ✅ `v2.0.0-beta.1` → Version: 2.0.0-beta.1

**Invalid tag formats**:
- ❌ `release` → Version: 0.0.0-alpha.0.X
- ❌ `v1` → Version: 0.0.0-alpha.0.X
- ❌ `1.0` → Version: 0.0.0-alpha.0.X

### ✅ Tags Are Permanent

Once you push a tag and publish to NuGet.org:
- ⚠️ **Don't delete the tag** (users depend on it)
- ⚠️ **Can't re-publish same version** (NuGet rejects duplicates)
- ✅ **If mistake**: Create a new patch version (v1.0.1)

### ✅ Check Version Before Tagging

Before creating a tag, verify MinVer will calculate the right version:

```bash
# See what version MinVer would calculate
git tag -a v1.0.0 -m "Test tag"
dotnet build --no-restore -v:m | findstr "MinVer:"
git tag -d v1.0.0  # Delete test tag

# Expected output:
# MinVer: 1.0.0
```

---

## Troubleshooting

### Problem: CI/CD Fails

**Check**:
1. GitHub Actions logs
2. NUGET_API_KEY secret is set correctly
3. Build succeeds locally: `dotnet build`

### Problem: Version is 0.0.0-alpha.0.X

**Cause**: No version tags exist

**Fix**: Create a tag: `git tag -a v1.0.0 -m "Release 1.0.0"`

### Problem: NuGet Push Fails with 403/401

**Cause**: Invalid or missing NUGET_API_KEY

**Fix**:
1. Verify secret at: https://github.com/Clifftech123/AzureStroage.Standard/settings/secrets/actions
2. Regenerate NuGet API key if needed

### Problem: Package Already Exists

**Cause**: Trying to publish same version twice

**Fix**: Bump to next version (v1.0.1)

---

## Ready to Release?

Follow the steps in order:
1. ✅ Commit to `develop`
2. ✅ Merge to `main`
3. ✅ Create tag `v1.0.0`
4. ✅ Push tag
5. ✅ Watch CI/CD publish automatically!

Good luck! 🚀
