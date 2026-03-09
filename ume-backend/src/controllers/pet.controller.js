const Pet = require('../models/Pet');

exports.getAll = async (req, res) => {
  try {
    const { type, listingType, search, page = 1, limit = 20 } = req.query;
    const query = { isDeleted: false };

    // Non-admin/staff users only see listed pets (marketplace)
    const isStaffOrAdmin = req.user && (req.user.role === 'Admin' || req.user.role === 'Staff');
    if (!isStaffOrAdmin) {
      query.listingType = { $ne: 'None' };
      query.listingStatus = 'Active';
    }

    if (type) query.type = type;
    if (listingType) query.listingType = listingType;
    if (search) {
      query.$or = [
        { name: { $regex: search, $options: 'i' } },
        { breed: { $regex: search, $options: 'i' } }
      ];
    }

    const total = await Pet.countDocuments(query);
    const pets = await Pet.find(query)
      .populate('owner', 'fullName avatarUrl')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { pets, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getById = async (req, res) => {
  try {
    const pet = await Pet.findById(req.params.id).populate('owner', 'fullName email phoneNumber avatarUrl');
    if (!pet || pet.isDeleted) return res.status(404).json({ success: false, message: 'Không tìm thấy thú cưng' });
    res.json({ success: true, data: pet });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const petData = { ...req.body, owner: req.userId };
    // Customers can only save their own pets, not create listings
    if (req.user.role === 'Customer') {
      petData.listingType = 'None';
      petData.listingStatus = 'Inactive';
    }
    const pet = new Pet(petData);
    if (req.file) pet.imageUrl = `/uploads/pets/${req.file.filename}`;
    await pet.save();
    res.status(201).json({ success: true, message: 'Thêm thú cưng thành công', data: pet });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.update = async (req, res) => {
  try {
    const pet = await Pet.findById(req.params.id);
    if (!pet || pet.isDeleted) return res.status(404).json({ success: false, message: 'Không tìm thấy thú cưng' });

    if (pet.owner.toString() !== req.userId.toString() && req.user.role !== 'Admin' && req.user.role !== 'Staff') {
      return res.status(403).json({ success: false, message: 'Không có quyền chỉnh sửa' });
    }

    const allowedFields = [
      'name', 'type', 'breed', 'age', 'ageUnit', 'weight', 'weightUnit',
      'gender', 'color', 'description', 'healthNotes', 'allergies',
      'vaccinated', 'neutered', 'microchipId', 'dateOfBirth',
      'listingType', 'listingPrice', 'listingDescription', 'listingStatus'
    ];
    allowedFields.forEach(field => {
      if (req.body[field] !== undefined) pet[field] = req.body[field];
    });
    // Customers cannot change listing type
    if (req.user.role === 'Customer') {
      pet.listingType = 'None';
      pet.listingStatus = 'Inactive';
    }
    if (req.file) pet.imageUrl = `/uploads/pets/${req.file.filename}`;
    await pet.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: pet });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.delete = async (req, res) => {
  try {
    const pet = await Pet.findById(req.params.id);
    if (!pet) return res.status(404).json({ success: false, message: 'Không tìm thấy thú cưng' });

    if (pet.owner.toString() !== req.userId.toString() && req.user.role !== 'Admin' && req.user.role !== 'Staff') {
      return res.status(403).json({ success: false, message: 'Không có quyền xóa' });
    }

    pet.isDeleted = true;
    await pet.save();
    res.json({ success: true, message: 'Xóa thú cưng thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getMyPets = async (req, res) => {
  try {
    const pets = await Pet.find({ owner: req.userId, isDeleted: false }).sort('-createdAt');
    res.json({ success: true, data: pets });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getListings = async (req, res) => {
  try {
    const { type, listingType = 'Adoption', page = 1, limit = 20 } = req.query;
    const query = { isDeleted: false, listingType: { $ne: 'None' }, listingStatus: 'Active' };
    if (type) query.type = type;
    if (listingType) query.listingType = listingType;

    const total = await Pet.countDocuments(query);
    const pets = await Pet.find(query)
      .populate('owner', 'fullName avatarUrl')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { pets, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
