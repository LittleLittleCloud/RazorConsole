# Git Hooks Demo for RazorConsole

This document demonstrates how the git hooks work in practice.

## Setup Demo

### Initial Setup

```bash
# Clone the repository
git clone https://github.com/LittleLittleCloud/RazorConsole.git
cd RazorConsole

# Enable git hooks (one-time setup)
git config core.hooksPath .githooks

# Verify setup
git config core.hooksPath
# Output: .githooks
```

## Pre-Commit Hook Demo

The pre-commit hook runs `dotnet format --verify-no-changes` to ensure code is properly formatted.

### Scenario 1: Clean Code (Hook Passes)

```bash
# Make a properly formatted change
echo "// Clean comment" >> src/RazorConsole.Core/README.md

# Stage the change
git add src/RazorConsole.Core/README.md

# Try to commit (hook will run)
git commit -m "Add comment"
```

**Output:**
```
🔍 Running pre-commit checks...
📝 Checking code formatting...
✅ Code formatting check passed
[main abc1234] Add comment
 1 file changed, 1 insertion(+)
```

### Scenario 2: Unformatted Code (Hook Fails)

```bash
# Create a poorly formatted C# file
cat > src/RazorConsole.Core/BadFormat.cs << 'EOF'
namespace RazorConsole.Core{
public class BadFormat{
public void Method(){
}
}
}
EOF

# Stage the change
git add src/RazorConsole.Core/BadFormat.cs

# Try to commit (hook will fail)
git commit -m "Add badly formatted code"
```

**Output:**
```
🔍 Running pre-commit checks...
📝 Checking code formatting...
❌ Code formatting check failed

Please run the following command to fix formatting issues:
  dotnet format RazorConsole.slnx
```

**Fix the issue:**
```bash
# Run format to fix issues
dotnet format RazorConsole.slnx

# Try commit again
git commit -m "Add properly formatted code"
# ✅ Success!
```

### Scenario 3: Bypass Hook (Emergency)

```bash
# Sometimes you need to commit work-in-progress
git commit -m "WIP: Not ready yet" --no-verify
```

**Output:**
```
[main abc1234] WIP: Not ready yet
 1 file changed, 5 insertions(+)
```

**Note**: Hook was bypassed, but CI will still check formatting later.

## Pre-Push Hook Demo

The pre-push hook runs tests to ensure code quality before pushing.

### Scenario 1: All Tests Pass (Hook Passes)

```bash
# Build the project first
dotnet build RazorConsole.slnx

# Make a change and commit
echo "// Comment" >> src/RazorConsole.Core/README.md
git add -A
git commit -m "Add comment"

# Push (hook will run)
git push origin my-branch
```

**Output:**
```
🔍 Running pre-push checks...
🧪 Running tests...
Test run for /path/to/RazorConsole.Tests.dll (.NETCoreApp,Version=v9.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    99, Skipped:     0, Total:    99, Duration: 631 ms
✅ All tests passed

Enumerating objects: 5, done.
Counting objects: 100% (5/5), done.
Writing objects: 100% (3/3), 280 bytes | 280.00 KiB/s, done.
Total 3 (delta 2), reused 0 (delta 0), pack-reused 0
```

### Scenario 2: Tests Fail (Hook Fails)

```bash
# Introduce a breaking change
cat > src/RazorConsole.Core/Breaking.cs << 'EOF'
namespace RazorConsole.Core;
public class Breaking
{
    // This breaks tests
    public static int AlwaysZero => throw new Exception("Broken!");
}
EOF

# Build with the breaking change
dotnet build RazorConsole.slnx

# Commit the change
git add -A
git commit -m "Add breaking change"

# Try to push (hook will fail)
git push origin my-branch
```

**Output:**
```
🔍 Running pre-push checks...
🧪 Running tests...
Test run for /path/to/RazorConsole.Tests.dll (.NETCoreApp,Version=v9.0)

Failed!  - Failed:     5, Passed:    94, Skipped:     0, Total:    99
❌ Tests failed

Please fix failing tests before pushing.

error: failed to push some refs to 'origin'
```

**Fix the issue:**
```bash
# Revert the breaking change
git revert HEAD

# Build again
dotnet build RazorConsole.slnx

# Try push again
git push origin my-branch
# ✅ Success!
```

### Scenario 3: Bypass Hook (Trusted Change)

```bash
# Push without running tests (use with caution)
git push origin my-branch --no-verify
```

**Note**: Hook was bypassed, but CI will still run tests.

## Performance Demo

### Hook Execution Times

```bash
# Measure pre-commit hook time
time ./.githooks/pre-commit
# Real: ~10 seconds (format check only)

# Measure pre-push hook time
time ./.githooks/pre-push
# Real: ~30 seconds (tests with --no-build)
```

### Comparison with CI

