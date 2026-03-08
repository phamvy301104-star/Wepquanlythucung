#!/bin/bash
# =============================================
# UME Pet Salon - Deploy Script
# Chạy trên VPS sau khi đã setup xong
# =============================================

set -e

APP_DIR="/var/www/ume-pet-salon"
BRANCH="main"

echo "=========================================="
echo "  UME Pet Salon - Deploy"
echo "=========================================="

# Pull latest code
echo "[1/5] Pulling latest code..."
cd $APP_DIR
git pull origin $BRANCH

# Install backend dependencies
echo "[2/5] Installing backend dependencies..."
cd $APP_DIR/ume-backend
npm install --production

# Build frontend
echo "[3/5] Building React frontend..."
cd $APP_DIR/ume-react
npm install
npm run build

# Create logs directory
echo "[4/5] Setting up logs..."
mkdir -p $APP_DIR/logs

# Restart PM2
echo "[5/5] Restarting application..."
cd $APP_DIR
pm2 restart ecosystem.config.js --update-env || pm2 start ecosystem.config.js

echo "=========================================="
echo "  Deploy completed successfully!"
echo "  Check: pm2 status"
echo "  Logs:  pm2 logs ume-pet-salon"
echo "=========================================="
