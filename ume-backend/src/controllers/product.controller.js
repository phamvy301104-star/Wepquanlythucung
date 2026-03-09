const Product = require('../models/Product');

// Get all products
exports.getAll = async (req, res) => {
  try {
    const { page = 1, limit = 20, search, category, brand, minPrice, maxPrice, sort = '-createdAt', isFeatured, isActive } = req.query;
    const query = {};

    if (search) {
      query.$or = [
        { name: { $regex: search, $options: 'i' } },
        { description: { $regex: search, $options: 'i' } },
        { sku: { $regex: search, $options: 'i' } }
      ];
    }
    if (category) query.category = category;
    if (brand) query.brand = brand;
    if (minPrice || maxPrice) {
      query.price = {};
      if (minPrice) query.price.$gte = parseFloat(minPrice);
      if (maxPrice) query.price.$lte = parseFloat(maxPrice);
    }
    if (isFeatured) query.isFeatured = isFeatured === 'true';
    // If isActive is explicitly set, use it; otherwise show only active products for public
    if (isActive === 'true' || isActive === 'false') {
      query.isActive = isActive === 'true';
    } else if (!req.user || req.user.role !== 'Admin') {
      query.isActive = true;
    }

    const total = await Product.countDocuments(query);
    const products = await Product.find(query)
      .populate('category', 'name slug')
      .populate('brand', 'name slug')
      .sort(sort)
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { products, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Get product by ID or slug
exports.getById = async (req, res) => {
  try {
    const { id } = req.params;
    let product;
    if (id.match(/^[0-9a-fA-F]{24}$/)) {
      product = await Product.findById(id).populate('category', 'name slug').populate('brand', 'name slug');
    } else {
      product = await Product.findOne({ slug: id }).populate('category', 'name slug').populate('brand', 'name slug');
    }
    if (!product) return res.status(404).json({ success: false, message: 'Không tìm thấy sản phẩm' });
    
    // Increment view count
    product.viewCount += 1;
    await product.save();

    res.json({ success: true, data: product });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Create product
exports.create = async (req, res) => {
  try {
    const data = { ...req.body };
    // Map frontend 'stock' to model's 'stockQuantity'
    if (data.stock !== undefined) {
      data.stockQuantity = data.stock;
      delete data.stock;
    }
    const product = new Product(data);
    
    if (req.files) {
      const imgFile = req.files.image || req.files.mainImage;
      if (imgFile) {
        product.imageUrl = `/uploads/products/${imgFile[0].filename}`;
      }
      if (req.files.additionalImages) {
        product.additionalImages = req.files.additionalImages.map(f => `/uploads/products/${f.filename}`);
      }
    }

    await product.save();
    
    // Update category & brand product counts
    if (product.category) {
      const Product2 = require('../models/Product');
      const count = await Product2.countDocuments({ category: product.category, isDeleted: false });
      await require('../models/Category').findByIdAndUpdate(product.category, { productCount: count });
    }
    if (product.brand) {
      const Product2 = require('../models/Product');
      const count = await Product2.countDocuments({ brand: product.brand, isDeleted: false });
      await require('../models/Brand').findByIdAndUpdate(product.brand, { productCount: count });
    }
    
    res.status(201).json({ success: true, message: 'Tạo sản phẩm thành công', data: product });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Update product
exports.update = async (req, res) => {
  try {
    const product = await Product.findById(req.params.id);
    if (!product) return res.status(404).json({ success: false, message: 'Không tìm thấy sản phẩm' });

    const oldCategory = product.category?.toString();
    const oldBrand = product.brand?.toString();

    const allowedFields = [
      'name', 'shortDescription', 'description', 'price', 'originalPrice',
      'discountPercent', 'costPrice', 'stockQuantity', 'lowStockThreshold',
      'category', 'brand', 'weight', 'volume', 'unit', 'ingredients',
      'usage', 'warnings', 'origin', 'isFeatured', 'isBestSeller',
      'isNewArrival', 'isOnSale', 'isActive', 'allowBackorder', 'tags',
      'metaTitle', 'metaDescription', 'metaKeywords', 'barcode', 'videoUrl'
    ];
    // Map frontend 'stock' to model's 'stockQuantity'
    if (req.body.stock !== undefined) {
      product.stockQuantity = req.body.stock;
    }
    allowedFields.forEach(field => {
      if (req.body[field] !== undefined) product[field] = req.body[field];
    });
    
    if (req.files) {
      const imgFile = req.files.image || req.files.mainImage;
      if (imgFile) {
        product.imageUrl = `/uploads/products/${imgFile[0].filename}`;
      }
      if (req.files.additionalImages) {
        product.additionalImages = req.files.additionalImages.map(f => `/uploads/products/${f.filename}`);
      }
    }

    await product.save();
    
    // Update category & brand product counts if changed
    const Category = require('../models/Category');
    const Brand = require('../models/Brand');
    const catIds = new Set([oldCategory, product.category?.toString()].filter(Boolean));
    for (const cId of catIds) {
      const cnt = await Product.countDocuments({ category: cId, isDeleted: false });
      await Category.findByIdAndUpdate(cId, { productCount: cnt });
    }
    const brandIds = new Set([oldBrand, product.brand?.toString()].filter(Boolean));
    for (const bId of brandIds) {
      const cnt = await Product.countDocuments({ brand: bId, isDeleted: false });
      await Brand.findByIdAndUpdate(bId, { productCount: cnt });
    }
    
    res.json({ success: true, message: 'Cập nhật sản phẩm thành công', data: product });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server', error: error.message });
  }
};

// Delete product (soft delete)
exports.delete = async (req, res) => {
  try {
    const product = await Product.findById(req.params.id);
    if (!product) return res.status(404).json({ success: false, message: 'Không tìm thấy sản phẩm' });
    
    product.isDeleted = true;
    await product.save();
    
    // Update category & brand product counts
    if (product.category) {
      const cnt = await Product.countDocuments({ category: product.category, isDeleted: false });
      await require('../models/Category').findByIdAndUpdate(product.category, { productCount: cnt });
    }
    if (product.brand) {
      const cnt = await Product.countDocuments({ brand: product.brand, isDeleted: false });
      await require('../models/Brand').findByIdAndUpdate(product.brand, { productCount: cnt });
    }
    
    res.json({ success: true, message: 'Xóa sản phẩm thành công' });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Get featured products
exports.getFeatured = async (req, res) => {
  try {
    const products = await Product.find({ isFeatured: true, isActive: true })
      .populate('category', 'name slug')
      .populate('brand', 'name slug')
      .limit(10);
    res.json({ success: true, data: products });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};

// Get products by category
exports.getByCategory = async (req, res) => {
  try {
    const { page = 1, limit = 20 } = req.query;
    const query = { category: req.params.categoryId, isActive: true };
    const total = await Product.countDocuments(query);
    const products = await Product.find(query)
      .populate('brand', 'name slug')
      .sort('-createdAt')
      .skip((page - 1) * limit)
      .limit(parseInt(limit));

    res.json({
      success: true,
      data: { products, pagination: { total, page: parseInt(page), limit: parseInt(limit), pages: Math.ceil(total / limit) } }
    });
  } catch (error) {
    res.status(500).json({ success: false, message: 'Lỗi server' });
  }
};
