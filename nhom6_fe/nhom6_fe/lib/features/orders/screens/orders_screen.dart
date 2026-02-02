import 'package:flutter/material.dart';
import 'package:hugeicons/hugeicons.dart';
import '../../../core/constants/constants.dart';
import '../../../core/services/booking_service.dart';
import 'package:intl/intl.dart';

/// Orders Screen for UME App
/// Shows appointment history and status
class OrdersScreen extends StatefulWidget {
  const OrdersScreen({super.key});

  @override
  State<OrdersScreen> createState() => _OrdersScreenState();
}

class _OrdersScreenState extends State<OrdersScreen> {
  final BookingService _bookingService = BookingService();

  List<dynamic> _appointments = [];
  bool _isLoading = true;
  String _selectedFilter = 'All';
  String? _errorMessage;

  final List<Map<String, String>> _filters = [
    {'value': 'All', 'label': 'Tất cả'},
    {'value': 'Pending', 'label': 'Chờ xác nhận'},
    {'value': 'Confirmed', 'label': 'Đã xác nhận'},
    {'value': 'Completed', 'label': 'Hoàn thành'},
    {'value': 'Cancelled', 'label': 'Đã hủy'},
  ];

  @override
  void initState() {
    super.initState();
    _loadAppointments();
  }

  Future<void> _loadAppointments() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final result = await _bookingService.getMyAppointments(
        status: _selectedFilter == 'All' ? null : _selectedFilter,
      );

