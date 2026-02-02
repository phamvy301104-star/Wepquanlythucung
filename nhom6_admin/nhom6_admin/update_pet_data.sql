-- Update Pet Data with proper values for testing
USE QuanLyPet;
GO

-- Update existing pets with proper data
UPDATE Pets SET 
    Species = 'Cho', 
    Breed = 'Golden Retriever', 
    Gender = 'Duc', 
    Color = 'Vang',
    AgeInMonths = 6,
    Weight = 8.5,
    HealthStatus = 'Khoe manh',
    IsVaccinated = 1,
    VaccinationDetails = 'Da tiem 5 trong 1',
    Price = 15000000,
    OriginalPrice = 18000000,
    Status = 'Available',
    IsForSale = 1,
    IsFeatured = 1,
    Description = 'Cho Golden Retriever thuan chung, than thien, de huan luyen',
    ImageUrl = 'https://images.unsplash.com/photo-1552053831-71594a27632d?w=400'
WHERE Id = 1;

UPDATE Pets SET 
    Species = 'Meo', 
    Breed = 'Persian', 
    Gender = 'Cai', 
    Color = 'Trang',
    AgeInMonths = 4,
    Weight = 3.2,
    HealthStatus = 'Khoe manh',
    IsVaccinated = 1,
    VaccinationDetails = 'Da tiem 3 trong 1',
    Price = 8000000,
    OriginalPrice = 10000000,
    Status = 'Available',
    IsForSale = 1,
    IsFeatured = 0,
    Description = 'Meo Ba Tu long dai, hien lanh',
    ImageUrl = 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=400'
WHERE Id = 2;

UPDATE Pets SET 
    Species = 'Cho', 
    Breed = 'Poodle', 
    Gender = 'Duc', 
    Color = 'Trang kem',
    AgeInMonths = 3,
    Weight = 2.8,
    HealthStatus = 'Khoe manh',
    IsVaccinated = 1,
    VaccinationDetails = 'Da tiem day du',
    Price = 12000000,
    OriginalPrice = 15000000,
    Status = 'Reserved',
    IsForSale = 1,
    IsFeatured = 1,
    Description = 'Cho Poodle mini, long xoan, de thuong',
    ImageUrl = 'https://images.unsplash.com/photo-1591160690555-5debfba289f0?w=400'
WHERE Id = 3;

UPDATE Pets SET 
    Species = 'Meo', 
    Breed = 'British Shorthair', 
    Gender = 'Duc', 
    Color = 'Xam xanh',
    AgeInMonths = 5,
    Weight = 4.5,
    HealthStatus = 'Khoe manh',
    IsVaccinated = 1,
    VaccinationDetails = 'Da tiem phong dai',
    Price = 25000000,
    OriginalPrice = 28000000,
    Status = 'Available',
    IsForSale = 1,
    IsFeatured = 1,
    Description = 'Meo Anh long ngan blue thuan chung',
    ImageUrl = 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=400'
WHERE Id = 4;

UPDATE Pets SET 
    Species = 'Hamster', 
    Breed = 'Winter White', 
    Gender = 'Cai', 
    Color = 'Trang',
    AgeInMonths = 2,
    Weight = 0.03,
    HealthStatus = 'Khoe manh',
    IsVaccinated = 0,
    VaccinationDetails = NULL,
    Price = 150000,
    OriginalPrice = 200000,
    Status = 'Available',
    IsForSale = 1,
    IsFeatured = 0,
    Description = 'Hamster Winter White de thuong, hien lanh',
    ImageUrl = 'https://images.unsplash.com/photo-1425082661705-1834bfd09dca?w=400'
WHERE Id = 5;

UPDATE Pets SET 
    Species = 'Cho', 
    Breed = 'Corgi', 
    Gender = 'Cai', 
    Color = 'Vang trang',
    AgeInMonths = 8,
    Weight = 12.5,
    HealthStatus = 'Khoe manh',
    IsVaccinated = 1,
    VaccinationDetails = 'Da tiem day du + tri ve',
    Price = 35000000,
    OriginalPrice = 40000000,
    Status = 'Sold',
    IsForSale = 0,
    IsFeatured = 0,
    BuyerName = 'Nguyen Van A',
    BuyerPhone = '0901234567',
    SoldDate = GETDATE(),
    Description = 'Cho Corgi chan ngan de thuong, thuan chung',
    ImageUrl = 'https://images.unsplash.com/photo-1612536057832-2ff7ead58194?w=400'
WHERE Id = 6;

-- Insert more sample pets if not exists
IF NOT EXISTS (SELECT 1 FROM Pets WHERE PetCode = 'PET007')
BEGIN
    INSERT INTO Pets (PetCode, Name, Species, Breed, Gender, AgeInMonths, Color, Weight, Description, HealthStatus, IsVaccinated, VaccinationDetails, Price, OriginalPrice, Status, IsForSale, IsFeatured, ImageUrl, CreatedAt, IsDeleted, ViewCount)
    VALUES 
    ('PET007', 'Lucky', 'Cho', 'Shiba Inu', 'Duc', 10, 'Vang nau', 10.5, 'Cho Shiba Inu nhat ban thuan chung, thong minh', 'Khoe manh', 1, 'Da tiem day du', 45000000, 50000000, 'Available', 1, 1, 'https://images.unsplash.com/photo-1583337130417-3346a1be7dee?w=400', GETDATE(), 0, 0),
    ('PET008', 'Kitty', 'Meo', 'Scottish Fold', 'Cai', 3, 'Vang', 3.8, 'Meo Scottish Fold tai cup de thuong', 'Khoe manh', 1, 'Da tiem 3 trong 1', 30000000, 35000000, 'Available', 1, 1, 'https://images.unsplash.com/photo-1592194996308-7b43878e84a6?w=400', GETDATE(), 0, 0),
    ('PET009', 'Bunny', 'Tho', 'Holland Lop', 'Cai', 4, 'Trang xam', 1.5, 'Tho Holland Lop tai cup hien lanh', 'Khoe manh', 0, NULL, 800000, 1000000, 'Available', 1, 0, 'https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=400', GETDATE(), 0, 0),
    ('PET010', 'Tweety', 'Chim', 'Vet Hong Kong', 'Duc', 12, 'Xanh la', 0.4, 'Chim Vet Hong Kong biet noi', 'Khoe manh', 0, NULL, 15000000, 18000000, 'Reserved', 1, 0, 'https://images.unsplash.com/photo-1552728089-57bdde30beb3?w=400', GETDATE(), 0, 0);
END

PRINT 'Pet data updated successfully!';

-- Show results
SELECT Id, PetCode, Name, Species, Breed, Gender, Color, Price, Status, IsFeatured FROM Pets WHERE IsDeleted = 0;
