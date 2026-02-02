// Cart Providers
// Riverpod providers for Cart state management

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'models/cart_model.dart';
import 'cart_service.dart';

// ============================================
// SERVICE PROVIDER
// ============================================
final cartServiceProvider = Provider<CartService>((ref) {
  return CartService();
});

// ============================================
// CART STATE NOTIFIER
// ============================================
class CartNotifier extends StateNotifier<AsyncValue<Cart>> {
  final CartService _service;

  CartNotifier(this._service) : super(const AsyncValue.loading()) {
    loadCart();
  }

  /// Load cart from API or local storage
  Future<void> loadCart() async {
    state = const AsyncValue.loading();
    try {
      final cart = await _service.getCart();
      state = AsyncValue.data(cart);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
    }
  }

  /// Add item to cart
  Future<void> addToCart(
    int productId,
    int quantity, {
    double? price,
    String? productName,
    String? imageUrl,
    int? stockQuantity,
  }) async {
    try {
      final cart = await _service.addToCart(
        productId,
        quantity,
        price: price,
        productName: productName,
        imageUrl: imageUrl,
        stockQuantity: stockQuantity,
      );
      state = AsyncValue.data(cart);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
      rethrow;
    }
  }

  /// Update item quantity
  Future<void> updateQuantity(int cartItemId, int quantity) async {
    try {
      final cart = await _service.updateQuantity(cartItemId, quantity);
      state = AsyncValue.data(cart);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
      rethrow;
    }
  }

  /// Remove item from cart
  Future<void> removeItem(int cartItemId) async {
    try {
      final cart = await _service.removeItem(cartItemId);
      state = AsyncValue.data(cart);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
      rethrow;
    }
  }

  /// Clear cart
  Future<void> clearCart() async {
    try {
      final cart = await _service.clearCart();
      state = AsyncValue.data(cart);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
      rethrow;
    }
  }

  /// Sync local cart to server after login
  Future<void> syncAfterLogin() async {
    try {
      final cart = await _service.syncCartAfterLogin();
      state = AsyncValue.data(cart);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
    }
  }

  /// Get current cart (non-async)
  Cart get currentCart => state.valueOrNull ?? Cart.empty();
}

// ============================================
// CART PROVIDERS
// ============================================

/// Main cart provider - manages cart state
final cartProvider = StateNotifierProvider<CartNotifier, AsyncValue<Cart>>((
  ref,
) {
  final service = ref.watch(cartServiceProvider);
  return CartNotifier(service);
});

/// Cart item count provider - for badge display
final cartItemCountProvider = Provider<int>((ref) {
  final cartState = ref.watch(cartProvider);
  return cartState.when(
    data: (cart) => cart.totalQuantity,
    loading: () => 0,
    error: (_, __) => 0,
  );
});

/// Cart total price provider
final cartTotalPriceProvider = Provider<double>((ref) {
  final cartState = ref.watch(cartProvider);
  return cartState.when(
    data: (cart) => cart.totalPrice,
    loading: () => 0,
    error: (_, __) => 0,
  );
});

/// Cart is empty provider
final cartIsEmptyProvider = Provider<bool>((ref) {
  final cartState = ref.watch(cartProvider);
  return cartState.when(
    data: (cart) => cart.isEmpty,
    loading: () => true,
    error: (_, __) => true,
  );
});
