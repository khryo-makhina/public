# Run / Start

Quick commands to start and stop the stack. These are the recommended, copy-paste commands for common workflows.

Start (legacy compose file — recommended)
```bash
docker compose -f docker-compose-ollama-with-open-webui.legacy.yml up -d
```

Start (modern Compose V2 with device_requests support)
```bash
docker compose -f docker-compose-ollama-with-open-webui.yml up -d
```

Start (build local `Dockerfile` first) — recommended if you edit `entrypoint.sh` or Dockerfile
```bash
docker compose -f docker-compose-ollama-with-open-webui.legacy.yml up -d --build
```

Stop and remove containers
```bash
docker compose -f docker-compose-ollama-with-open-webui.legacy.yml down
```

Remove volumes and images (clean)
```bash
docker compose -f docker-compose-ollama-with-open-webui.legacy.yml down -v --rmi all
```

Compatibility notes
- Use the **legacy file** (`docker-compose-ollama-with-open-webui.legacy.yml`) if your `docker compose` rejects `device_requests` (older Compose versions).
- Use the **primary file** (`docker-compose-ollama-with-open-webui.yml`) if you have Docker Compose V2 with full `device_requests` support.
