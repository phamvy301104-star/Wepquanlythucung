const Category = require('../models/Category');

exports.getAll = async (req, res) => {
  try {
    const { search, parentOnly } = req.query;
    const query = { isActive: true };
    if (search) query.name = { $regex: search, $options: 'i' };
    if (parentOnly === 'true') query.parentCategory = null;

    const categories = await Category.find(query).populate('parentCategory', 'name slug').sort('sortOrder name');
    res.json({ success: true, data: categories });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getById = async (req, res) => {
  try {
    const category = await Category.findById(req.params.id).populate('parentCategory', 'name slug');
    if (!category) return res.status(404).json({ success: false, message: 'Không tìm thấy danh mục' });
    res.json({ success: true, data: category });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.create = async (req, res) => {
  try {
    const category = new Category(req.body);
    if (req.file) category.imageUrl = `/uploads/categories/${req.file.filename}`;
    await category.save();
    res.status(201).json({ success: true, message: 'Tạo danh mục thành công', data: category });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.update = async (req, res) => {
  try {
    const category = await Category.findById(req.params.id);
    if (!category) return res.status(404).json({ success: false, message: 'Không tìm thấy danh mục' });
    const allowedFields = ['name', 'description', 'icon', 'parentCategory', 'displayOrder', 'isActive', 'showOnHomePage'];
    allowedFields.forEach(field => {
      if (req.body[field] !== undefined) category[field] = req.body[field];
    });
    if (req.file) category.imageUrl = `/uploads/categories/${req.file.filename}`;
    await category.save();
    res.json({ success: true, message: 'Cập nhật thành công', data: category });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

exports.delete = async (req, res) => {
  try {
    const category = await Category.findById(req.params.id);
    if (!category) return res.status(404).json({ success: false, message: 'Không tìm thấy danh mục' });
    category.isActive = false;
    await category.save();
    res.json({ success: true, message: 'Xóa danh mục thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

exports.getTree = async (req, res) => {
  try {
    const categories = await Category.find({ isActive: true }).sort('sortOrder name');
    const tree = categories.filter(c => !c.parentCategory).map(parent => ({
      ...parent.toObject(),
      children: categories.filter(c => c.parentCategory && c.parentCategory.toString() === parent._id.toString())
    }));
    res.json({ success: true, data: tree });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
