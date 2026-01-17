#!/bin/bash

# Configure NVIDIA runtime (optional, if needed inside the container)
if command -v nvidia-ctk &> /dev/null; then
    echo "Configuring NVIDIA runtime..."
    nvidia-ctk runtime configure --runtime=docker
fi

# Check if NVIDIA GPUs are available
if command -v nvidia-smi &> /dev/null; then
    echo "NVIDIA GPUs detected:"
    nvidia-smi
else
    echo "No NVIDIA GPUs detected. Exiting..."
    exit 1
fi

# Start the Ollama service
ollama serve &

# Wait for the service to initialize
sleep 5

# # Pull the required model
# ollama pull llama3.2:3b

# Keep the container running
wait