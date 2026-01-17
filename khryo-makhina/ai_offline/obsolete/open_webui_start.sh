#!/bin/bash

# Print a message to indicate the service is starting
echo "Starting Open WebUI service..."

# Run the renamed application script
python3 open_webui.py

# Keep the container running
wait