GitHub Copilot — Custom Instructions (khryo-makhina)

Purpose
- Repository-specific guidance for `khryo-makhina` contents (Docker, n8n, Ollama, k8s manifests, helper scripts).

Additional/override guidelines (in addition to root)
- Focus on containerization best practices: small images, minimal layers, explicit versions for base images.
- Keep `docker-compose` and Kubernetes manifests declarative and idempotent.
- For shell scripts and Dockerfiles, prefer POSIX-compatible patterns where feasible and document platform specifics (Windows vs Linux).
- When modifying `README.md` or deployment docs, include exact commands to reproduce local setups and any environment prerequisites.
- Preserve existing folder conventions (local_files, caddy_config, etc.) and don't restructure without explicit approval.

Cross-reference
- These instructions complement the root `.github/copilot-instructions.md`. Follow root rules first; apply these repo-specific points when working inside this folder.