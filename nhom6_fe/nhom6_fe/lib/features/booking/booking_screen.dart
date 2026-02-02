import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../core/constants/constants.dart';
import '../../core/services/booking_service.dart';
import '../../shared/widgets/custom_snackbar.dart';

class BookingScreen extends StatefulWidget {
  const BookingScreen({super.key});

  @override
  State<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends State<BookingScreen> {
  final BookingService _bookingService = BookingService();

  // State
  int _currentStep = 0;
  bool _isLoading = false;

  // Controllers cho thông tin liên hệ
  final TextEditingController _nameController = TextEditingController();
  final TextEditingController _phoneController = TextEditingController();
  final TextEditingController _notesController = TextEditingController();

  // Selected data
  List<Map<String, dynamic>> _selectedServices = [];
  DateTime _selectedDate = DateTime.now().add(const Duration(days: 1));
  String _selectedTime = '09:00';
  Map<String, dynamic>? _selectedStaff;

  // Data from API
  List<Map<String, dynamic>> _services = [];
  List<Map<String, dynamic>> _availableStaff = [];

  // Time slots
  final List<String> _timeSlots = [
    '08:00',
    '08:30',
    '09:00',
    '09:30',
    '10:00',
    '10:30',
    '11:00',
    '11:30',
    '13:00',
    '13:30',
    '14:00',
    '14:30',
    '15:00',
    '15:30',
    '16:00',
    '16:30',
    '17:00',
    '17:30',
    '18:00',
    '18:30',
    '19:00',
    '19:30',
  ];

  @override
  void initState() {
    super.initState();
    _loadServices();
    _loadUserInfo();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _phoneController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  /// Load thông tin user từ SharedPreferences để pre-fill form
  Future<void> _loadUserInfo() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final userJson = prefs.getString('user_info');
      if (userJson != null) {
        final userInfo = jsonDecode(userJson);
        setState(() {
          _nameController.text =
              userInfo['fullName'] ?? userInfo['userName'] ?? '';
          _phoneController.text = userInfo['phoneNumber'] ?? '';
        });
      }
    } catch (e) {
      print('[BookingScreen] Error loading user info: $e');
    }
  }

  Future<void> _loadServices() async {
    setState(() => _isLoading = true);

    final result = await _bookingService.getServices();

    if (result['success'] == true && result['data'] != null) {
      setState(() {
        final data = result['data'];
        // Handle both array and object with 'items' property
        if (data is List) {
          _services = List<Map<String, dynamic>>.from(data);
        } else if (data is Map && data['items'] != null) {
          _services = List<Map<String, dynamic>>.from(data['items']);
        } else {
          _services = [];
        }
      });
    }

    setState(() => _isLoading = false);
  }

  Future<void> _loadAvailableStaff() async {
    if (_selectedServices.isEmpty) return;

    setState(() => _isLoading = true);

    // Tính tổng thời gian dịch vụ
    int totalMinutes = _selectedServices.fold(0, (sum, s) {
      final duration = s['durationMinutes'];
      final durationInt = duration is int
          ? duration
          : (duration is double ? duration.toInt() : 30);
      return sum + durationInt;
    });

    final result = await _bookingService.getAvailableStaff(
      date: _selectedDate,
      startTime: _selectedTime,
      durationMinutes: totalMinutes,
    );

    if (result['success'] == true && result['data'] != null) {
      setState(() {
        final data = result['data'];
        // Handle both array and object with 'items' property
        if (data is List) {
          _availableStaff = List<Map<String, dynamic>>.from(data);
        } else if (data is Map && data['items'] != null) {
          _availableStaff = List<Map<String, dynamic>>.from(data['items']);
        } else if (data is Map) {
          // Single staff object, wrap in list
          _availableStaff = [Map<String, dynamic>.from(data)];
        } else {
          _availableStaff = [];
        }
      });
    } else {
      // Fallback: load all staff
      final allStaff = await _bookingService.getAllStaff();
      if (allStaff['success'] == true && allStaff['data'] != null) {
        setState(() {
          final data = allStaff['data'];
          if (data is List) {
            _availableStaff = List<Map<String, dynamic>>.from(data);
          } else if (data is Map && data['items'] != null) {
            _availableStaff = List<Map<String, dynamic>>.from(data['items']);
          } else {
            _availableStaff = [];
          }
        });
      }
    }

    setState(() => _isLoading = false);
  }

