# GitHub Copilot — Custom Instructions (Workspace root)

## Purpose
- Provide general, workspace-wide behavior for GitHub Copilot suggestions.

## General guidelines
- Tone: concise, direct, friendly.
- Prioritize readability, maintainability, and existing repository style.
- Match the language of the target file: e.g., C# for .cs, Python for .py, YAML for compose/k8s manifests.
- Prefer small, well-named functions and clear separation of concerns.
- Add or update unit/integration tests when changing behavior.
- Do not introduce secrets, hardcoded credentials, or copyrighted text verbatim.
- Use UTF-8 encoding and preserve line endings consistent with existing files.
- When unsure, ask a clarifying question rather than guessing.

## When producing code changes
- Keep diffs minimal and focused. Include tests and documentation when appropriate.
- Follow the repository's existing formatting and styling conventions.
- Add descriptive commit messages and short PR descriptions.

## Cross-reference
- Subfolder custom instructions are complementary to these root instructions and apply when a subfolder needs project-specific behavior or overrides.
- Always apply the root rules unless a subfolder file explicitly specifies an override for that area.

## Maintainer contact
- If a suggestion touches deployment, security, or CI, request review from the repository owner before merging.