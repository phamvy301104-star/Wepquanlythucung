// Order Model for UME App
// Model representing product orders for users

class Order {
  final int id;
  final String orderCode;
  final String? userId;
  final String customerName;
  final String? customerEmail;
  final String customerPhone;
  final String? shippingAddressText;
  final String? receiverName;
  final String? receiverPhone;
  final double subTotal;
  final double shippingFee;
  final double discountAmount;
  final double taxAmount;
  final double totalAmount;
  final double paidAmount;
  final String? couponCode;
  final String? paymentMethodName;
  final String paymentStatus;
  final String? shippingMethodName;
  final String? trackingNumber;
  final String status;
  final String? customerNotes;
  final String? staffNotes;
  final DateTime? estimatedDeliveryDate;
  final DateTime? deliveredAt;
  final DateTime? cancelledAt;
  final String? cancellationReason;
  final DateTime createdAt;
  final DateTime updatedAt;
  final List<OrderItem> items;
  final List<OrderStatusHistory>? statusHistory;

  Order({
    required this.id,
    required this.orderCode,
    this.userId,
    required this.customerName,
    this.customerEmail,
    required this.customerPhone,
    this.shippingAddressText,
    this.receiverName,
    this.receiverPhone,
    required this.subTotal,
    this.shippingFee = 0,
    this.discountAmount = 0,
    this.taxAmount = 0,
    required this.totalAmount,
    this.paidAmount = 0,
    this.couponCode,
    this.paymentMethodName,
    this.paymentStatus = 'Pending',
    this.shippingMethodName,
    this.trackingNumber,
    required this.status,
    this.customerNotes,
    this.staffNotes,
    this.estimatedDeliveryDate,
    this.deliveredAt,
    this.cancelledAt,
    this.cancellationReason,
    required this.createdAt,
    required this.updatedAt,
    this.items = const [],
    this.statusHistory,
  });

  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      id: json['id'] ?? 0,
      orderCode: json['orderCode'] ?? '',
      userId: json['userId'],
      customerName: json['customerName'] ?? '',
      customerEmail: json['customerEmail'],
      customerPhone: json['customerPhone'] ?? '',
      shippingAddressText: json['shippingAddressText'],
      receiverName: json['receiverName'],
      receiverPhone: json['receiverPhone'],
      subTotal: _parseDouble(json['subTotal']),
      shippingFee: _parseDouble(json['shippingFee']),
      discountAmount: _parseDouble(json['discountAmount']),
      taxAmount: _parseDouble(json['taxAmount']),
      totalAmount: _parseDouble(json['totalAmount']),
      paidAmount: _parseDouble(json['paidAmount']),
      couponCode: json['couponCode'],
      paymentMethodName: json['paymentMethodName'],
      paymentStatus: json['paymentStatus'] ?? 'Pending',
      shippingMethodName: json['shippingMethodName'],
      trackingNumber: json['trackingNumber'],
      status: json['status'] ?? 'Pending',
      customerNotes: json['customerNotes'],
      staffNotes: json['staffNotes'],
      estimatedDeliveryDate: json['estimatedDeliveryDate'] != null
          ? DateTime.tryParse(json['estimatedDeliveryDate'])
          : null,
      deliveredAt: json['deliveredAt'] != null
          ? DateTime.tryParse(json['deliveredAt'])
          : null,
      cancelledAt: json['cancelledAt'] != null
          ? DateTime.tryParse(json['cancelledAt'])
          : null,
      cancellationReason: json['cancellationReason'],
      createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now(),
      updatedAt: DateTime.tryParse(json['updatedAt'] ?? '') ?? DateTime.now(),
      items: (json['items'] as List<dynamic>?)
              ?.map((e) => OrderItem.fromJson(e))
              .toList() ??
          [],
      statusHistory: (json['statusHistory'] as List<dynamic>?)
          ?.map((e) => OrderStatusHistory.fromJson(e))
          .toList(),
    );
  }

  static double _parseDouble(dynamic value) {
    if (value == null) return 0.0;
    if (value is double) return value;
    if (value is int) return value.toDouble();
    if (value is String) return double.tryParse(value) ?? 0.0;
    return 0.0;
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderCode': orderCode,
      'userId': userId,
      'customerName': customerName,
      'customerEmail': customerEmail,
      'customerPhone': customerPhone,
      'shippingAddressText': shippingAddressText,
      'receiverName': receiverName,
      'receiverPhone': receiverPhone,
      'subTotal': subTotal,
      'shippingFee': shippingFee,
      'discountAmount': discountAmount,
      'taxAmount': taxAmount,
      'totalAmount': totalAmount,
      'paidAmount': paidAmount,
      'couponCode': couponCode,
      'paymentMethodName': paymentMethodName,
      'paymentStatus': paymentStatus,
      'shippingMethodName': shippingMethodName,
      'trackingNumber': trackingNumber,
      'status': status,
      'customerNotes': customerNotes,
      'staffNotes': staffNotes,
      'estimatedDeliveryDate': estimatedDeliveryDate?.toIso8601String(),
      'deliveredAt': deliveredAt?.toIso8601String(),
      'cancelledAt': cancelledAt?.toIso8601String(),
      'cancellationReason': cancellationReason,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
      'items': items.map((e) => e.toJson()).toList(),
    };
  }

  /// Get display status in Vietnamese
  String get displayStatus {
    switch (status) {
      case 'Pending':
        return 'Chờ xác nhận';
      case 'Confirmed':
        return 'Đã xác nhận';
      case 'Processing':
        return 'Đang xử lý';
      case 'Shipping':
        return 'Đang giao hàng';
      case 'Delivered':
        return 'Đã giao hàng';
      case 'Completed':
        return 'Hoàn thành';
      case 'Cancelled':
        return 'Đã hủy';
      case 'Refunded':
        return 'Đã hoàn tiền';
      default:
        return status;
    }
  }

  /// Get status color
  int get statusColor {
    switch (status) {
      case 'Pending':
        return 0xFFFFA726; // Orange
      case 'Confirmed':
        return 0xFF42A5F5; // Blue
      case 'Processing':
        return 0xFF7E57C2; // Purple
      case 'Shipping':
        return 0xFF26C6DA; // Cyan
      case 'Delivered':
        return 0xFF66BB6A; // Green
      case 'Completed':
        return 0xFF4CAF50; // Dark Green
      case 'Cancelled':
        return 0xFFEF5350; // Red
      case 'Refunded':
        return 0xFF78909C; // Grey
      default:
        return 0xFF9E9E9E;
    }
  }
}

