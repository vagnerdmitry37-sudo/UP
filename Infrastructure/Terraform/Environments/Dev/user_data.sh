#!/bin/bash

set -e

apt-get update
apt-get install -y docker.io docker-compose-v2

systemctl enable docker
systemctl start docker

echo "Waiting for Docker..."

until docker info >/dev/null 2>&1; do
    sleep 2
done

echo "Docker is ready"

usermod -aG docker ubuntu

mkdir -p /opt/up

cat > /opt/up/docker-compose.yml <<'EOF'
${docker_compose}
EOF

cd /opt/up

docker compose up -d