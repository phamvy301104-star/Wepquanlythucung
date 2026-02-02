# UME App - Flutter Frontend

UME Salon Booking App với tích hợp AI Features (Face Analysis, Hair Try-On, AI Chatbot)

## 🚨 LỖI CHATBOT? ĐÃ FIX!

**✅ FIXED (2026-01-04):** Model cũ `gemini-1.5-flash` deprecated → Updated sang `gemini-2.5-flash`

**Triệu chứng cũ:** 
```
models/gemini-1.5-flash is not found for API version v1beta
```

**Giải pháp:**  
Chỉ cần **restart app** (Shift + F5 → F5) - Model mới **VẪN MIỄN PHÍ 100%**!

**Xem chi tiết:**
- 💰 **[GEMINI_MODELS_PRICING.md](GEMINI_MODELS_PRICING.md)** - Models & Pricing (Free vs Paid)
- 🚨 **[FIX_CHATBOT_API_KEY.md](FIX_CHATBOT_API_KEY.md)** - Fix lỗi API key
- 🐛 **[DEBUG_GUIDE.md](DEBUG_GUIDE.md)** - Hướng dẫn debug

**Test API key:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
.\test_gemini_api.ps1
```

---

## 📚 Tài Liệu

- **[DOCS.md](DOCS.md)** - Tổng hợp tất cả hướng dẫn debug & fix lỗi
- **[FIX_CHATBOT_API_KEY.md](FIX_CHATBOT_API_KEY.md)** - Fix lỗi chatbot trong 5 phút
- **[DEBUG_GUIDE.md](DEBUG_GUIDE.md)** - Hướng dẫn debug chi tiết, xem logs
- **[test_gemini_api.ps1](test_gemini_api.ps1)** - Script test API key

---

## 🚀 Getting Started

### Prerequisites
- Flutter SDK
- Android Studio / Xcode
- .NET 8.0 (cho backend)

### Setup

1. **Clone repository**
2. **Cấu hình .env:**
   ```env
   API_BASE_URL=https://your-ngrok-url.ngrok-free.dev
   API_URL=https://your-ngrok-url.ngrok-free.dev/api
   GEMINI_API_KEY=AIzaSy_YOUR_KEY_HERE
   ```

3. **Install dependencies:**
   ```bash
   flutter pub get
   ```

4. **Run app:**
   ```bash
   flutter run
   ```

### Xem Logs
- VS Code: `Ctrl + Shift + Y` (Debug Console)
- Tìm `[ChatbotService]` để xem logs chatbot

---

## 🎯 Features

- ✅ Face Analysis với ML Kit
- ✅ Hair Try-On với HuggingFace API
- ✅ AI Chatbot với Gemini 1.5 Flash
- ✅ Booking Management
- ✅ Product Catalog
- ✅ AI History với backend persistence

---

## 🐛 Troubleshooting

| Vấn đề | Solution |
|--------|----------|
| Chatbot lỗi API key | [FIX_CHATBOT_API_KEY.md](FIX_CHATBOT_API_KEY.md) |
| Cần xem logs | [DEBUG_GUIDE.md](DEBUG_GUIDE.md) |
| Test API key | `.\test_gemini_api.ps1` |
| Lỗi khác | [DOCS.md](DOCS.md) |

---

## 📖 Flutter Resources

- [Lab: Write your first Flutter app](https://docs.flutter.dev/get-started/codelab)
- [Cookbook: Useful Flutter samples](https://docs.flutter.dev/cookbook)
- [Flutter Documentation](https://docs.flutter.dev/)

