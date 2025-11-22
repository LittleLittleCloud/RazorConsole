The project should contains the following files structure:

.github/
  workflows/ - CI/CD workflows
design-doc/
  project-overview.md - This document
  syntax-highlighter.md - Syntax highlighter component design
src/
  RazorConsole.Core - Core library with rendering logic and Razor components
  RazorConsole.Gallery - Sample RazorConsole app
  RazorConsole.Tests - Unit tests for core library

Directory.Build.targets - Centralized build targets
Directory.Build.props - Centralized build properties
Directory.Packages.props - Centralized package versions

global.json - .NET SDK version
README.md - Project overview and instructions
LICENSE - License file
RazorConsole.slnx - Solution file

.gitignore - Git ignore file
