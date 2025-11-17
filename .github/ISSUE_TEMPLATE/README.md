# GitHub Issue Templates and Labels

This directory contains issue templates and label configurations for the RazorConsole repository.

## Issue Templates

We provide two issue templates to help contributors report bugs and request features effectively:

### Bug Report (`bug-report.yml`)
Use this template to report bugs or unexpected behavior in RazorConsole. It includes fields for:
- Description of the bug
- Steps to reproduce
- Expected vs. actual behavior
- Environment information
- Additional context

### Feature Request (`feature-request.yml`)
Use this template to suggest new features or enhancements. It includes fields for:
- Feature description
- Problem statement
- Proposed solution
- Alternatives considered
- Contribution willingness

### Configuration (`config.yml`)
Configures the issue creation experience with:
- Blank issues enabled
- Links to Discussions and Documentation

## Labels

The `labels.yml` file defines recommended labels for the repository. These labels are used by the issue templates and can be applied manually:

- **bug** - Something isn't working (auto-applied by Bug Report template)
- **feature-request** - New feature or request (auto-applied by Feature Request template)
- **contribution-requested** - Looking for community contributions
- **docs** - Documentation improvements or additions

### Setting Up Labels

Labels need to be created in the repository. You can do this via:

1. **GitHub Web Interface**: Go to Settings → Labels
2. **GitHub CLI**: Use the following commands:

```bash
gh label create bug --description "Something isn't working" --color d73a4a
gh label create feature-request --description "New feature or request" --color a2eeef
gh label create contribution-requested --description "Looking for community contributions" --color 7057ff
gh label create docs --description "Documentation improvements or additions" --color 0075ca
```

## Usage

When contributors create a new issue, they will be prompted to choose between:
- Bug Report template
- Feature Request template
- Blank issue (still enabled)
- Link to Discussions
- Link to Documentation

The templates automatically apply appropriate labels (bug or feature-request) and guide users through providing relevant information.
