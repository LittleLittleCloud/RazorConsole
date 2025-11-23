# Lefthook Setup for RazorConsole

[Lefthook](https://github.com/evilmartians/lefthook) is a fast, cross-platform Git hooks manager. This is an alternative to using native Git hooks.

## What is Lefthook?

Lefthook is a Git hooks manager that:
- ✅ Runs hooks in parallel for better performance
- ✅ Available as a dotnet tool (fits .NET workflow)
- ✅ Cross-platform (Windows, macOS, Linux)
- ✅ Simple YAML configuration
- ✅ Used by GitLab and other major projects

## Installation

### Option 1: As a .NET Local Tool (Recommended)

If the repository already has `.config/dotnet-tools.json`:

```bash
dotnet tool restore
```

If not, you can add it:

```bash
# Create tool manifest if it doesn't exist
dotnet new tool-manifest

# Install lefthook
dotnet tool install lefthook

# Install hooks
dotnet lefthook install
```

### Option 2: Global Installation

Install globally so it's available in all repositories:

```bash
# As dotnet global tool
dotnet tool install --global lefthook

# Install hooks in this repository
lefthook install
```

### Option 3: Standalone Binary

Download from [GitHub releases](https://github.com/evilmartians/lefthook/releases) or use a package manager:

```bash
# macOS (Homebrew)
brew install lefthook

# Windows (Scoop)
scoop install lefthook

# Then install hooks
lefthook install
```

## Configuration

Lefthook reads configuration from `.lefthook.yml` in the repository root. The current configuration:

```yaml
pre-commit:
  parallel: true
  commands:
    format:
      run: dotnet format RazorConsole.slnx --verify-no-changes
      glob: "*.cs"

pre-push:
  commands:
    test:
      run: dotnet test RazorConsole.slnx --no-build --nologo --verbosity minimal
```

## Usage

Once installed, hooks run automatically:

- **pre-commit**: Runs when you execute `git commit`
- **pre-push**: Runs when you execute `git push`

### Manual Execution

Run hooks manually without committing/pushing:

```bash
# Run all hooks
dotnet lefthook run pre-commit
dotnet lefthook run pre-push

# Or if installed globally
lefthook run pre-commit
lefthook run pre-push
```

### Skipping Hooks

Skip hooks when needed (not recommended for regular use):

```bash
# Skip pre-commit
git commit --no-verify

# Skip pre-push
git push --no-verify

# Or set environment variable
LEFTHOOK=0 git commit -m "skip hooks"
```

## Verifying Installation

Check if Lefthook is installed and working:

```bash
# If installed as local tool
dotnet lefthook version

# If installed globally
lefthook version

# List installed hooks
git config --list | grep hooks
```

## Uninstalling

To remove Lefthook hooks:

```bash
# Remove hooks
dotnet lefthook uninstall

# Uninstall tool (if local tool)
dotnet tool uninstall lefthook
```

## Comparison with Native Hooks

| Feature | Native Hooks | Lefthook |
|---------|--------------|----------|
| Dependencies | None (Git only) | Lefthook tool |
| Configuration | Shell scripts | YAML file |
| Parallel execution | Manual | Built-in |
| File filtering | Manual | Built-in (glob) |
| Setup | `git config` | `lefthook install` |
| Cross-platform | Bash scripts | Native binary |

## Recommendation

- **Use native hooks (.githooks/)** if you want zero dependencies
- **Use Lefthook** if you want advanced features like parallel execution and cleaner configuration

Both approaches are fully supported in RazorConsole. Choose based on your preference.

## Resources

- [Lefthook GitHub Repository](https://github.com/evilmartians/lefthook)
- [Lefthook Documentation](https://github.com/evilmartians/lefthook/blob/master/docs/usage.md)
- [Native Git Hooks Setup](./README.md)
