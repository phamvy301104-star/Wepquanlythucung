// Staff Main Screen
// Màn hình chính cho nhân viên với Bottom Navigation Bar

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hugeicons/hugeicons.dart';
import 'staff_home_screen.dart';
import 'staff_attendance_screen.dart';
import 'staff_salary_screen.dart';
import 'staff_chat_screen.dart';
import '../providers/staff_providers.dart';

class StaffMainScreen extends ConsumerStatefulWidget {
  const StaffMainScreen({super.key});

  @override
  ConsumerState<StaffMainScreen> createState() => _StaffMainScreenState();
}

class _StaffMainScreenState extends ConsumerState<StaffMainScreen> {
  int _currentIndex = 0;

  final List<Widget> _screens = [
    const StaffHomeScreen(),
    const StaffAttendanceScreen(),
    const SizedBox(), // Placeholder for FAB
    const StaffSalaryScreen(),
    const StaffChatScreen(),
  ];

  @override
  void initState() {
    super.initState();
    // Connect to SignalR for real-time chat
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final signalR = ref.read(staffChatSignalRServiceProvider);
      signalR.connect();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(
        index: _currentIndex == 2 ? 0 : _currentIndex,
        children: _screens,
      ),
      bottomNavigationBar: _buildBottomNavBar(),
      floatingActionButton: _buildCheckInFAB(),
      floatingActionButtonLocation: FloatingActionButtonLocation.centerDocked,
    );
  }

  Widget _buildBottomNavBar() {
    return Container(
      decoration: BoxDecoration(
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 20,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      child: BottomAppBar(
        shape: const CircularNotchedRectangle(),
        notchMargin: 8,
        color: Colors.white,
        elevation: 0,
        child: SizedBox(
          height: 65,
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceAround,
            children: [
              _buildNavItem(0, HugeIcons.strokeRoundedHome01, 'Trang chủ'),
              _buildNavItem(
                1,
                HugeIcons.strokeRoundedCalendarCheckIn01,
                'Chấm công',
              ),
              const SizedBox(width: 60), // Space for FAB
              _buildNavItem(3, HugeIcons.strokeRoundedMoney04, 'Lương'),
              _buildNavItem(4, HugeIcons.strokeRoundedMessage01, 'Chat'),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildNavItem(int index, IconData icon, String label) {
    final isSelected = _currentIndex == index;

    return InkWell(
      onTap: () => setState(() => _currentIndex = index),
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              icon,
              color: isSelected ? const Color(0xFF6366F1) : Colors.grey,
              size: 22,
            ),
            const SizedBox(height: 2),
            Text(
              label,
              style: TextStyle(
                fontSize: 10,
                fontWeight: isSelected ? FontWeight.w600 : FontWeight.w400,
                color: isSelected ? const Color(0xFF6366F1) : Colors.grey,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCheckInFAB() {
    // Get today's attendance to show current status
    final attendanceAsync = ref.watch(attendanceNotifierProvider);

    return attendanceAsync.when(
      data: (data) {
        final nextCheck = data?.nextCheckType ?? 1;
        final isCompleted = nextCheck == 0;

        return GestureDetector(
          onTap: () {
            if (!isCompleted) {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) =>
                      const StaffAttendanceScreen(autoOpenCamera: true),
                ),
              );
            }
          },
          child: Container(
            width: 65,
            height: 65,
            decoration: BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: isCompleted
                    ? [Colors.green, Colors.green.shade700]
                    : [const Color(0xFF6366F1), const Color(0xFF8B5CF6)],
              ),
              shape: BoxShape.circle,
              boxShadow: [
                BoxShadow(
                  color: (isCompleted ? Colors.green : const Color(0xFF6366F1))
                      .withValues(alpha: 0.4),
                  blurRadius: 20,
                  offset: const Offset(0, 8),
                ),
              ],
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  isCompleted
                      ? HugeIcons.strokeRoundedCheckmarkCircle01
                      : HugeIcons.strokeRoundedFaceId,
                  color: Colors.white,
                  size: 24,
                ),
                if (!isCompleted) ...[
                  const SizedBox(height: 2),
                  Text(
                    'Check $nextCheck',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 9,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ],
            ),
          ),
        );
      },
      loading: () => Container(
        width: 65,
        height: 65,
        decoration: BoxDecoration(
          color: Colors.grey.shade300,
          shape: BoxShape.circle,
        ),
        child: const CircularProgressIndicator(color: Colors.white),
      ),
      error: (_, __) => Container(
        width: 65,
        height: 65,
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [Color(0xFF6366F1), Color(0xFF8B5CF6)],
          ),
          shape: BoxShape.circle,
        ),
        child: const Icon(
          HugeIcons.strokeRoundedFaceId,
          color: Colors.white,
          size: 28,
        ),
      ),
    );
  }
}
