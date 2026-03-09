const Service = require('../models/Service');
const ServiceCategory = require('../models/ServiceCategory');

// Services
exports.getAll = async (req, res) => {
  try {
    const { search, category, minPrice, maxPrice, isActive, limit } = req.query;
    const query = {};
    // If isActive is explicitly set, use it; otherwise show only active for public
    if (isActive === 'true' || isActive === 'false') {
      query.isActive = isActive === 'true';
    } else if (!limit) {
      query.isActive = true;
    }
    if (search) query.name = { $regex: search, $options: 'i' };
    if (category) query.category = category;
    if (minPrice || maxPrice) {
      query.price = {};
      if (minPrice) query.price.$gte = parseFloat(minPrice);
      if (maxPrice) query.price.$lte = parseFloat(maxPrice);
    }

    const services = await Service.find(query).populate('category', 'name').sort('sortOrder name');
    res.json({ success: true, data: services });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getById = async (req, res) => {
  try {
    const service = await Service.findById(req.params.id).populate('category', 'name');
    if (!service) return res.status(404).json({ success: false, message: 'Không tìm thấy dịch vụ' });
    res.json({ success: true, data: service });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const service = new Service(req.body);
    if (req.file) {
      service.imageUrl = `/uploads/services/${req.file.filename}`;
    }
    await service.save();
    res.status(201).json({ success: true, message: 'Tạo dịch vụ thành công', data: service });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.update = async (req, res) => {
  try {
    const service = await Service.findById(req.params.id);
    if (!service) return res.status(404).json({ success: false, message: 'Không tìm thấy dịch vụ' });

    const allowedFields = [
      'name', 'shortDescription', 'description', 'price', 'originalPrice',
      'minPrice', 'maxPrice', 'durationMinutes', 'bufferMinutes', 'requiredStaff',
      'gender', 'category', 'isFeatured', 'isPopular', 'isNewArrival', 'isActive',
      'displayOrder', 'videoUrl'
    ];
    allowedFields.forEach(field => {
      if (req.body[field] !== undefined) service[field] = req.body[field];
    });

    if (req.file) {
      service.imageUrl = `/uploads/services/${req.file.filename}`;
    }
    await service.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: service });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.delete = async (req, res) => {
  try {
    const service = await Service.findByIdAndDelete(req.params.id);
    if (!service) return res.status(404).json({ success: false, message: 'Không tìm thấy dịch vụ' });
    res.json({ success: true, message: 'Xóa dịch vụ thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Service Categories
exports.getAllCategories = async (req, res) => {
  try {
    const categories = await ServiceCategory.find({ isActive: true }).populate('parentCategory', 'name').sort('displayOrder name');
    res.json({ success: true, data: categories });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.createCategory = async (req, res) => {
  try {
    const category = new ServiceCategory(req.body);
    if (req.file) {
      category.imageUrl = `/uploads/service-categories/${req.file.filename}`;
    }
    await category.save();
    res.status(201).json({ success: true, message: 'Tạo danh mục dịch vụ thành công', data: category });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.updateCategory = async (req, res) => {
  try {
    const category = await ServiceCategory.findById(req.params.id);
    if (!category) return res.status(404).json({ success: false, message: 'Không tìm thấy danh mục dịch vụ' });

    const allowedFields = ['name', 'description', 'displayOrder', 'isActive', 'parentCategory'];
    allowedFields.forEach(field => {
      if (req.body[field] !== undefined) category[field] = req.body[field];
    });

    if (req.file) {
      category.imageUrl = `/uploads/service-categories/${req.file.filename}`;
    }
    await category.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: category });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.deleteCategory = async (req, res) => {
  try {
    const category = await ServiceCategory.findById(req.params.id);
    if (!category) return res.status(404).json({ success: false, message: 'Không tìm thấy danh mục dịch vụ' });
    category.isActive = false;
    await category.save();
    res.json({ success: true, message: 'Xóa danh mục dịch vụ thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