class OrderItem {
  final int id;
  final int orderId;
  final int productId;
  final int? productVariantId;
  final String? sku;
  final String productName;
  final String? variantName;
  final String? productImageUrl;
  final double unitPrice;
  final int quantity;
  final double discountAmount;
  final double totalPrice;
  final String? notes;
  final DateTime createdAt;

  OrderItem({
    required this.id,
    required this.orderId,
    required this.productId,
    this.productVariantId,
    this.sku,
    required this.productName,
    this.variantName,
    this.productImageUrl,
    required this.unitPrice,
    required this.quantity,
    this.discountAmount = 0,
    required this.totalPrice,
    this.notes,
    required this.createdAt,
  });

  factory OrderItem.fromJson(Map<String, dynamic> json) {
    return OrderItem(
      id: json['id'] ?? 0,
      orderId: json['orderId'] ?? 0,
      productId: json['productId'] ?? 0,
      productVariantId: json['productVariantId'],
      sku: json['sku'],
      productName: json['productName'] ?? '',
      variantName: json['variantName'],
      productImageUrl: json['productImageUrl'],
      unitPrice: Order._parseDouble(json['unitPrice']),
      quantity: json['quantity'] ?? 0,
      discountAmount: Order._parseDouble(json['discountAmount']),
      totalPrice: Order._parseDouble(json['totalPrice']),
      notes: json['notes'],
      createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderId': orderId,
      'productId': productId,
      'productVariantId': productVariantId,
      'sku': sku,
      'productName': productName,
      'variantName': variantName,
      'productImageUrl': productImageUrl,
      'unitPrice': unitPrice,
      'quantity': quantity,
      'discountAmount': discountAmount,
      'totalPrice': totalPrice,
      'notes': notes,
      'createdAt': createdAt.toIso8601String(),
    };
  }
}

class OrderStatusHistory {
  final int id;
  final int orderId;
  final String fromStatus;
  final String toStatus;
  final String? notes;
  final String? changedBy;
  final DateTime createdAt;

  OrderStatusHistory({
    required this.id,
    required this.orderId,
    required this.fromStatus,
    required this.toStatus,
    this.notes,
    this.changedBy,
    required this.createdAt,
  });

  factory OrderStatusHistory.fromJson(Map<String, dynamic> json) {
    return OrderStatusHistory(
      id: json['id'] ?? 0,
      orderId: json['orderId'] ?? 0,
      fromStatus: json['fromStatus'] ?? '',
      toStatus: json['toStatus'] ?? '',
      notes: json['notes'],
      changedBy: json['changedBy'],
      createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderId': orderId,
      'fromStatus': fromStatus,
      'toStatus': toStatus,
      'notes': notes,
      'changedBy': changedBy,
      'createdAt': createdAt.toIso8601String(),
    };
  }
}
