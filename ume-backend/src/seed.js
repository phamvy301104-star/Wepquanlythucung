require('dotenv').config();
const mongoose = require('mongoose');
const User = require('./models/User');
const Category = require('./models/Category');
const Brand = require('./models/Brand');
const Product = require('./models/Product');
const Staff = require('./models/Staff');
const Service = require('./models/Service');
const ServiceCategory = require('./models/ServiceCategory');
const Order = require('./models/Order');
const Appointment = require('./models/Appointment');
const Pet = require('./models/Pet');
const Settings = require('./models/Settings');
const Promotion = require('./models/Promotion');
const Review = require('./models/Review');
const Notification = require('./models/Notification');

const toSlug = (s) => s.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');

const seedComplete = async () => {
  try {
    await mongoose.connect(process.env.MONGODB_URI);
    console.log('✅ Connected to MongoDB');

    // ============================
    // 1. ADMIN USER
    // ============================
    let admin = await User.findOne({ email: 'admin@ume.com' });
    if (!admin) {
      admin = await User.create({
        email: 'admin@ume.com',
        password: 'Admin@123',
        fullName: 'Admin UME',
        role: 'Admin',
        isEmailVerified: true,
        isActive: true
      });
      console.log('✅ Admin user created (admin@ume.com / Admin@123)');
    } else {
      console.log('ℹ️ Admin user already exists');
    }

    // ============================
    // 2. PRODUCT CATEGORIES
    // ============================
    const categoryData = [
      { name: 'Thức ăn', description: 'Thức ăn cho thú cưng các loại', icon: '🍖' },
      { name: 'Phụ kiện', description: 'Phụ kiện, đồ dùng cho thú cưng', icon: '🎀' },
      { name: 'Đồ chơi', description: 'Đồ chơi giải trí cho thú cưng', icon: '🎾' },
      { name: 'Sức khỏe', description: 'Sản phẩm chăm sóc sức khỏe thú cưng', icon: '💊' },
      { name: 'Vệ sinh', description: 'Sản phẩm vệ sinh, tắm rửa thú cưng', icon: '🧴' },
      { name: 'Quần áo', description: 'Quần áo, trang phục cho thú cưng', icon: '👕' },
    ];
    const cats = [];
    for (const c of categoryData) {
      const cat = await Category.findOneAndUpdate(
        { name: c.name },
        { ...c, slug: toSlug(c.name), isActive: true },
        { upsert: true, new: true }
      );
      cats.push(cat);
    }
    console.log('✅ 6 product categories seeded');

    // ============================
    // 3. BRANDS
    // ============================
    const brandData = [
      { name: 'Royal Canin', description: 'Thương hiệu thức ăn cao cấp từ Pháp', website: 'https://www.royalcanin.com' },
      { name: 'Whiskas', description: 'Thức ăn cho mèo uy tín toàn cầu', website: 'https://www.whiskas.com' },
      { name: 'Pedigree', description: 'Thức ăn cho chó hàng đầu thế giới', website: 'https://www.pedigree.com' },
      { name: 'Me-O', description: 'Thức ăn cho mèo phổ biến', website: '' },
      { name: 'Catidea', description: 'Phụ kiện và đồ dùng cho thú cưng', website: '' },
      { name: 'PetMart', description: 'Chuỗi cửa hàng thú cưng lớn nhất Việt Nam', website: 'https://www.petmart.vn' },
    ];
    const brands = [];
    for (const b of brandData) {
      const brand = await Brand.findOneAndUpdate(
        { name: b.name },
        { ...b, slug: toSlug(b.name), isActive: true },
        { upsert: true, new: true }
      );
      brands.push(brand);
    }
    console.log('✅ 6 brands seeded');

    // ============================
    // 4. SERVICE CATEGORIES
    // ============================
    const svcCatData = [
      { name: 'Grooming', description: 'Dịch vụ cắt tỉa lông thú cưng', icon: '✂️' },
      { name: 'Tắm & Vệ sinh', description: 'Dịch vụ tắm rửa, vệ sinh thú cưng', icon: '🛁' },
      { name: 'Chăm sóc sức khỏe', description: 'Dịch vụ khám, chăm sóc sức khỏe thú cưng', icon: '🏥' },
      { name: 'Spa & Thư giãn', description: 'Dịch vụ spa, massage cho thú cưng', icon: '💆' },
    ];
    for (const sc of svcCatData) {
      await ServiceCategory.findOneAndUpdate(
        { name: sc.name },
        { ...sc, slug: toSlug(sc.name), isActive: true },
        { upsert: true, new: true }
      );
    }
    console.log('✅ 4 service categories seeded');

    // ============================
    // 5. SERVICES
    // ============================
    const groomingCat = await ServiceCategory.findOne({ name: 'Grooming' });
    const bathCat = await ServiceCategory.findOne({ name: 'Tắm & Vệ sinh' });
    const healthCat = await ServiceCategory.findOne({ name: 'Chăm sóc sức khỏe' });
    const spaCat = await ServiceCategory.findOne({ name: 'Spa & Thư giãn' });

    const serviceData = [
      { name: 'Cắt tỉa lông chó', serviceCode: 'SVC001', slug: 'cat-tia-long-cho', price: 150000, duration: 60, category: groomingCat?._id, description: 'Cắt tỉa lông chuyên nghiệp cho chó, tạo kiểu theo yêu cầu', isFeatured: true },
      { name: 'Cắt tỉa lông mèo', serviceCode: 'SVC002', slug: 'cat-tia-long-meo', price: 180000, duration: 60, category: groomingCat?._id, description: 'Cắt tỉa lông chuyên nghiệp cho mèo, nhẹ nhàng và an toàn', isFeatured: true },
      { name: 'Tắm chó nhỏ (dưới 10kg)', serviceCode: 'SVC003', slug: 'tam-cho-nho', price: 100000, duration: 30, category: bathCat?._id, description: 'Tắm sạch, sấy khô cho chó dưới 10kg' },
      { name: 'Tắm chó lớn (trên 10kg)', serviceCode: 'SVC004', slug: 'tam-cho-lon', price: 150000, duration: 45, category: bathCat?._id, description: 'Tắm sạch, sấy khô cho chó trên 10kg' },
      { name: 'Tắm mèo', serviceCode: 'SVC005', slug: 'tam-meo', price: 120000, duration: 30, category: bathCat?._id, description: 'Tắm sạch, sấy khô cho mèo, dùng sữa tắm chuyên dụng' },
      { name: 'Combo Tắm + Cắt tỉa chó', serviceCode: 'SVC006', slug: 'combo-tam-cat-tia-cho', price: 220000, duration: 90, category: groomingCat?._id, description: 'Trọn gói tắm và cắt tỉa lông cho chó', isFeatured: true },
      { name: 'Combo Tắm + Cắt tỉa mèo', serviceCode: 'SVC007', slug: 'combo-tam-cat-tia-meo', price: 250000, duration: 90, category: groomingCat?._id, description: 'Trọn gói tắm và cắt tỉa lông cho mèo', isFeatured: true },
      { name: 'Khám sức khỏe tổng quát', serviceCode: 'SVC008', slug: 'kham-suc-khoe-tong-quat', price: 200000, duration: 45, category: healthCat?._id, description: 'Khám sức khỏe tổng quát cho thú cưng, tư vấn dinh dưỡng' },
      { name: 'Tiêm phòng vaccine', serviceCode: 'SVC009', slug: 'tiem-phong-vaccine', price: 300000, duration: 30, category: healthCat?._id, description: 'Tiêm phòng vaccine đầy đủ cho chó mèo' },
      { name: 'Spa thư giãn cho thú cưng', serviceCode: 'SVC010', slug: 'spa-thu-gian', price: 350000, duration: 60, category: spaCat?._id, description: 'Dịch vụ spa cao cấp, massage thư giãn cho thú cưng', isFeatured: true },
      { name: 'Cắt móng + Vệ sinh tai', serviceCode: 'SVC011', slug: 'cat-mong-ve-sinh-tai', price: 80000, duration: 20, category: bathCat?._id, description: 'Cắt móng và vệ sinh tai chuyên nghiệp' },
      { name: 'Tẩy giun định kỳ', serviceCode: 'SVC012', slug: 'tay-giun-dinh-ky', price: 100000, duration: 15, category: healthCat?._id, description: 'Tẩy giun sán định kỳ cho thú cưng' },
    ];
    const services = [];
    for (const svc of serviceData) {
      const s = await Service.findOneAndUpdate(
        { serviceCode: svc.serviceCode },
        { ...svc, isActive: true },
        { upsert: true, new: true }
      );
      services.push(s);
    }
    console.log('✅ 12 services seeded');

    // ============================
    // 6. CUSTOMERS
    // ============================
    const customerData = [
      { email: 'nguyenvana@gmail.com', fullName: 'Nguyễn Văn A', phoneNumber: '0901234567', role: 'Customer', address: '12 Nguyễn Huệ, Q.1, TP.HCM' },
      { email: 'tranthib@gmail.com', fullName: 'Trần Thị B', phoneNumber: '0912345678', role: 'Customer', address: '45 Lê Lợi, Q.3, TP.HCM' },
      { email: 'lequangc@gmail.com', fullName: 'Lê Quang C', phoneNumber: '0923456789', role: 'Customer', address: '78 Trần Hưng Đạo, Q.5, TP.HCM' },
      { email: 'phamthid@gmail.com', fullName: 'Phạm Thị D', phoneNumber: '0934567890', role: 'Customer', address: '99 Nguyễn Trãi, Q.7, TP.HCM' },
      { email: 'hoangmine@gmail.com', fullName: 'Hoàng Minh E', phoneNumber: '0945678901', role: 'Customer', address: '200 Võ Văn Tần, Q.3, TP.HCM' },
      { email: 'vuthif@gmail.com', fullName: 'Vũ Thị F', phoneNumber: '0956789012', role: 'Customer', address: '55 Hai Bà Trưng, Q.1, TP.HCM' },
      { email: 'dangvanG@gmail.com', fullName: 'Đặng Văn G', phoneNumber: '0967890123', role: 'Customer', address: '33 Điện Biên Phủ, Bình Thạnh, TP.HCM' },
      { email: 'buithih@gmail.com', fullName: 'Bùi Thị H', phoneNumber: '0978901234', role: 'Customer', address: '88 Cách Mạng Tháng 8, Q.10, TP.HCM' },
    ];
    const customers = [];
    for (const c of customerData) {
      const u = await User.findOneAndUpdate(
        { email: c.email },
        { ...c, password: 'User@123', isActive: true, isEmailVerified: true },
        { upsert: true, new: true }
      );
      customers.push(u);
    }
    console.log('✅ 8 customers seeded');

    // ============================
    // 7. PRODUCTS
    // ============================
    const productData = [
      { name: 'Royal Canin Maxi Adult', sku: 'RC-MA-01', price: 450000, originalPrice: 520000, stockQuantity: 50, soldCount: 120, category: cats[0]?._id, brand: brands[0]?._id, description: 'Thức ăn hạt cho chó lớn trên 26 tháng tuổi, hỗ trợ xương khớp và hệ tiêu hóa', isFeatured: true, averageRating: 4.5 },
      { name: 'Whiskas Tuna Adult', sku: 'WK-TA-01', price: 89000, originalPrice: 99000, stockQuantity: 100, soldCount: 250, category: cats[0]?._id, brand: brands[1]?._id, description: 'Thức ăn ướt vị cá ngừ cho mèo trưởng thành, giàu protein', averageRating: 4.2 },
      { name: 'Pedigree Puppy', sku: 'PD-PP-01', price: 180000, originalPrice: 210000, stockQuantity: 75, soldCount: 85, category: cats[0]?._id, brand: brands[2]?._id, description: 'Thức ăn hạt cho chó con, hỗ trợ phát triển xương và răng', isFeatured: true, averageRating: 4.3 },
      { name: 'Me-O Salmon', sku: 'MO-SA-01', price: 65000, originalPrice: 75000, stockQuantity: 200, soldCount: 310, category: cats[0]?._id, brand: brands[3]?._id, description: 'Thức ăn hạt vị cá hồi cho mèo, bổ sung Omega 3-6', averageRating: 4.0 },
      { name: 'Vòng cổ LED cho chó', sku: 'PK-LED-01', price: 120000, originalPrice: 150000, stockQuantity: 30, soldCount: 45, category: cats[1]?._id, brand: brands[5]?._id, description: 'Vòng cổ phát sáng LED đổi màu, an toàn khi dắt chó ban đêm', averageRating: 4.7, isFeatured: true },
      { name: 'Dây dắt tự động 5m', sku: 'PK-DD-01', price: 250000, originalPrice: 280000, stockQuantity: 20, soldCount: 35, category: cats[1]?._id, brand: brands[5]?._id, description: 'Dây dắt tự cuốn 5m cho chó dưới 20kg, chất liệu bền bỉ', averageRating: 4.1 },
      { name: 'Bóng cao su cho chó', sku: 'DC-BC-01', price: 35000, originalPrice: 45000, stockQuantity: 150, soldCount: 200, category: cats[2]?._id, brand: brands[4]?._id, description: 'Bóng cao su bền, an toàn cho chó nhai và chơi', averageRating: 4.4 },
      { name: 'Cần câu lông cho mèo', sku: 'DC-CL-01', price: 55000, originalPrice: 65000, stockQuantity: 80, soldCount: 175, category: cats[2]?._id, brand: brands[4]?._id, description: 'Cần câu lông vũ kích thích bản năng săn mồi của mèo', averageRating: 4.6, isFeatured: true },
      { name: 'Sữa tắm SOS cho chó', sku: 'VS-ST-01', price: 95000, originalPrice: 110000, stockQuantity: 60, soldCount: 150, category: cats[4]?._id, brand: brands[5]?._id, description: 'Sữa tắm dưỡng lông, khử mùi cho chó, chiết xuất thiên nhiên', averageRating: 4.3, isFeatured: true },
      { name: 'Áo hoodie cho chó', sku: 'QA-HD-01', price: 160000, originalPrice: 200000, stockQuantity: 40, soldCount: 55, category: cats[5]?._id, brand: brands[5]?._id, description: 'Áo hoodie ấm áp cho chó nhỏ, nhiều size', averageRating: 4.5 },
      { name: 'Royal Canin Indoor Cat', sku: 'RC-IC-01', price: 320000, originalPrice: 380000, stockQuantity: 45, soldCount: 90, category: cats[0]?._id, brand: brands[0]?._id, description: 'Thức ăn cho mèo nuôi trong nhà, kiểm soát cân nặng', averageRating: 4.6, isFeatured: true },
      { name: 'Khay vệ sinh cho mèo', sku: 'VS-KV-01', price: 180000, originalPrice: 220000, stockQuantity: 35, soldCount: 70, category: cats[4]?._id, brand: brands[4]?._id, description: 'Khay vệ sinh có nắp đậy, chống mùi hiệu quả', averageRating: 4.2 },
      { name: 'Nhà cho chó size M', sku: 'PK-NC-01', price: 450000, originalPrice: 550000, stockQuantity: 15, soldCount: 25, category: cats[1]?._id, brand: brands[5]?._id, description: 'Nhà cho chó kích thước vừa, chất liệu gỗ tự nhiên', averageRating: 4.8 },
      { name: 'Vitamin cho mèo', sku: 'SK-VM-01', price: 150000, originalPrice: 180000, stockQuantity: 55, soldCount: 80, category: cats[3]?._id, brand: brands[0]?._id, description: 'Viên nhai bổ sung vitamin tổng hợp cho mèo', averageRating: 4.3 },
      { name: 'Balo vận chuyển thú cưng', sku: 'PK-BL-01', price: 350000, originalPrice: 420000, stockQuantity: 25, soldCount: 40, category: cats[1]?._id, brand: brands[5]?._id, description: 'Balo trong suốt mang thú cưng đi chơi, thoáng khí', averageRating: 4.5, isFeatured: true },
      { name: 'Cát vệ sinh cho mèo 10L', sku: 'VS-CV-01', price: 85000, originalPrice: 100000, stockQuantity: 120, soldCount: 280, category: cats[4]?._id, brand: brands[4]?._id, description: 'Cát vệ sinh vón cục, khử mùi tốt, ít bụi', averageRating: 4.1 },
    ];
    const products = [];
    for (const p of productData) {
      const prod = await Product.findOneAndUpdate(
        { sku: p.sku },
        { ...p, slug: toSlug(p.name), isActive: true, imageUrl: `https://picsum.photos/seed/${p.sku}/400/400` },
        { upsert: true, new: true }
      );
      products.push(prod);
    }
    console.log('✅ 16 products seeded');

    // ============================
    // 8. STAFF
    // ============================
    const staffData = [
      { fullName: 'Nguyễn Minh Tuấn', staffCode: 'STF001', email: 'tuan.nv@ume.com', phoneNumber: '0901111111', position: 'Barber', level: 'Master', yearsOfExperience: 8, averageRating: 4.8, totalCustomersServed: 520, baseSalary: 12000000, status: 'Active', bio: 'Thợ cắt tỉa lông chuyên nghiệp với 8 năm kinh nghiệm, chuyên grooming chó lớn' },
      { fullName: 'Trần Thị Hương', staffCode: 'STF002', email: 'huong.tt@ume.com', phoneNumber: '0902222222', position: 'PetGroomer', level: 'Senior', yearsOfExperience: 5, averageRating: 4.6, totalCustomersServed: 380, baseSalary: 10000000, status: 'Active', bio: 'Chuyên gia grooming thú cưng, đặc biệt mèo các giống' },
      { fullName: 'Lê Văn Đức', staffCode: 'STF003', email: 'duc.lv@ume.com', phoneNumber: '0903333333', position: 'Stylist', level: 'Senior', yearsOfExperience: 6, averageRating: 4.7, totalCustomersServed: 420, baseSalary: 11000000, status: 'Active', bio: 'Stylist sáng tạo, chuyên tạo kiểu lông thú cưng theo trend' },
      { fullName: 'Phạm Thành Long', staffCode: 'STF004', email: 'long.pt@ume.com', phoneNumber: '0904444444', position: 'Barber', level: 'Junior', yearsOfExperience: 2, averageRating: 4.2, totalCustomersServed: 150, baseSalary: 7000000, status: 'Active', bio: 'Thợ trẻ đầy nhiệt huyết, chăm chỉ học hỏi' },
      { fullName: 'Hoàng Thị Mai', staffCode: 'STF005', email: 'mai.ht@ume.com', phoneNumber: '0905555555', position: 'PetGroomer', level: 'Expert', yearsOfExperience: 10, averageRating: 4.9, totalCustomersServed: 680, baseSalary: 15000000, status: 'Active', bio: 'Chuyên gia chăm sóc thú cưng hàng đầu, 10 năm kinh nghiệm' },
      { fullName: 'Võ Thanh Sơn', staffCode: 'STF006', email: 'son.vt@ume.com', phoneNumber: '0906666666', position: 'Veterinarian', level: 'Senior', yearsOfExperience: 7, averageRating: 4.7, totalCustomersServed: 450, baseSalary: 14000000, status: 'Active', bio: 'Bác sĩ thú y, chuyên khám và tư vấn sức khỏe thú cưng' },
    ];
    const staffMembers = [];
    for (const s of staffData) {
      const st = await Staff.findOneAndUpdate(
        { staffCode: s.staffCode },
        { ...s, services: services.slice(0, 4).map(sv => sv._id), avatarUrl: `https://ui-avatars.com/api/?name=${encodeURIComponent(s.fullName)}&background=D4AF37&color=fff&size=200` },
        { upsert: true, new: true }
      );
      staffMembers.push(st);
    }
    console.log('✅ 6 staff members seeded');

    // ============================
    // 9. ORDERS
    // ============================
    const orderStatuses = ['Pending', 'Confirmed', 'Processing', 'Shipping', 'Completed', 'Cancelled'];
    const payStatuses = ['Unpaid', 'Paid', 'Paid', 'Unpaid', 'Paid', 'Unpaid'];
    for (let i = 0; i < 15; i++) {
      const cust = customers[i % customers.length];
      const status = orderStatuses[i % orderStatuses.length];
      const numItems = 1 + (i % 3);
      const items = [];
      for (let j = 0; j < numItems; j++) {
        const prod = products[(i + j) % products.length];
        const qty = 1 + (j % 3);
        items.push({
          product: prod._id,
          productName: prod.name,
          productImage: prod.imageUrl,
          sku: prod.sku,
          quantity: qty,
          unitPrice: prod.price,
          totalPrice: prod.price * qty
        });
      }
      const subtotal = items.reduce((s, it) => s + it.totalPrice, 0);
      const orderDate = new Date(2026, 0, 5 + i);
      await Order.findOneAndUpdate(
        { orderCode: `ORD-2026${String(i + 1).padStart(4, '0')}` },
        {
          orderCode: `ORD-2026${String(i + 1).padStart(4, '0')}`,
          customer: cust._id,
          items,
          subtotal,
          shippingFee: 30000,
          totalAmount: subtotal + 30000,
          status,
          paymentMethod: i % 3 === 0 ? 'COD' : 'BankTransfer',
          paymentStatus: payStatuses[i % payStatuses.length],
          shippingAddress: {
            fullName: cust.fullName,
            phone: cust.phoneNumber,
            address: cust.address || '123 Đường ABC',
            ward: 'Phường 1',
            district: 'Quận 1',
            city: 'TP.HCM'
          },
          notes: i % 4 === 0 ? 'Giao giờ hành chính' : '',
          createdAt: orderDate,
          updatedAt: orderDate
        },
        { upsert: true, new: true, timestamps: false }
      );
    }
    console.log('✅ 15 orders seeded');

    // ============================
    // 10. APPOINTMENTS
    // ============================
    const apptStatuses = ['Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled', 'Pending'];
    const times = ['09:00', '10:30', '13:00', '14:30', '16:00', '09:30'];
    for (let i = 0; i < 20; i++) {
      const cust = customers[i % customers.length];
      const staff = staffMembers[i % staffMembers.length];
      const status = apptStatuses[i % apptStatuses.length];
      const selSvcs = services.slice(i % 4, (i % 4) + 2);
      const apptServices = selSvcs.map(sv => ({
        service: sv._id,
        serviceName: sv.name,
        price: sv.price,
        duration: sv.duration || 30
      }));
      const totalAmount = apptServices.reduce((s, sv) => s + sv.price, 0);
      const apptDate = new Date(2026, 1, 1 + (i % 25));
      await Appointment.findOneAndUpdate(
        { appointmentCode: `APT-2026${String(i + 1).padStart(4, '0')}` },
        {
          appointmentCode: `APT-2026${String(i + 1).padStart(4, '0')}`,
          customer: cust._id,
          staff: staff._id,
          appointmentDate: apptDate,
          startTime: times[i % times.length],
          endTime: `${parseInt(times[i % times.length]) + 1}:${times[i % times.length].split(':')[1]}`,
          services: apptServices,
          totalAmount,
          finalAmount: totalAmount,
          status,
          paymentStatus: status === 'Completed' ? 'Paid' : 'Unpaid',
          paymentMethod: i % 2 === 0 ? 'Cash' : 'BankTransfer',
          notes: i % 5 === 0 ? 'Khách hàng VIP, ưu tiên' : '',
          createdAt: new Date(2026, 0, 25 + (i % 15)),
          updatedAt: new Date(2026, 0, 25 + (i % 15))
        },
        { upsert: true, new: true, timestamps: false }
      );
    }
    console.log('✅ 20 appointments seeded');

    // ============================
    // 11. PETS
    // ============================
    const petData = [
      { name: 'Buddy', type: 'Dog', breed: 'Golden Retriever', age: 2, ageUnit: 'years', weight: 28, gender: 'Male', color: 'Vàng', description: 'Chú chó Golden Retriever thân thiện, thích chơi đùa và rất ngoan ngoãn.', healthNotes: 'Sức khỏe tốt, đã tiêm phòng đầy đủ', vaccinated: true, neutered: true, microchipId: 'VN-DOG-001', listingType: 'Sale', listingPrice: 8500000, listingDescription: 'Bán chó Golden Retriever thuần chủng, có giấy tờ', imageUrl: 'https://images.unsplash.com/photo-1552053831-71594a27632d?w=400&h=400&fit=crop' },
      { name: 'Miu Miu', type: 'Cat', breed: 'Mèo Anh lông ngắn', age: 1, ageUnit: 'years', weight: 4.5, gender: 'Female', color: 'Xám xanh', description: 'Mèo Anh lông ngắn xinh đẹp, tính cách điềm đạm.', healthNotes: 'Khỏe mạnh, ăn uống tốt', vaccinated: true, neutered: false, microchipId: 'VN-CAT-001', listingType: 'Sale', listingPrice: 12000000, listingDescription: 'Mèo Anh lông ngắn màu xám xanh, thuần chủng', imageUrl: 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=400&h=400&fit=crop' },
      { name: 'Lucky', type: 'Dog', breed: 'Corgi', age: 8, ageUnit: 'months', weight: 10, gender: 'Male', color: 'Vàng trắng', description: 'Corgi chân ngắn siêu dễ thương, hoạt bát và thông minh.', healthNotes: 'Đã tiêm 3 mũi, tẩy giun định kỳ', vaccinated: true, neutered: false, microchipId: 'VN-DOG-002', listingType: 'Sale', listingPrice: 15000000, listingDescription: 'Corgi Pembroke thuần chủng nhập khẩu', imageUrl: 'https://images.unsplash.com/photo-1612536057832-2ff7ead58194?w=400&h=400&fit=crop' },
      { name: 'Bông', type: 'Cat', breed: 'Mèo Ba Tư', age: 3, ageUnit: 'years', weight: 5, gender: 'Female', color: 'Trắng', description: 'Mèo Ba Tư lông dài trắng muốt, mắt xanh biếc.', healthNotes: 'Cần chải lông thường xuyên', vaccinated: true, neutered: true, microchipId: 'VN-CAT-002', listingType: 'Adoption', listingPrice: 0, listingDescription: 'Tìm mái ấm mới cho bé Bông', imageUrl: 'https://images.unsplash.com/photo-1513245543132-31f507417b26?w=400&h=400&fit=crop' },
      { name: 'Rex', type: 'Dog', breed: 'Phú Quốc', age: 1, ageUnit: 'years', weight: 18, gender: 'Male', color: 'Vện', description: 'Chó Phú Quốc thuần chủng có xoáy lưng đặc trưng.', healthNotes: 'Sức khỏe tốt, năng động', vaccinated: true, neutered: false, microchipId: 'VN-DOG-003', listingType: 'Sale', listingPrice: 20000000, listingDescription: 'Chó Phú Quốc xoáy lưng đẹp, thuần chủng', imageUrl: 'https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=400&h=400&fit=crop' },
      { name: 'Sunny', type: 'Bird', breed: 'Vẹt Cockatiel', age: 6, ageUnit: 'months', weight: 0.1, gender: 'Male', color: 'Vàng xám', description: 'Vẹt Cockatiel đã thuần, biết huýt sáo.', healthNotes: 'Khỏe mạnh, lông đẹp', vaccinated: false, neutered: false, listingType: 'Sale', listingPrice: 3500000, listingDescription: 'Vẹt Cockatiel đã thuần, có lồng kèm', imageUrl: 'https://images.unsplash.com/photo-1552728089-57bdde30beb3?w=400&h=400&fit=crop' },
      { name: 'Kiki', type: 'Cat', breed: 'Mèo Munchkin', age: 5, ageUnit: 'months', weight: 2.5, gender: 'Female', color: 'Tam thể', description: 'Mèo Munchkin chân ngắn cực kỳ đáng yêu.', healthNotes: 'Đã tiêm 2 mũi', vaccinated: true, neutered: false, microchipId: 'VN-CAT-003', listingType: 'Sale', listingPrice: 18000000, listingDescription: 'Munchkin tam thể chân ngắn, siêu hiếm', imageUrl: 'https://images.unsplash.com/photo-1606214174585-fe31582dc6ee?w=400&h=400&fit=crop' },
      { name: 'Cún Con', type: 'Dog', breed: 'Poodle', age: 4, ageUnit: 'months', weight: 3, gender: 'Male', color: 'Nâu đỏ', description: 'Poodle Toy nâu đỏ siêu cute, không rụng lông.', healthNotes: 'Đã tiêm phòng đầy đủ', vaccinated: true, neutered: false, microchipId: 'VN-DOG-004', listingType: 'Sale', listingPrice: 9000000, listingDescription: 'Poodle Toy nâu đỏ, bố mẹ thuần chủng', imageUrl: 'https://images.unsplash.com/photo-1537151625747-768eb6cf92b2?w=400&h=400&fit=crop' },
      { name: 'Hammy', type: 'Hamster', breed: 'Hamster Winter White', age: 3, ageUnit: 'months', weight: 0.04, gender: 'Female', color: 'Trắng ngọc trai', description: 'Hamster Winter White nhỏ xinh, hiền lành.', healthNotes: 'Khỏe mạnh', vaccinated: false, neutered: false, listingType: 'Sale', listingPrice: 150000, listingDescription: 'Hamster WW pearl, tặng kèm lồng nhỏ', imageUrl: 'https://images.unsplash.com/photo-1425082661507-d6d2f66e5d56?w=400&h=400&fit=crop' },
      { name: 'Mập', type: 'Dog', breed: 'Shiba Inu', age: 1, ageUnit: 'years', weight: 10, gender: 'Male', color: 'Vàng lửa', description: 'Shiba Inu thuần Nhật, mặt cười dễ thương.', healthNotes: 'Đã tiêm phòng đầy đủ 5 bệnh', vaccinated: true, neutered: true, microchipId: 'VN-DOG-005', listingType: 'Sale', listingPrice: 25000000, listingDescription: 'Shiba Inu thuần Nhật, có VKA', imageUrl: 'https://images.unsplash.com/photo-1583337130417-13104dec14a8?w=400&h=400&fit=crop' },
      { name: 'Lulu', type: 'Rabbit', breed: 'Holland Lop', age: 4, ageUnit: 'months', weight: 1.5, gender: 'Female', color: 'Nâu trắng', description: 'Thỏ Holland Lop tai cụp siêu dễ thương.', healthNotes: 'Khỏe mạnh', vaccinated: false, neutered: false, listingType: 'Adoption', listingPrice: 0, listingDescription: 'Cho bé thỏ Holland Lop, ai yêu thương xin liên hệ', imageUrl: 'https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=400&h=400&fit=crop' },
      { name: 'Vàng', type: 'Dog', breed: 'Chó ta', age: 2, ageUnit: 'years', weight: 15, gender: 'Female', color: 'Vàng', description: 'Chó ta khỏe mạnh, trung thành.', healthNotes: 'Đã tiêm phòng dại', vaccinated: true, neutered: true, microchipId: 'VN-DOG-006', listingType: 'Adoption', listingPrice: 0, listingDescription: 'Tìm chủ mới cho bé Vàng', imageUrl: 'https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=400&h=400&fit=crop' },
      { name: 'Mochi', type: 'Cat', breed: 'Mèo Scottish Fold', age: 7, ageUnit: 'months', weight: 3.5, gender: 'Male', color: 'Xám bạc', description: 'Scottish Fold tai cụp, mắt tròn to, cực kỳ dễ thương.', healthNotes: 'Sức khỏe tốt, đã tiêm phòng', vaccinated: true, neutered: false, microchipId: 'VN-CAT-004', listingType: 'Sale', listingPrice: 22000000, listingDescription: 'Scottish Fold xám bạc, thuần chủng', imageUrl: 'https://images.unsplash.com/photo-1574158622682-e40e69881006?w=400&h=400&fit=crop' },
      { name: 'Choco', type: 'Dog', breed: 'Labrador Retriever', age: 1, ageUnit: 'years', weight: 25, gender: 'Male', color: 'Nâu chocolate', description: 'Labrador nâu chocolate, rất thân thiện và năng động.', healthNotes: 'Đã tiêm phòng đầy đủ', vaccinated: true, neutered: false, microchipId: 'VN-DOG-007', listingType: 'Sale', listingPrice: 10000000, listingDescription: 'Labrador Retriever chocolate, thuần chủng', imageUrl: 'https://images.unsplash.com/photo-1579213838058-60e482a74d02?w=400&h=400&fit=crop' },
      { name: 'Mèo Mun', type: 'Cat', breed: 'Mèo đen', age: 2, ageUnit: 'years', weight: 4, gender: 'Female', color: 'Đen', description: 'Mèo đen tuyền, mắt vàng, rất bí ẩn và đáng yêu.', healthNotes: 'Khỏe mạnh, ăn uống tốt', vaccinated: true, neutered: true, listingType: 'Adoption', listingPrice: 0, listingDescription: 'Cho mèo đen 2 tuổi, rất hiền', imageUrl: 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=400&h=400&fit=crop' },
    ];
    for (let i = 0; i < petData.length; i++) {
      const ownerIdx = i % customers.length;
      await Pet.findOneAndUpdate(
        { name: petData[i].name, owner: customers[ownerIdx]._id },
        { ...petData[i], owner: customers[ownerIdx]._id, listingStatus: 'Active', isDeleted: false },
        { upsert: true, new: true }
      );
    }
    console.log('✅ 15 pets seeded');

    // ============================
    // 12. SETTINGS
    // ============================
    await Settings.findOneAndUpdate(
      {},
      {
        storeName: 'UME Pet Salon',
        storeDescription: 'Salon chăm sóc thú cưng chuyên nghiệp hàng đầu tại TP.HCM. Dịch vụ grooming, tắm spa, khám sức khỏe và cung cấp sản phẩm chất lượng cho thú cưng của bạn.',
        address: '123 Đường Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh',
        phone: '0384 731 104',
        email: 'bezubts@gmail.com',
        workingHours: '8:00 - 20:00 (Thứ 2 - Chủ nhật)',
        facebook: 'https://facebook.com/umepetsalon',
        instagram: 'https://instagram.com/umepetsalon',
        tiktok: 'https://tiktok.com/@umepetsalon',
        zalo: '0384731104',
        mapEmbedUrl: 'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.5!2d106.7!3d10.78!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zMTDCsDQ2JzQ4LjAiTiAxMDbCsDQyJzAwLjAiRQ!5e0!3m2!1svi!2s!4v1',
        shippingStandardPrice: 30000,
        shippingExpressPrice: 50000,
        freeShipStandardThreshold: 500000,
        freeShipExpressThreshold: 1000000,
        shippingPolicy: 'Miễn phí vận chuyển cho đơn hàng từ 500.000đ. Giao hàng trong 2-3 ngày làm việc tại TP.HCM.',
        returnPolicy: 'Hỗ trợ đổi trả trong 7 ngày nếu sản phẩm lỗi từ nhà sản xuất. Sản phẩm phải còn nguyên tem, nhãn mác.',
        codEnabled: true,
        codDescription: 'Thanh toán bằng tiền mặt khi nhận hàng',
        bankTransferEnabled: true,
        bankName: 'Ngân hàng Vietcombank',
        bankAccountNumber: '1234567890',
        bankAccountHolder: 'UME PET SALON',
        bankBranch: 'Chi nhánh TP.HCM',
        bankDescription: 'Chuyển khoản trước khi giao hàng. Ghi nội dung: [Mã đơn hàng] + [Tên người đặt]',
      },
      { upsert: true, new: true }
    );
    console.log('✅ Settings seeded');

    // ============================
    // 13. PROMOTIONS
    // ============================
    const promotionData = [
      { code: 'WELCOME10', name: 'Chào mừng khách mới', description: 'Giảm 10% cho khách hàng mới đăng ký', type: 'Percentage', value: 10, minOrderAmount: 200000, maxDiscountAmount: 100000, usageLimit: 100, usedCount: 23, perUserLimit: 1, startDate: new Date(2026, 0, 1), endDate: new Date(2026, 11, 31), isActive: true },
      { code: 'FREESHIP', name: 'Miễn phí vận chuyển', description: 'Miễn phí vận chuyển cho mọi đơn hàng', type: 'FreeShipping', value: 0, minOrderAmount: 100000, usageLimit: 200, usedCount: 55, perUserLimit: 3, startDate: new Date(2026, 0, 1), endDate: new Date(2026, 5, 30), isActive: true },
      { code: 'TET2026', name: 'Khuyến mãi Tết 2026', description: 'Giảm 15% mừng Tết Nguyên Đán', type: 'Percentage', value: 15, minOrderAmount: 300000, maxDiscountAmount: 200000, usageLimit: 50, usedCount: 48, perUserLimit: 1, startDate: new Date(2026, 0, 15), endDate: new Date(2026, 1, 15), isActive: false },
      { code: 'GIAM50K', name: 'Giảm 50K', description: 'Giảm trực tiếp 50.000đ cho đơn từ 500K', type: 'FixedAmount', value: 50000, minOrderAmount: 500000, usageLimit: 100, usedCount: 30, perUserLimit: 2, startDate: new Date(2026, 1, 1), endDate: new Date(2026, 6, 31), isActive: true },
      { code: 'SUMMER20', name: 'Hè vui vẻ', description: 'Giảm 20% cho dịch vụ spa và grooming mùa hè', type: 'Percentage', value: 20, minOrderAmount: 0, maxDiscountAmount: 300000, usageLimit: 80, usedCount: 5, perUserLimit: 2, startDate: new Date(2026, 5, 1), endDate: new Date(2026, 7, 31), isActive: true },
      { code: 'VIP100K', name: 'Ưu đãi VIP', description: 'Giảm 100K cho khách hàng VIP, đơn từ 1 triệu', type: 'FixedAmount', value: 100000, minOrderAmount: 1000000, usageLimit: 30, usedCount: 8, perUserLimit: 1, startDate: new Date(2026, 0, 1), endDate: new Date(2026, 11, 31), isActive: true },
    ];
    for (const promo of promotionData) {
      await Promotion.findOneAndUpdate(
        { code: promo.code },
        promo,
        { upsert: true, new: true }
      );
    }
    console.log('✅ 6 promotions seeded');

    // ============================
    // 14. REVIEWS
    // ============================
    const reviewComments = [
      { rating: 5, title: 'Tuyệt vời!', comment: 'Sản phẩm chất lượng, giao hàng nhanh. Rất hài lòng!' },
      { rating: 4, title: 'Khá tốt', comment: 'Sản phẩm đúng mô tả, bé nhà mình rất thích.' },
      { rating: 5, title: 'Xuất sắc', comment: 'Mua lần 2 rồi, chất lượng luôn ổn định. Sẽ tiếp tục ủng hộ!' },
      { rating: 3, title: 'Bình thường', comment: 'Sản phẩm tạm ổn, giao hàng hơi chậm.' },
      { rating: 5, title: 'Rất chuyên nghiệp', comment: 'Nhân viên thân thiện, dịch vụ tốt lắm!' },
      { rating: 4, title: 'Hài lòng', comment: 'Bé cún nhà mình sau khi grooming trông đẹp hẳn.' },
      { rating: 5, title: 'Đáng tiền', comment: 'Giá cả hợp lý, chất lượng tuyệt vời.' },
      { rating: 4, title: 'OK', comment: 'Mọi thứ đều ổn, sẽ quay lại.' },
      { rating: 5, title: 'Perfect!', comment: 'Bé mèo nhà mình cực thích, ăn hết sạch!' },
      { rating: 3, title: 'Cũng được', comment: 'Chất lượng tạm ổn so với giá tiền.' },
    ];
    for (let i = 0; i < 20; i++) {
      const cust = customers[i % customers.length];
      const rv = reviewComments[i % reviewComments.length];
      const reviewData = {
        customer: cust._id,
        rating: rv.rating,
        title: rv.title,
        comment: rv.comment,
        isVerifiedPurchase: true,
        isApproved: true,
        helpfulCount: Math.floor(Math.random() * 10),
        isDeleted: false,
        createdAt: new Date(2026, 0, 10 + i),
        updatedAt: new Date(2026, 0, 10 + i)
      };

      // Alternate between product reviews and service reviews
      if (i % 2 === 0) {
        reviewData.product = products[i % products.length]._id;
      } else {
        reviewData.service = services[i % services.length]._id;
        reviewData.staff = staffMembers[i % staffMembers.length]._id;
      }

      // Add admin reply to some reviews
      if (i % 3 === 0) {
        reviewData.reply = 'Cảm ơn bạn đã đánh giá! Chúng tôi rất vui khi bạn hài lòng với dịch vụ. 🐾';
        reviewData.repliedAt = new Date(2026, 0, 11 + i);
        reviewData.repliedBy = admin._id;
      }

      await Review.findOneAndUpdate(
        { customer: reviewData.customer, product: reviewData.product, service: reviewData.service, createdAt: reviewData.createdAt },
        reviewData,
        { upsert: true, new: true, timestamps: false }
      );
    }
    console.log('✅ 20 reviews seeded');

    // ============================
    // 15. NOTIFICATIONS
    // ============================
    const notifData = [
      { type: 'Order', title: 'Đơn hàng mới', message: 'Bạn có đơn hàng mới #ORD-20260001 cần xử lý' },
      { type: 'Appointment', title: 'Lịch hẹn mới', message: 'Bạn có lịch hẹn mới #APT-20260001 vào lúc 09:00' },
      { type: 'Promotion', title: 'Khuyến mãi mới', message: 'Giảm 10% cho khách hàng mới với mã WELCOME10!' },
      { type: 'System', title: 'Chào mừng!', message: 'Chào mừng bạn đến với UME Pet Salon. Khám phá dịch vụ ngay!' },
      { type: 'Order', title: 'Đơn hàng đã giao', message: 'Đơn hàng #ORD-20260005 đã được giao thành công' },
      { type: 'Review', title: 'Đánh giá mới', message: 'Khách hàng vừa đánh giá 5 sao cho dịch vụ Grooming' },
      { type: 'Appointment', title: 'Nhắc lịch hẹn', message: 'Bạn có lịch hẹn vào ngày mai lúc 10:30. Đừng quên nhé!' },
      { type: 'Pet', title: 'Thú cưng mới', message: 'Có thú cưng mới được đăng bán trên hệ thống' },
    ];

    // Notifications for admin
    for (let i = 0; i < notifData.length; i++) {
      await Notification.findOneAndUpdate(
        { recipient: admin._id, title: notifData[i].title, message: notifData[i].message },
        {
          recipient: admin._id,
          ...notifData[i],
          isRead: i < 3,
          readAt: i < 3 ? new Date(2026, 1, 1) : null,
          createdAt: new Date(2026, 0, 20 + i),
          updatedAt: new Date(2026, 0, 20 + i)
        },
        { upsert: true, new: true, timestamps: false }
      );
    }

    // Notifications for customers
    for (let i = 0; i < customers.length; i++) {
      await Notification.findOneAndUpdate(
        { recipient: customers[i]._id, type: 'System', title: 'Chào mừng!' },
        {
          recipient: customers[i]._id,
          type: 'System',
          title: 'Chào mừng!',
          message: `Chào mừng ${customers[i].fullName} đến với UME Pet Salon! Khám phá dịch vụ và sản phẩm ngay.`,
          isRead: false,
          createdAt: new Date(2026, 0, 15 + i),
          updatedAt: new Date(2026, 0, 15 + i)
        },
        { upsert: true, new: true, timestamps: false }
      );

      // Order notification
      await Notification.findOneAndUpdate(
        { recipient: customers[i]._id, type: 'Promotion', title: 'Ưu đãi dành cho bạn' },
        {
          recipient: customers[i]._id,
          type: 'Promotion',
          title: 'Ưu đãi dành cho bạn',
          message: `Nhập mã WELCOME10 để được giảm 10% cho đơn hàng đầu tiên!`,
          isRead: i < 3,
          createdAt: new Date(2026, 0, 16 + i),
          updatedAt: new Date(2026, 0, 16 + i)
        },
        { upsert: true, new: true, timestamps: false }
      );
    }
    console.log('✅ Notifications seeded');

    // ============================
    // DONE
    // ============================
    console.log('\n========================================');
    console.log('🎉 TOÀN BỘ DỮ LIỆU ĐÃ ĐƯỢC TẠO ĐẦY ĐỦ!');
    console.log('========================================');
    console.log('📊 Tổng kết:');
    console.log('  - 1 Admin (admin@ume.com / Admin@123)');
    console.log('  - 8 Khách hàng (User@123)');
    console.log('  - 6 Danh mục sản phẩm');
    console.log('  - 6 Thương hiệu');
    console.log('  - 4 Danh mục dịch vụ');
    console.log('  - 12 Dịch vụ');
    console.log('  - 16 Sản phẩm');
    console.log('  - 6 Nhân viên');
    console.log('  - 15 Đơn hàng');
    console.log('  - 20 Lịch hẹn');
    console.log('  - 15 Thú cưng');
    console.log('  - 1 Cấu hình cửa hàng (Settings)');
    console.log('  - 6 Khuyến mãi');
    console.log('  - 20 Đánh giá');
    console.log('  - Thông báo cho admin + khách hàng');
    console.log('========================================');

    process.exit(0);
  } catch (error) {
    console.error('❌ Error:', error.message);
    console.error(error.stack);
    process.exit(1);
  }
};

seedComplete();
