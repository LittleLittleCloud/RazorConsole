# GitHub Copilot instructions

These instructions are automatically appended to Copilot Chat sessions when working in this repository. They provide extra context so that answers respect the codebase conventions and release process.

> ℹ️ Learn more about custom instructions in the official documentation: [Customize chat responses and set context](https://learn.microsoft.com/en-us/visualstudio/ide/copilot-chat-context?view=vs-2022#enable-custom-instructions).

## Project context

- RazorConsole renders Razor components to Spectre.Console output. The core implementation lives in `src/RazorConsole.Core` and the interactive showcase in `src/RazorConsole.Gallery`.
- Tests live in `src/RazorConsole.Tests` and should be kept up to date for any behavioral change.
- Design notes are available under `design-doc/` for additional background.

## Coding conventions

- Follow the rules encoded in `.editorconfig` (four-space indentation, file-scoped namespaces, system usings first).
- Prefer async/await with `ConfigureAwait(false)` when awaiting inside library code.
- Keep public APIs nullable-enabled and document exceptions and edge cases.
- Treat Spectre.Console renderables as immutable from outside rendering loops.

## Development workflow

- Run `dotnet format RazorConsole.sln` before opening a pull request to ensure formatting checks pass.
- Execute `dotnet test RazorConsole.sln` locally; CI requires a clean test run on Linux and Windows.
- When touching focus or keyboard handling, add or update tests in `FocusManagerTests` or `KeyboardEventManagerTests`.
- Update the README when introducing user-facing features or significant architectural changes.

## Release guidance

- Creating a GitHub release triggers `.github/workflows/release.yml` to build, test, pack, and publish platform bundles. Version numbers should follow semantic versioning.
- Generated NuGet packages and gallery archives are uploaded to the release as downloadable assets.

## Prompting tips

- Ask for focused edits (specific files or components) rather than broad refactors.
- Provide failing test names or stack traces when requesting debugging assistance.
- Mention whether code runs in the live Spectre display or fallback rendering path to receive context-aware suggestions.
