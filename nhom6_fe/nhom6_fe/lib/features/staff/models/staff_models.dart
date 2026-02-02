// Staff Module Models
// Models cho Staff App bao gồm: Profile, Attendance, Salary, Chat

// ============================================
// STAFF PROFILE MODEL
// ============================================
class StaffProfile {
  final int staffId;
  final String userId;
  final String userName;
  final String? fullName;
  final String? email;
  final String? phone;
  final String? avatarUrl;
  final String? position;
  final double baseSalary;
  final double commissionPercent;
  final DateTime hireDate;
  final bool isActive;

  StaffProfile({
    required this.staffId,
    required this.userId,
    required this.userName,
    this.fullName,
    this.email,
    this.phone,
    this.avatarUrl,
    this.position,
    required this.baseSalary,
    required this.commissionPercent,
    required this.hireDate,
    required this.isActive,
  });

  factory StaffProfile.fromJson(Map<String, dynamic> json) {
    return StaffProfile(
      staffId: json['staffId'] ?? 0,
      userId: json['userId'] ?? '',
      userName: json['userName'] ?? '',
      fullName: json['fullName'],
      email: json['email'],
      phone: json['phone'],
      avatarUrl: json['avatarUrl'],
      position: json['position'],
      baseSalary: (json['baseSalary'] ?? 0).toDouble(),
      commissionPercent: (json['commissionPercent'] ?? 0).toDouble(),
      hireDate: DateTime.tryParse(json['hireDate'] ?? '') ?? DateTime.now(),
      isActive: json['isActive'] ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'staffId': staffId,
      'userId': userId,
      'userName': userName,
      'fullName': fullName,
      'email': email,
      'phone': phone,
      'avatarUrl': avatarUrl,
      'position': position,
      'baseSalary': baseSalary,
      'commissionPercent': commissionPercent,
      'hireDate': hireDate.toIso8601String(),
      'isActive': isActive,
    };
  }
}

// ============================================
// STAFF STATS MODEL
// ============================================
class StaffStats {
  final int totalAppointmentsThisMonth;
  final int completedAppointmentsThisMonth;
  final double totalRevenueThisMonth;
  final double avgRating;
  final int totalWorkDaysThisMonth;
  final int onTimeCheckIns;
  final int lateCheckIns;

  StaffStats({
    required this.totalAppointmentsThisMonth,
    required this.completedAppointmentsThisMonth,
    required this.totalRevenueThisMonth,
    required this.avgRating,
    required this.totalWorkDaysThisMonth,
    required this.onTimeCheckIns,
    required this.lateCheckIns,
  });

  factory StaffStats.fromJson(Map<String, dynamic> json) {
    return StaffStats(
      totalAppointmentsThisMonth: json['totalAppointmentsThisMonth'] ?? 0,
      completedAppointmentsThisMonth:
          json['completedAppointmentsThisMonth'] ?? 0,
      totalRevenueThisMonth: (json['totalRevenueThisMonth'] ?? 0).toDouble(),
      avgRating: (json['avgRating'] ?? 0).toDouble(),
      totalWorkDaysThisMonth: json['totalWorkDaysThisMonth'] ?? 0,
      onTimeCheckIns: json['onTimeCheckIns'] ?? 0,
      lateCheckIns: json['lateCheckIns'] ?? 0,
    );
  }
}

// ============================================
// STAFF SCHEDULE MODEL
// ============================================
class StaffSchedule {
  final int scheduleId;
  final int staffId;
  final int dayOfWeek;
  final String dayName;
  final String startTime;
  final String endTime;
  final String? breakStartTime;
  final String? breakEndTime;
  final bool isWorkingDay;

  StaffSchedule({
    required this.scheduleId,
    required this.staffId,
    required this.dayOfWeek,
    required this.dayName,
    required this.startTime,
    required this.endTime,
    this.breakStartTime,
    this.breakEndTime,
    required this.isWorkingDay,
  });

  factory StaffSchedule.fromJson(Map<String, dynamic> json) {
    return StaffSchedule(
      scheduleId: json['scheduleId'] ?? 0,
      staffId: json['staffId'] ?? 0,
      dayOfWeek: json['dayOfWeek'] ?? 0,
      dayName: json['dayName'] ?? '',
      startTime: json['startTime'] ?? '08:00',
      endTime: json['endTime'] ?? '17:00',
      breakStartTime: json['breakStartTime'],
      breakEndTime: json['breakEndTime'],
      isWorkingDay: json['isWorkingDay'] ?? false,
    );
  }
}

// ============================================
// ATTENDANCE MODEL
// ============================================
class Attendance {
  final int attendanceId;
  final int staffId;
  final DateTime date;

