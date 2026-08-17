#!/bin/bash

set -e

apt-get update
apt-get install -y docker.io docker-compose-v2

systemctl enable docker
systemctl start docker

usermod -aG docker ubuntu

mkdir -p /opt/up

cat > /opt/up/docker-compose.yml <<'EOF'
${docker_compose}
EOF

if [ ! -f /opt/up/.env ]; then
    echo "Creating initial environment..."

    cat > /opt/up/.env <<EOF
ASPNETCORE_ENVIRONMENT=Production
POSTGRES_PASSWORD=$(openssl rand -hex 32)
JWT_KEY=$(openssl rand -hex 64)
UP_API_IMAGE_ADDRESS=${up_api_image_address}
EOF

    chmod 600 /opt/up/.env
else
    echo "Using existing /opt/up/.env"
fi

cd /opt/up

docker compose up -d