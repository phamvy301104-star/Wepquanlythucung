#!/bin/bash
# =============================================
# UME Pet Salon - VPS Auto Setup Script
# Chạy trên Ubuntu 22.04 với quyền root
# =============================================

set -e
export DEBIAN_FRONTEND=noninteractive

echo "=========================================="
echo "  UME Pet Salon - VPS Setup"
echo "  $(date)"
echo "=========================================="

# ---- 1. Cập nhật hệ thống ----
echo ""
echo "[1/8] Cập nhật hệ thống..."
apt update && apt upgrade -y

# ---- 2. Cài Node.js 20 ----
echo ""
echo "[2/8] Cài đặt Node.js 20..."
curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
apt install -y nodejs
echo "Node.js version: $(node -v)"
echo "NPM version: $(npm -v)"

# ---- 3. Cài MongoDB 7 ----
echo ""
echo "[3/8] Cài đặt MongoDB 7..."
curl -fsSL https://www.mongodb.org/static/pgp/server-7.0.asc | \
  gpg -o /usr/share/keyrings/mongodb-server-7.0.gpg --dearmor

echo "deb [ arch=amd64,arm64 signed-by=/usr/share/keyrings/mongodb-server-7.0.gpg ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/7.0 multiverse" | \
  tee /etc/apt/sources.list.d/mongodb-org-7.0.list

apt update
apt install -y mongodb-org
systemctl start mongod
systemctl enable mongod
echo "MongoDB status: $(systemctl is-active mongod)"

# ---- 4. Cài Nginx + PM2 + Git ----
echo ""
echo "[4/8] Cài đặt Nginx, PM2, Git..."
apt install -y nginx git
npm install -g pm2
systemctl enable nginx

# ---- 5. Clone project ----
echo ""
echo "[5/8] Clone project từ GitHub..."
mkdir -p /var/www/ume-pet-salon
cd /var/www/ume-pet-salon

if [ -d ".git" ]; then
  echo "Project đã tồn tại, pull latest..."
  git pull origin main
else
  git clone https://github.com/phamvy301104-star/Wepquanlythucung.git .
fi

# ---- 6. Setup Backend ----
echo ""
echo "[6/8] Setup Backend..."
cd /var/www/ume-pet-salon/ume-backend

# Tạo thư mục uploads
mkdir -p uploads/{pets,products,services,brands,categories,general,settings,staff}

# Tạo file .env production
SERVER_IP=$(curl -s ifconfig.me)
cat > .env << 'ENVEOF'
PORT=5000
NODE_ENV=production
MONGODB_URI=mongodb://localhost:27017/ume_pet_salon

JWT_SECRET=UmePetSalonSuperSecretKey2026!@#$%^&*()
JWT_REFRESH_SECRET=UmePetSalonRefreshSecretKey2026!@#
JWT_EXPIRES_IN=1d
JWT_REFRESH_EXPIRES_IN=7d

GOOGLE_CLIENT_ID=__GOOGLE_CLIENT_ID__
GOOGLE_CLIENT_SECRET=__GOOGLE_CLIENT_SECRET__

FACEBOOK_APP_ID=YOUR_FB_APP_ID
FACEBOOK_APP_SECRET=YOUR_FB_APP_SECRET

UPLOAD_DIR=uploads
ENVEOF

# Thêm FRONTEND_URL với IP thật
echo "FRONTEND_URL=http://${SERVER_IP}" >> .env

echo "File .env đã tạo với FRONTEND_URL=http://${SERVER_IP}"

# Cài dependencies
npm install --production

# Seed dữ liệu
echo "Seeding database..."
npm run seed 2>/dev/null || echo "Seed skipped (có thể đã seed rồi)"

# ---- 7. Build Frontend ----
echo ""
echo "[7/8] Build React Frontend..."
cd /var/www/ume-pet-salon/ume-react
npm install
npm run build

echo "Frontend build xong!"

# ---- 8. Cấu hình Nginx + PM2 ----
echo ""
echo "[8/8] Cấu hình Nginx + PM2..."

# Tạo Nginx config
cat > /etc/nginx/sites-available/ume-pet-salon << NGINXEOF
server {
    listen 80;
    server_name ${SERVER_IP};

    client_max_body_size 10M;

    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml text/javascript image/svg+xml;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }

    location /socket.io/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
    }
}
NGINXEOF

# Kích hoạt config
rm -f /etc/nginx/sites-enabled/default
ln -sf /etc/nginx/sites-available/ume-pet-salon /etc/nginx/sites-enabled/
nginx -t
systemctl restart nginx

# Tạo logs directory
mkdir -p /var/www/ume-pet-salon/logs

# Khởi động PM2
cd /var/www/ume-pet-salon
pm2 stop all 2>/dev/null || true
pm2 start ecosystem.config.js
pm2 save
pm2 startup systemd -u root --hp /root 2>/dev/null || true

# Mở firewall
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
echo "y" | ufw enable 2>/dev/null || true

echo ""
echo "=========================================="
echo "  SETUP HOÀN TẤT!"
echo "=========================================="
echo ""
echo "  Website:  http://${SERVER_IP}"
echo "  API:      http://${SERVER_IP}/api/health"
echo ""
echo "  Lệnh hữu ích:"
echo "    pm2 status          - Xem trạng thái app"
echo "    pm2 logs            - Xem logs"
echo "    pm2 restart all     - Restart app"
echo ""
echo "=========================================="
