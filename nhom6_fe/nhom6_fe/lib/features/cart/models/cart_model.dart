/// Cart Model
/// Used for shopping cart functionality
class CartItem {
  final int id;
  final int productId;
  final String productName;
  final String? imageUrl;
  final double price;
  final double? salePrice;
  final int quantity;
  final int stockQuantity;

  CartItem({
    required this.id,
    required this.productId,
    required this.productName,
    this.imageUrl,
    required this.price,
    this.salePrice,
    required this.quantity,
    this.stockQuantity = 999, // Default to high stock for local cart
  });

  factory CartItem.fromJson(Map<String, dynamic> json) {
    return CartItem(
      id: json['id'] ?? 0,
      productId: json['productId'] ?? json['sanPhamId'] ?? 0,
      productName: json['productName'] ?? json['tenSanPham'] ?? '',
      imageUrl: json['imageUrl'] ?? json['hinhAnh'],
      price: (json['price'] ?? json['giaBan'] ?? 0).toDouble(),
      salePrice:
          json['salePrice']?.toDouble() ?? json['giaKhuyenMai']?.toDouble(),
      quantity: json['quantity'] ?? json['soLuong'] ?? 1,
      stockQuantity:
          json['stockQuantity'] ??
          json['soLuongTonKho'] ??
          999, // Default to 999 if not provided
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'productId': productId,
      'productName': productName,
      'imageUrl': imageUrl,
      'price': price,
      'salePrice': salePrice,
      'quantity': quantity,
      'stockQuantity': stockQuantity,
    };
  }

  /// Get display price (sale price if available, otherwise regular price)
  double get displayPrice => salePrice ?? price;

  /// Calculate total price for this item
  double get totalPrice => displayPrice * quantity;

  /// Check if product is in stock
  bool get isInStock => stockQuantity > 0;

  /// Check if can increase quantity
  bool get canIncrease => quantity < stockQuantity;

  /// Check if can decrease quantity
  bool get canDecrease => quantity > 1;

  /// Format price to Vietnamese currency
  String formatPrice(double price) {
    return '${price.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]}.')}đ';
  }

  String get formattedPrice => formatPrice(price);
  String get formattedDisplayPrice => formatPrice(displayPrice);
  String get formattedTotalPrice => formatPrice(totalPrice);

  /// Create a copy with updated quantity
  CartItem copyWith({int? quantity}) {
    return CartItem(
      id: id,
      productId: productId,
      productName: productName,
      imageUrl: imageUrl,
      price: price,
      salePrice: salePrice,
      quantity: quantity ?? this.quantity,
      stockQuantity: stockQuantity,
    );
  }
}

class Cart {
  final int id;
  final int? userId;
  final List<CartItem> items;

  Cart({this.id = 0, this.userId, this.items = const []});

  factory Cart.fromJson(Map<String, dynamic> json) {
    List<CartItem> itemsList = [];

    final itemsData = json['items'] ?? json['cartItems'] ?? [];
    if (itemsData is List) {
      itemsList = itemsData.map((item) => CartItem.fromJson(item)).toList();
    }

    return Cart(
      id: json['id'] ?? 0,
      userId: json['userId'] ?? json['customerId'],
      items: itemsList,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userId': userId,
      'items': items.map((item) => item.toJson()).toList(),
    };
  }

  /// Empty cart
  factory Cart.empty() => Cart();

  /// Check if cart is empty
  bool get isEmpty => items.isEmpty;

  /// Get total items count
  int get itemCount => items.length;

  /// Get total quantity of all items
  int get totalQuantity => items.fold(0, (sum, item) => sum + item.quantity);

  /// Get total price of all items
  double get totalPrice => items.fold(0, (sum, item) => sum + item.totalPrice);

  /// Format total price to Vietnamese currency
  String get formattedTotalPrice {
    return '${totalPrice.toStringAsFixed(0).replaceAllMapped(RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'), (Match m) => '${m[1]}.')}đ';
  }

  /// Check if a product is in cart
  bool containsProduct(int productId) {
    return items.any((item) => item.productId == productId);
  }

  /// Get quantity of a product in cart
  int getProductQuantity(int productId) {
    final item = items.firstWhere(
      (item) => item.productId == productId,
      orElse: () =>
          CartItem(id: 0, productId: 0, productName: '', price: 0, quantity: 0),
    );
    return item.quantity;
  }
}
