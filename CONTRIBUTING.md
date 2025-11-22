# Contributing to RazorConsole

Thank you for your interest in contributing to RazorConsole! We welcome contributions from the community and appreciate your help in making this project better.

## 📋 Getting Started

### Before You Start

- **For large PRs, refactors, or new features**: Please create an issue first to discuss your proposed changes. This helps ensure your contribution aligns with the project's goals and prevents duplicate work.
- **Join our Discord**: For timely responses and real-time collaboration, [join our Discord server](https://discord.gg/DphHAnJxCM). The maintainers and community are active there and can help guide your contributions.

## 🐛 Reporting Issues

When reporting bugs or requesting features, please use our issue templates:

- **Bug Report**: For reporting bugs or unexpected behavior ([use template](../../issues/new?template=bug-report.yml))
- **Feature Request**: For suggesting new features or enhancements ([use template](../../issues/new?template=feature-request.yml))

The templates will guide you through providing the necessary information:

1. **Search existing issues** to avoid duplicates
2. **Provide a clear description** of the problem or feature request
3. **Include reproduction steps** for bugs
4. **Share your environment details** (.NET version, OS, etc.)
5. **Use appropriate labels**: bug, feature-request, contribution-requested, docs

## 🔧 Development Setup

### Prerequisites

- [.NET 8.0 or 9.0 SDK](https://dotnet.microsoft.com/download)
- [Git LFS](https://git-lfs.github.io/) for handling large media files

### Clone and Setup

```bash
# Install Git LFS if not already installed
git lfs install

# Clone the repository
git clone https://github.com/LittleLittleCloud/RazorConsole.git
cd RazorConsole

# Build the solution
dotnet build RazorConsole.slnx

# Run tests
dotnet test RazorConsole.slnx
```

## 💻 Making Changes

### Coding Standards

- Follow the rules encoded in `.editorconfig` (four-space indentation, file-scoped namespaces, system usings first)
- Prefer async/await with `ConfigureAwait(false)` when awaiting inside library code
- Keep public APIs nullable-enabled and document exceptions and edge cases
- Treat Spectre.Console renderables as immutable from outside rendering loops

### Before Submitting

1. **Format your code**: Run `dotnet format RazorConsole.slnx` before opening a pull request
2. **Run tests**: Execute `dotnet test RazorConsole.slnx` locally. CI requires a clean test run on Linux and Windows
3. **Update tests**: When touching focus or keyboard handling, add or update tests in `FocusManagerTests` or `KeyboardEventManagerTests`
4. **Update documentation**: Update the README when introducing user-facing features or significant architectural changes

## 📝 Pull Request Process

1. **Create an issue first** for large changes, refactors, or new features
2. **Fork the repository** and create a branch from `main`
3. **Make your changes** following the coding standards
4. **Write or update tests** to cover your changes
5. **Run `dotnet format`** to ensure formatting is correct
6. **Run `dotnet test`** to ensure all tests pass
7. **Submit a pull request** with a clear description of the changes

### Pull Request Guidelines

- Keep PRs focused on a single feature or bug fix
- Write clear, descriptive commit messages
- Reference related issues in your PR description
- Be responsive to feedback and questions

## 🧪 Testing

- Add tests for new features and bug fixes
- Ensure all existing tests pass
- Tests should be placed in the `src/RazorConsole.Tests` project

## 📚 Documentation

- Update the README.md for user-facing changes
- Add or update XML documentation comments for public APIs
- Consider adding examples to the `examples/` directory for new features
- Design notes can be added to `design-doc/` for architectural changes

## 🚀 Release Process

Creating a GitHub release triggers `.github/workflows/release.yml` to build, test, pack, and publish platform bundles. Version numbers should follow semantic versioning.

## 💬 Getting Help

- **Discord**: [Join our server](https://discord.gg/DphHAnJxCM) for quick questions and real-time help
- **Issues**: Create an issue on GitHub for bugs or feature requests
- **Discussions**: Use GitHub Discussions for general questions and ideas

## 📜 Code of Conduct

Please be respectful and constructive in all interactions. We aim to maintain a welcoming and inclusive community.

## 🙏 Thank You!

Your contributions help make RazorConsole better for everyone. We appreciate your time and effort!
