# Copilot configuration spec

This document explains how GitHub Copilot Chat is configured for the RazorConsole repository and how contributors should collaborate with the agentic workflows.

## Goals

- Provide Copilot with enough context to respect the repository's architecture and coding standards.
- Encourage concise, iterative prompts that align with the engineering workflow documented in the README.
- Capture references to official guidance so the configuration remains maintainable over time.

## Custom instructions file

- The canonical custom instruction file lives at `.github/copilot-instructions.md` and is automatically picked up by GitHub Copilot Chat in compatible editors.
- Instructions emphasise formatting via `.editorconfig`, running `dotnet format`, and executing the test suite before opening pull requests.
- Contributors should review and update the file whenever conventions change (for example, new testing utilities or additional solutions).

## Prompting guidance

- Break down large requests into smaller sequential prompts. This mirrors the best practices described in [Customize chat responses and set context](https://learn.microsoft.com/en-us/visualstudio/ide/copilot-chat-context?view=vs-2022#enable-custom-instructions).
- Include failing test names or stack traces when asking Copilot to debug runtime issues.
- Reference concrete files or methods (for example, `ConsoleApp<TComponent>` in `AppHost.cs`) to ensure Copilot focuses on the correct code paths.
- Prefer letting Copilot execute queued actions in agent mode so it keeps full context, especially during formatting or multi-step refactors.

## Maintenance checklist

- Validate that `.github/copilot-instructions.md` stays in sync with repository guidelines after significant refactors.
- Consider adding prompt files under `.github/prompts/` for recurring tasks such as scaffolding new Razor components or updating CI.
- When introducing new workflows (for example, additional release targets), document them in both the README and the Copilot instructions so the assistant can propose accurate steps.
