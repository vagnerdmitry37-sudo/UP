#!/bin/bash

set -euxo pipefail

# Log everything from this script
exec > >(tee /var/log/up-bootstrap.log | logger -t up-bootstrap -s 2>/dev/console) 2>&1

echo "===== UP bootstrap started ====="
date

# Install Docker and Docker Compose
apt-get update
apt-get install -y docker.io docker-compose-v2

# Start Docker
systemctl enable docker
systemctl start docker

echo "Waiting for Docker..."

until docker info >/dev/null 2>&1; do
    sleep 2
done

echo "Docker is ready"

# Prepare application directory
mkdir -p /opt/up

# Create docker-compose.yml
cat > /opt/up/docker-compose.yml <<'EOF'
${docker_compose}
EOF

cd /opt/up

echo "===== Docker versions ====="
docker --version
docker compose version

echo "===== Compose configuration ====="
docker compose config

echo "===== Starting containers ====="

# Start containers
docker compose up -d

echo "===== Containers started ====="

# Give Docker a moment to update container state
sleep 2

echo "===== Container status ====="
docker compose ps

echo "===== Waiting for PostgreSQL ====="

# Wait until PostgreSQL becomes healthy
for i in {1..30}; do

    if docker inspect up-postgres-1 \
        --format='{{if .State.Health}}{{.State.Health.Status}}{{else}}no-healthcheck{{end}}' \
        2>/dev/null | grep -q "healthy"; then

        echo "PostgreSQL is healthy"
        break
    fi

    echo "PostgreSQL is not healthy yet. Attempt $i/30"

    docker compose ps

    sleep 2

done

echo "===== Final container status ====="
docker compose ps

echo "===== Recent container logs ====="
docker compose logs --tail=100

echo "===== UP bootstrap finished ====="
date