  // 4 check times
  final DateTime? checkIn1Time; // Ca bắt đầu
  final DateTime? checkIn2Time; // Bắt đầu nghỉ trưa
  final DateTime? checkIn3Time; // Kết thúc nghỉ trưa
  final DateTime? checkIn4Time; // Ca kết thúc

  // Photo URLs
  final String? checkIn1PhotoUrl;
  final String? checkIn2PhotoUrl;
  final String? checkIn3PhotoUrl;
  final String? checkIn4PhotoUrl;

  // Scheduled times
  final String scheduledStartTime;
  final String scheduledEndTime;
  final String? scheduledBreakStart;
  final String? scheduledBreakEnd;

  // Late calculations
  final int lateMinutes;
  final int missedChecks;
  final double totalPenalty;

  // Status
  final String status;
  final String? notes;

  Attendance({
    required this.attendanceId,
    required this.staffId,
    required this.date,
    this.checkIn1Time,
    this.checkIn2Time,
    this.checkIn3Time,
    this.checkIn4Time,
    this.checkIn1PhotoUrl,
    this.checkIn2PhotoUrl,
    this.checkIn3PhotoUrl,
    this.checkIn4PhotoUrl,
    required this.scheduledStartTime,
    required this.scheduledEndTime,
    this.scheduledBreakStart,
    this.scheduledBreakEnd,
    required this.lateMinutes,
    required this.missedChecks,
    required this.totalPenalty,
    required this.status,
    this.notes,
  });

  factory Attendance.fromJson(Map<String, dynamic> json) {
    return Attendance(
      attendanceId: json['attendanceId'] ?? 0,
      staffId: json['staffId'] ?? 0,
      date: DateTime.tryParse(json['date'] ?? '') ?? DateTime.now(),
      checkIn1Time: json['checkIn1Time'] != null
          ? DateTime.tryParse(json['checkIn1Time'])
          : null,
      checkIn2Time: json['checkIn2Time'] != null
          ? DateTime.tryParse(json['checkIn2Time'])
          : null,
      checkIn3Time: json['checkIn3Time'] != null
          ? DateTime.tryParse(json['checkIn3Time'])
          : null,
      checkIn4Time: json['checkIn4Time'] != null
          ? DateTime.tryParse(json['checkIn4Time'])
          : null,
      checkIn1PhotoUrl: json['checkIn1PhotoUrl'],
      checkIn2PhotoUrl: json['checkIn2PhotoUrl'],
      checkIn3PhotoUrl: json['checkIn3PhotoUrl'],
      checkIn4PhotoUrl: json['checkIn4PhotoUrl'],
      scheduledStartTime: json['scheduledStartTime'] ?? '08:00',
      scheduledEndTime: json['scheduledEndTime'] ?? '17:00',
      scheduledBreakStart: json['scheduledBreakStart'],
      scheduledBreakEnd: json['scheduledBreakEnd'],
      lateMinutes: json['lateMinutes'] ?? 0,
      missedChecks: json['missedChecks'] ?? 0,
      totalPenalty: (json['totalPenalty'] ?? 0).toDouble(),
      status: json['status'] ?? 'Pending',
      notes: json['notes'],
    );
  }

  // Check type helper
  int get nextCheckType {
    if (checkIn1Time == null) return 1;
    if (checkIn2Time == null) return 2;
    if (checkIn3Time == null) return 3;
    if (checkIn4Time == null) return 4;
    return 0; // All checks completed
  }

  String get nextCheckLabel {
    switch (nextCheckType) {
      case 1:
        return 'Bắt đầu ca';
      case 2:
        return 'Nghỉ trưa';
      case 3:
        return 'Kết thúc nghỉ';
      case 4:
        return 'Kết thúc ca';
      default:
        return 'Hoàn thành';
    }
  }

  bool get isAllChecksCompleted => nextCheckType == 0;
}

// ============================================
// ATTENDANCE TODAY DTO
// ============================================
class AttendanceToday {
  final Attendance? attendance;
  final StaffSchedule? schedule;
  final int nextCheckType;
  final String nextCheckLabel;
  final String? scheduledTime;
  final bool canCheckIn;
  final String message;

  AttendanceToday({
    this.attendance,
    this.schedule,
    required this.nextCheckType,
    required this.nextCheckLabel,
    this.scheduledTime,
    required this.canCheckIn,
    required this.message,
  });

  factory AttendanceToday.fromJson(Map<String, dynamic> json) {
    return AttendanceToday(
      attendance: json['attendance'] != null
          ? Attendance.fromJson(json['attendance'])
          : null,
      schedule: json['schedule'] != null
          ? StaffSchedule.fromJson(json['schedule'])
          : null,
      nextCheckType: json['nextCheckType'] ?? 1,
      nextCheckLabel: json['nextCheckLabel'] ?? 'Bắt đầu ca',
      scheduledTime: json['scheduledTime'],
      canCheckIn: json['canCheckIn'] ?? true,
      message: json['message'] ?? '',
    );
  }
}

