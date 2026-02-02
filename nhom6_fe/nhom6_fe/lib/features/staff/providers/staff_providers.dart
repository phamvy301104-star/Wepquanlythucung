// Staff Providers
// Riverpod providers for Staff module state management

import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../models/staff_models.dart';
import '../services/staff_api_service.dart';
import '../services/staff_chat_signalr_service.dart';

// ============================================
// SERVICE PROVIDERS
// ============================================
final staffApiServiceProvider = Provider<StaffApiService>((ref) {
  return StaffApiService();
});

final staffChatSignalRServiceProvider = Provider<StaffChatSignalRService>((
  ref,
) {
  final service = StaffChatSignalRService();
  ref.onDispose(() => service.dispose());
  return service;
});

// ============================================
// PROFILE PROVIDERS
// Use .autoDispose to prevent memory leaks, keepAlive to cache results
// Enhanced với error handling để tránh API spam
// ============================================
final staffProfileProvider = FutureProvider.autoDispose<StaffProfile?>((
  ref,
) async {
  // Keep provider alive for 5 minutes to avoid re-fetching
  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 5), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  final profile = await service.getProfile();

  // Nếu profile null, có thể do lỗi hoặc staff chưa được tạo
  // Không throw error, để UI handle gracefully
  return profile;
});

final staffStatsProvider = FutureProvider.autoDispose<StaffStats?>((ref) async {
  // Chỉ load stats nếu profile đã loaded thành công
  final profile = ref.watch(staffProfileProvider).valueOrNull;
  if (profile == null) {
    // Không có profile, không cần load stats
    return null;
  }

  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 5), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  return service.getStats();
});

final staffScheduleProvider = FutureProvider.autoDispose<List<StaffSchedule>>((
  ref,
) async {
  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 5), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  return service.getSchedule();
});

// ============================================
// ATTENDANCE PROVIDERS
// Chỉ load khi đã có profile
// ============================================
final todayAttendanceProvider = FutureProvider.autoDispose<AttendanceToday?>((
  ref,
) async {
  // Chỉ load nếu profile đã loaded
  final profile = ref.watch(staffProfileProvider).valueOrNull;
  if (profile == null) {
    return null;
  }

  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 2), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  return service.getTodayAttendance();
});

// Attendance history with filters
class AttendanceHistoryParams {
  final DateTime? startDate;
  final DateTime? endDate;
  final int page;
  final int pageSize;

  AttendanceHistoryParams({
    this.startDate,
    this.endDate,
    this.page = 1,
    this.pageSize = 20,
  });
}

final attendanceHistoryProvider = FutureProvider.family
    .autoDispose<List<Attendance>, AttendanceHistoryParams>((
      ref,
      params,
    ) async {
      // Kiểm tra profile trước khi load history
      final profile = ref.watch(staffProfileProvider).valueOrNull;
      if (profile == null) {
        return []; // Trả về list rỗng nếu chưa có profile
      }

      final link = ref.keepAlive();
      Future.delayed(const Duration(minutes: 2), () => link.close());

      final service = ref.watch(staffApiServiceProvider);
      return service.getAttendanceHistory(
        startDate: params.startDate,
        endDate: params.endDate,
        page: params.page,
        pageSize: params.pageSize,
      );
    });

// Attendance stats by month
class AttendanceStatsParams {
  final int month;
  final int year;

  AttendanceStatsParams({required this.month, required this.year});
}

final attendanceStatsProvider = FutureProvider.family
    .autoDispose<Map<String, dynamic>, AttendanceStatsParams>((
      ref,
      params,
    ) async {
      final link = ref.keepAlive();
      Future.delayed(const Duration(minutes: 2), () => link.close());

      final service = ref.watch(staffApiServiceProvider);
      return service.getAttendanceStats(params.month, params.year);
    });

// ============================================
// SALARY PROVIDERS
// Chỉ load khi đã có profile
// ============================================
final currentSalaryProvider = FutureProvider.autoDispose<SalarySlip?>((
  ref,
) async {
  // Chỉ load nếu profile đã loaded
  final profile = ref.watch(staffProfileProvider).valueOrNull;
  if (profile == null) {
    return null;
  }

  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 5), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  return service.getCurrentSalary();
});

class SalaryMonthParams {
  final int month;
  final int year;

  SalaryMonthParams({required this.month, required this.year});
}

final salaryByMonthProvider = FutureProvider.family
    .autoDispose<SalarySlip?, SalaryMonthParams>((ref, params) async {
      // Kiểm tra profile trước khi load salary
      final profile = ref.watch(staffProfileProvider).valueOrNull;
      if (profile == null) {
        return null; // Trả về null nếu chưa có profile
      }

      final link = ref.keepAlive();
      Future.delayed(const Duration(minutes: 5), () => link.close());

      final service = ref.watch(staffApiServiceProvider);
      return service.getSalaryByMonth(params.month, params.year);
    });

final salaryHistoryProvider = FutureProvider.autoDispose<List<SalarySlip>>((
  ref,
) async {
  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 5), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  return service.getSalaryHistory();
});

// ============================================
// APPOINTMENTS PROVIDERS
// Chỉ load khi đã có profile
// ============================================
final staffAppointmentsProvider = FutureProvider.family
    .autoDispose<List<StaffAppointment>, DateTime?>((ref, date) async {
      // Chỉ load nếu profile đã loaded
      final profile = ref.watch(staffProfileProvider).valueOrNull;
      if (profile == null) {
        return [];
      }

      final link = ref.keepAlive();
      Future.delayed(const Duration(minutes: 2), () => link.close());

      final service = ref.watch(staffApiServiceProvider);
      return service.getAppointments(date: date);
    });

