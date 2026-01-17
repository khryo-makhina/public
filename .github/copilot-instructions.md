# GitHub Copilot — Custom Instructions (khryo-makhina)

## Purpose
- Provide general, workspace-wide behavior for GitHub Copilot suggestions.
- Repository-specific guidance for `khryo-makhina` contents (Docker, n8n, Ollama, k8s manifests, helper scripts).

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


## Additional/override guidelines
- Focus on containerization best practices: small images, minimal layers, explicit versions for base images.
- Keep `docker-compose` and Kubernetes manifests declarative and idempotent.
- For shell scripts and Dockerfiles, prefer POSIX-compatible patterns where feasible and document platform specifics (Windows vs Linux).
- When modifying `README.md` or deployment docs, include exact commands to reproduce local setups and any environment prerequisites.
- Preserve existing folder conventions (local_files, caddy_config, etc.) and don't restructure without explicit approval.
