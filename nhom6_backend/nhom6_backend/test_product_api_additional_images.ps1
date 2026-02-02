# Test Product API - Additional Images
# Script kiểm tra API có trả về AdditionalImages không

Write-Host "=== TEST PRODUCT API - ADDITIONAL IMAGES ===" -ForegroundColor Cyan
Write-Host ""

# Backend URL - thay đổi nếu cần
$baseUrl = "https://localhost:7079"

# Test 1: Get all products
Write-Host "1. Testing GET /api/ProductApi (All Products)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/ProductApi?page=1&pageSize=5" `
        -Method Get `
        -SkipCertificateCheck `
        -ErrorAction Stop

    Write-Host "✓ Status: SUCCESS" -ForegroundColor Green
    Write-Host "Total Items: $($response.totalItems)"
    Write-Host "Products returned: $($response.items.Count)"
    
    if ($response.items.Count -gt 0) {
        $firstProduct = $response.items[0]
        Write-Host "`nFirst Product:"
        Write-Host "  ID: $($firstProduct.id)"
        Write-Host "  Name: $($firstProduct.name)"
        Write-Host "  ImageUrl: $($firstProduct.imageUrl)"
        Write-Host "  AdditionalImages: $($firstProduct.additionalImages)"
        
        if ($firstProduct.additionalImages) {
            Write-Host "  ✓ AdditionalImages có dữ liệu" -ForegroundColor Green
        } else {
            Write-Host "  ✗ AdditionalImages NULL hoặc empty" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n---`n"

# Test 2: Get product by ID
Write-Host "2. Testing GET /api/ProductApi/{id}" -ForegroundColor Yellow
$testProductId = 1  # Thay ID này nếu cần

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/ProductApi/$testProductId" `
        -Method Get `
        -SkipCertificateCheck `
        -ErrorAction Stop

    Write-Host "✓ Status: SUCCESS" -ForegroundColor Green
    Write-Host "Product Details:"
    Write-Host "  ID: $($response.id)"
    Write-Host "  Name: $($response.name)"
    Write-Host "  SKU: $($response.sku)"
    Write-Host "  ImageUrl: $($response.imageUrl)"
    Write-Host "  AdditionalImages: $($response.additionalImages)"
    
    if ($response.additionalImages) {
        Write-Host "`n  ✓ AdditionalImages có dữ liệu!" -ForegroundColor Green
        
        # Parse JSON nếu là string
        if ($response.additionalImages -is [string]) {
            try {
                $images = $response.additionalImages | ConvertFrom-Json
                Write-Host "  Số lượng ảnh phụ: $($images.Count)"
                Write-Host "  Danh sách ảnh phụ:"
                $images | ForEach-Object { Write-Host "    - $_" }
            } catch {
                Write-Host "  Không parse được JSON: $($response.additionalImages)"
            }
        }
    } else {
        Write-Host "`n  ✗ AdditionalImages NULL hoặc empty" -ForegroundColor Red
        Write-Host "  Hãy dùng admin panel để thêm ảnh phụ cho sản phẩm này!"
    }
} catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n---`n"

# Test 3: Check multiple products
Write-Host "3. Checking AdditionalImages in first 10 products" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/ProductApi?page=1&pageSize=10" `
        -Method Get `
        -SkipCertificateCheck `
        -ErrorAction Stop

    $withImages = 0
    $withoutImages = 0
    
    foreach ($product in $response.items) {
        if ($product.additionalImages) {
            $withImages++
            Write-Host "  ✓ ID $($product.id): $($product.name) - CÓ ảnh phụ" -ForegroundColor Green
        } else {
            $withoutImages++
            Write-Host "  ✗ ID $($product.id): $($product.name) - KHÔNG có ảnh phụ" -ForegroundColor Gray
        }
    }
    
    Write-Host "`nSummary:"
    Write-Host "  Có ảnh phụ: $withImages" -ForegroundColor Green
    Write-Host "  Không có: $withoutImages" -ForegroundColor Gray
    
    if ($withoutImages -gt 0) {
        Write-Host "`n  → Hãy vào admin panel thêm ảnh phụ cho các sản phẩm!" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Cyan
