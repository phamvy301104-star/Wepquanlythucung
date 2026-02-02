// Staff Salary Screen
// Màn hình xem bảng lương chi tiết

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hugeicons/hugeicons.dart';
import 'package:intl/intl.dart';
import '../providers/staff_providers.dart';
import '../models/staff_models.dart';

class StaffSalaryScreen extends ConsumerStatefulWidget {
  const StaffSalaryScreen({super.key});

  @override
  ConsumerState<StaffSalaryScreen> createState() => _StaffSalaryScreenState();
}

class _StaffSalaryScreenState extends ConsumerState<StaffSalaryScreen> {
  int _selectedMonth = DateTime.now().month;
  int _selectedYear = DateTime.now().year;

  final _currencyFormat = NumberFormat.currency(
    locale: 'vi',
    symbol: '',
    decimalDigits: 0,
  );

  @override
  Widget build(BuildContext context) {
    final salaryAsync = ref.watch(
      salaryByMonthProvider(
        SalaryMonthParams(month: _selectedMonth, year: _selectedYear),
      ),
    );

    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      body: SafeArea(
        child: Column(
          children: [
            _buildHeader(),
            Expanded(
              child: RefreshIndicator(
                onRefresh: () async {
                  ref.invalidate(
                    salaryByMonthProvider(
                      SalaryMonthParams(
                        month: _selectedMonth,
                        year: _selectedYear,
                      ),
                    ),
                  );
                },
                child: SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.all(20),
                  child: salaryAsync.when(
                    data: (salary) => salary != null
                        ? _buildSalaryContent(salary)
                        : _buildNoData(),
                    loading: () => const Center(
                      child: Padding(
                        padding: EdgeInsets.all(50),
                        child: CircularProgressIndicator(),
                      ),
                    ),
                    error: (_, __) => const Center(
                      child: Text('Không thể tải dữ liệu lương'),
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(color: Colors.black.withValues(alpha: 0.05), blurRadius: 10),
        ],
      ),
      child: Column(
        children: [
          const Text(
            'Bảng lương',
            style: TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
              color: Color(0xFF1E293B),
            ),
          ),
          const SizedBox(height: 16),
          // Month selector
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              IconButton(
                onPressed: _previousMonth,
                icon: const Icon(HugeIcons.strokeRoundedArrowLeft01),
              ),
              GestureDetector(
                onTap: _showMonthPicker,
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 20,
                    vertical: 10,
                  ),
                  decoration: BoxDecoration(
                    color: const Color(0xFF6366F1).withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(
                    'Tháng $_selectedMonth/$_selectedYear',
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF6366F1),
                      fontSize: 16,
                    ),
                  ),
                ),
              ),
              IconButton(
                onPressed: _nextMonth,
                icon: const Icon(HugeIcons.strokeRoundedArrowRight01),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildNoData() {
    return Container(
      padding: const EdgeInsets.all(40),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Column(
        children: [
          Icon(
            HugeIcons.strokeRoundedMoney04,
            size: 64,
            color: Colors.grey.shade300,
          ),
          const SizedBox(height: 16),
          Text(
            'Chưa có bảng lương cho tháng $_selectedMonth/$_selectedYear',
            style: TextStyle(color: Colors.grey.shade500, fontSize: 16),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  Widget _buildSalaryContent(SalarySlip salary) {
    return Column(
      children: [
        // Net Salary Card
        _buildNetSalaryCard(salary),
        const SizedBox(height: 20),

        // Earnings Section
        _buildSection(
          title: 'Thu nhập',
          icon: HugeIcons.strokeRoundedCoins01,
          iconColor: Colors.green,
          children: [
            _buildSalaryRow(
              'Lương cơ bản',
              salary.baseSalary,
              isPositive: true,
            ),
            _buildSalaryRow(
              'Thưởng tăng ca',
              salary.overtimeBonus,
              isPositive: true,
            ),
            _buildSalaryRow(
              'Hoa hồng',
              salary.commissionBonus,
              isPositive: true,
            ),
            _buildDivider(),
            _buildSalaryRow(
              'Tổng thu nhập',
              salary.grossSalary,
              isPositive: true,
              isBold: true,
            ),
          ],
        ),
        const SizedBox(height: 16),

        // Deductions Section
        _buildSection(
          title: 'Khấu trừ',
          icon: HugeIcons.strokeRoundedMinusSign,
          iconColor: Colors.red,
          children: [
            _buildSalaryRow(
              'Phạt đi trễ (${salary.totalLateMinutes} phút)',
              salary.latePenalty,
              isPositive: false,
            ),
            _buildSalaryRow(
              'Phạt thiếu chấm công (${salary.missedCheckIns} lần)',
              salary.missedCheckPenalty,
              isPositive: false,
            ),
            _buildSalaryRow(
              'Khấu trừ khác',
              salary.otherDeductions,
              isPositive: false,
            ),
            _buildDivider(),
            _buildSalaryRow(
              'Tổng khấu trừ',
              salary.totalDeductions,
              isPositive: false,
              isBold: true,
            ),
          ],
        ),
        const SizedBox(height: 16),

        // Insurance Section
        _buildSection(
          title: 'Bảo hiểm',
          icon: HugeIcons.strokeRoundedShield01,
          iconColor: Colors.blue,
          children: [
            _buildSalaryRow('BHXH (8%)', salary.bhxh, isPositive: false),
            _buildSalaryRow('BHYT (1.5%)', salary.bhyt, isPositive: false),
            _buildSalaryRow('BHTN (1%)', salary.bhtn, isPositive: false),
            _buildDivider(),
            _buildSalaryRow(
              'Tổng BH',
              salary.totalInsurance,
              isPositive: false,
              isBold: true,
            ),
          ],
        ),
        const SizedBox(height: 16),

        // Attendance Stats Section
        _buildSection(
          title: 'Thống kê chấm công',
          icon: HugeIcons.strokeRoundedCalendarCheckIn01,
          iconColor: const Color(0xFF6366F1),
          children: [
            _buildStatRow(
              'Tổng ngày công',
              '${salary.actualWorkDays}/${salary.totalWorkDays} ngày',
            ),
            _buildStatRow('Số lần đi trễ', '${salary.lateCheckIns} lần'),
            _buildStatRow('Thiếu chấm công', '${salary.missedCheckIns} lần'),
            _buildStatRow('Tổng phút trễ', '${salary.totalLateMinutes} phút'),
          ],
        ),
        const SizedBox(height: 16),

        // Status Section
        _buildStatusCard(salary),
      ],
    );
  }

  Widget _buildNetSalaryCard(SalarySlip salary) {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF6366F1), Color(0xFF8B5CF6)],
        ),
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFF6366F1).withValues(alpha: 0.4),
            blurRadius: 20,
            offset: const Offset(0, 10),
          ),
        ],
      ),
      child: Column(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Thực lãnh',
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.8),
                      fontSize: 14,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '${_currencyFormat.format(salary.netSalary)}đ',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 32,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 8,
                ),
                decoration: BoxDecoration(
                  color: _getStatusColor(salary.status).withValues(alpha: 0.2),
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(color: Colors.white.withValues(alpha: 0.3)),
                ),
                child: Text(
                  salary.statusLabel,
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 20),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.white.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _buildMiniStat(
                  'Thu nhập',
                  '${_currencyFormat.format(salary.grossSalary)}đ',
                  Icons.trending_up,
                ),
                Container(
                  width: 1,
                  height: 40,
                  color: Colors.white.withValues(alpha: 0.2),
                ),
                _buildMiniStat(
                  'Khấu trừ',
                  '-${_currencyFormat.format(salary.totalDeductions + salary.totalInsurance)}đ',
                  Icons.trending_down,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMiniStat(String label, String value, IconData icon) {
    return Column(
      children: [
        Row(
          children: [
            Icon(icon, color: Colors.white.withValues(alpha: 0.8), size: 16),
            const SizedBox(width: 4),
            Text(
              label,
              style: TextStyle(
                color: Colors.white.withValues(alpha: 0.8),
                fontSize: 12,
              ),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Text(
          value,
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
            fontSize: 14,
          ),
        ),
      ],
    );
  }

  Widget _buildSection({
    required String title,
    required IconData icon,
    required Color iconColor,
    required List<Widget> children,
  }) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(color: Colors.black.withValues(alpha: 0.05), blurRadius: 10),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: iconColor.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Icon(icon, color: iconColor, size: 20),
              ),
              const SizedBox(width: 12),
              Text(
                title,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFF1E293B),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          ...children,
        ],
      ),
    );
  }

  Widget _buildSalaryRow(
    String label,
    double amount, {
    bool isPositive = true,
    bool isBold = false,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              color: Colors.grey.shade700,
              fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
            ),
          ),
          Text(
            '${isPositive ? '+' : '-'}${_currencyFormat.format(amount)}đ',
            style: TextStyle(
              color: isPositive ? Colors.green : Colors.red,
              fontWeight: isBold ? FontWeight.bold : FontWeight.w600,
              fontSize: isBold ? 16 : 14,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: TextStyle(color: Colors.grey.shade700)),
          Text(
            value,
            style: const TextStyle(
              fontWeight: FontWeight.w600,
              color: Color(0xFF1E293B),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDivider() {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Divider(color: Colors.grey.shade200),
    );
  }

  Widget _buildStatusCard(SalarySlip salary) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: _getStatusColor(salary.status).withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: _getStatusColor(salary.status).withValues(alpha: 0.3),
        ),
      ),
      child: Row(
        children: [
          Icon(
            _getStatusIcon(salary.status),
            color: _getStatusColor(salary.status),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Trạng thái: ${salary.statusLabel}',
                  style: TextStyle(
                    fontWeight: FontWeight.bold,
                    color: _getStatusColor(salary.status),
                  ),
                ),
                if (salary.paidDate != null)
                  Text(
                    'Ngày thanh toán: ${DateFormat('dd/MM/yyyy').format(salary.paidDate!)}',
                    style: TextStyle(fontSize: 13, color: Colors.grey.shade600),
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Color _getStatusColor(String status) {
    switch (status) {
      case 'Pending':
        return Colors.orange;
      case 'Processed':
        return Colors.blue;
      case 'Paid':
        return Colors.green;
      default:
        return Colors.grey;
    }
  }

  IconData _getStatusIcon(String status) {
    switch (status) {
      case 'Pending':
        return HugeIcons.strokeRoundedTime04;
      case 'Processed':
        return HugeIcons.strokeRoundedTick01;
      case 'Paid':
        return HugeIcons.strokeRoundedCheckmarkCircle01;
      default:
        return HugeIcons.strokeRoundedInformationCircle;
    }
  }

  void _previousMonth() {
    setState(() {
      if (_selectedMonth == 1) {
        _selectedMonth = 12;
        _selectedYear--;
      } else {
        _selectedMonth--;
      }
    });
  }

  void _nextMonth() {
    final now = DateTime.now();
    if (_selectedYear < now.year ||
        (_selectedYear == now.year && _selectedMonth < now.month)) {
      setState(() {
        if (_selectedMonth == 12) {
          _selectedMonth = 1;
          _selectedYear++;
        } else {
          _selectedMonth++;
        }
      });
    }
  }

  void _showMonthPicker() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => Container(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Chọn tháng',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 20),
            SizedBox(
              height: 200,
              child: GridView.builder(
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 4,
                  childAspectRatio: 1.5,
                ),
                itemCount: 12,
                itemBuilder: (context, index) {
                  final month = index + 1;
                  final isSelected = month == _selectedMonth;

                  return GestureDetector(
                    onTap: () {
                      setState(() => _selectedMonth = month);
                      Navigator.pop(context);
                    },
                    child: Container(
                      margin: const EdgeInsets.all(4),
                      decoration: BoxDecoration(
                        color: isSelected
                            ? const Color(0xFF6366F1)
                            : Colors.grey.shade100,
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Center(
                        child: Text(
                          'T$month',
                          style: TextStyle(
                            color: isSelected ? Colors.white : Colors.black,
                            fontWeight: isSelected
                                ? FontWeight.bold
                                : FontWeight.normal,
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