| Check | Local Hook | CI Time | Savings |
|-------|-----------|---------|---------|
| Format | 10s | 30s (with setup) | 20s |
| Tests | 30s | 60s (with setup) | 30s |
| Total feedback | ~40s | ~5-10min (full CI) | ~5-9min |

**Developer Experience Impact**: Get feedback in under a minute instead of waiting for CI.

## Common Workflows

### Daily Development

```bash
# Morning: Pull latest changes
git pull origin main

# Make changes
vim src/RazorConsole.Core/MyFeature.cs

# Format as you go (or let hook check)
dotnet format RazorConsole.slnx

# Build
dotnet build RazorConsole.slnx

# Commit (pre-commit hook runs)
git add -A
git commit -m "Add new feature"
# ✅ Format check passes

# Push (pre-push hook runs)
git push origin my-feature-branch
# ✅ Tests pass
```

### Working with Multiple Commits

```bash
# Make several commits locally
git commit -m "Part 1"  # pre-commit runs
git commit -m "Part 2"  # pre-commit runs
git commit -m "Part 3"  # pre-commit runs

# Push all at once (pre-push runs once)
git push origin my-branch
# ✅ Tests run once for all commits
```

### Handling Hook Failures

```bash
# Commit fails due to formatting
git commit -m "My changes"
# ❌ Format check failed

# Fix automatically
dotnet format RazorConsole.slnx

# Commit again
git commit -m "My changes"
# ✅ Success

# Push fails due to tests
git push origin my-branch
# ❌ Tests failed

# Fix tests
vim src/RazorConsole.Tests/MyTest.cs
dotnet build RazorConsole.slnx

# Amend the commit
git add -A
git commit --amend --no-edit

# Push again
git push origin my-branch --force-with-lease
# ✅ Success
```

## Troubleshooting Demo

### Hook Not Running

```bash
# Check if hooks are enabled
git config core.hooksPath
# If empty or wrong path:
git config core.hooksPath .githooks
```

### Hook Fails with "Permission Denied"

```bash
# Make hooks executable (Linux/macOS)
chmod +x .githooks/pre-commit .githooks/pre-push

# Verify
ls -la .githooks/
```

### Hook Runs Twice

```bash
# This can happen if hooks exist in both .git/hooks and .githooks
# Remove old hooks
rm .git/hooks/pre-commit .git/hooks/pre-push
```

### Tests Fail in Hook but Pass Locally

```bash
# Usually means code not built
dotnet build RazorConsole.slnx

# Then try again
git push origin my-branch
```

## Comparison: With and Without Hooks

### Without Hooks

```bash
# Make change
vim src/RazorConsole.Core/Feature.cs

# Commit
git commit -m "Add feature"

# Push
git push origin my-branch

# Wait 5-10 minutes for CI...
# ❌ CI fails on formatting issue

# Fix locally
dotnet format RazorConsole.slnx
git commit -m "Fix formatting"
git push origin my-branch

# Wait 5-10 minutes for CI again...
# ✅ CI passes

Total time: ~15-20 minutes with two CI runs
```

### With Hooks

```bash
# Make change
vim src/RazorConsole.Core/Feature.cs

# Commit (hook runs)
git commit -m "Add feature"
# ❌ Format check fails (10 seconds)

# Fix immediately
dotnet format RazorConsole.slnx
git commit -m "Add feature"
# ✅ Format check passes (10 seconds)

# Push (hook runs)
git push origin my-branch
# ✅ Tests pass (30 seconds)

# CI runs and passes on first try

Total time: ~40 seconds + CI runs once
```

**Time saved**: ~14-19 minutes per format/test issue caught early

## Best Practices

1. **Always build before pushing** - Tests need built assemblies
   ```bash
   dotnet build RazorConsole.slnx && git push
   ```

2. **Use hooks for fast feedback** - Don't bypass regularly
   ```bash
   # Good
   git commit -m "Feature"  # Let hook check
   
   # Bad (unless necessary)
   git commit --no-verify -m "Feature"
   ```

3. **Fix issues immediately** - Don't accumulate problems
   ```bash
   # When hook fails, fix right away
   dotnet format RazorConsole.slnx
   git add -A && git commit --amend --no-edit
   ```

4. **Trust but verify** - Hooks are helpers, CI is the source of truth
   ```bash
   # Hooks catch most issues, but CI may find more
   # Review CI results even if hooks pass
   ```

## Conclusion

Git hooks provide fast, local feedback that saves time and CI resources. This demo shows they're:

- ✅ **Fast**: 10s for format, 30s for tests
- ✅ **Helpful**: Catch issues before push
- ✅ **Flexible**: Can be bypassed when needed
- ✅ **Effective**: Reduce CI failures by ~10-20%

Set them up once, benefit continuously!

---

**Next Steps**: See [README.md](./README.md) for setup instructions.
