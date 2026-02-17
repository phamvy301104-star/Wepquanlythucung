require('dotenv').config();
const mongoose = require('mongoose');
const User = require('./models/User');
const Category = require('./models/Category');
const Brand = require('./models/Brand');
const Product = require('./models/Product');
const Staff = require('./models/Staff');
const Service = require('./models/Service');
const Order = require('./models/Order');
const Appointment = require('./models/Appointment');
const Pet = require('./models/Pet');

const seedAll = async () => {
  try {
    await mongoose.connect(process.env.MONGODB_URI);
    console.log('Connected to MongoDB');

    // Get refs
    const admin = await User.findOne({ email: 'admin@ume.com' });
    const cats = await Category.find({});
    const brands = await Brand.find({});
    const services = await Service.find({});

    if (!admin) { console.error('Admin not found, run seed.js first'); process.exit(1); }

    // === Create test customers ===
    const customerData = [
      { email: 'nguyenvana@gmail.com', fullName: 'Nguyễn Văn A', phoneNumber: '0901234567', role: 'Customer' },
      { email: 'tranthib@gmail.com', fullName: 'Trần Thị B', phoneNumber: '0912345678', role: 'Customer' },
      { email: 'lequangc@gmail.com', fullName: 'Lê Quang C', phoneNumber: '0923456789', role: 'Customer' },
      { email: 'phamthid@gmail.com', fullName: 'Phạm Thị D', phoneNumber: '0934567890', role: 'Customer' },
      { email: 'hoangmine@gmail.com', fullName: 'Hoàng Minh E', phoneNumber: '0945678901', role: 'Customer' },
    ];
    const customers = [];
    for (const c of customerData) {
      const u = await User.findOneAndUpdate({ email: c.email }, { ...c, password: 'User@123', isActive: true }, { upsert: true, new: true });
      customers.push(u);
    }
    console.log('✅ 5 customers seeded');

    // === Products ===
    const productData = [
      { name: 'Royal Canin Maxi Adult', sku: 'RC-MA-01', price: 450000, originalPrice: 520000, stockQuantity: 50, soldCount: 120, category: cats[0]?._id, brand: brands[0]?._id, description: 'Thức ăn hạt cho chó lớn trên 26 tháng tuổi', isFeatured: true, averageRating: 4.5 },
      { name: 'Whiskas Tuna Adult', sku: 'WK-TA-01', price: 89000, originalPrice: 99000, stockQuantity: 100, soldCount: 250, category: cats[0]?._id, brand: brands[1]?._id, description: 'Thức ăn ướt vị cá ngừ cho mèo trưởng thành', averageRating: 4.2 },
      { name: 'Pedigree Puppy', sku: 'PD-PP-01', price: 180000, originalPrice: 210000, stockQuantity: 75, soldCount: 85, category: cats[0]?._id, brand: brands[2]?._id, description: 'Thức ăn hạt cho chó con', isFeatured: true, averageRating: 4.3 },
      { name: 'Me-O Salmon', sku: 'MO-SA-01', price: 65000, originalPrice: 75000, stockQuantity: 200, soldCount: 310, category: cats[0]?._id, brand: brands[3]?._id, description: 'Thức ăn hạt vị cá hồi cho mèo', averageRating: 4.0 },
      { name: 'Vòng cổ LED cho chó', sku: 'PK-LED-01', price: 120000, originalPrice: 150000, stockQuantity: 30, soldCount: 45, category: cats[1]?._id, brand: brands[5]?._id, description: 'Vòng cổ phát sáng LED đổi màu', averageRating: 4.7 },
      { name: 'Dây dắt tự động 5m', sku: 'PK-DD-01', price: 250000, originalPrice: 280000, stockQuantity: 20, soldCount: 35, category: cats[1]?._id, brand: brands[5]?._id, description: 'Dây dắt tự cuốn 5m cho chó dưới 20kg', averageRating: 4.1 },
      { name: 'Bóng cao su cho chó', sku: 'DC-BC-01', price: 35000, originalPrice: 45000, stockQuantity: 150, soldCount: 200, category: cats[2]?._id, brand: brands[4]?._id, description: 'Bóng cao su bền, an toàn cho chó', averageRating: 4.4 },
      { name: 'Cần câu lông cho mèo', sku: 'DC-CL-01', price: 55000, originalPrice: 65000, stockQuantity: 80, soldCount: 175, category: cats[2]?._id, brand: brands[4]?._id, description: 'Cần câu lông vũ kích thích bản năng săn mồi', averageRating: 4.6 },
      { name: 'Sữa tắm SOS cho chó', sku: 'VS-ST-01', price: 95000, originalPrice: 110000, stockQuantity: 60, soldCount: 150, category: cats[4]?._id, brand: brands[5]?._id, description: 'Sữa tắm dưỡng lông, khử mùi cho chó', averageRating: 4.3, isFeatured: true },
      { name: 'Áo hoodie cho chó', sku: 'QA-HD-01', price: 160000, originalPrice: 200000, stockQuantity: 40, soldCount: 55, category: cats[5]?._id, brand: brands[5]?._id, description: 'Áo hoodie ấm áp cho chó nhỏ', averageRating: 4.5 },
    ];
    const toSlug = (s) => s.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
    const products = [];
    for (const p of productData) {
      const prod = await Product.findOneAndUpdate({ sku: p.sku }, { ...p, slug: toSlug(p.name), isActive: true, imageUrl: `https://picsum.photos/seed/${p.sku}/300/300` }, { upsert: true, new: true });
      products.push(prod);
    }
    console.log('✅ 10 products seeded');

    // === Staff ===
    const staffData = [
      { fullName: 'Nguyễn Minh Tuấn', staffCode: 'STF001', email: 'tuan.nv@ume.com', phoneNumber: '0901111111', position: 'Barber', level: 'Master', yearsOfExperience: 8, averageRating: 4.8, totalCustomersServed: 520, baseSalary: 12000000, status: 'Active', bio: 'Thợ cắt tóc chuyên nghiệp với 8 năm kinh nghiệm' },
      { fullName: 'Trần Thị Hương', staffCode: 'STF002', email: 'huong.tt@ume.com', phoneNumber: '0902222222', position: 'PetGroomer', level: 'Senior', yearsOfExperience: 5, averageRating: 4.6, totalCustomersServed: 380, baseSalary: 10000000, status: 'Active', bio: 'Chuyên gia grooming thú cưng' },
      { fullName: 'Lê Văn Đức', staffCode: 'STF003', email: 'duc.lv@ume.com', phoneNumber: '0903333333', position: 'Stylist', level: 'Senior', yearsOfExperience: 6, averageRating: 4.7, totalCustomersServed: 420, baseSalary: 11000000, status: 'Active', bio: 'Stylist sáng tạo, chuyên kiểu tóc hiện đại' },
      { fullName: 'Phạm Thành Long', staffCode: 'STF004', email: 'long.pt@ume.com', phoneNumber: '0904444444', position: 'Barber', level: 'Junior', yearsOfExperience: 2, averageRating: 4.2, totalCustomersServed: 150, baseSalary: 7000000, status: 'Active', bio: 'Thợ trẻ đầy nhiệt huyết' },
      { fullName: 'Hoàng Thị Mai', staffCode: 'STF005', email: 'mai.ht@ume.com', phoneNumber: '0905555555', position: 'PetGroomer', level: 'Expert', yearsOfExperience: 10, averageRating: 4.9, totalCustomersServed: 680, baseSalary: 15000000, status: 'OnLeave', bio: 'Chuyên gia chăm sóc thú cưng hàng đầu' },
    ];
    const staffMembers = [];
    for (const s of staffData) {
      const st = await Staff.findOneAndUpdate({ staffCode: s.staffCode }, { ...s, services: services.slice(0, 3).map(sv => sv._id), avatarUrl: `https://ui-avatars.com/api/?name=${encodeURIComponent(s.fullName)}&background=D4AF37&color=fff&size=100` }, { upsert: true, new: true });
      staffMembers.push(st);
    }
    console.log('✅ 5 staff members seeded');

    // === Orders ===
    const statuses = ['Pending', 'Confirmed', 'Processing', 'Shipping', 'Completed', 'Cancelled'];
    const payStatuses = ['Unpaid', 'Paid', 'Paid', 'Unpaid', 'Paid', 'Unpaid'];
    for (let i = 0; i < 12; i++) {
      const cust = customers[i % customers.length];
      const status = statuses[i % statuses.length];
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
      const orderDate = new Date(2026, 1, 1 + i);
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
          paymentMethod: i % 2 === 0 ? 'COD' : 'BankTransfer',
          paymentStatus: payStatuses[i % payStatuses.length],
          shippingAddress: { fullName: cust.fullName, phone: cust.phoneNumber, address: '123 Đường ABC', ward: 'Phường 1', district: 'Quận 1', city: 'TP.HCM' },
          createdAt: orderDate,
          updatedAt: orderDate
        },
        { upsert: true, new: true, timestamps: false }
      );
    }
    console.log('✅ 12 orders seeded');

    // === Appointments ===
    const apptStatuses = ['Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled', 'Pending'];
    const times = ['09:00', '10:30', '13:00', '14:30', '16:00', '09:30'];
    for (let i = 0; i < 15; i++) {
      const cust = customers[i % customers.length];
      const staff = staffMembers[i % staffMembers.length];
      const status = apptStatuses[i % apptStatuses.length];
      const selSvcs = services.slice(i % 3, (i % 3) + 2);
      const apptServices = selSvcs.map(sv => ({
        service: sv._id,
        serviceName: sv.name,
        price: sv.price,
        duration: sv.duration || 30
      }));
      const totalAmount = apptServices.reduce((s, sv) => s + sv.price, 0);
      const apptDate = new Date(2026, 1, 10 + (i % 20));
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
          paymentMethod: 'Cash',
          notes: i % 3 === 0 ? 'Khách hàng VIP' : '',
          createdAt: new Date(2026, 1, 8 + i),
          updatedAt: new Date(2026, 1, 8 + i)
        },
        { upsert: true, new: true, timestamps: false }
      );
    }
    console.log('✅ 15 appointments seeded');

    // === Pets ===
    const petData = [
      {
        name: 'Buddy', type: 'Dog', breed: 'Golden Retriever', age: 2, ageUnit: 'years',
        weight: 28, gender: 'Male', color: 'Vàng', description: 'Chú chó Golden Retriever thân thiện, thích chơi đùa và rất ngoan ngoãn. Đã được huấn luyện cơ bản.',
        healthNotes: 'Sức khỏe tốt, đã tiêm phòng đầy đủ', vaccinated: true, neutered: true, microchipId: 'VN-DOG-001',
        listingType: 'Sale', listingPrice: 8500000, listingDescription: 'Bán chó Golden Retriever thuần chủng, có giấy tờ',
        imageUrl: 'https://images.unsplash.com/photo-1552053831-71594a27632d?w=400&h=400&fit=crop'
      },
      {
        name: 'Miu Miu', type: 'Cat', breed: 'Mèo Anh lông ngắn', age: 1, ageUnit: 'years',
        weight: 4.5, gender: 'Female', color: 'Xám xanh', description: 'Mèo Anh lông ngắn (British Shorthair) xinh đẹp, tính cách điềm đạm, thích được vuốt ve.',
        healthNotes: 'Khỏe mạnh, ăn uống tốt', vaccinated: true, neutered: false, microchipId: 'VN-CAT-001',
        listingType: 'Sale', listingPrice: 12000000, listingDescription: 'Mèo Anh lông ngắn màu xám xanh, thuần chủng',
        imageUrl: 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=400&h=400&fit=crop'
      },
      {
        name: 'Lucky', type: 'Dog', breed: 'Corgi', age: 8, ageUnit: 'months',
        weight: 10, gender: 'Male', color: 'Vàng trắng', description: 'Corgi chân ngắn siêu dễ thương, hoạt bát và thông minh. Mông cực tròn!',
        healthNotes: 'Đã tiêm 3 mũi, tẩy giun định kỳ', vaccinated: true, neutered: false, microchipId: 'VN-DOG-002',
        listingType: 'Sale', listingPrice: 15000000, listingDescription: 'Corgi Pembroke thuần chủng nhập khẩu',
        imageUrl: 'https://images.unsplash.com/photo-1612536057832-2ff7ead58194?w=400&h=400&fit=crop'
      },
      {
        name: 'Bông', type: 'Cat', breed: 'Mèo Ba Tư', age: 3, ageUnit: 'years',
        weight: 5, gender: 'Female', color: 'Trắng', description: 'Mèo Ba Tư lông dài trắng muốt, mắt xanh biếc. Rất dịu dàng và thích nằm phơi nắng.',
        healthNotes: 'Cần chải lông thường xuyên', vaccinated: true, neutered: true, microchipId: 'VN-CAT-002',
        listingType: 'Adoption', listingPrice: 0, listingDescription: 'Tìm mái ấm mới cho bé Bông, chủ cũ chuyển nước ngoài',
        imageUrl: 'https://images.unsplash.com/photo-1513245543132-31f507417b26?w=400&h=400&fit=crop'
      },
      {
        name: 'Rex', type: 'Dog', breed: 'Phú Quốc', age: 1, ageUnit: 'years',
        weight: 18, gender: 'Male', color: 'Vện', description: 'Chó Phú Quốc thuần chủng có xoáy lưng đặc trưng. Rất trung thành và dũng cảm.',
        healthNotes: 'Sức khỏe tốt, năng động', vaccinated: true, neutered: false, microchipId: 'VN-DOG-003',
        listingType: 'Sale', listingPrice: 20000000, listingDescription: 'Chó Phú Quốc xoáy lưng đẹp, thuần chủng',
        imageUrl: 'https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=400&h=400&fit=crop'
      },
      {
        name: 'Sunny', type: 'Bird', breed: 'Vẹt Cockatiel', age: 6, ageUnit: 'months',
        weight: 0.1, gender: 'Male', color: 'Vàng xám', description: 'Vẹt Cockatiel đã thuần, biết huýt sáo vài bài hát. Rất thích tương tác với người.',
        healthNotes: 'Khỏe mạnh, lông đẹp', vaccinated: false, neutered: false,
        listingType: 'Sale', listingPrice: 3500000, listingDescription: 'Vẹt Cockatiel đã thuần, có lồng kèm',
        imageUrl: 'https://images.unsplash.com/photo-1552728089-57bdde30beb3?w=400&h=400&fit=crop'
      },
      {
        name: 'Kiki', type: 'Cat', breed: 'Mèo Munchkin', age: 5, ageUnit: 'months',
        weight: 2.5, gender: 'Female', color: 'Tam thể', description: 'Mèo Munchkin chân ngắn cực kỳ đáng yêu. Màu tam thể hiếm, tính cách vui vẻ.',
        healthNotes: 'Đã tiêm 2 mũi, cần tiêm thêm 1 mũi', vaccinated: true, neutered: false, microchipId: 'VN-CAT-003',
        listingType: 'Sale', listingPrice: 18000000, listingDescription: 'Munchkin tam thể chân ngắn, siêu hiếm',
        imageUrl: 'https://images.unsplash.com/photo-1606214174585-fe31582dc6ee?w=400&h=400&fit=crop'
      },
      {
        name: 'Cún Con', type: 'Dog', breed: 'Poodle', age: 4, ageUnit: 'months',
        weight: 3, gender: 'Male', color: 'Nâu đỏ', description: 'Poodle Toy nâu đỏ siêu cute, không rụng lông, phù hợp nuôi trong chung cư.',
        healthNotes: 'Đã tiêm phòng đầy đủ, tẩy giun', vaccinated: true, neutered: false, microchipId: 'VN-DOG-004',
        listingType: 'Sale', listingPrice: 9000000, listingDescription: 'Poodle Toy nâu đỏ, bố mẹ thuần chủng',
        imageUrl: 'https://images.unsplash.com/photo-1537151625747-768eb6cf92b2?w=400&h=400&fit=crop'
      },
      {
        name: 'Hammy', type: 'Hamster', breed: 'Hamster Winter White', age: 3, ageUnit: 'months',
        weight: 0.04, gender: 'Female', color: 'Trắng ngọc trai', description: 'Hamster Winter White nhỏ xinh, hiền lành, dễ nuôi. Phù hợp cho người mới.',
        healthNotes: 'Khỏe mạnh', vaccinated: false, neutered: false,
        listingType: 'Sale', listingPrice: 150000, listingDescription: 'Hamster WW pearl, tặng kèm lồng nhỏ',
        imageUrl: 'https://images.unsplash.com/photo-1425082661507-d6d2f66e5d56?w=400&h=400&fit=crop'
      },
      {
        name: 'Mập', type: 'Dog', breed: 'Shiba Inu', age: 1, ageUnit: 'years',
        weight: 10, gender: 'Male', color: 'Vàng lửa', description: 'Shiba Inu thuần Nhật, mặt cười dễ thương. Đã được huấn luyện ngồi, bắt tay, nằm.',
        healthNotes: 'Đã tiêm phòng đầy đủ 5 bệnh', vaccinated: true, neutered: true, microchipId: 'VN-DOG-005',
        listingType: 'Sale', listingPrice: 25000000, listingDescription: 'Shiba Inu thuần Nhật, có VKA',
        imageUrl: 'https://images.unsplash.com/photo-1583337130417-13104dec14a8?w=400&h=400&fit=crop'
      },
      {
        name: 'Lulu', type: 'Rabbit', breed: 'Holland Lop', age: 4, ageUnit: 'months',
        weight: 1.5, gender: 'Female', color: 'Nâu trắng', description: 'Thỏ Holland Lop tai cụp siêu dễ thương, hiền lành và thích được ôm ấp.',
        healthNotes: 'Khỏe mạnh, ăn rau cỏ tươi', vaccinated: false, neutered: false,
        listingType: 'Adoption', listingPrice: 0, listingDescription: 'Cho bé thỏ Holland Lop, ai yêu thương bé xin liên hệ',
        imageUrl: 'https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=400&h=400&fit=crop'
      },
      {
        name: 'Vàng', type: 'Dog', breed: 'Chó ta', age: 2, ageUnit: 'years',
        weight: 15, gender: 'Female', color: 'Vàng', description: 'Chó ta khỏe mạnh, trung thành, đã được nuôi từ nhỏ. Rất giữ nhà và thân thiện với trẻ em.',
        healthNotes: 'Đã tiêm phòng dại, sức khỏe tốt', vaccinated: true, neutered: true, microchipId: 'VN-DOG-006',
        listingType: 'Adoption', listingPrice: 0, listingDescription: 'Tìm chủ mới cho bé Vàng do gia đình chuyển nhà',
        imageUrl: 'https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=400&h=400&fit=crop'
      }
    ];

    for (let i = 0; i < petData.length; i++) {
      const ownerIdx = i % customers.length;
      await Pet.findOneAndUpdate(
        { name: petData[i].name, owner: customers[ownerIdx]._id },
        { ...petData[i], owner: customers[ownerIdx]._id, listingStatus: 'Active', isDeleted: false },
        { upsert: true, new: true }
      );
    }
    console.log('✅ 12 pets seeded');

    console.log('\n🎉 Full seed completed!');
    process.exit(0);
  } catch (error) {
    console.error('❌ Error:', error.message);
    process.exit(1);
  }
};

seedAll();