  Future<void> _checkAndBookAppointment() async {
    if (_selectedStaff != null) {
      // Kiểm tra nhân viên có rảnh không
      int totalMinutes = _selectedServices.fold(0, (sum, s) {
        final duration = s['durationMinutes'];
        final durationInt = duration is int
            ? duration
            : (duration is double ? duration.toInt() : 30);
        return sum + durationInt;
      });

      final staffId = _getIntValue(_selectedStaff!['id']);
      if (staffId == 0) {
        if (mounted) {
          CustomSnackBar.showWarning(context, 'Không tìm thấy nhân viên');
        }
        return;
      }

      final checkResult = await _bookingService.checkStaffAvailability(
        staffId: staffId,
        date: _selectedDate,
        startTime: _selectedTime,
        durationMinutes: totalMinutes,
      );

      if (checkResult['available'] != true) {
        if (mounted) {
          CustomSnackBar.showWarning(
            context,
            checkResult['message'] ??
                'Nhân viên đang bận, vui lòng chọn nhân viên khác hoặc thời gian khác',
          );
        }
        return;
      }
    }

    // Tạo lịch hẹn
    setState(() => _isLoading = true);

    // Filter out invalid service IDs (0 or null)
    final validServiceIds = _selectedServices
        .map((s) => _getIntValue(s['id']))
        .where((id) => id > 0)
        .toList();

    if (validServiceIds.isEmpty) {
      setState(() => _isLoading = false);
      if (mounted) {
        CustomSnackBar.showError(
          context,
          'Vui lòng chọn ít nhất một dịch vụ hợp lệ',
        );
      }
      return;
    }

    final result = await _bookingService.createAppointment(
      staffId: _selectedStaff != null
          ? _getIntValue(_selectedStaff!['id'])
          : null,
      date: _selectedDate,
      startTime: _selectedTime,
      serviceIds: validServiceIds,
      notes: _notesController.text,
      guestName: _nameController.text.trim(),
      guestPhone: _phoneController.text.trim(),
    );

    setState(() => _isLoading = false);

    if (!mounted) return;

    if (result['success'] == true) {
      CustomSnackBar.showSuccess(
        context,
        result['message'] ?? 'Đặt lịch thành công!',
      );
      Navigator.pop(context, true);
    } else {
      CustomSnackBar.showError(
        context,
        result['message'] ?? 'Đặt lịch thất bại',
      );
    }
  }

  int get _totalPrice => _selectedServices.fold(0, (sum, s) {
    final price = s['price'];
    final priceInt = price is int
        ? price
        : (price is double ? price.toInt() : 0);
    return sum + priceInt;
  });

  int get _totalDuration => _selectedServices.fold(0, (sum, s) {
    final duration = s['durationMinutes'];
    final durationInt = duration is int
        ? duration
        : (duration is double ? duration.toInt() : 30);
    return sum + durationInt;
  });

