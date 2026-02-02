# Test Gemini API Key
# Usage: .\test_gemini_api.ps1 [YOUR_API_KEY]

param(
    [string]$ApiKey = ""
)

# Load API key from .env if not provided
if ($ApiKey -eq "") {
    Write-Host "📄 Đang đọc API key từ .env file..." -ForegroundColor Yellow
    
    if (Test-Path ".env") {
        $envContent = Get-Content ".env"
        foreach ($line in $envContent) {
            if ($line -match "GEMINI_API_KEY=(.+)") {
                $ApiKey = $matches[1].Trim()
                break
            }
        }
    }
    
    if ($ApiKey -eq "") {
        Write-Host "❌ Không tìm thấy GEMINI_API_KEY trong .env!" -ForegroundColor Red
        Write-Host "💡 Cách dùng: .\test_gemini_api.ps1 YOUR_API_KEY" -ForegroundColor Cyan
        exit 1
    }
}

# Mask API key for display
$maskedKey = if ($ApiKey.Length -gt 12) {
    $ApiKey.Substring(0, 8) + "..." + $ApiKey.Substring($ApiKey.Length - 4)
} else {
    $ApiKey.Substring(0, 4) + "..."
}

Write-Host "`n╔════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   🧪 TEST GEMINI API KEY              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor Cyan

Write-Host "🔑 API Key: $maskedKey (length: $($ApiKey.Length))" -ForegroundColor Yellow

# Prepare request
$headers = @{
    "Content-Type" = "application/json"
}

$body = @{
    contents = @(
        @{
            parts = @(
                @{
                    text = "Chào bạn! Trả lời ngắn gọn bằng tiếng Việt."
                }
            )
        }
    )
} | ConvertTo-Json -Depth 10

$url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=$ApiKey"

Write-Host "`n📡 Đang gửi request đến Gemini API..." -ForegroundColor Cyan
Write-Host "   Model: gemini-2.5-flash (MỚI NHẤT, MIỄN PHÍ)"
Write-Host "   Endpoint: generativelanguage.googleapis.com`n"

try {
    $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body -ErrorAction Stop
    
    Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║   ✅ API KEY HOẠT ĐỘNG TỐT!          ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor Green
    
    $aiResponse = $response.candidates[0].content.parts[0].text
    Write-Host "🤖 Response từ Gemini:" -ForegroundColor Green
    Write-Host "   $aiResponse`n"
    
    Write-Host "📊 Thông tin thêm:" -ForegroundColor Cyan
    Write-Host "   - Candidate Count: $($response.candidates.Count)"
    Write-Host "   - Finish Reason: $($response.candidates[0].finishReason)"
    if ($response.usageMetadata) {
        Write-Host "   - Prompt Tokens: $($response.usageMetadata.promptTokenCount)"
        Write-Host "   - Response Tokens: $($response.usageMetadata.candidatesTokenCount)"
        Write-Host "   - Total Tokens: $($response.usageMetadata.totalTokenCount)"
    }
    
    Write-Host "`n✅ API key đã sẵn sàng sử dụng trong Flutter app!" -ForegroundColor Green
    
} catch {
    Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║   ❌ API KEY KHÔNG HOẠT ĐỘNG!        ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════╝`n" -ForegroundColor Red
    
    $errorMessage = $_.Exception.Message
    Write-Host "❌ Error: $errorMessage`n" -ForegroundColor Red
    
    # Parse error details
    if ($errorMessage -like "*400*") {
        Write-Host "🔍 Chi tiết lỗi: BAD REQUEST (400)" -ForegroundColor Yellow
        Write-Host "   - API key format có thể sai"
        Write-Host "   - Hoặc request body không đúng`n"
    }
    elseif ($errorMessage -like "*401*" -or $errorMessage -like "*403*") {
        Write-Host "🔍 Chi tiết lỗi: UNAUTHORIZED (401/403)" -ForegroundColor Yellow
        Write-Host "   - API key không hợp lệ hoặc đã hết hạn"
        Write-Host "   - Hoặc chưa enable Generative Language API`n"
    }
    elseif ($errorMessage -like "*404*") {
        Write-Host "🔍 Chi tiết lỗi: NOT FOUND (404)" -ForegroundColor Yellow
        Write-Host "   - Model hoặc endpoint không tồn tại`n"
    }
    elseif ($errorMessage -like "*429*") {
        Write-Host "🔍 Chi tiết lỗi: TOO MANY REQUESTS (429)" -ForegroundColor Yellow
        Write-Host "   - Vượt quá rate limit (15 requests/min)`n"
    }
    
    Write-Host "💡 HƯỚNG DẪN FIX:" -ForegroundColor Cyan
    Write-Host "`n1️⃣  Truy cập Google AI Studio:"
    Write-Host "   👉 https://aistudio.google.com/app/apikey`n"
    
    Write-Host "2️⃣  Tạo API Key mới:"
    Write-Host "   - Click [Create API Key]"
    Write-Host "   - Chọn project (hoặc tạo mới)"
    Write-Host "   - Copy API key (dạng: AIzaSy...)`n"
    
    Write-Host "3️⃣  Enable Generative Language API:"
    Write-Host "   👉 https://console.cloud.google.com/apis/library/generativelanguage.googleapis.com"
    Write-Host "   - Click [ENABLE]`n"
    
    Write-Host "4️⃣  Cập nhật file .env:"
    Write-Host "   GEMINI_API_KEY=AIzaSy_YOUR_NEW_KEY_HERE`n"
    
    Write-Host "5️⃣  Chạy lại script này để test:"
    Write-Host "   .\test_gemini_api.ps1`n"
    
    exit 1
}

Write-Host "`n╔════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║   📱 SẴN SÀNG CHẠY FLUTTER APP        ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host "`nLệnh chạy app:"
Write-Host "   flutter run`n" -ForegroundColor Yellow

Write-Host "Để xem logs trong app:"
Write-Host "   1. Mở Debug Console (Ctrl + Shift + Y)"
Write-Host "   2. Tìm logs [ChatbotService]"
Write-Host "   3. Xem chi tiết DEBUG_GUIDE.md`n" -ForegroundColor Gray
