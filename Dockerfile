# WOS2.3_V2 Naval MMO - Edgegap Server Dockerfile
# Optimized for Unity Linux headless server builds
# Based on Ubuntu 22.04 LTS with minimal dependencies

FROM ubuntu:22.04

# Build arguments
ARG DEBIAN_FRONTEND=noninteractive
ARG SERVER_BUILD_PATH=Builds/EdgegapServer
ARG SERVER_PORT=7777

# Metadata
LABEL maintainer="WOS2.3 Development Team"
LABEL description="WOS2.3 Naval MMO Dedicated Server"
LABEL version="0.3.0"

# Copy Unity server build to container
COPY ${SERVER_BUILD_PATH} /root/build/

# Set working directory
WORKDIR /root/

# Make server executable runnable
RUN chmod +x /root/build/ServerBuild

# Install required system dependencies
# - ca-certificates: SSL/TLS support for HTTPS connections
# - libgdiplus: For Unity System.Drawing support (if needed)
RUN apt-get update && \
    apt-get install -y \
        ca-certificates \
        libgdiplus \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* \
    && update-ca-certificates

# Expose server port (default: 7777 for Mirror Telepathy)
EXPOSE ${SERVER_PORT}

# Health check (optional but recommended)
# Verifies server process is running
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD pgrep -f "ServerBuild" || exit 1

# Environment variables for runtime configuration
# These can be overridden in Edgegap deployment settings
ENV SERVER_PORT=${SERVER_PORT}
ENV MAX_PLAYERS=300
ENV SERVER_NAME="WOS2.3 Server"
ENV SERVER_REGION="auto"

# Server startup command
# Unity headless flags:
# -batchmode: Run without user interaction
# -nographics: Don't initialize graphics device (headless mode)
# $UNITY_COMMANDLINE_ARGS: Additional arguments from Edgegap
#
# Additional flags from ServerLauncher:
# -server: Force server mode
# -port: Override server port
# -maxplayers: Override max connections
# -scene: Starting scene name
# -verbose: Enable verbose logging
CMD ["/bin/bash", "-c", "\
    echo '═══════════════════════════════════════════'; \
    echo '🌊 WOS2.3 Naval MMO - Dedicated Server'; \
    echo '═══════════════════════════════════════════'; \
    echo 'Server Name: ${SERVER_NAME}'; \
    echo 'Port: ${SERVER_PORT}'; \
    echo 'Max Players: ${MAX_PLAYERS}'; \
    echo 'Region: ${SERVER_REGION}'; \
    echo '═══════════════════════════════════════════'; \
    echo 'Starting server...'; \
    echo ''; \
    /root/build/ServerBuild -batchmode -nographics -server -port ${SERVER_PORT} -maxplayers ${MAX_PLAYERS} $UNITY_COMMANDLINE_ARGS \
"]

# Notes:
# 1. Build this image from project root directory
# 2. Ensure Builds/EdgegapServer/ contains ServerBuild executable
# 3. Edgegap plugin automatically handles Docker build and deployment
# 4. For manual testing: docker build -t wos23-server:latest .
# 5. For manual run: docker run -p 7777:7777 wos23-server:latest