      if (result['success'] == true) {
        setState(() {
          _appointments = result['data']?['items'] ?? [];
          _isLoading = false;
        });
      } else {
        setState(() {
          _errorMessage = result['message'] ?? 'Không thể tải lịch hẹn';
          _isLoading = false;
        });
      }
    } catch (e) {
      setState(() {
        _errorMessage = 'Lỗi kết nối: $e';
        _isLoading = false;
      });
    }
  }

  void _onFilterChanged(String filter) {
    setState(() {
      _selectedFilter = filter;
    });
    _loadAppointments();
  }

  String _formatDate(String? dateStr) {
    if (dateStr == null) return '';
    try {
      final date = DateTime.parse(dateStr);
      return DateFormat('dd/MM/yyyy').format(date);
    } catch (e) {
      return dateStr;
    }
  }

  String _formatTime(String? timeStr) {
    if (timeStr == null) return '';
    try {
      // Handle TimeSpan format from API (HH:mm:ss)
      final parts = timeStr.split(':');
      if (parts.length >= 2) {
        return '${parts[0]}:${parts[1]}';
      }
      return timeStr;
    } catch (e) {
      return timeStr;
    }
  }

  String _formatPrice(dynamic price) {
    if (price == null) return '0đ';
    final formatter = NumberFormat('#,###', 'vi_VN');
    return '${formatter.format(price)}đ';
  }

  String _getStatusLabel(String? status) {
    switch (status) {
      case 'Pending':
        return 'Chờ xác nhận';
      case 'Confirmed':
        return 'Đã xác nhận';
      case 'InProgress':
        return 'Đang thực hiện';
      case 'Completed':
        return 'Hoàn thành';
      case 'Cancelled':
        return 'Đã hủy';
      case 'NoShow':
        return 'Không đến';
      default:
        return status ?? 'N/A';
    }
  }

  Color _getStatusColor(String? status) {
    switch (status) {
      case 'Pending':
        return AppColors.warning;
      case 'Confirmed':
        return AppColors.info;
      case 'InProgress':
        return AppColors.primary;
      case 'Completed':
        return AppColors.success;
      case 'Cancelled':
      case 'NoShow':
        return AppColors.error;
      default:
        return AppColors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Header
            Container(
              padding: const EdgeInsets.all(AppSizes.screenPaddingH),
              color: AppColors.white,
              child: Row(
                children: [
                  Text('Lịch hẹn của tôi', style: AppTextStyles.h2),
                  const Spacer(),
                  GestureDetector(
                    onTap: _loadAppointments,
                    child: HugeIcon(
                      icon: HugeIcons.strokeRoundedRefresh,
                      color: AppColors.iconActive,
                      size: AppSizes.iconM,
                    ),
                  ),
                ],
              ),
            ),

            // Tabs Filter
            Container(
              color: AppColors.white,
              padding: const EdgeInsets.symmetric(
                horizontal: AppSizes.screenPaddingH,
              ),
              child: SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                child: Row(
                  children: _filters.map((filter) {
                    final isSelected = _selectedFilter == filter['value'];
                    return GestureDetector(
                      onTap: () => _onFilterChanged(filter['value']!),
                      child: _TabItem(
                        label: filter['label']!,
                        isSelected: isSelected,
                      ),
                    );
                  }).toList(),
                ),
              ),
            ),

            const SizedBox(height: AppSizes.spacingM),

            // Content
            Expanded(child: _buildContent()),
          ],
        ),
      ),
    );
  }

  Widget _buildContent() {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_errorMessage != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.error_outline, size: 64, color: AppColors.error),
            const SizedBox(height: 16),
            Text(
              _errorMessage!,
              style: AppTextStyles.bodyMedium,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: _loadAppointments,
              child: const Text('Thử lại'),
            ),
          ],
        ),
      );
    }

    if (_appointments.isEmpty) {
      return _EmptyState(
        onBookNow: () {
          // Navigate to booking screen
          Navigator.pushNamed(context, '/booking');
        },
      );
    }

    return RefreshIndicator(
      onRefresh: _loadAppointments,
      child: ListView.builder(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSizes.screenPaddingH,
        ),
        itemCount: _appointments.length,
        itemBuilder: (context, index) {
          final appointment = _appointments[index];

          // Get services list
          final services = appointment['services'] as List<dynamic>? ?? [];
          final serviceNames = services
              .map((s) => s['serviceName'] ?? 'Dịch vụ')
              .join(', ');

          return _OrderCard(
            id: appointment['appointmentCode'] ?? '#${appointment['id']}',
            service: serviceNames.isNotEmpty ? serviceNames : 'Dịch vụ',
            date: _formatDate(appointment['appointmentDate']),
            time: _formatTime(appointment['startTime']),
            status: _getStatusLabel(appointment['status']),
            statusColor: _getStatusColor(appointment['status']),
            price: _formatPrice(appointment['totalAmount']),
            staffName: appointment['staffName'],
            onTap: () {
              // Navigate to detail
              _showAppointmentDetail(appointment);
            },
          );
        },
      ),
    );
  }

  void _showAppointmentDetail(dynamic appointment) {
    final services = appointment['services'] as List<dynamic>? ?? [];

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => Container(
        height: MediaQuery.of(context).size.height * 0.7,
        decoration: const BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(20),
            topRight: Radius.circular(20),
          ),
        ),
        child: Column(
          children: [
            // Handle bar
            Container(
              margin: const EdgeInsets.only(top: 12),
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(2),
              ),
            ),

            // Header
            Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  Text('Chi tiết lịch hẹn', style: AppTextStyles.h3),
                  const Spacer(),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: () => Navigator.pop(context),
                  ),
                ],
              ),
            ),

            const Divider(height: 1),

            // Content
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Status badge
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: _getStatusColor(
                          appointment['status'],
                        ).withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        _getStatusLabel(appointment['status']),
                        style: TextStyle(
                          color: _getStatusColor(appointment['status']),
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),

                    const SizedBox(height: 16),

                    // Appointment Code
                    _DetailRow(
                      icon: Icons.confirmation_number,
                      label: 'Mã lịch hẹn',
                      value:
                          appointment['appointmentCode'] ??
                          '#${appointment['id']}',
                    ),

                    // Date & Time
                    _DetailRow(
                      icon: Icons.calendar_today,
                      label: 'Ngày hẹn',
                      value: _formatDate(appointment['appointmentDate']),
                    ),

                    _DetailRow(
                      icon: Icons.access_time,
                      label: 'Giờ hẹn',
                      value:
                          '${_formatTime(appointment['startTime'])} - ${_formatTime(appointment['endTime'])}',
                    ),

                    // Staff
                    if (appointment['staffName'] != null)
                      _DetailRow(
                        icon: Icons.person,
                        label: 'Nhân viên',
                        value: appointment['staffName'],
                      ),

                    const Divider(height: 32),

                    // Services
                    Text('Dịch vụ', style: AppTextStyles.labelLarge),
                    const SizedBox(height: 8),

                    ...services.map(
                      (service) => Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: Row(
                          children: [
                            const Icon(
                              Icons.check_circle,
                              size: 16,
                              color: AppColors.success,
                            ),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                service['serviceName'] ?? 'Dịch vụ',
                                style: AppTextStyles.bodyMedium,
                              ),
                            ),
                            Text(
                              _formatPrice(service['price']),
                              style: AppTextStyles.labelMedium,
                            ),
                          ],
                        ),
                      ),
                    ),

                    const Divider(height: 32),

                    // Total
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('Tổng cộng', style: AppTextStyles.labelLarge),
                        Text(
                          _formatPrice(appointment['totalAmount']),
                          style: AppTextStyles.h3.copyWith(
                            color: AppColors.primary,
                          ),
                        ),
                      ],
                    ),

                    // Notes
                    if (appointment['customerNotes'] != null &&
                        appointment['customerNotes'].toString().isNotEmpty) ...[
                      const Divider(height: 32),
                      Text('Ghi chú', style: AppTextStyles.labelLarge),
                      const SizedBox(height: 8),
                      Text(
                        appointment['customerNotes'],
                        style: AppTextStyles.bodyMedium.copyWith(
                          color: AppColors.textSecondary,
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ),

            // Cancel button (only for Pending status)
            if (appointment['status'] == 'Pending')
              Padding(
                padding: const EdgeInsets.all(16),
                child: SizedBox(
                  width: double.infinity,
                  child: OutlinedButton(
                    onPressed: () => _cancelAppointment(appointment['id']),
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.error,
                      side: const BorderSide(color: AppColors.error),
                    ),
                    child: const Text('Hủy lịch hẹn'),
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _cancelAppointment(int? appointmentId) async {
    if (appointmentId == null) return;

    Navigator.pop(context); // Close bottom sheet

    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Xác nhận hủy'),
        content: const Text('Bạn có chắc chắn muốn hủy lịch hẹn này?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Không'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            style: TextButton.styleFrom(foregroundColor: AppColors.error),
            child: const Text('Hủy lịch'),
          ),
        ],
      ),
    );

    if (confirm == true) {
      final result = await _bookingService.cancelAppointment(appointmentId);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(result['message'] ?? 'Đã hủy lịch hẹn'),
            backgroundColor: result['success'] == true
                ? AppColors.success
                : AppColors.error,
          ),
        );

        if (result['success'] == true) {
          _loadAppointments();
        }
      }
    }
  }
}

