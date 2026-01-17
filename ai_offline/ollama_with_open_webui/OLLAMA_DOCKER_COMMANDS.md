# Ollama Docker Quick Reference

## 🐳 Docker Commands

```bash
# Build and run (basic)
docker build -t ollama-custom .
docker run -d -p 11434:11434 -v ollama_data:/root/.ollama --name ollama --gpus all ollama-custom

# Using compose
docker-compose -f docker-compose-ollama-with-open-webui.yml up -d

# Access container
docker exec -it ollama bash

# Cleanup
docker-compose -f docker-compose-ollama-with-open-webui.yml down -v --rmi all
```

## 🤖 Ollama CLI Basics

```bash
# Model management
ollama pull llama3:8b          # Download model
ollama list                    # Show installed models
ollama run llama3:8b           # Start chat
ollama stop llama3:8b          # Stop model

# Model operations
ollama push my-model           # Share custom model
ollama create my-model -f Modelfile  # Create from Modelfile
```

## 🔌 API Access
- Base URL: `http://localhost:11434`
```bash
# Check API status
curl http://localhost:11434

# List models (API)
curl http://localhost:11434/api/tags

# Chat example
curl http://localhost:11434/api/chat -d '{
  "model": "llama3:8b",
  "messages": [{ "role": "user", "content": "Hello" }]
}'
```

## ⚙️ Recommended Models
```bash
ollama pull llama3:8b-instruct   # Best overall
ollama pull starcoder2:3b        # For coding
ollama pull mistral:7b           # Lightweight
```

## 🚨 Troubleshooting
```bash
# GPU check
docker exec ollama nvidia-smi

# Logs
docker logs ollama

# Reset
docker stop ollama && docker rm ollama
docker volume rm ollama_data
```

## 💡 Pro Tips
1. Use `-v ollama_data:/root/.ollama` to persist models
2. Add `--gpus all` for GPU acceleration
3. Set `OLLAMA_KEEP_ALIVE=30m` to prevent unloading