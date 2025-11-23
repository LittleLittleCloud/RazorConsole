# Git Hooks for RazorConsole

This directory contains Git hooks that help maintain code quality by running checks locally before commits and pushes.

## Available Hooks

### pre-commit
Runs `dotnet format` to verify code formatting matches the project's style guidelines defined in `.editorconfig`.

**What it does:**
- Checks if all code is properly formatted
- Fails the commit if formatting issues are found
- Suggests running `dotnet format RazorConsole.slnx` to fix issues

### pre-push
Runs the test suite before pushing to the remote repository.

**What it does:**
- Executes all tests in the solution
- Fails the push if any tests fail
- Helps catch issues before they reach CI

## Setup

To enable these hooks, run the following command from the repository root:

```bash
git config core.hooksPath .githooks
```

This tells Git to use the hooks in `.githooks/` instead of the default `.git/hooks/` directory.

### Windows Users

The default hooks (`pre-commit` and `pre-push`) are written in Bash and work with Git Bash (installed with Git for Windows).

**PowerShell Alternative**: If you prefer PowerShell, wrapper scripts are provided (`pre-commit.ps1` and `pre-push.ps1`). To use them, modify the bash hooks to call PowerShell:

```bash
# Edit .githooks/pre-commit to contain:
#!/usr/bin/env bash
pwsh -NoProfile -File "$(dirname "$0")/pre-commit.ps1"

# Edit .githooks/pre-push to contain:
#!/usr/bin/env bash
pwsh -NoProfile -File "$(dirname "$0")/pre-push.ps1"
```

Or simply use the bash versions - they work fine on Windows with Git Bash.

## Verifying Setup

After setup, you can verify the hooks are working:

```bash
# Check current hooks path
git config core.hooksPath

# Try making a commit (hook will run automatically)
git commit -m "test"
```

## Bypassing Hooks

If you need to bypass hooks temporarily (not recommended for regular use):

```bash
# Skip pre-commit hook
git commit --no-verify

# Skip pre-push hook  
git push --no-verify
```

## Disabling Hooks

To disable these hooks:

```bash
git config --unset core.hooksPath
```

## Alternative: Lefthook

If you prefer using a git hook manager, see [LEFTHOOK_SETUP.md](./LEFTHOOK_SETUP.md) for an alternative approach using the Lefthook tool.

## Notes

- These hooks are **optional** but recommended for contributors
- CI will still run the same checks even if you don't use hooks
- Hooks provide faster feedback by catching issues before pushing
- Hooks are cross-platform compatible (Linux, macOS, Windows with Git Bash)
