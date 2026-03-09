const Brand = require('../models/Brand');

exports.getAll = async (req, res) => {
  try {
    const { search } = req.query;
    const query = { isActive: true };
    if (search) query.name = { $regex: search, $options: 'i' };
    const brands = await Brand.find(query).sort('name');
    res.json({ success: true, data: brands });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getById = async (req, res) => {
  try {
    const brand = await Brand.findById(req.params.id);
    if (!brand) return res.status(404).json({ success: false, message: 'Không tìm thấy thương hiệu' });
    res.json({ success: true, data: brand });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const brand = new Brand(req.body);
    if (req.file) brand.logoUrl = `/uploads/brands/${req.file.filename}`;
    await brand.save();
    res.status(201).json({ success: true, message: 'Tạo thương hiệu thành công', data: brand });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.update = async (req, res) => {
  try {
    const brand = await Brand.findById(req.params.id);
    if (!brand) return res.status(404).json({ success: false, message: 'Không tìm thấy thương hiệu' });
    const allowedFields = ['name', 'description', 'websiteUrl', 'countryOfOrigin', 'yearEstablished', 'displayOrder', 'isActive', 'isFeatured'];
    allowedFields.forEach(field => {
      if (req.body[field] !== undefined) brand[field] = req.body[field];
    });
    if (req.file) brand.logoUrl = `/uploads/brands/${req.file.filename}`;
    await brand.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: brand });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.delete = async (req, res) => {
  try {
    const brand = await Brand.findById(req.params.id);
    if (!brand) return res.status(404).json({ success: false, message: 'Không tìm thấy thương hiệu' });
    brand.isActive = false;
    await brand.save();
    res.json({ success: true, message: 'Xóa thương hiệu thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
