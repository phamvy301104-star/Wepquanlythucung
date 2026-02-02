// Staff Attendance Screen
// Màn hình chấm công với camera nhận diện khuôn mặt

import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hugeicons/hugeicons.dart';
import 'package:intl/intl.dart';
import 'package:camera/camera.dart';
import 'package:google_mlkit_face_detection/google_mlkit_face_detection.dart';
import '../providers/staff_providers.dart';
import '../models/staff_models.dart';

class StaffAttendanceScreen extends ConsumerStatefulWidget {
  final bool autoOpenCamera;

  const StaffAttendanceScreen({super.key, this.autoOpenCamera = false});

  @override
  ConsumerState<StaffAttendanceScreen> createState() =>
      _StaffAttendanceScreenState();
}

class _StaffAttendanceScreenState extends ConsumerState<StaffAttendanceScreen>
    with TickerProviderStateMixin {
  CameraController? _cameraController;
  late FaceDetector _faceDetector;
  bool _isDetecting = false;
  bool _faceDetected = false;
  bool _isProcessing = false;
  late AnimationController _pulseController;

  @override
  void initState() {
    super.initState();
    _faceDetector = FaceDetector(
      options: FaceDetectorOptions(
        enableContours: true,
        enableClassification: true,
        performanceMode: FaceDetectorMode.accurate,
      ),
    );
    _pulseController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    )..repeat(reverse: true);

    if (widget.autoOpenCamera) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        _openCamera();
      });
    }
  }

  @override
  void dispose() {
    _cameraController?.dispose();
    _faceDetector.close();
    _pulseController.dispose();
    super.dispose();
  }

  Future<void> _openCamera() async {
    try {
      final cameras = await availableCameras();
      final frontCamera = cameras.firstWhere(
        (camera) => camera.lensDirection == CameraLensDirection.front,
        orElse: () => cameras.first,
      );

      _cameraController = CameraController(
        frontCamera,
        ResolutionPreset.high,
        enableAudio: false,
        imageFormatGroup: ImageFormatGroup.jpeg,
      );

      await _cameraController!.initialize();
      if (mounted) {
        setState(() {});
        _startFaceDetection();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('Không thể mở camera: $e')));
      }
    }
  }

  void _startFaceDetection() {
    if (_cameraController == null || !_cameraController!.value.isInitialized) {
      return;
    }

    _cameraController!.startImageStream((CameraImage image) async {
      if (_isDetecting || _isProcessing) return;
      _isDetecting = true;

      try {
        final inputImage = _convertCameraImage(image);
        if (inputImage != null) {
          final faces = await _faceDetector.processImage(inputImage);
          if (mounted) {
            setState(() {
              _faceDetected = faces.isNotEmpty;
            });
          }
        }
      } catch (e) {
        debugPrint('Face detection error: $e');
      }

      _isDetecting = false;
    });
  }

  InputImage? _convertCameraImage(CameraImage image) {
    try {
      final camera = _cameraController!.description;
      final rotation = InputImageRotationValue.fromRawValue(
        camera.sensorOrientation,
      );

      if (rotation == null) return null;

      final format = InputImageFormatValue.fromRawValue(image.format.raw);
      if (format == null) return null;

      return InputImage.fromBytes(
        bytes: image.planes[0].bytes,
        metadata: InputImageMetadata(
          size: Size(image.width.toDouble(), image.height.toDouble()),
          rotation: rotation,
          format: format,
          bytesPerRow: image.planes[0].bytesPerRow,
        ),
      );
    } catch (e) {
      return null;
    }
  }

  Future<void> _captureAndCheckIn() async {
    if (_cameraController == null || !_faceDetected || _isProcessing) return;

    setState(() => _isProcessing = true);

    try {
      // Stop image stream before taking picture
      await _cameraController!.stopImageStream();

      // Take picture
      final XFile photo = await _cameraController!.takePicture();

      // Read image bytes
      final bytes = await File(photo.path).readAsBytes();
      final base64Image = base64Encode(bytes);

      // Get today's attendance to know which check type
      final todayData = ref.read(attendanceNotifierProvider).value;
      final checkType = todayData?.nextCheckType ?? 1;

      // Send check-in request
      final result = await ref
          .read(attendanceNotifierProvider.notifier)
          .checkIn(checkType: checkType, photoBase64: base64Image);

      if (mounted) {
        if (result['success'] == true) {
          _showSuccessDialog(checkType);
        } else {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(result['message'] ?? 'Chấm công thất bại'),
              backgroundColor: Colors.red,
            ),
          );
          // Restart camera
          _startFaceDetection();
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi: $e'), backgroundColor: Colors.red),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isProcessing = false);
      }
    }
  }

  void _showSuccessDialog(int checkType) {
    final checkLabels = {
      1: 'Bắt đầu ca',
      2: 'Nghỉ trưa',
      3: 'Kết thúc nghỉ',
      4: 'Kết thúc ca',
    };

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => Dialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.green.shade50,
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  HugeIcons.strokeRoundedCheckmarkCircle01,
                  color: Colors.green,
                  size: 48,
                ),
              ),
              const SizedBox(height: 16),
              const Text(
                'Chấm công thành công!',
                style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              Text(
                checkLabels[checkType] ?? 'Check $checkType',
                style: TextStyle(fontSize: 16, color: Colors.grey.shade600),
              ),
              Text(
                DateFormat('HH:mm - dd/MM/yyyy').format(DateTime.now()),
                style: TextStyle(fontSize: 14, color: Colors.grey.shade500),
              ),
              const SizedBox(height: 24),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () {
                    Navigator.of(context).pop();
                    Navigator.of(context).pop(); // Return to main screen
                  },
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF6366F1),
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: const Text(
                    'Đóng',
                    style: TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final attendanceAsync = ref.watch(attendanceNotifierProvider);

    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(
            HugeIcons.strokeRoundedArrowLeft01,
            color: Color(0xFF1E293B),
          ),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Chấm công',
          style: TextStyle(
            color: Color(0xFF1E293B),
            fontWeight: FontWeight.bold,
          ),
        ),
        centerTitle: true,
      ),
      body: _cameraController != null && _cameraController!.value.isInitialized
          ? _buildCameraView()
          : _buildAttendanceOverview(attendanceAsync),
    );
  }

  Widget _buildAttendanceOverview(
    AsyncValue<AttendanceToday?> attendanceAsync,
  ) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        children: [
          // Today status card
          attendanceAsync.when(
            data: (data) => _buildTodayStatusCard(data),
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (_, __) => const Text('Không thể tải dữ liệu'),
          ),
          const SizedBox(height: 24),

          // Check-in button
          _buildCheckInButton(attendanceAsync),
          const SizedBox(height: 24),

          // History section
          _buildHistorySection(),
        ],
      ),
    );
  }

  Widget _buildTodayStatusCard(AttendanceToday? data) {
    final attendance = data?.attendance;

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 20,
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: const Color(0xFF6366F1).withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Icon(
                  HugeIcons.strokeRoundedCalendar01,
                  color: Color(0xFF6366F1),
                ),
              ),
              const SizedBox(width: 12),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Hôm nay',
                    style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                  Text(
                    DateFormat('EEEE, dd/MM/yyyy', 'vi').format(DateTime.now()),
                    style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
                  ),
                ],
              ),
            ],
          ),
          const SizedBox(height: 20),

          // Check times grid
          Row(
            children: [
              _buildCheckTimeItem(
                'Bắt đầu ca',
                attendance?.checkIn1Time,
                1,
                data?.nextCheckType ?? 1,
              ),
              _buildCheckTimeItem(
                'Nghỉ trưa',
                attendance?.checkIn2Time,
                2,
                data?.nextCheckType ?? 1,
              ),
              _buildCheckTimeItem(
                'Kết thúc nghỉ',
                attendance?.checkIn3Time,
                3,
                data?.nextCheckType ?? 1,
              ),
              _buildCheckTimeItem(
                'Kết thúc ca',
                attendance?.checkIn4Time,
                4,
                data?.nextCheckType ?? 1,
              ),
            ],
          ),

          if (attendance != null &&
              (attendance.lateMinutes > 0 || attendance.totalPenalty > 0)) ...[
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.red.shade50,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Row(
                children: [
                  Icon(
                    HugeIcons.strokeRoundedAlert02,
                    color: Colors.red.shade400,
                    size: 20,
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      'Trễ ${attendance.lateMinutes} phút • Phạt ${NumberFormat.currency(locale: 'vi', symbol: '', decimalDigits: 0).format(attendance.totalPenalty)}đ',
                      style: TextStyle(
                        color: Colors.red.shade700,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildCheckTimeItem(
    String label,
    DateTime? time,
    int checkNum,
    int nextCheck,
  ) {
    final isCompleted = time != null;
    final isCurrent = checkNum == nextCheck;

    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 12),
        child: Column(
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: isCompleted
                    ? Colors.green
                    : isCurrent
                    ? const Color(0xFF6366F1)
                    : Colors.grey.shade200,
                shape: BoxShape.circle,
              ),
              child: Icon(
                isCompleted
                    ? HugeIcons.strokeRoundedTick01
                    : HugeIcons.strokeRoundedTime04,
                color: isCompleted || isCurrent
                    ? Colors.white
                    : Colors.grey.shade400,
                size: 20,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              label,
              style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
              textAlign: TextAlign.center,
            ),
            if (time != null)
              Text(
                DateFormat('HH:mm').format(time),
                style: const TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 13,
                  color: Color(0xFF6366F1),
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildCheckInButton(AsyncValue<AttendanceToday?> attendanceAsync) {
    return attendanceAsync.when(
      data: (data) {
        final nextCheck = data?.nextCheckType ?? 1;
        final isCompleted = nextCheck == 0;

        return GestureDetector(
          onTap: isCompleted ? null : _openCamera,
          child: Container(
            width: double.infinity,
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              gradient: isCompleted
                  ? LinearGradient(
                      colors: [Colors.grey.shade300, Colors.grey.shade400],
                    )
                  : const LinearGradient(
                      begin: Alignment.topLeft,
                      end: Alignment.bottomRight,
                      colors: [Color(0xFF6366F1), Color(0xFF8B5CF6)],
                    ),
              borderRadius: BorderRadius.circular(20),
              boxShadow: isCompleted
                  ? null
                  : [
                      BoxShadow(
                        color: const Color(0xFF6366F1).withValues(alpha: 0.4),
                        blurRadius: 20,
                        offset: const Offset(0, 10),
                      ),
                    ],
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  isCompleted
                      ? HugeIcons.strokeRoundedCheckmarkCircle01
                      : HugeIcons.strokeRoundedFaceId,
                  color: Colors.white,
                  size: 28,
                ),
                const SizedBox(width: 12),
                Text(
                  isCompleted
                      ? 'Đã hoàn thành hôm nay'
                      : 'Chấm công: ${data?.nextCheckLabel ?? "Bắt đầu ca"}',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),
        );
      },
      loading: () => const CircularProgressIndicator(),
      error: (_, __) => const Text('Lỗi'),
    );
  }

  Widget _buildHistorySection() {
    final historyAsync = ref.watch(
      attendanceHistoryProvider(AttendanceHistoryParams()),
    );

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Lịch sử chấm công',
          style: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.bold,
            color: Color(0xFF1E293B),
          ),
        ),
        const SizedBox(height: 16),
        historyAsync.when(
          data: (history) {
            if (history.isEmpty) {
              return Container(
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Center(
                  child: Text(
                    'Chưa có lịch sử chấm công',
                    style: TextStyle(color: Colors.grey.shade500),
                  ),
                ),
              );
            }

            return Column(
              children: history
                  .take(5)
                  .map((att) => _buildHistoryItem(att))
                  .toList(),
            );
          },
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (_, __) => const Text('Không thể tải lịch sử'),
        ),
      ],
    );
  }

  Widget _buildHistoryItem(Attendance attendance) {
    final isComplete = attendance.isAllChecksCompleted;
    final hasLate = attendance.lateMinutes > 0;

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: hasLate ? Border.all(color: Colors.red.shade100) : null,
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: isComplete
                  ? Colors.green.withValues(alpha: 0.1)
                  : Colors.orange.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(
              isComplete
                  ? HugeIcons.strokeRoundedCheckmarkCircle01
                  : HugeIcons.strokeRoundedTime04,
              color: isComplete ? Colors.green : Colors.orange,
              size: 20,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  DateFormat('EEEE, dd/MM/yyyy', 'vi').format(attendance.date),
                  style: const TextStyle(fontWeight: FontWeight.w600),
                ),
                Text(
                  '${attendance.nextCheckType == 0 ? 4 : attendance.nextCheckType - 1}/4 lần chấm công',
                  style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                ),
              ],
            ),
          ),
          if (hasLate)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              decoration: BoxDecoration(
                color: Colors.red.shade50,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                'Trễ ${attendance.lateMinutes}p',
                style: TextStyle(
                  fontSize: 11,
                  color: Colors.red.shade700,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildCameraView() {
    return Stack(
      fit: StackFit.expand,
      children: [
        // Camera preview
        ClipRRect(
          borderRadius: BorderRadius.circular(20),
          child: CameraPreview(_cameraController!),
        ),

        // Face detection overlay
        Center(
          child: AnimatedBuilder(
            animation: _pulseController,
            builder: (context, child) {
              return Container(
                width: 280 + (_pulseController.value * 10),
                height: 350 + (_pulseController.value * 10),
                decoration: BoxDecoration(
                  border: Border.all(
                    color: _faceDetected ? Colors.green : Colors.white,
                    width: 3,
                  ),
                  borderRadius: BorderRadius.circular(140),
                ),
              );
            },
          ),
        ),

        // Top info
        Positioned(
          top: 20,
          left: 20,
          right: 20,
          child: Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.black.withValues(alpha: 0.5),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              children: [
                Icon(
                  _faceDetected
                      ? HugeIcons.strokeRoundedCheckmarkCircle01
                      : HugeIcons.strokeRoundedFaceId,
                  color: _faceDetected ? Colors.green : Colors.white,
                ),
                const SizedBox(width: 12),
                Text(
                  _faceDetected
                      ? 'Đã nhận diện khuôn mặt'
                      : 'Đưa khuôn mặt vào khung hình',
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),
        ),

        // Bottom capture button
        Positioned(
          bottom: 40,
          left: 0,
          right: 0,
          child: Column(
            children: [
              GestureDetector(
                onTap: _faceDetected && !_isProcessing
                    ? _captureAndCheckIn
                    : null,
                child: Container(
                  width: 80,
                  height: 80,
                  decoration: BoxDecoration(
                    color: _faceDetected ? Colors.green : Colors.grey,
                    shape: BoxShape.circle,
                    boxShadow: _faceDetected
                        ? [
                            BoxShadow(
                              color: Colors.green.withValues(alpha: 0.5),
                              blurRadius: 20,
                            ),
                          ]
                        : null,
                  ),
                  child: _isProcessing
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Icon(
                          HugeIcons.strokeRoundedCamera01,
                          color: Colors.white,
                          size: 36,
                        ),
                ),
              ),
              const SizedBox(height: 16),
              Text(
                _faceDetected ? 'Nhấn để chấm công' : 'Đang chờ nhận diện...',
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 16,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        ),

        // Close button
        Positioned(
          top: 20,
          right: 20,
          child: IconButton(
            onPressed: () {
              _cameraController?.dispose();
              _cameraController = null;
              setState(() {});
            },
            icon: Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: Colors.black.withValues(alpha: 0.5),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                HugeIcons.strokeRoundedCancel01,
                color: Colors.white,
              ),
            ),
          ),
        ),
      ],
    );
  }
}