class CalendarParams {
  final int month;
  final int year;

  CalendarParams({required this.month, required this.year});
}

final appointmentsCalendarProvider = FutureProvider.family
    .autoDispose<Map<String, List<StaffAppointment>>, CalendarParams>((
      ref,
      params,
    ) async {
      final link = ref.keepAlive();
      Future.delayed(const Duration(minutes: 5), () => link.close());

      final service = ref.watch(staffApiServiceProvider);
      return service.getAppointmentsCalendar(params.month, params.year);
    });

// ============================================
// CHAT PROVIDERS
// Chỉ load khi đã có profile
// ============================================
final colleaguesProvider = FutureProvider.autoDispose<List<StaffColleague>>((
  ref,
) async {
  // Chỉ load nếu profile đã loaded
  final profile = ref.watch(staffProfileProvider).valueOrNull;
  if (profile == null) {
    return [];
  }

  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 5), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  return service.getColleagues();
});

final chatRoomsProvider = FutureProvider.autoDispose<List<StaffChatRoom>>((
  ref,
) async {
  // Chỉ load nếu profile đã loaded
  final profile = ref.watch(staffProfileProvider).valueOrNull;
  if (profile == null) {
    return [];
  }

  final link = ref.keepAlive();
  Future.delayed(const Duration(minutes: 2), () => link.close());

  final service = ref.watch(staffApiServiceProvider);
  return service.getChatRooms();
});

final chatMessagesProvider = FutureProvider.family
    .autoDispose<List<StaffChatMessage>, int>((ref, roomId) async {
      final link = ref.keepAlive();
      Future.delayed(const Duration(minutes: 1), () => link.close());

      final service = ref.watch(staffApiServiceProvider);
      return service.getMessages(roomId);
    });

// Selected room state
final selectedChatRoomProvider = StateProvider<StaffChatRoom?>((ref) => null);

// SignalR connection state
final chatConnectionStateProvider = StreamProvider<bool>((ref) {
  final signalR = ref.watch(staffChatSignalRServiceProvider);
  return signalR.onConnectionStateChanged;
});

// Real-time message stream
final incomingMessageProvider = StreamProvider<StaffChatMessage>((ref) {
  final signalR = ref.watch(staffChatSignalRServiceProvider);
  return signalR.onMessageReceived;
});

// ============================================
// ATTENDANCE STATE NOTIFIER
// ============================================
class AttendanceNotifier extends StateNotifier<AsyncValue<AttendanceToday?>> {
  final StaffApiService _apiService;

  AttendanceNotifier(this._apiService) : super(const AsyncValue.loading()) {
    loadToday();
  }

  Future<void> loadToday() async {
    state = const AsyncValue.loading();
    try {
      final data = await _apiService.getTodayAttendance();
      state = AsyncValue.data(data);
    } catch (e, st) {
      state = AsyncValue.error(e, st);
    }
  }

  Future<Map<String, dynamic>> checkIn({
    required int checkType,
    required String photoBase64,
    String? notes,
  }) async {
    final result = await _apiService.checkIn(
      checkType: checkType,
      photoBase64: photoBase64,
      notes: notes,
    );

    if (result['success'] == true) {
      // Reload today's attendance
      await loadToday();
    }

    return result;
  }
}

final attendanceNotifierProvider =
    StateNotifierProvider<AttendanceNotifier, AsyncValue<AttendanceToday?>>((
      ref,
    ) {
      final service = ref.watch(staffApiServiceProvider);
      return AttendanceNotifier(service);
    });

// ============================================
// CHAT STATE NOTIFIER
// ============================================
class ChatNotifier extends StateNotifier<List<StaffChatMessage>> {
  final StaffApiService _apiService;
  final StaffChatSignalRService _signalRService;
  int? _currentRoomId;

  ChatNotifier(this._apiService, this._signalRService) : super([]) {
    _listenToIncomingMessages();
  }

  void _listenToIncomingMessages() {
    _signalRService.onMessageReceived.listen((message) {
      if (message.roomId == _currentRoomId) {
        state = [...state, message];
      }
    });
  }

  Future<void> loadMessages(int roomId) async {
    _currentRoomId = roomId;
    final messages = await _apiService.getMessages(roomId);
    state = messages;

    // Join the room for real-time updates
    await _signalRService.joinRoom(roomId);

    // Mark as read
    await _apiService.markMessagesAsRead(roomId);
  }

  Future<void> sendMessage(String content) async {
    if (_currentRoomId == null) return;

    final message = await _apiService.sendMessage(
      roomId: _currentRoomId!,
      content: content,
    );

    if (message != null) {
      // Message will be added via SignalR, but add immediately for responsiveness
      state = [...state, message];
    }
  }

  void leaveRoom() {
    if (_currentRoomId != null) {
      _signalRService.leaveRoom(_currentRoomId!);
      _currentRoomId = null;
    }
  }
}

final chatNotifierProvider =
    StateNotifierProvider<ChatNotifier, List<StaffChatMessage>>((ref) {
      final apiService = ref.watch(staffApiServiceProvider);
      final signalRService = ref.watch(staffChatSignalRServiceProvider);
      return ChatNotifier(apiService, signalRService);
    });
