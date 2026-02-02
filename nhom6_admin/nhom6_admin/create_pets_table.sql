-- Create Pets table for Pet Management
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pets]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Pets] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PetCode] NVARCHAR(20) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Species] NVARCHAR(50) NOT NULL,
        [Breed] NVARCHAR(100) NULL,
        [Gender] NVARCHAR(20) NULL,
        [DateOfBirth] DATETIME2 NULL,
        [AgeInMonths] INT NULL,
        [Color] NVARCHAR(100) NULL,
        [Weight] DECIMAL(5,2) NULL,
        [Description] NVARCHAR(MAX) NULL,
        [HealthStatus] NVARCHAR(500) NULL,
        [IsVaccinated] BIT NOT NULL DEFAULT 0,
        [VaccinationDetails] NVARCHAR(500) NULL,
        [IsNeutered] BIT NOT NULL DEFAULT 0,
        [HasMicrochip] BIT NOT NULL DEFAULT 0,
        [MicrochipNumber] NVARCHAR(50) NULL,
        [ImageUrl] NVARCHAR(500) NULL,
        [AdditionalImages] NVARCHAR(MAX) NULL,
        [VideoUrl] NVARCHAR(500) NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [OriginalPrice] DECIMAL(18,2) NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Available',
        [IsForSale] BIT NOT NULL DEFAULT 1,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [Origin] NVARCHAR(200) NULL,
        [ArrivalDate] DATETIME2 NULL,
        [SoldDate] DATETIME2 NULL,
        [BuyerId] NVARCHAR(450) NULL,
        [BuyerName] NVARCHAR(200) NULL,
        [BuyerPhone] NVARCHAR(20) NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [ViewCount] INT NOT NULL DEFAULT 0,
        [QRCodeData] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );

    -- Create unique index for PetCode
    CREATE UNIQUE INDEX [IX_Pets_PetCode] ON [dbo].[Pets] ([PetCode]);
    
    PRINT 'Pets table created successfully';
END
ELSE
BEGIN
    PRINT 'Pets table already exists';
END

-- Insert sample data
IF NOT EXISTS (SELECT 1 FROM [dbo].[Pets])
BEGIN
    INSERT INTO [dbo].[Pets] ([PetCode], [Name], [Species], [Breed], [Gender], [AgeInMonths], [Color], [Weight], [Description], [HealthStatus], [IsVaccinated], [VaccinationDetails], [IsNeutered], [HasMicrochip], [Price], [OriginalPrice], [Status], [IsForSale], [IsFeatured], [Origin])
    VALUES 
    ('PET260202001', N'Lucky', N'Chó', N'Corgi', N'Đực', 6, N'Vàng nâu', 8.5, N'Chú chó Corgi siêu đáng yêu, thông minh và năng động. Đã được huấn luyện cơ bản.', N'Khỏe mạnh, hoạt bát', 1, N'5 in 1, Dại', 1, 1, 15000000, 18000000, 'Available', 1, 1, N'Trang trại Happy Pets'),
    ('PET260202002', N'Miu Miu', N'Mèo', N'British Shorthair', N'Cái', 4, N'Xám xanh', 3.2, N'Mèo British Shorthair lông xám xanh, mắt to tròn, tính cách hiền lành.', N'Khỏe mạnh', 1, N'3 in 1, Dại', 0, 0, 12000000, NULL, 'Available', 1, 1, N'Trang trại Royal Cats'),
    ('PET260202003', N'Bông', N'Chó', N'Poodle', N'Cái', 3, N'Trắng', 2.5, N'Poodle Toy trắng muốt, lông xoăn đẹp. Rất thích được vuốt ve.', N'Khỏe mạnh, đã tẩy giun', 1, N'5 in 1', 0, 0, 8000000, 9500000, 'Available', 1, 0, N'Trang trại Happy Pets'),
    ('PET260202004', N'Gấu', N'Chó', N'Samoyed', N'Đực', 5, N'Trắng', 12.0, N'Samoyed lông trắng như tuyết, nụ cười đặc trưng. Rất thân thiện với trẻ em.', N'Khỏe mạnh, năng động', 1, N'5 in 1, Dại, Ho cũi', 0, 1, 25000000, 28000000, 'Available', 1, 1, N'Trang trại Northern Pets'),
    ('PET260202005', N'Kitty', N'Mèo', N'Scottish Fold', N'Cái', 6, N'Cam', 4.0, N'Mèo Scottish Fold tai cụp dễ thương, tính cách nhẹ nhàng.', N'Khỏe mạnh', 1, N'3 in 1', 1, 0, 18000000, NULL, 'Reserved', 1, 0, N'Trang trại Cute Cats'),
    ('PET260202006', N'Cún', N'Chó', N'Shiba Inu', N'Đực', 8, N'Vàng đỏ', 9.0, N'Shiba Inu thuần chủng, khuôn mặt biểu cảm, rất trung thành.', N'Khỏe mạnh', 1, N'5 in 1, Dại', 1, 1, 20000000, 22000000, 'Available', 1, 1, N'Nhập khẩu Nhật Bản');
    
    PRINT 'Sample pets inserted successfully';
END
GO
