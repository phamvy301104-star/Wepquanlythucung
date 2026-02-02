import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../../core/config/config_url.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'models/cart_model.dart';

/// Cart Service - Handle cart operations via API
class CartService {
  static final CartService _instance = CartService._internal();
  factory CartService() => _instance;
  CartService._internal();

  // Local cart storage key
  static const String _localCartKey = 'local_cart';

  /// Get authorization headers
  Future<Map<String, String>> _getHeaders() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('auth_token') ?? prefs.getString('token');

    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  /// Get current user ID
  Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('user_id');
  }

  /// Get cart from API or local storage
  Future<Cart> getCart() async {
    try {
      final headers = await _getHeaders();
      final userId = await _getUserId();

      // If user is logged in, fetch from API
      if (userId != null && headers.containsKey('Authorization')) {
        final url = '${ConfigUrl.baseUrl}Cart';
        debugPrint('[CartService] Fetching cart: $url');

        final response = await http.get(Uri.parse(url), headers: headers);

        if (response.statusCode == 200) {
          final data = jsonDecode(response.body);
          return Cart.fromJson(data);
        } else if (response.statusCode == 404) {
          // No cart exists yet
          return Cart.empty();
        } else {
          debugPrint(
            '[CartService] Error: ${response.statusCode} - ${response.body}',
          );
          // Fall back to local cart
          return _getLocalCart();
        }
      }

      // Not logged in, use local cart
      return _getLocalCart();
    } catch (e) {
      debugPrint('[CartService] Error getting cart: $e');
      return _getLocalCart();
    }
  }

  /// Add item to cart
  Future<Cart> addToCart(
    int productId,
    int quantity, {
    double? price,
    String? productName,
    String? imageUrl,
    int? stockQuantity,
  }) async {
    try {
      final headers = await _getHeaders();
      final userId = await _getUserId();

      // If user is logged in, use API
      if (userId != null && headers.containsKey('Authorization')) {
        final url = '${ConfigUrl.baseUrl}Cart/add';
        debugPrint('[CartService] Adding to cart: $url');

        final response = await http.post(
          Uri.parse(url),
          headers: headers,
          body: jsonEncode({'productId': productId, 'quantity': quantity}),
        );

        if (response.statusCode == 200 || response.statusCode == 201) {
          final data = jsonDecode(response.body);
          return Cart.fromJson(data);
        } else {
          throw Exception(
            'Không thể thêm vào giỏ hàng: ${response.statusCode}',
          );
        }
      }

      // Not logged in, use local cart
      return _addToLocalCart(
        productId,
        quantity,
        price: price,
        productName: productName,
        imageUrl: imageUrl,
        stockQuantity: stockQuantity,
      );
    } catch (e) {
      debugPrint('[CartService] Error adding to cart: $e');
      // Fall back to local cart
      return _addToLocalCart(
        productId,
        quantity,
        price: price,
        productName: productName,
        imageUrl: imageUrl,
        stockQuantity: stockQuantity,
      );
    }
  }

  /// Update item quantity in cart
  Future<Cart> updateQuantity(int cartItemId, int quantity) async {
    try {
      final headers = await _getHeaders();
      final userId = await _getUserId();

      if (userId != null && headers.containsKey('Authorization')) {
        final url = '${ConfigUrl.baseUrl}Cart/update';
        debugPrint('[CartService] Updating cart: $url');

        final response = await http.put(
          Uri.parse(url),
          headers: headers,
          body: jsonEncode({'cartItemId': cartItemId, 'quantity': quantity}),
        );

        if (response.statusCode == 200) {
          final data = jsonDecode(response.body);
          return Cart.fromJson(data);
        } else {
          throw Exception(
            'Không thể cập nhật số lượng: ${response.statusCode}',
          );
        }
      }

      // Local cart update
      return _updateLocalCartQuantity(cartItemId, quantity);
    } catch (e) {
      debugPrint('[CartService] Error updating quantity: $e');
      return _updateLocalCartQuantity(cartItemId, quantity);
    }
  }

  /// Remove item from cart
  Future<Cart> removeItem(int cartItemId) async {
    try {
      final headers = await _getHeaders();
      final userId = await _getUserId();

      if (userId != null && headers.containsKey('Authorization')) {
        final url = '${ConfigUrl.baseUrl}Cart/remove/$cartItemId';
        debugPrint('[CartService] Removing from cart: $url');

        final response = await http.delete(Uri.parse(url), headers: headers);

        if (response.statusCode == 200) {
          final data = jsonDecode(response.body);
          return Cart.fromJson(data);
        } else if (response.statusCode == 204) {
          // Successfully removed, fetch updated cart
          return getCart();
        } else {
          throw Exception('Không thể xóa sản phẩm: ${response.statusCode}');
        }
      }

      // Local cart remove
      return _removeFromLocalCart(cartItemId);
    } catch (e) {
      debugPrint('[CartService] Error removing item: $e');
      return _removeFromLocalCart(cartItemId);
    }
  }

  /// Clear cart
  Future<Cart> clearCart() async {
    try {
      final headers = await _getHeaders();
      final userId = await _getUserId();

      if (userId != null && headers.containsKey('Authorization')) {
        final url = '${ConfigUrl.baseUrl}Cart/clear';
        debugPrint('[CartService] Clearing cart: $url');

        final response = await http.delete(Uri.parse(url), headers: headers);

        if (response.statusCode == 200 || response.statusCode == 204) {
          return Cart.empty();
        }
      }

      // Clear local cart
      return _clearLocalCart();
    } catch (e) {
      debugPrint('[CartService] Error clearing cart: $e');
      return _clearLocalCart();
    }
  }

  // ============ LOCAL CART METHODS ============

  Future<Cart> _getLocalCart() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final cartJson = prefs.getString(_localCartKey);

      if (cartJson != null) {
        final data = jsonDecode(cartJson);
        return Cart.fromJson(data);
      }
    } catch (e) {
      debugPrint('[CartService] Error getting local cart: $e');
    }
    return Cart.empty();
  }

  Future<void> _saveLocalCart(Cart cart) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_localCartKey, jsonEncode(cart.toJson()));
  }

  Future<Cart> _addToLocalCart(
    int productId,
    int quantity, {
    double? price,
    String? productName,
    String? imageUrl,
    int? stockQuantity,
  }) async {
    final cart = await _getLocalCart();
    final items = List<CartItem>.from(cart.items);

    // Check if item already exists
    final existingIndex = items.indexWhere(
      (item) => item.productId == productId,
    );

    if (existingIndex >= 0) {
      // Update quantity
      final existing = items[existingIndex];
      items[existingIndex] = existing.copyWith(
        quantity: existing.quantity + quantity,
      );
    } else {
      // Add new item with stock quantity (default to 999 for local cart to allow operations)
      items.add(
        CartItem(
          id: DateTime.now().millisecondsSinceEpoch,
          productId: productId,
          productName: productName ?? 'Sản phẩm #$productId',
          imageUrl: imageUrl,
          price: price ?? 0,
          quantity: quantity,
          stockQuantity:
              stockQuantity ?? 999, // Default to high stock for local cart
        ),
      );
    }

    final newCart = Cart(id: cart.id, userId: cart.userId, items: items);
    await _saveLocalCart(newCart);
    return newCart;
  }

  Future<Cart> _updateLocalCartQuantity(int cartItemId, int quantity) async {
    final cart = await _getLocalCart();
    final items = List<CartItem>.from(cart.items);

    final index = items.indexWhere((item) => item.id == cartItemId);
    if (index >= 0) {
      if (quantity <= 0) {
        items.removeAt(index);
      } else {
        items[index] = items[index].copyWith(quantity: quantity);
      }
    }

    final newCart = Cart(id: cart.id, userId: cart.userId, items: items);
    await _saveLocalCart(newCart);
    return newCart;
  }

  Future<Cart> _removeFromLocalCart(int cartItemId) async {
    final cart = await _getLocalCart();
    final items = List<CartItem>.from(cart.items);
    items.removeWhere((item) => item.id == cartItemId);

    final newCart = Cart(id: cart.id, userId: cart.userId, items: items);
    await _saveLocalCart(newCart);
    return newCart;
  }

  Future<Cart> _clearLocalCart() async {
    final cart = Cart.empty();
    await _saveLocalCart(cart);
    return cart;
  }

  /// Sync local cart to server after login
  Future<Cart> syncCartAfterLogin() async {
    try {
      final localCart = await _getLocalCart();
      if (localCart.isEmpty) {
        return getCart();
      }

      // Add each local item to server cart
      Cart serverCart = await getCart();
      for (final item in localCart.items) {
        serverCart = await addToCart(item.productId, item.quantity);
      }

      // Clear local cart after sync
      await _clearLocalCart();
      return serverCart;
    } catch (e) {
      debugPrint('[CartService] Error syncing cart: $e');
      return getCart();
    }
  }
}
