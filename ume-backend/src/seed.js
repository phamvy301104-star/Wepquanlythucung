require('dotenv').config();
const mongoose = require('mongoose');
const bcrypt = require('bcryptjs');
const User = require('./models/User');
const Category = require('./models/Category');
const Brand = require('./models/Brand');
const ServiceCategory = require('./models/ServiceCategory');
const Service = require('./models/Service');

const seed = async () => {
  try {
    await mongoose.connect(process.env.MONGODB_URI);
    console.log('Connected to MongoDB');

    // Create admin user
    const existingAdmin = await User.findOne({ email: 'admin@ume.com' });
    if (!existingAdmin) {
      await User.create({
        email: 'admin@ume.com',
        password: 'Admin@123',
        fullName: 'Admin UME',
        role: 'Admin',
        isEmailVerified: true
      });
      console.log('✅ Admin user created (admin@ume.com / Admin@123)');
    }

    // Product Categories
    const categories = ['Thức ăn', 'Phụ kiện', 'Đồ chơi', 'Sức khỏe', 'Vệ sinh', 'Quần áo'];
    const toSlug = (s) => s.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
    for (const name of categories) {
      await Category.findOneAndUpdate({ name }, { name, slug: toSlug(name), isActive: true }, { upsert: true, new: true });
    }
    console.log('✅ Product categories seeded');

    // Brands
    const brands = ['Royal Canin', 'Whiskas', 'Pedigree', 'Me-O', 'Catidea', 'PetMart'];
    for (const name of brands) {
      await Brand.findOneAndUpdate({ name }, { name, slug: toSlug(name), isActive: true }, { upsert: true, new: true });
    }
    console.log('✅ Brands seeded');

    // Service Categories
    const svcCats = ['Grooming', 'Tắm & Vệ sinh', 'Chăm sóc sức khỏe', 'Spa & Thư giãn'];
    for (const name of svcCats) {
      await ServiceCategory.findOneAndUpdate({ name }, { name, slug: toSlug(name), isActive: true }, { upsert: true, new: true });
    }
    console.log('✅ Service categories seeded');

    // Services
    const groomingCat = await ServiceCategory.findOne({ name: 'Grooming' });
    const bathCat = await ServiceCategory.findOne({ name: 'Tắm & Vệ sinh' });

    const services = [
      { name: 'Cắt tỉa lông chó', serviceCode: 'SVC001', slug: 'cat-tia-long-cho', price: 150000, duration: 60, category: groomingCat?._id, description: 'Cắt tỉa lông chuyên nghiệp cho chó' },
      { name: 'Cắt tỉa lông mèo', serviceCode: 'SVC002', slug: 'cat-tia-long-meo', price: 180000, duration: 60, category: groomingCat?._id, description: 'Cắt tỉa lông chuyên nghiệp cho mèo' },
      { name: 'Tắm chó nhỏ', serviceCode: 'SVC003', slug: 'tam-cho-nho', price: 100000, duration: 30, category: bathCat?._id, description: 'Tắm sạch cho chó dưới 10kg' },
      { name: 'Tắm chó lớn', serviceCode: 'SVC004', slug: 'tam-cho-lon', price: 150000, duration: 45, category: bathCat?._id, description: 'Tắm sạch cho chó trên 10kg' },
      { name: 'Tắm mèo', serviceCode: 'SVC005', slug: 'tam-meo', price: 120000, duration: 30, category: bathCat?._id, description: 'Tắm sạch cho mèo' },
      { name: 'Combo Tắm + Cắt tỉa chó', serviceCode: 'SVC006', slug: 'combo-tam-cat-tia-cho', price: 220000, duration: 90, category: groomingCat?._id, description: 'Trọn gói tắm và cắt tỉa lông cho chó' },
      { name: 'Combo Tắm + Cắt tỉa mèo', serviceCode: 'SVC007', slug: 'combo-tam-cat-tia-meo', price: 250000, duration: 90, category: groomingCat?._id, description: 'Trọn gói tắm và cắt tỉa lông cho mèo' },
    ];

    for (const svc of services) {
      await Service.findOneAndUpdate({ name: svc.name }, svc, { upsert: true, new: true });
    }
    console.log('✅ Services seeded');

    console.log('\n🎉 Seed completed!');
    process.exit(0);
  } catch (error) {
    console.error('❌ Seed error:', error.message);
    process.exit(1);
  }
};

seed();
