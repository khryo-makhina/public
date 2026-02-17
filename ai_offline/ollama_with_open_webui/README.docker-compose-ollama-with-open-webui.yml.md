# README for docker-compose-ollama-with-open-webui.yml

Run / Start — modern Compose (V2) commands for `docker-compose-ollama-with-open-webui.yml` (copy-paste):

```bash
# Start (modern Compose V2 with device_requests support)
docker compose -f docker-compose-ollama-with-open-webui.yml up -d

# Start (build local Dockerfile first)
docker compose -f docker-compose-ollama-with-open-webui.yml up -d --build

# Stop and remove containers
docker compose -f docker-compose-ollama-with-open-webui.yml down

# Remove volumes and images (clean)
docker compose -f docker-compose-ollama-with-open-webui.yml down -v --rmi all
```

Compatibility note:
- `docker-compose-ollama-with-open-webui.yml` uses `device_requests` and requires Docker Compose V2 and a recent Docker Engine.
- If your environment does not support `device_requests`, use the legacy compose file (`docker-compose-ollama-with-open-webui.legacy.yml`).
