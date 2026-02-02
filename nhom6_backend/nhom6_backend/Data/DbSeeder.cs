using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nhom6_backend.Models;
using nhom6_backend.Models.Entities;

namespace nhom6_backend.Data
{
    /// <summary>
    /// Seed data cho database UME Salon
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager);

            // Seed Categories
            await SeedCategoriesAsync(context);
            await context.SaveChangesAsync();

            // Seed Brands
            await SeedBrandsAsync(context);
            await context.SaveChangesAsync();

            // Seed Service Categories
            // await SeedServiceCategoriesAsync(context);

            // Seed Services
            // await SeedServicesAsync(context);
            
            // Seed Products (NEW)
            await SeedProductsAsync(context);
            await context.SaveChangesAsync();
            
            // Seed Staff (NEW)
            await SeedStaffAsync(context);
            await context.SaveChangesAsync();

            // Seed Payment Methods - Skip for now (table may not exist)
            // await SeedPaymentMethodsAsync(context);

            // Seed Shipping Methods - Skip for now (table may not exist)
            // await SeedShippingMethodsAsync(context);

            // Seed Store Info - Skip for now (table may not exist)
            // await SeedStoreInfoAsync(context);

            // Seed Banners - Skip for now (table may not exist)
            // await SeedBannersAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Staff", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<User> userManager)
        {
            var adminEmail = "admin@ume.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "UME Administrator",
                    Initials = "AD",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            // Kiểm tra xem đã có categories từ seeder chưa
            var existingSlugs = new[] { "dau-goi", "dau-xa", "sap-toc" };
            if (await context.Categories.AnyAsync(c => existingSlugs.Contains(c.Slug)))
            {
                return; // Đã seed rồi
            }

            var categories = new List<Category>
            {
                // Danh mục sản phẩm chính
                new Category
                {
                    Name = "Dầu Gội",
                    Slug = "dau-goi",
                    Description = "Các loại dầu gội đầu cho nam",
                    Icon = "💆",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Dầu Xả",
                    Slug = "dau-xa",
                    Description = "Các loại dầu xả, dưỡng tóc",
                    Icon = "✨",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Keo Vuốt Tóc",
                    Slug = "keo-vuot-toc",
                    Description = "Wax, pomade, gel và các sản phẩm tạo kiểu tóc",
                    Icon = "💫",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Sáp Tóc",
                    Slug = "sap-toc",
                    Description = "Sáp vuốt tóc, pomade cao cấp",
                    Icon = "💎",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Gôm Tóc",
                    Slug = "gom-toc",
                    Description = "Gôm xịt, gel tạo kiểu",
                    Icon = "💨",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Thuốc Nhuộm Tóc",
                    Slug = "thuoc-nhuom-toc",
                    Description = "Thuốc nhuộm tóc các loại màu",
                    Icon = "🎨",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Dưỡng Tóc",
                    Slug = "duong-toc",
                    Description = "Serum, tinh dầu dưỡng tóc",
                    Icon = "💎",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Dụng Cụ Cắt Tóc",
                    Slug = "dung-cu-cat-toc",
                    Description = "Tông đơ, kéo, lược và phụ kiện",
                    Icon = "✂️",
                    ShowOnHomePage = false
                },
                new Category
                {
                    Name = "Phụ Kiện",
                    Slug = "phu-kien",
                    Description = "Các phụ kiện cắt tóc khác",
                    Icon = "🛠️",
                    ShowOnHomePage = false
                },
                new Category
                {
                    Name = "Chăm Sóc Da",
                    Slug = "cham-soc-da",
                    Description = "Sữa rửa mặt, kem dưỡng da nam",
                    Icon = "🧴",
                    ShowOnHomePage = true
                },
                new Category
                {
                    Name = "Nước Hoa Nam",
                    Slug = "nuoc-hoa-nam",
                    Description = "Nước hoa và xịt khử mùi",
                    Icon = "🌟",
                    ShowOnHomePage = true
                }
            };

            await context.Categories.AddRangeAsync(categories);
        }

        private static async Task SeedBrandsAsync(ApplicationDbContext context)
        {
            // Kiểm tra xem đã có brands từ seeder chưa
            var existingSlugs = new[] { "gatsby", "romano", "clear-men" };
            if (await context.Brands.AnyAsync(b => existingSlugs.Contains(b.Slug)))
            {
                return; // Đã seed rồi
            }

            var brands = new List<Brand>
            {
                new Brand
                {
                    Name = "Gatsby",
                    Slug = "gatsby",
                    Description = "Thương hiệu chăm sóc tóc hàng đầu Nhật Bản",
                    CountryOfOrigin = "Nhật Bản",
                    YearEstablished = 1978,
                    IsFeatured = true
                },
                new Brand
                {
                    Name = "Romano",
                    Slug = "romano",
                    Description = "Thương hiệu chăm sóc cá nhân dành cho nam",
                    CountryOfOrigin = "Việt Nam",
                    IsFeatured = true
                },
                new Brand
                {
                    Name = "Clear Men",
                    Slug = "clear-men",
                    Description = "Dầu gội trị gàu hàng đầu thế giới",
                    CountryOfOrigin = "Mỹ",
                    IsFeatured = true
                },
                new Brand
                {
                    Name = "Head & Shoulders",
                    Slug = "head-shoulders",
                    Description = "Dầu gội trị gàu số 1 thế giới",
                    CountryOfOrigin = "Mỹ",
                    IsFeatured = true
                },
                new Brand
                {
                    Name = "Tresemmé",
                    Slug = "tresemme",
                    Description = "Thương hiệu chăm sóc tóc chuyên nghiệp",
                    CountryOfOrigin = "Mỹ",
                    IsFeatured = false
                },
                new Brand
                {
                    Name = "L'Oreal Men Expert",
                    Slug = "loreal-men-expert",
                    Description = "Dòng sản phẩm cao cấp cho nam giới",
                    CountryOfOrigin = "Pháp",
                    IsFeatured = true
                },
                new Brand
                {
                    Name = "Dove Men+Care",
                    Slug = "dove-men-care",
                    Description = "Sản phẩm chăm sóc da và tóc cho nam",
                    CountryOfOrigin = "Mỹ",
                    IsFeatured = false
                },
                new Brand
                {
                    Name = "Reuzel",
                    Slug = "reuzel",
                    Description = "Pomade cao cấp từ Hà Lan",
                    CountryOfOrigin = "Hà Lan",
                    IsFeatured = true
                },
                new Brand
                {
                    Name = "Suavecito",
                    Slug = "suavecito",
                    Description = "Pomade phong cách vintage từ Mỹ",
                    CountryOfOrigin = "Mỹ",
                    IsFeatured = true
                },
                new Brand
                {
                    Name = "By Vilain",
                    Slug = "by-vilain",
                    Description = "Wax cao cấp từ Đan Mạch",
                    CountryOfOrigin = "Đan Mạch",
                    IsFeatured = true
                }
            };

            await context.Brands.AddRangeAsync(brands);
        }

        private static async Task SeedServiceCategoriesAsync(ApplicationDbContext context)
        {
            if (context.ServiceCategories.Any()) return;

            var serviceCategories = new List<ServiceCategory>
            {
                new ServiceCategory
                {
                    Name = "Cắt Tóc",
                    Slug = "cat-toc",
                    Description = "Dịch vụ cắt tóc nam các kiểu",
                    Icon = "✂️",
                    IsActive = true
                },
                new ServiceCategory
                {
                    Name = "Uốn Tóc",
                    Slug = "uon-toc",
                    Description = "Dịch vụ uốn tóc Hàn Quốc",
                    Icon = "🌀",
                    IsActive = true
                },
                new ServiceCategory
                {
                    Name = "Nhuộm Tóc",
                    Slug = "nhuom-toc",
                    Description = "Dịch vụ nhuộm tóc các màu",
                    Icon = "🎨",
                    IsActive = true
                },
                new ServiceCategory
                {
                    Name = "Duỗi Tóc",
                    Slug = "duoi-toc",
                    Description = "Dịch vụ duỗi tóc chuyên nghiệp",
                    Icon = "📏",
                    IsActive = true
                },
                new ServiceCategory
                {
                    Name = "Gội Đầu & Massage",
                    Slug = "goi-dau-massage",
                    Description = "Dịch vụ gội đầu và massage thư giãn",
                    Icon = "💆",
                    IsActive = true
                },
                new ServiceCategory
                {
                    Name = "Chăm Sóc Da",
                    Slug = "cham-soc-da",
                    Description = "Đắp mặt nạ, chăm sóc da mặt",
                    Icon = "🧖",
                    IsActive = true
                },
                new ServiceCategory
                {
                    Name = "Dịch Vụ Khác",
                    Slug = "dich-vu-khac",
                    Description = "Cạo mặt, lấy ráy tai và các dịch vụ khác",
                    Icon = "✨",
                    IsActive = true
                }
            };

            await context.ServiceCategories.AddRangeAsync(serviceCategories);
        }

        private static async Task SeedServicesAsync(ApplicationDbContext context)
        {
            if (context.Services.Any()) return;

            // Lấy service categories đã seed
            var catToc = context.ServiceCategories.Local.FirstOrDefault(c => c.Slug == "cat-toc");
            var uonToc = context.ServiceCategories.Local.FirstOrDefault(c => c.Slug == "uon-toc");
            var nhuomToc = context.ServiceCategories.Local.FirstOrDefault(c => c.Slug == "nhuom-toc");
            var duoiToc = context.ServiceCategories.Local.FirstOrDefault(c => c.Slug == "duoi-toc");
            var goiMassage = context.ServiceCategories.Local.FirstOrDefault(c => c.Slug == "goi-dau-massage");
            var chamSocDa = context.ServiceCategories.Local.FirstOrDefault(c => c.Slug == "cham-soc-da");
            var dichVuKhac = context.ServiceCategories.Local.FirstOrDefault(c => c.Slug == "dich-vu-khac");

            var services = new List<Service>
            {
                // Cắt tóc
                new Service
                {
                    ServiceCode = "CT001",
                    Name = "Cắt Tóc Nam Cơ Bản",
                    Slug = "cat-toc-nam-co-ban",
                    ShortDescription = "Cắt tóc nam theo yêu cầu, bao gồm gội đầu",
                    Price = 80000,
                    OriginalPrice = 100000,
                    DurationMinutes = 30,
                    Gender = "Male",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "CT002",
                    Name = "Cắt Tóc Undercut",
                    Slug = "cat-toc-undercut",
                    ShortDescription = "Kiểu tóc undercut thời thượng",
                    Price = 100000,
                    DurationMinutes = 40,
                    Gender = "Male",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "CT003",
                    Name = "Cắt Tóc Fade",
                    Slug = "cat-toc-fade",
                    ShortDescription = "Kiểu tóc fade hiện đại, cạo viền đẹp",
                    Price = 120000,
                    DurationMinutes = 45,
                    Gender = "Male",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "CT004",
                    Name = "Cắt Tóc + Cạo Mặt",
                    Slug = "cat-toc-cao-mat",
                    ShortDescription = "Combo cắt tóc và cạo mặt sạch sẽ",
                    Price = 150000,
                    DurationMinutes = 50,
                    Gender = "Male",
                    IsFeatured = false
                },

                // Uốn tóc
                new Service
                {
                    ServiceCode = "UT001",
                    Name = "Uốn Tóc Hàn Quốc",
                    Slug = "uon-toc-han-quoc",
                    ShortDescription = "Uốn xoăn nhẹ kiểu Hàn Quốc",
                    Price = 350000,
                    MinPrice = 300000,
                    MaxPrice = 500000,
                    DurationMinutes = 120,
                    Gender = "Male",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "UT002",
                    Name = "Uốn Tóc Layer",
                    Slug = "uon-toc-layer",
                    ShortDescription = "Uốn tóc tạo độ phồng tự nhiên",
                    Price = 400000,
                    DurationMinutes = 150,
                    Gender = "All",
                    IsFeatured = true
                },

                // Nhuộm tóc
                new Service
                {
                    ServiceCode = "NT001",
                    Name = "Nhuộm Tóc Một Màu",
                    Slug = "nhuom-toc-mot-mau",
                    ShortDescription = "Nhuộm tóc đơn màu theo yêu cầu",
                    Price = 250000,
                    MinPrice = 200000,
                    MaxPrice = 400000,
                    DurationMinutes = 90,
                    Gender = "All",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "NT002",
                    Name = "Nhuộm Highlight",
                    Slug = "nhuom-highlight",
                    ShortDescription = "Nhuộm highlight phong cách",
                    Price = 450000,
                    DurationMinutes = 150,
                    Gender = "All",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "NT003",
                    Name = "Tẩy Tóc",
                    Slug = "tay-toc",
                    ShortDescription = "Tẩy tóc để nhuộm màu sáng",
                    Price = 300000,
                    DurationMinutes = 60,
                    Gender = "All",
                    IsFeatured = false
                },

                // Duỗi tóc
                new Service
                {
                    ServiceCode = "DT001",
                    Name = "Duỗi Tóc Cơ Bản",
                    Slug = "duoi-toc-co-ban",
                    ShortDescription = "Duỗi tóc thẳng tự nhiên",
                    Price = 300000,
                    MinPrice = 250000,
                    MaxPrice = 450000,
                    DurationMinutes = 120,
                    Gender = "All",
                    IsFeatured = true
                },

                // Gội đầu & Massage
                new Service
                {
                    ServiceCode = "GM001",
                    Name = "Gội Đầu Thư Giãn",
                    Slug = "goi-dau-thu-gian",
                    ShortDescription = "Gội đầu với massage đầu cổ vai",
                    Price = 50000,
                    DurationMinutes = 20,
                    Gender = "All",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "GM002",
                    Name = "Massage Đầu Cổ Vai",
                    Slug = "massage-dau-co-vai",
                    ShortDescription = "Massage thư giãn 30 phút",
                    Price = 100000,
                    DurationMinutes = 30,
                    Gender = "All",
                    IsFeatured = true
                },
                new Service
                {
                    ServiceCode = "GM003",
                    Name = "Combo Gội + Massage",
                    Slug = "combo-goi-massage",
                    ShortDescription = "Gói combo gội đầu + massage đầy đủ",
                    Price = 120000,
                    OriginalPrice = 150000,
                    DurationMinutes = 45,
                    Gender = "All",
                    IsFeatured = true
                },

                // Chăm sóc da
                new Service
                {
                    ServiceCode = "CSD001",
                    Name = "Đắp Mặt Nạ Dưỡng Da",
                    Slug = "dap-mat-na-duong-da",
                    ShortDescription = "Chăm sóc da mặt với mặt nạ cao cấp",
                    Price = 80000,
                    DurationMinutes = 20,
                    Gender = "All",
                    IsFeatured = false
                },

                // Dịch vụ khác
                new Service
                {
                    ServiceCode = "DV001",
                    Name = "Cạo Mặt",
                    Slug = "cao-mat",
                    ShortDescription = "Cạo mặt sạch sẽ, thư giãn",
                    Price = 50000,
                    DurationMinutes = 15,
                    Gender = "Male",
                    IsFeatured = false
                },
                new Service
                {
                    ServiceCode = "DV002",
                    Name = "Lấy Ráy Tai",
                    Slug = "lay-ray-tai",
                    ShortDescription = "Lấy ráy tai an toàn",
                    Price = 30000,
                    DurationMinutes = 10,
                    Gender = "All",
                    IsFeatured = false
                }
            };

            await context.Services.AddRangeAsync(services);
        }

        private static async Task SeedPaymentMethodsAsync(ApplicationDbContext context)
        {
            if (context.PaymentMethods.Any()) return;

            var paymentMethods = new List<PaymentMethod>
            {
                new PaymentMethod
                {
                    Code = "COD",
                    Name = "Thanh toán khi nhận hàng",
                    Description = "Thanh toán tiền mặt khi nhận hàng",
                    Type = "COD",
                    IsActive = true
                },
                new PaymentMethod
                {
                    Code = "VIETQR",
                    Name = "Chuyển khoản ngân hàng (VietQR)",
                    Description = "Quét mã QR để chuyển khoản",
                    Instructions = "Quét mã VietQR và chuyển khoản theo số tiền đơn hàng. Nội dung chuyển khoản: [Mã đơn hàng]",
                    Type = "BankTransfer",
                    IsActive = true
                },
                new PaymentMethod
                {
                    Code = "MOMO",
                    Name = "Ví MoMo",
                    Description = "Thanh toán qua ví điện tử MoMo",
                    Type = "EWallet",
                    IsActive = false // Chưa tích hợp
                },
                new PaymentMethod
                {
                    Code = "ZALOPAY",
                    Name = "ZaloPay",
                    Description = "Thanh toán qua ZaloPay",
                    Type = "EWallet",
                    IsActive = false // Chưa tích hợp
                }
            };

            await context.PaymentMethods.AddRangeAsync(paymentMethods);
        }

        private static async Task SeedShippingMethodsAsync(ApplicationDbContext context)
        {
            if (context.ShippingMethods.Any()) return;

            var shippingMethods = new List<ShippingMethod>
            {
                new ShippingMethod
                {
                    Code = "STANDARD",
                    Name = "Giao hàng tiêu chuẩn",
                    Description = "Giao hàng trong 3-5 ngày làm việc",
                    Type = "Standard",
                    BaseFee = 30000,
                    FeePerKg = 5000,
                    FreeShippingMinAmount = 500000,
                    EstimatedDays = 4,
                    MinDays = 3,
                    MaxDays = 5,
                    IsActive = true
                },
                new ShippingMethod
                {
                    Code = "EXPRESS",
                    Name = "Giao hàng nhanh",
                    Description = "Giao hàng trong 1-2 ngày",
                    Type = "Express",
                    BaseFee = 50000,
                    FeePerKg = 8000,
                    FreeShippingMinAmount = 1000000,
                    EstimatedDays = 2,
                    MinDays = 1,
                    MaxDays = 2,
                    IsActive = true
                },
                new ShippingMethod
                {
                    Code = "PICKUP",
                    Name = "Nhận tại cửa hàng",
                    Description = "Đến lấy hàng tại salon",
                    Type = "Pickup",
                    BaseFee = 0,
                    EstimatedDays = 0,
                    MinDays = 0,
                    MaxDays = 1,
                    IsActive = true
                }
            };

            await context.ShippingMethods.AddRangeAsync(shippingMethods);
        }

        private static async Task SeedStoreInfoAsync(ApplicationDbContext context)
        {
            if (context.StoreInfos.Any()) return;

            var storeInfo = new StoreInfo
            {
                StoreCode = "UME001",
                Name = "UME Salon",
                Slogan = "Ultimate Makeover Experience - Trải nghiệm lột xác đỉnh cao",
                Description = "UME Salon là hệ thống salon cắt tóc nam hàng đầu Việt Nam, mang đến trải nghiệm cắt tóc đẳng cấp với đội ngũ thợ chuyên nghiệp và dịch vụ tận tâm.",
                Address = "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                ProvinceCode = "79",
                DistrictCode = "760",
                WardCode = "26734",
                PhoneNumber = "0909123456",
                Hotline = "1900123456",
                Email = "contact@ume.vn",
                Website = "https://ume.vn",
                FacebookUrl = "https://facebook.com/umesalon",
                InstagramUrl = "https://instagram.com/umesalon",
                TikTokUrl = "https://tiktok.com/@umesalon",
                ZaloNumber = "0909123456",
                BusinessHours = @"{
                    ""monday"": {""open"": ""08:00"", ""close"": ""21:00""},
                    ""tuesday"": {""open"": ""08:00"", ""close"": ""21:00""},
                    ""wednesday"": {""open"": ""08:00"", ""close"": ""21:00""},
                    ""thursday"": {""open"": ""08:00"", ""close"": ""21:00""},
                    ""friday"": {""open"": ""08:00"", ""close"": ""21:00""},
                    ""saturday"": {""open"": ""08:00"", ""close"": ""22:00""},
                    ""sunday"": {""open"": ""09:00"", ""close"": ""20:00""}
                }",
                BankName = "Vietcombank",
                BankAccountNumber = "1234567890",
                BankAccountName = "UME SALON CO LTD",
                Currency = "VND",
                TimeZone = "SE Asia Standard Time",
                MetaTitle = "UME Salon - Cắt Tóc Nam Đẳng Cấp",
                MetaDescription = "UME Salon - Hệ thống salon cắt tóc nam hàng đầu với dịch vụ chuyên nghiệp, đội ngũ thợ giỏi và trải nghiệm khách hàng tuyệt vời.",
                Status = "Open"
            };

            await context.StoreInfos.AddAsync(storeInfo);
        }

        private static async Task SeedBannersAsync(ApplicationDbContext context)
        {
            if (context.Banners.Any()) return;

            var banners = new List<Banner>
            {
                new Banner
                {
                    Title = "Khai Trương UME Salon",
                    Subtitle = "Giảm 30% tất cả dịch vụ",
                    Description = "Ưu đãi đặc biệt nhân dịp khai trương chi nhánh mới",
                    ImageUrl = "/images/banners/opening-banner.jpg",
                    LinkUrl = "/services",
                    ButtonText = "Đặt lịch ngay",
                    Position = "HomeSlider",
                    IsActive = true
                },
                new Banner
                {
                    Title = "Sản Phẩm Mới",
                    Subtitle = "Pomade Reuzel đã có hàng",
                    Description = "Sản phẩm cao cấp từ Hà Lan",
                    ImageUrl = "/images/banners/product-banner.jpg",
                    LinkUrl = "/products?brand=reuzel",
                    ButtonText = "Xem ngay",
                    Position = "HomeSlider",
                    IsActive = true
                },
                new Banner
                {
                    Title = "AI Tư Vấn Kiểu Tóc",
                    Subtitle = "Tính năng mới",
                    Description = "Upload ảnh để AI gợi ý kiểu tóc phù hợp với khuôn mặt",
                    ImageUrl = "/images/banners/ai-banner.jpg",
                    LinkUrl = "/chatbot",
                    ButtonText = "Thử ngay",
                    Position = "HomeSlider",
                    IsActive = true
                }
            };

            await context.Banners.AddRangeAsync(banners);
        }

        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            // Kiểm tra xem đã có products với SKU từ seeder chưa
            var existingSKUs = new[] { "ROMANO-001", "GATSBY-001", "REUZEL-001" };
            if (await context.Products.AnyAsync(p => existingSKUs.Contains(p.SKU)))
            {
                return; // Đã seed rồi
            }

            // Lấy categories đã seed
            var catDauGoi = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "dau-goi");
            var catSap = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "sap-toc");
            var catGom = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "gom-toc");
            var catDauXa = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "dau-xa");
            var catPhuKien = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "phu-kien");

            // Lấy brands
            var brandGatsby = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "gatsby");
            var brandRomano = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "romano");
            var brandClearMen = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "clear-men");
            var brandReuzel = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "reuzel");
            var brandSuavecito = await context.Brands.FirstOrDefaultAsync(b => b.Slug == "suavecito");

            var products = new List<Product>
            {
                // Dầu Gội (5 sản phẩm)
                new Product
                {
                    Name = "Dầu Gội Romano Attitude",
                    Slug = "dau-goi-romano-attitude",
                    SKU = "ROMANO-001",
                    ShortDescription = "Dầu gội cho nam giới hiện đại, hương thơm mạnh mẽ",
                    Price = 85000,
                    OriginalPrice = 100000,
                    StockQuantity = 100,
                    IsFeatured = true,
                    IsNew = true,
                    Category = catDauGoi,
                    Brand = brandRomano
                },
                new Product
                {
                    Name = "Dầu Gội Clear Men Sạch Gàu",
                    Slug = "dau-goi-clear-men-sach-gau",
                    SKU = "CLEAR-001",
                    ShortDescription = "Dầu gội trị gàu hiệu quả cho nam giới",
                    Price = 95000,
                    OriginalPrice = 120000,
                    StockQuantity = 80,
                    IsFeatured = true,
                    Category = catDauGoi,
                    Brand = brandClearMen
                },
                new Product
                {
                    Name = "Dầu Gội Gatsby Hair & Scalp Care",
                    Slug = "dau-goi-gatsby-hair-scalp-care",
                    SKU = "GATSBY-DG-001",
                    ShortDescription = "Dầu gội chăm sóc tóc và da đầu chuyên sâu",
                    Price = 75000,
                    StockQuantity = 120,
                    Category = catDauGoi,
                    Brand = brandGatsby
                },
                new Product
                {
                    Name = "Dầu Gội Romano Classic",
                    Slug = "dau-goi-romano-classic",
                    SKU = "ROMANO-002",
                    ShortDescription = "Dầu gội Romano phiên bản cổ điển",
                    Price = 70000,
                    OriginalPrice = 85000,
                    StockQuantity = 150,
                    Category = catDauGoi,
                    Brand = brandRomano
                },
                new Product
                {
                    Name = "Dầu Gội Clear Men Deep Cleanse",
                    Slug = "dau-goi-clear-men-deep-cleanse",
                    SKU = "CLEAR-002",
                    ShortDescription = "Dầu gội làm sạch sâu, mát da đầu",
                    Price = 89000,
                    StockQuantity = 90,
                    Category = catDauGoi,
                    Brand = brandClearMen
                },

                // Sáp Vuốt Tóc (5 sản phẩm)
                new Product
                {
                    Name = "Sáp Vuốt Tóc Gatsby Mat & Hard",
                    Slug = "sap-vuot-toc-gatsby-mat-hard",
                    SKU = "GATSBY-001",
                    ShortDescription = "Sáp cứng, giữ nếp cực tốt, không bóng",
                    Price = 120000,
                    OriginalPrice = 150000,
                    StockQuantity = 200,
                    IsFeatured = true,
                    Category = catSap,
                    Brand = brandGatsby
                },
                new Product
                {
                    Name = "Reuzel Blue Strong Hold Pomade",
                    Slug = "reuzel-blue-strong-hold",
                    SKU = "REUZEL-001",
                    ShortDescription = "Pomade cao cấp giữ nếp cực mạnh, có bóng",
                    Price = 350000,
                    StockQuantity = 50,
                    IsFeatured = true,
                    IsNew = true,
                    Category = catSap,
                    Brand = brandReuzel
                },
                new Product
                {
                    Name = "Suavecito Original Hold Pomade",
                    Slug = "suavecito-original-hold",
                    SKU = "SUAV-001",
                    ShortDescription = "Pomade giữ nếp vừa, dễ tạo kiểu, dễ rửa",
                    Price = 280000,
                    StockQuantity = 70,
                    IsFeatured = true,
                    Category = catSap,
                    Brand = brandSuavecito
                },
                new Product
                {
                    Name = "Gatsby Moving Rubber Spiky Edge",
                    Slug = "gatsby-moving-rubber-spiky",
                    SKU = "GATSBY-002",
                    ShortDescription = "Sáp tạo kiểu nhọn, phong cách năng động",
                    Price = 110000,
                    StockQuantity = 180,
                    Category = catSap,
                    Brand = brandGatsby
                },
                new Product
                {
                    Name = "Reuzel Pink Grease Heavy Hold",
                    Slug = "reuzel-pink-grease",
                    SKU = "REUZEL-002",
                    ShortDescription = "Pomade giữ nếp mạnh, hương hồng táo",
                    Price = 380000,
                    StockQuantity = 40,
                    Category = catSap,
                    Brand = brandReuzel
                },

                // Gôm/Gel Tóc (5 sản phẩm)
                new Product
                {
                    Name = "Gel Tóc Gatsby Set & Keep Spray",
                    Slug = "gel-toc-gatsby-set-keep",
                    SKU = "GATSBY-GEL-001",
                    ShortDescription = "Gel xịt giữ nếp cực tốt, không dính",
                    Price = 95000,
                    StockQuantity = 100,
                    Category = catGom,
                    Brand = brandGatsby
                },
                new Product
                {
                    Name = "Gôm Romano Hair Spray",
                    Slug = "gom-romano-hair-spray",
                    SKU = "ROMANO-GOM-001",
                    ShortDescription = "Gôm xịt cứng, giữ nếp 24h",
                    Price = 75000,
                    StockQuantity = 120,
                    Category = catGom,
                    Brand = brandRomano
                },
                new Product
                {
                    Name = "Gel Vuốt Tóc Gatsby Water Gloss",
                    Slug = "gel-vuot-toc-gatsby-water-gloss",
                    SKU = "GATSBY-GEL-002",
                    ShortDescription = "Gel có bóng tự nhiên, dễ tạo kiểu",
                    Price = 85000,
                    StockQuantity = 90,
                    Category = catGom,
                    Brand = brandGatsby
                },
                new Product
                {
                    Name = "Gôm Xịt Romano Extra Hold",
                    Slug = "gom-xit-romano-extra-hold",
                    SKU = "ROMANO-GOM-002",
                    ShortDescription = "Gôm xịt siêu cứng, chịu mưa tốt",
                    Price = 80000,
                    StockQuantity = 110,
                    Category = catGom,
                    Brand = brandRomano
                },
                new Product
                {
                    Name = "Gel Tóc Clear Men Hair Styling Gel",
                    Slug = "gel-toc-clear-men-styling",
                    SKU = "CLEAR-GEL-001",
                    ShortDescription = "Gel tạo kiểu mạnh mẽ, không gàu",
                    Price = 70000,
                    StockQuantity = 100,
                    Category = catGom,
                    Brand = brandClearMen
                },

                // Dầu Xả (5 sản phẩm)
                new Product
                {
                    Name = "Dầu Xả Romano Attitude",
                    Slug = "dau-xa-romano-attitude",
                    SKU = "ROMANO-DX-001",
                    ShortDescription = "Dầu xả dưỡng tóc mềm mượt",
                    Price = 75000,
                    StockQuantity = 80,
                    Category = catDauXa,
                    Brand = brandRomano
                },
                new Product
                {
                    Name = "Dầu Xả Clear Men Anti-Dandruff",
                    Slug = "dau-xa-clear-men-anti-dandruff",
                    SKU = "CLEAR-DX-001",
                    ShortDescription = "Dầu xả ngăn gàu, tóc khỏe mạnh",
                    Price = 85000,
                    StockQuantity = 70,
                    Category = catDauXa,
                    Brand = brandClearMen
                },
                new Product
                {
                    Name = "Dầu Xả Gatsby Hair Treatment",
                    Slug = "dau-xa-gatsby-hair-treatment",
                    SKU = "GATSBY-DX-001",
                    ShortDescription = "Dầu xả phục hồi tóc hư tổn",
                    Price = 90000,
                    StockQuantity = 60,
                    Category = catDauXa,
                    Brand = brandGatsby
                },
                new Product
                {
                    Name = "Dầu Xả Romano Classic",
                    Slug = "dau-xa-romano-classic",
                    SKU = "ROMANO-DX-002",
                    ShortDescription = "Dầu xả cổ điển, thơm lâu",
                    Price = 65000,
                    StockQuantity = 90,
                    Category = catDauXa,
                    Brand = brandRomano
                },
                new Product
                {
                    Name = "Dầu Xả Gatsby Silk Protein",
                    Slug = "dau-xa-gatsby-silk-protein",
                    SKU = "GATSBY-DX-002",
                    ShortDescription = "Dầu xả protein tơ tằm, tóc siêu mượt",
                    Price = 95000,
                    StockQuantity = 50,
                    Category = catDauXa,
                    Brand = brandGatsby
                },

                // Phụ Kiện (5 sản phẩm)
                new Product
                {
                    Name = "Lược Cắt Tóc Chuyên Nghiệp",
                    Slug = "luoc-cat-toc-chuyen-nghiep",
                    SKU = "ACC-001",
                    ShortDescription = "Lược cắt tóc barber cao cấp",
                    Price = 150000,
                    StockQuantity = 40,
                    IsFeatured = true,
                    Category = catPhuKien
                },
                new Product
                {
                    Name = "Khăn Tắm Barber",
                    Slug = "khan-tam-barber",
                    SKU = "ACC-002",
                    ShortDescription = "Khăn tắm chuyên dụng cho salon",
                    Price = 80000,
                    StockQuantity = 100,
                    Category = catPhuKien
                },
                new Product
                {
                    Name = "Lưỡi Dao Cạo Feather",
                    Slug = "luoi-dao-cao-feather",
                    SKU = "ACC-003",
                    ShortDescription = "Lưỡi dao cạo sắc bén từ Nhật Bản",
                    Price = 120000,
                    StockQuantity = 200,
                    IsNew = true,
                    Category = catPhuKien
                },
                new Product
                {
                    Name = "Máy Cắt Tóc Barber",
                    Slug = "may-cat-toc-barber",
                    SKU = "ACC-004",
                    ShortDescription = "Máy cắt tóc chuyên nghiệp",
                    Price = 850000,
                    OriginalPrice = 1000000,
                    StockQuantity = 15,
                    IsFeatured = true,
                    Category = catPhuKien
                },
                new Product
                {
                    Name = "Áo Choàng Cắt Tóc",
                    Slug = "ao-choang-cat-toc",
                    SKU = "ACC-005",
                    ShortDescription = "Áo choàng chống nước cho khách hàng",
                    Price = 120000,
                    StockQuantity = 50,
                    Category = catPhuKien
                }
            };

            await context.Products.AddRangeAsync(products);
        }

        private static async Task SeedStaffAsync(ApplicationDbContext context)
        {
            // Kiểm tra xem đã có staff với code từ seeder chưa
            var existingStaffCodes = new[] { "STAFF001", "STAFF002", "STAFF003" };
            if (await context.Staff.AnyAsync(s => existingStaffCodes.Contains(s.StaffCode)))
            {
                return; // Đã seed rồi
            }

            var staff = new List<Staff>
            {
                new Staff
                {
                    StaffCode = "STAFF001",
                    FullName = "Nguyễn Văn An",
                    NickName = "Anh An",
                    Email = "an.nguyen@ume.com",
                    PhoneNumber = "0901234567",
                    Position = "Senior Barber",
                    Level = "Senior",
                    Specialties = "Cắt tóc Undercut, Fade, Tạo kiểu Layer",
                    YearsOfExperience = 5,
                    DateOfBirth = new DateTime(1995, 3, 15),
                    Gender = "Male",
                    Bio = "5 năm kinh nghiệm, chuyên các kiểu tóc hiện đại và tạo kiểu sáng tạo",
                    IsAvailable = true,
                    AcceptOnlineBooking = true,
                },
                new Staff
                {
                    StaffCode = "STAFF002",
                    FullName = "Trần Minh Khôi",
                    NickName = "Anh Khôi",
                    Email = "khoi.tran@ume.com",
                    PhoneNumber = "0902345678",
                    Position = "Master Stylist",
                    Level = "Master",
                    Specialties = "Uốn tóc, Nhuộm tóc, Tư vấn kiểu tóc",
                    YearsOfExperience = 8,
                    DateOfBirth = new DateTime(1992, 7, 22),
                    Gender = "Male",
                    Bio = "8 năm kinh nghiệm, từng làm việc tại salon Hàn Quốc, chuyên uốn nhuộm cao cấp",
                    IsAvailable = true,
                    AcceptOnlineBooking = true,
                },
                new Staff
                {
                    StaffCode = "STAFF003",
                    FullName = "Lê Quốc Bảo",
                    NickName = "Anh Bảo",
                    Email = "bao.le@ume.com",
                    PhoneNumber = "0903456789",
                    Position = "Barber",
                    Level = "Junior",
                    Specialties = "Cắt tóc cơ bản, Cạo mặt, Gội massage",
                    YearsOfExperience = 2,
                    DateOfBirth = new DateTime(1998, 11, 5),
                    Gender = "Male",
                    Bio = "2 năm kinh nghiệm, nhiệt huyết và chu đáo với khách hàng",
                    IsAvailable = true,
                    AcceptOnlineBooking = true,
                }
            };

            await context.Staff.AddRangeAsync(staff);
        }    }
}
