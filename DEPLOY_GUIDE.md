# Hướng Dẫn Deploy UME Pet Salon Lên VPS

## Mục lục
1. [Thuê VPS](#1-thuê-vps)
2. [Setup Server](#2-setup-server)
3. [Cài đặt MongoDB](#3-cài-đặt-mongodb)
4. [Clone và Cấu hình Project](#4-clone-và-cấu-hình-project)
5. [Build Frontend](#5-build-frontend)
6. [Chạy Backend với PM2](#6-chạy-backend-với-pm2)
7. [Cấu hình Nginx](#7-cấu-hình-nginx)
8. [Cài SSL (HTTPS)](#8-cài-ssl-https)
9. [Cấu hình Google OAuth cho domain mới](#9-cấu-hình-google-oauth)
10. [Bảo trì & Cập nhật](#10-bảo-trì--cập-nhật)

---

## 1. Thuê VPS

### Nhà cung cấp đề xuất:
| Nhà cung cấp | Giá | Cấu hình |
|---|---|---|
| **Vultr** | $5/tháng (~125k VNĐ) | 1 CPU, 1GB RAM, 25GB SSD |
| **DigitalOcean** | $6/tháng | 1 CPU, 1GB RAM, 25GB SSD |
| **Linode** | $5/tháng | 1 CPU, 1GB RAM, 25GB SSD |
| **Aeza** | $3/tháng | 1 CPU, 1GB RAM, 10GB SSD |

> **Chọn hệ điều hành: Ubuntu 22.04 LTS**

### Sau khi thuê VPS:
- Bạn sẽ nhận được **IP** và **mật khẩu root**
- Dùng terminal SSH vào: `ssh root@YOUR_IP`

---

## 2. Setup Server

### SSH vào VPS:
```bash
ssh root@YOUR_IP_ADDRESS
```

### Cập nhật hệ thống:
```bash
apt update && apt upgrade -y
```

### Tạo user mới (không dùng root):
```bash
adduser ume
usermod -aG sudo ume
su - ume
```

### Cài Node.js 20:
```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
node -v  # Kiểm tra version
npm -v
```

### Cài các công cụ cần thiết:
```bash
sudo apt install -y git nginx certbot python3-certbot-nginx
sudo npm install -g pm2
```

---

## 3. Cài đặt MongoDB

```bash
# Import GPG key
curl -fsSL https://www.mongodb.org/static/pgp/server-7.0.asc | \
  sudo gpg -o /usr/share/keyrings/mongodb-server-7.0.gpg --dearmor

# Thêm repo
echo "deb [ arch=amd64,arm64 signed-by=/usr/share/keyrings/mongodb-server-7.0.gpg ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/7.0 multiverse" | \
  sudo tee /etc/apt/sources.list.d/mongodb-org-7.0.list

# Cài đặt
sudo apt update
sudo apt install -y mongodb-org

# Khởi động MongoDB
sudo systemctl start mongod
sudo systemctl enable mongod
sudo systemctl status mongod  # Kiểm tra đang chạy
```

---

## 4. Clone và Cấu hình Project

### Clone project:
```bash
sudo mkdir -p /var/www/ume-pet-salon
sudo chown ume:ume /var/www/ume-pet-salon
cd /var/www/ume-pet-salon

# Clone từ GitHub (hoặc copy từ máy local)
git clone https://github.com/YOUR_USERNAME/UngdungChoThuCung.git .
```

### Hoặc Upload từ máy local (nếu chưa có GitHub):
```bash
# Chạy từ máy Windows local:
scp -r C:\UngdungChoThuCung\* ume@YOUR_IP:/var/www/ume-pet-salon/
```

### Cấu hình Backend:
```bash
cd /var/www/ume-pet-salon/ume-backend

# Tạo file .env
cp .env.example .env
nano .env
```

**Sửa file `.env` như sau:**
```
PORT=5000
NODE_ENV=production
MONGODB_URI=mongodb://localhost:27017/ume_pet_salon

JWT_SECRET=YOUR_STRONG_SECRET_CHANGE_THIS
JWT_REFRESH_SECRET=YOUR_STRONG_REFRESH_SECRET_CHANGE_THIS
JWT_EXPIRES_IN=1d
JWT_REFRESH_EXPIRES_IN=7d

GOOGLE_CLIENT_ID=your_google_client_id_here
GOOGLE_CLIENT_SECRET=your_google_client_secret_here

FRONTEND_URL=https://yourdomain.com
UPLOAD_DIR=uploads
```

> ⚠️ **Quan trọng:** Thay `yourdomain.com` bằng domain thật của bạn. Nếu chưa có domain, dùng IP: `http://YOUR_IP`

### Cài dependencies:
```bash
npm install --production
```

### Tạo thư mục uploads:
```bash
mkdir -p uploads/{pets,products,services,brands,categories,general,settings,staff}
```

### Seed dữ liệu (nếu cần):
```bash
npm run seed
```

---

## 5. Build Frontend

```bash
cd /var/www/ume-pet-salon/ume-frontend
npm install
npx ng build --configuration=production
```

> Build xong sẽ tạo thư mục `dist/ume-frontend/browser/` chứa các file tĩnh.

---

## 6. Chạy Backend với PM2

```bash
cd /var/www/ume-pet-salon

# Tạo thư mục logs
mkdir -p logs

# Khởi động với PM2
pm2 start ecosystem.config.js

# Kiểm tra trạng thái
pm2 status
pm2 logs ume-pet-salon

# Lưu để tự khởi động khi restart VPS
pm2 save
pm2 startup
# Chạy lệnh mà PM2 in ra (sudo env PATH=...)
```

### Test thử:
```bash
curl http://localhost:5000/api/health
# Kết quả: {"status":"OK","timestamp":"..."}
```

---

## 7. Cấu hình Nginx

### Copy config Nginx:
```bash
sudo cp /var/www/ume-pet-salon/nginx.conf /etc/nginx/sites-available/ume-pet-salon
```

### Sửa domain trong config:
```bash
sudo nano /etc/nginx/sites-available/ume-pet-salon
# Thay tất cả "yourdomain.com" bằng domain thật
```

### Nếu CHƯA CÓ DOMAIN (dùng IP):
```bash
sudo nano /etc/nginx/sites-available/ume-pet-salon
```

Thay nội dung bằng config đơn giản hơn:
```nginx
server {
    listen 80;
    server_name YOUR_IP_ADDRESS;

    client_max_body_size 10M;

    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml text/javascript image/svg+xml;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    location /socket.io/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
    }
}
```

### Kích hoạt config:
```bash
# Xóa default site
sudo rm -f /etc/nginx/sites-enabled/default

# Tạo symlink
sudo ln -s /etc/nginx/sites-available/ume-pet-salon /etc/nginx/sites-enabled/

# Test config
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
sudo systemctl enable nginx
```

### Mở firewall:
```bash
sudo ufw allow 22    # SSH
sudo ufw allow 80    # HTTP
sudo ufw allow 443   # HTTPS
sudo ufw enable
```

### Test:
Mở trình duyệt vào `http://YOUR_IP` - website sẽ hiện!

---

## 8. Cài SSL (HTTPS)

> ⚠️ **Cần có domain trỏ về IP VPS trước khi làm bước này**

### Trỏ domain:
1. Vào nhà cung cấp domain (Namecheap, GoDaddy, Tenten...)
2. Thêm bản ghi DNS:
   - **A Record**: `@` → `YOUR_IP`
   - **A Record**: `www` → `YOUR_IP`

### Cài SSL miễn phí với Let's Encrypt:
```bash
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
```

Certbot sẽ tự động cấu hình SSL trong Nginx. Done!

### Tự động gia hạn SSL:
```bash
sudo certbot renew --dry-run  # Test thử
# Certbot tự thêm cronjob để gia hạn
```

---

## 9. Cấu hình Google OAuth

Khi deploy lên domain mới, cần cập nhật Google OAuth:

1. Vào [Google Cloud Console](https://console.cloud.google.com/)
2. Chọn project → **APIs & Services** → **Credentials**
3. Chọn OAuth Client ID đang dùng
4. Thêm vào **Authorized JavaScript origins**:
   - `https://yourdomain.com`
   - `http://YOUR_IP` (nếu dùng IP)
5. Thêm vào **Authorized redirect URIs**:
   - `https://yourdomain.com/api/auth/google/callback`
6. Lưu lại

---

## 10. Bảo trì & Cập nhật

### Cập nhật code mới:
```bash
cd /var/www/ume-pet-salon
chmod +x deploy.sh
./deploy.sh
```

### Xem logs:
```bash
pm2 logs ume-pet-salon        # Xem logs realtime
pm2 logs ume-pet-salon --lines 100  # Xem 100 dòng gần nhất
```

### Restart app:
```bash
pm2 restart ume-pet-salon
```

### Kiểm tra trạng thái:
```bash
pm2 status           # Xem app đang chạy
pm2 monit            # Monitor CPU/RAM
sudo systemctl status nginx    # Xem Nginx
sudo systemctl status mongod   # Xem MongoDB
```

### Backup database:
```bash
# Backup
mongodump --db ume_pet_salon --out /backup/$(date +%Y%m%d)

# Restore
mongorestore --db ume_pet_salon /backup/20250101/ume_pet_salon/
```

### Cronjob backup tự động (mỗi ngày lúc 2h sáng):
```bash
crontab -e
# Thêm dòng:
0 2 * * * mongodump --db ume_pet_salon --out /backup/$(date +\%Y\%m\%d) && find /backup -mtime +7 -delete
```

---

## Tóm tắt các lệnh quan trọng

| Hành động | Lệnh |
|---|---|
| SSH vào server | `ssh ume@YOUR_IP` |
| Xem app status | `pm2 status` |
| Restart app | `pm2 restart ume-pet-salon` |
| Xem logs | `pm2 logs ume-pet-salon` |
| Deploy code mới | `./deploy.sh` |
| Restart Nginx | `sudo systemctl restart nginx` |
| Backup DB | `mongodump --db ume_pet_salon --out /backup/` |

---

## Chi phí ước tính

| Dịch vụ | Chi phí |
|---|---|
| VPS (Vultr/DO) | ~$5/tháng (~125k VNĐ) |
| Domain .com | ~$10/năm (~250k VNĐ) |
| SSL | Miễn phí (Let's Encrypt) |
| **Tổng** | **~$5/tháng + $10/năm** |