// ============================================
// SALARY SLIP MODEL
// ============================================
class SalarySlip {
  final int salarySlipId;
  final int staffId;
  final String? staffName;
  final int month;
  final int year;

  // Earnings
  final double baseSalary;
  final double overtimeBonus;
  final double commissionBonus;
  final double totalBonus;

  // Deductions
  final double latePenalty;
  final double missedCheckPenalty;
  final double otherDeductions;
  final double totalDeductions;

  // Insurance
  final double bhxh; // 8%
  final double bhyt; // 1.5%
  final double bhtn; // 1%
  final double totalInsurance;

  // Final
  final double grossSalary;
  final double netSalary;

  // Attendance stats
  final int totalWorkDays;
  final int actualWorkDays;
  final int lateCheckIns;
  final int missedCheckIns;
  final int totalLateMinutes;

  // Status
  final String status;
  final DateTime? paidDate;
  final String? notes;
  final DateTime createdAt;

  SalarySlip({
    required this.salarySlipId,
    required this.staffId,
    this.staffName,
    required this.month,
    required this.year,
    required this.baseSalary,
    required this.overtimeBonus,
    required this.commissionBonus,
    required this.totalBonus,
    required this.latePenalty,
    required this.missedCheckPenalty,
    required this.otherDeductions,
    required this.totalDeductions,
    required this.bhxh,
    required this.bhyt,
    required this.bhtn,
    required this.totalInsurance,
    required this.grossSalary,
    required this.netSalary,
    required this.totalWorkDays,
    required this.actualWorkDays,
    required this.lateCheckIns,
    required this.missedCheckIns,
    required this.totalLateMinutes,
    required this.status,
    this.paidDate,
    this.notes,
    required this.createdAt,
  });

  factory SalarySlip.fromJson(Map<String, dynamic> json) {
    return SalarySlip(
      salarySlipId: json['salarySlipId'] ?? 0,
      staffId: json['staffId'] ?? 0,
      staffName: json['staffName'],
      month: json['month'] ?? DateTime.now().month,
      year: json['year'] ?? DateTime.now().year,
      baseSalary: (json['baseSalary'] ?? 0).toDouble(),
      overtimeBonus: (json['overtimeBonus'] ?? 0).toDouble(),
      commissionBonus: (json['commissionBonus'] ?? 0).toDouble(),
      totalBonus: (json['totalBonus'] ?? 0).toDouble(),
      latePenalty: (json['latePenalty'] ?? 0).toDouble(),
      missedCheckPenalty: (json['missedCheckPenalty'] ?? 0).toDouble(),
      otherDeductions: (json['otherDeductions'] ?? 0).toDouble(),
      totalDeductions: (json['totalDeductions'] ?? 0).toDouble(),
      bhxh: (json['bhxh'] ?? 0).toDouble(),
      bhyt: (json['bhyt'] ?? 0).toDouble(),
      bhtn: (json['bhtn'] ?? 0).toDouble(),
      totalInsurance: (json['totalInsurance'] ?? 0).toDouble(),
      grossSalary: (json['grossSalary'] ?? 0).toDouble(),
      netSalary: (json['netSalary'] ?? 0).toDouble(),
      totalWorkDays: json['totalWorkDays'] ?? 0,
      actualWorkDays: json['actualWorkDays'] ?? 0,
      lateCheckIns: json['lateCheckIns'] ?? 0,
      missedCheckIns: json['missedCheckIns'] ?? 0,
      totalLateMinutes: json['totalLateMinutes'] ?? 0,
      status: json['status'] ?? 'Pending',
      paidDate: json['paidDate'] != null
          ? DateTime.tryParse(json['paidDate'])
          : null,
      notes: json['notes'],
      createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now(),
    );
  }

  String get monthYearLabel {
    const months = [
      'Tháng 1',
      'Tháng 2',
      'Tháng 3',
      'Tháng 4',
      'Tháng 5',
      'Tháng 6',
      'Tháng 7',
      'Tháng 8',
      'Tháng 9',
      'Tháng 10',
      'Tháng 11',
      'Tháng 12',
    ];
    return '${months[month - 1]}/$year';
  }

  String get statusLabel {
    switch (status) {
      case 'Pending':
        return 'Chờ xử lý';
      case 'Processed':
        return 'Đã tính';
      case 'Paid':
        return 'Đã thanh toán';
      default:
        return status;
    }
  }
}

// ============================================
// CHAT MODELS
// ============================================
class StaffColleague {
  final int staffId;
  final String userId;
  final String userName;
  final String? fullName;
  final String? avatarUrl;
  final String? position;
  final bool isOnline;
  final DateTime? lastSeen;