  // Helper method to safely convert dynamic to int
  int _getIntValue(dynamic value) {
    if (value == null) return 0;
    if (value is int) return value;
    if (value is double) return value.toInt();
    if (value is String) return int.tryParse(value) ?? 0;
    return 0;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Đặt lịch hẹn'),
        backgroundColor: AppColors.white,
        foregroundColor: AppColors.textPrimary,
        elevation: 0,
      ),
      body: _isLoading && _services.isEmpty
          ? const Center(child: CircularProgressIndicator())
          : Column(
              children: [
                // Stepper indicator
                _buildStepIndicator(),

                // Content
                Expanded(
                  child: IndexedStack(
                    index: _currentStep,
                    children: [
                      _buildServiceSelection(),
                      _buildDateTimeSelection(),
                      _buildStaffSelection(),
                      _buildConfirmation(),
                    ],
                  ),
                ),

                // Bottom buttons
                _buildBottomButtons(),
              ],
            ),
    );
  }

  Widget _buildStepIndicator() {
    final steps = ['Dịch vụ', 'Ngày giờ', 'Nhân viên', 'Xác nhận'];

    return Container(
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 8),
      color: AppColors.white,
      child: Row(
        children: List.generate(steps.length, (index) {
          final isActive = index <= _currentStep;
          final isCompleted = index < _currentStep;

          return Expanded(
            child: Row(
              children: [
                if (index > 0)
                  Expanded(
                    child: Container(
                      height: 2,
                      color: isActive ? AppColors.primary : AppColors.border,
                    ),
                  ),
                Column(
                  children: [
                    Container(
                      width: 28,
                      height: 28,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: isActive ? AppColors.primary : AppColors.white,
                        border: Border.all(
                          color: isActive
                              ? AppColors.primary
                              : AppColors.border,
                          width: 2,
                        ),
                      ),
                      child: Center(
                        child: isCompleted
                            ? const Icon(
                                Icons.check,
                                size: 16,
                                color: Colors.white,
                              )
                            : Text(
                                '${index + 1}',
                                style: TextStyle(
                                  fontSize: 12,
                                  fontWeight: FontWeight.bold,
                                  color: isActive
                                      ? Colors.white
                                      : AppColors.grey,
                                ),
                              ),
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      steps[index],
                      style: TextStyle(
                        fontSize: 10,
                        color: isActive ? AppColors.primary : AppColors.grey,
                        fontWeight: isActive
                            ? FontWeight.w600
                            : FontWeight.normal,
                      ),
                    ),
                  ],
                ),
                if (index < steps.length - 1)
                  Expanded(
                    child: Container(
                      height: 2,
                      color: index < _currentStep
                          ? AppColors.primary
                          : AppColors.border,
                    ),
                  ),
              ],
            ),
          );
        }),
      ),
    );
  }

  Widget _buildServiceSelection() {
    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: _services.length,
      itemBuilder: (context, index) {
        final service = _services[index];
        final isSelected = _selectedServices.any(
          (s) => _getIntValue(s['id']) == _getIntValue(service['id']),
        );

        return Card(
          margin: const EdgeInsets.only(bottom: 12),
          elevation: isSelected ? 4 : 1,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
            side: BorderSide(
              color: isSelected ? AppColors.primary : Colors.transparent,
              width: 2,
            ),
          ),
          child: InkWell(
            onTap: () {
              setState(() {
                if (isSelected) {
                  _selectedServices.removeWhere(
                    (s) => _getIntValue(s['id']) == _getIntValue(service['id']),
                  );
                } else {
                  _selectedServices.add(service);
                }
              });
            },
            borderRadius: BorderRadius.circular(12),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  // Image
                  ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: service['imageUrl'] != null
                        ? Image.network(
                            service['imageUrl'],
                            width: 70,
                            height: 70,
                            fit: BoxFit.cover,
                            errorBuilder: (_, __, ___) =>
                                _buildServicePlaceholder(),
                          )
                        : _buildServicePlaceholder(),
                  ),
                  const SizedBox(width: 12),

                  // Info
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          service['name'] ?? 'Dịch vụ',
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          '${service['durationMinutes'] ?? 30} phút',
                          style: TextStyle(fontSize: 13, color: AppColors.grey),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          NumberFormat.currency(
                            locale: 'vi_VN',
                            symbol: '₫',
                            decimalDigits: 0,
                          ).format(service['price'] ?? 0),
                          style: TextStyle(
                            fontSize: 15,
                            fontWeight: FontWeight.bold,
                            color: AppColors.primary,
                          ),
                        ),
                      ],
                    ),
                  ),

                  // Checkbox
                  Checkbox(
                    value: isSelected,
                    onChanged: (value) {
                      setState(() {
                        if (value == true) {
                          _selectedServices.add(service);
                        } else {
                          _selectedServices.removeWhere(
                            (s) =>
                                _getIntValue(s['id']) ==
                                _getIntValue(service['id']),
                          );
                        }
                      });
                    },
                    activeColor: AppColors.primary,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(4),
                    ),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }

  Widget _buildServicePlaceholder() {
    return Container(
      width: 70,
      height: 70,
      color: AppColors.backgroundSecondary,
      child: const Icon(Icons.spa, color: AppColors.grey, size: 30),
    );
  }

  Widget _buildDateTimeSelection() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Date picker
          const Text(
            'Chọn ngày',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 12),

          SizedBox(
            height: 80,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              itemCount: 14, // Next 14 days
              itemBuilder: (context, index) {
                final date = DateTime.now().add(Duration(days: index + 1));
                final isSelected =
                    _selectedDate.day == date.day &&
                    _selectedDate.month == date.month &&
                    _selectedDate.year == date.year;

                return Padding(
                  padding: const EdgeInsets.only(right: 8),
                  child: InkWell(
                    onTap: () => setState(() => _selectedDate = date),
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      width: 60,
                      decoration: BoxDecoration(
                        color: isSelected ? AppColors.primary : AppColors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: isSelected
                              ? AppColors.primary
                              : AppColors.border,
                        ),
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            DateFormat('EEE', 'vi').format(date),
                            style: TextStyle(
                              fontSize: 12,
                              color: isSelected ? Colors.white : AppColors.grey,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            '${date.day}',
                            style: TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.bold,
                              color: isSelected
                                  ? Colors.white
                                  : AppColors.textPrimary,
                            ),
                          ),
                          Text(
                            DateFormat('MMM', 'vi').format(date),
                            style: TextStyle(
                              fontSize: 11,
                              color: isSelected ? Colors.white : AppColors.grey,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                );
              },
            ),
          ),

          const SizedBox(height: 24),

          // Time picker
          const Text(
            'Chọn giờ',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 12),

          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: _timeSlots.map((time) {
              final isSelected = _selectedTime == time;

              return InkWell(
                onTap: () => setState(() => _selectedTime = time),
                borderRadius: BorderRadius.circular(8),
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 10,
                  ),
                  decoration: BoxDecoration(
                    color: isSelected ? AppColors.primary : AppColors.white,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(
                      color: isSelected ? AppColors.primary : AppColors.border,
                    ),
                  ),
                  child: Text(
                    time,
                    style: TextStyle(
                      fontSize: 14,
                      fontWeight: isSelected
                          ? FontWeight.w600
                          : FontWeight.normal,
                      color: isSelected ? Colors.white : AppColors.textPrimary,
                    ),
                  ),
                ),
              );
            }).toList(),
          ),
        ],
      ),
    );
  }

  Widget _buildStaffSelection() {
    return Column(
      children: [
        // Option to skip staff selection
        Container(
          padding: const EdgeInsets.all(16),
          color: AppColors.white,
          child: Row(
            children: [
              Checkbox(
                value: _selectedStaff == null,
                onChanged: (value) {
                  if (value == true) {
                    setState(() => _selectedStaff = null);
                  }
                },
                activeColor: AppColors.primary,
              ),
              const Text('Để salon sắp xếp nhân viên'),
            ],
          ),
        ),

        const Divider(height: 1),

        // Staff list
        Expanded(
          child: _isLoading
              ? const Center(child: CircularProgressIndicator())
              : _availableStaff.isEmpty
              ? const Center(child: Text('Không có nhân viên phù hợp'))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _availableStaff.length,
                  itemBuilder: (context, index) {
                    final staff = _availableStaff[index];
                    final isSelected = _selectedStaff?['id'] == staff['id'];

                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      elevation: isSelected ? 4 : 1,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                        side: BorderSide(
                          color: isSelected
                              ? AppColors.primary
                              : Colors.transparent,
                          width: 2,
                        ),
                      ),
                      child: InkWell(
                        onTap: () => setState(() => _selectedStaff = staff),
                        borderRadius: BorderRadius.circular(12),
                        child: Padding(
                          padding: const EdgeInsets.all(16),
                          child: Row(
                            children: [
                              // Avatar
                              CircleAvatar(
                                radius: 30,
                                backgroundColor: AppColors.primary.withOpacity(
                                  0.1,
                                ),
                                backgroundImage: staff['avatarUrl'] != null
                                    ? NetworkImage(staff['avatarUrl'])
                                    : null,
                                child: staff['avatarUrl'] == null
                                    ? Text(
                                        staff['fullName']
                                                ?.substring(0, 1)
                                                .toUpperCase() ??
                                            'S',
                                        style: const TextStyle(
                                          fontSize: 24,
                                          fontWeight: FontWeight.bold,
                                          color: AppColors.primary,
                                        ),
                                      )
                                    : null,
                              ),
                              const SizedBox(width: 12),

                              // Info
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      staff['fullName'] ??
                                          staff['name'] ??
                                          'Nhân viên',
                                      style: const TextStyle(
                                        fontSize: 16,
                                        fontWeight: FontWeight.w600,
                                      ),
                                    ),
                                    const SizedBox(height: 4),
                                    Text(
                                      staff['position'] ?? 'Stylist',
                                      style: TextStyle(
                                        fontSize: 13,
                                        color: AppColors.grey,
                                      ),
                                    ),
                                    if (staff['level'] != null)
                                      Text(
                                        staff['level'],
                                        style: TextStyle(
                                          fontSize: 12,
                                          color: AppColors.primary,
                                        ),
                                      ),
                                  ],
                                ),
                              ),

                              // Radio
                              Radio<int>(
                                value: _getIntValue(staff['id']),
                                groupValue: _selectedStaff != null
                                    ? _getIntValue(_selectedStaff!['id'])
                                    : null,
                                onChanged: (value) =>
                                    setState(() => _selectedStaff = staff),
                                activeColor: AppColors.primary,
                              ),
                            ],
                          ),
                        ),
                      ),
                    );
                  },
                ),
        ),
      ],
    );
  }

  Widget _buildConfirmation() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Thông tin liên hệ - BẮT BUỘC
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.primary.withOpacity(0.05),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.primary.withOpacity(0.2)),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Icon(
                      Icons.person_outline,
                      color: AppColors.primary,
                      size: 20,
                    ),
                    const SizedBox(width: 8),
                    const Text(
                      'Thông tin liên hệ',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const Text(
                      ' *',
                      style: TextStyle(color: Colors.red, fontSize: 16),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _nameController,
                  decoration: InputDecoration(
                    labelText: 'Họ và tên *',
                    hintText: 'Nhập họ và tên của bạn',
                    prefixIcon: const Icon(Icons.person),
                    filled: true,
                    fillColor: AppColors.white,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(color: AppColors.border),
                    ),
                    enabledBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(color: AppColors.border),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(
                        color: AppColors.primary,
                        width: 2,
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _phoneController,
                  keyboardType: TextInputType.phone,
                  decoration: InputDecoration(
                    labelText: 'Số điện thoại *',
                    hintText: 'Nhập số điện thoại',
                    prefixIcon: const Icon(Icons.phone),
                    filled: true,
                    fillColor: AppColors.white,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(color: AppColors.border),
                    ),
                    enabledBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(color: AppColors.border),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(
                        color: AppColors.primary,
                        width: 2,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 20),

          // Selected services
          const Text(
            'Dịch vụ đã chọn',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 12),

          ...(_selectedServices.map(
            (service) => Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Row(
                children: [
                  const Icon(
                    Icons.check_circle,
                    color: AppColors.primary,
                    size: 20,
                  ),
                  const SizedBox(width: 8),
                  Expanded(child: Text(service['name'] ?? '')),
                  Text(
                    NumberFormat.currency(
                      locale: 'vi_VN',
                      symbol: '₫',
                      decimalDigits: 0,
                    ).format(service['price'] ?? 0),
                    style: const TextStyle(fontWeight: FontWeight.w600),
                  ),
                ],
              ),
            ),
          )),

          const Divider(height: 24),

          // Date & Time
          _buildInfoRow(
            Icons.calendar_today,
            'Ngày',
            DateFormat('EEEE, dd/MM/yyyy', 'vi').format(_selectedDate),
          ),
          const SizedBox(height: 12),
          _buildInfoRow(Icons.access_time, 'Giờ', _selectedTime),
          const SizedBox(height: 12),
          _buildInfoRow(Icons.timer, 'Thời lượng', '$_totalDuration phút'),
          const SizedBox(height: 12),
          _buildInfoRow(
            Icons.person,
            'Nhân viên',
            _selectedStaff?['fullName'] ??
                _selectedStaff?['name'] ??
                'Để salon sắp xếp',
          ),

          const Divider(height: 24),

          // Notes
          const Text(
            'Ghi chú',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 8),
          TextField(
            controller: _notesController,
            maxLines: 3,
            decoration: InputDecoration(
              hintText: 'Thêm ghi chú cho salon (không bắt buộc)',
              filled: true,
              fillColor: AppColors.backgroundSecondary,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide.none,
              ),
            ),
          ),

          const Divider(height: 24),

          // Total
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Tổng tiền',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              Text(
                NumberFormat.currency(
                  locale: 'vi_VN',
                  symbol: '₫',
                  decimalDigits: 0,
                ).format(_totalPrice),
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                  color: AppColors.primary,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
    return Row(
      children: [
        Icon(icon, size: 20, color: AppColors.grey),
        const SizedBox(width: 8),
        Text('$label: ', style: TextStyle(color: AppColors.grey)),
        Expanded(
          child: Text(
            value,
            style: const TextStyle(fontWeight: FontWeight.w600),
          ),
        ),
      ],
    );
  }

  Widget _buildBottomButtons() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: SafeArea(
        child: Row(
          children: [
            if (_currentStep > 0)
              Expanded(
                child: OutlinedButton(
                  onPressed: () => setState(() => _currentStep--),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    side: const BorderSide(color: AppColors.primary),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: const Text('Quay lại'),
                ),
              ),
            if (_currentStep > 0) const SizedBox(width: 12),

            Expanded(
              flex: 2,
              child: ElevatedButton(
                onPressed: _isLoading ? null : _onNextPressed,
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: _isLoading
                    ? const SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          valueColor: AlwaysStoppedAnimation<Color>(
                            Colors.white,
                          ),
                        ),
                      )
                    : Text(
                        _currentStep == 3 ? 'Xác nhận đặt lịch' : 'Tiếp tục',
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _onNextPressed() {
    switch (_currentStep) {
      case 0: // Service selection
        if (_selectedServices.isEmpty) {
          CustomSnackBar.showWarning(
            context,
            'Vui lòng chọn ít nhất 1 dịch vụ',
          );
          return;
        }
        setState(() => _currentStep++);
        break;

      case 1: // Date & Time
        setState(() => _currentStep++);
        _loadAvailableStaff();
        break;

      case 2: // Staff selection
        setState(() => _currentStep++);
        break;

      case 3: // Confirmation - Validate thông tin liên hệ
        if (_nameController.text.trim().isEmpty) {
          CustomSnackBar.showWarning(context, 'Vui lòng nhập họ và tên');
          return;
        }
        if (_phoneController.text.trim().isEmpty) {
          CustomSnackBar.showWarning(context, 'Vui lòng nhập số điện thoại');
          return;
        }
        // Validate phone format (đơn giản)
        final phone = _phoneController.text.trim();
        if (phone.length < 9 || phone.length > 11) {
          CustomSnackBar.showWarning(context, 'Số điện thoại không hợp lệ');
          return;
        }
        _checkAndBookAppointment();
        break;
    }
  }
}