class _TabItem extends StatelessWidget {
  final String label;
  final bool isSelected;

  const _TabItem({required this.label, required this.isSelected});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(right: AppSizes.spacingL),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(vertical: AppSizes.paddingM),
            child: Text(
              label,
              style: isSelected
                  ? AppTextStyles.labelLarge
                  : AppTextStyles.bodyMedium.copyWith(
                      color: AppColors.textHint,
                    ),
            ),
          ),
          Container(
            height: 2,
            width: 40,
            color: isSelected ? AppColors.black : Colors.transparent,
          ),
        ],
      ),
    );
  }
}

class _OrderCard extends StatelessWidget {
  final String id;
  final String service;
  final String date;
  final String time;
  final String status;
  final Color statusColor;
  final String price;
  final String? staffName;
  final VoidCallback? onTap;

  const _OrderCard({
    required this.id,
    required this.service,
    required this.date,
    required this.time,
    required this.status,
    required this.statusColor,
    required this.price,
    this.staffName,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.only(bottom: AppSizes.spacingM),
        padding: const EdgeInsets.all(AppSizes.paddingL),
        decoration: BoxDecoration(
          color: AppColors.white,
          borderRadius: BorderRadius.circular(AppSizes.radiusM),
          border: Border.all(color: AppColors.borderLight),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.05),
              blurRadius: 10,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  id,
                  style: AppTextStyles.labelMedium.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 8,
                    vertical: 4,
                  ),
                  decoration: BoxDecoration(
                    color: statusColor.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    status,
                    style: AppTextStyles.labelSmall.copyWith(
                      color: statusColor,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),

            const SizedBox(height: AppSizes.spacingS),

            // Service name
            Text(
              service,
              style: AppTextStyles.bodyLarge.copyWith(
                fontWeight: FontWeight.w600,
              ),
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),

            const SizedBox(height: AppSizes.spacingS),

            // Date, Time & Staff
            Row(
              children: [
                Icon(Icons.calendar_today, size: 14, color: AppColors.textHint),
                const SizedBox(width: 4),
                Text(
                  '$date • $time',
                  style: AppTextStyles.bodySmall.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
                if (staffName != null) ...[
                  const SizedBox(width: 12),
                  Icon(Icons.person, size: 14, color: AppColors.textHint),
                  const SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      staffName!,
                      style: AppTextStyles.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                ],
              ],
            ),

            const Divider(height: 24),

            // Price
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Tổng tiền',
                  style: AppTextStyles.bodyMedium.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
                Text(
                  price,
                  style: AppTextStyles.labelLarge.copyWith(
                    color: AppColors.primary,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _EmptyState extends StatelessWidget {
  final VoidCallback? onBookNow;

  const _EmptyState({this.onBookNow});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.calendar_today_outlined,
              size: 80,
              color: AppColors.grey,
            ),
            const SizedBox(height: 16),
            Text(
              'Chưa có lịch hẹn nào',
              style: AppTextStyles.h3.copyWith(color: AppColors.textSecondary),
            ),
            const SizedBox(height: 8),
            Text(
              'Đặt lịch ngay để trải nghiệm dịch vụ tại UME Barbershop',
              style: AppTextStyles.bodyMedium.copyWith(
                color: AppColors.textHint,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 24),
            if (onBookNow != null)
              ElevatedButton.icon(
                onPressed: onBookNow,
                icon: const Icon(Icons.add),
                label: const Text('Đặt lịch ngay'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(
                    horizontal: 24,
                    vertical: 12,
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;

  const _DetailRow({
    required this.icon,
    required this.label,
    required this.value,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Icon(icon, size: 20, color: AppColors.textSecondary),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: AppTextStyles.bodySmall.copyWith(
                    color: AppColors.textHint,
                  ),
                ),
                Text(value, style: AppTextStyles.bodyMedium),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
