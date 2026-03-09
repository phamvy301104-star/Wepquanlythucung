# UME Pet Salon

Hệ thống quản lý và đặt lịch dịch vụ thú cưng.

## Công nghệ

- **Frontend**: React 19 + TypeScript + Vite
- **Backend**: Node.js + Express + MongoDB
- **AI Chatbot**: Groq API (Llama 3.3 70B)
- **AI Pet Detection**: YOLO26n + EfficientNet (ONNX)

## Cấu trúc

```
ume-react/     # Frontend React
ume-backend/   # Backend API
```

## Chạy local

```bash
# Backend
cd ume-backend
npm install
npm run dev

# Frontend
cd ume-react
npm install
npm run dev
```