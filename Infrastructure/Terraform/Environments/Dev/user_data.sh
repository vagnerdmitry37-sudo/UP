#!/bin/bash

set -e

apt-get update
apt-get install -y docker.io docker-compose-v2

systemctl enable docker
systemctl start docker

usermod -aG docker ubuntu

mkdir -p /opt/up