  StaffColleague({
    required this.staffId,
    required this.userId,
    required this.userName,
    this.fullName,
    this.avatarUrl,
    this.position,
    required this.isOnline,
    this.lastSeen,
  });

  factory StaffColleague.fromJson(Map<String, dynamic> json) {
    return StaffColleague(
      staffId: json['staffId'] ?? 0,
      userId: json['userId'] ?? '',
      userName: json['userName'] ?? '',
      fullName: json['fullName'],
      avatarUrl: json['avatarUrl'],
      position: json['position'],
      isOnline: json['isOnline'] ?? false,
      lastSeen: json['lastSeen'] != null
          ? DateTime.tryParse(json['lastSeen'])
          : null,
    );
  }

  String get displayName => fullName ?? userName;
}

class StaffChatRoom {
  final int roomId;
  final int staff1Id;
  final int staff2Id;
  final String otherStaffName;
  final String? otherStaffAvatar;
  final String? otherStaffPosition;
  final bool isOtherOnline;
  final String? lastMessage;
  final DateTime? lastMessageTime;
  final int unreadCount;

  StaffChatRoom({
    required this.roomId,
    required this.staff1Id,
    required this.staff2Id,
    required this.otherStaffName,
    this.otherStaffAvatar,
    this.otherStaffPosition,
    required this.isOtherOnline,
    this.lastMessage,
    this.lastMessageTime,
    required this.unreadCount,
  });

  factory StaffChatRoom.fromJson(Map<String, dynamic> json) {
    return StaffChatRoom(
      roomId: json['roomId'] ?? 0,
      staff1Id: json['staff1Id'] ?? 0,
      staff2Id: json['staff2Id'] ?? 0,
      otherStaffName: json['otherStaffName'] ?? '',
      otherStaffAvatar: json['otherStaffAvatar'],
      otherStaffPosition: json['otherStaffPosition'],
      isOtherOnline: json['isOtherOnline'] ?? false,
      lastMessage: json['lastMessage'],
      lastMessageTime: json['lastMessageTime'] != null
          ? DateTime.tryParse(json['lastMessageTime'])
          : null,
      unreadCount: json['unreadCount'] ?? 0,
    );
  }
}

class StaffChatMessage {
  final int messageId;
  final int roomId;
  final int senderId;
  final String senderName;
  final String? senderAvatar;
  final String content;
  final String messageType;
  final bool isRead;
  final DateTime sentAt;
  final DateTime? readAt;
  final bool isMe;

  StaffChatMessage({
    required this.messageId,
    required this.roomId,
    required this.senderId,
    required this.senderName,
    this.senderAvatar,
    required this.content,
    required this.messageType,
    required this.isRead,
    required this.sentAt,
    this.readAt,
    required this.isMe,
  });

  factory StaffChatMessage.fromJson(Map<String, dynamic> json) {
    return StaffChatMessage(
      messageId: json['messageId'] ?? 0,
      roomId: json['roomId'] ?? 0,
      senderId: json['senderId'] ?? 0,
      senderName: json['senderName'] ?? '',
      senderAvatar: json['senderAvatar'],
      content: json['content'] ?? '',
      messageType: json['messageType'] ?? 'Text',
      isRead: json['isRead'] ?? false,
      sentAt: DateTime.tryParse(json['sentAt'] ?? '') ?? DateTime.now(),
      readAt: json['readAt'] != null ? DateTime.tryParse(json['readAt']) : null,
      isMe: json['isMe'] ?? false,
    );
  }
}

// ============================================
// STAFF APPOINTMENT MODEL
// ============================================
class StaffAppointment {
  final int appointmentId;
  final String customerName;
  final String? customerPhone;
  final DateTime appointmentDate;
  final String timeSlot;
  final String status;
  final List<String> services;
  final double totalAmount;
  final String? notes;

  StaffAppointment({
    required this.appointmentId,
    required this.customerName,
    this.customerPhone,
    required this.appointmentDate,
    required this.timeSlot,
    required this.status,
    required this.services,
    required this.totalAmount,
    this.notes,
  });

  factory StaffAppointment.fromJson(Map<String, dynamic> json) {
    return StaffAppointment(
      appointmentId: json['appointmentId'] ?? 0,
      customerName: json['customerName'] ?? '',
      customerPhone: json['customerPhone'],
      appointmentDate:
          DateTime.tryParse(json['appointmentDate'] ?? '') ?? DateTime.now(),
      timeSlot: json['timeSlot'] ?? '',
      status: json['status'] ?? 'Pending',
      services: json['services'] != null
          ? List<String>.from(json['services'])
          : [],
      totalAmount: (json['totalAmount'] ?? 0).toDouble(),
      notes: json['notes'],
    );
  }

  String get statusLabel {
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
      default:
        return status;
    }
  }
}
