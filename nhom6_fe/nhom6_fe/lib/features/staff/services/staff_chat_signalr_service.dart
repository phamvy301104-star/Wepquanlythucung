// Staff Chat SignalR Service
// Real-time chat using SignalR

import 'dart:async';
import 'dart:developer' as developer;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:signalr_netcore/signalr_client.dart';
import '../../../core/config/config_url.dart';
import '../models/staff_models.dart';

class StaffChatSignalRService {
  HubConnection? _hubConnection;
  bool _isConnected = false;

  // Stream controllers for real-time events
  final _messageReceivedController =
      StreamController<StaffChatMessage>.broadcast();
  final _userOnlineController =
      StreamController<Map<String, dynamic>>.broadcast();
  final _userTypingController =
      StreamController<Map<String, dynamic>>.broadcast();
  final _connectionStateController = StreamController<bool>.broadcast();

  // Exposed streams
  Stream<StaffChatMessage> get onMessageReceived =>
      _messageReceivedController.stream;
  Stream<Map<String, dynamic>> get onUserOnline => _userOnlineController.stream;
  Stream<Map<String, dynamic>> get onUserTyping => _userTypingController.stream;
  Stream<bool> get onConnectionStateChanged =>
      _connectionStateController.stream;

  bool get isConnected => _isConnected;

  void _log(String message) {
    developer.log(message, name: 'StaffChatSignalR');
  }

  Future<String> _getHubUrl() async {
    final baseUrl = ConfigUrl.baseUrl;
    // Remove /api/ and add /hubs/staff-chat
    final hubUrl = baseUrl.replaceAll('/api/', '/hubs/staff-chat');
    return hubUrl;
  }

  Future<String?> _getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('jwt_token');
  }

  /// Connect to SignalR Hub
  Future<bool> connect() async {
    try {
      if (_isConnected) {
        _log('Already connected');
        return true;
      }

      final token = await _getToken();
      if (token == null) {
        _log('No token available');
        return false;
      }

      final hubUrl = await _getHubUrl();
      _log('Connecting to: $hubUrl');

      _hubConnection = HubConnectionBuilder()
          .withUrl(
            '$hubUrl?access_token=$token',
            options: HttpConnectionOptions(
              accessTokenFactory: () async => token,
              skipNegotiation: true,
              transport: HttpTransportType.WebSockets,
            ),
          )
          .withAutomaticReconnect()
          .build();

      // Register event handlers
      _registerEventHandlers();

      // Start connection
      await _hubConnection!.start();

      _isConnected = true;
      _connectionStateController.add(true);
      _log('Connected successfully');

      return true;
    } catch (e) {
      _log('Connection error: $e');
      _isConnected = false;
      _connectionStateController.add(false);
      return false;
    }
  }

  void _registerEventHandlers() {
    if (_hubConnection == null) return;

    // Handle new message
    _hubConnection!.on('ReceiveMessage', (arguments) {
      _log('Received message: $arguments');
      if (arguments != null && arguments.isNotEmpty) {
        try {
          final messageData = arguments[0] as Map<String, dynamic>;
          final message = StaffChatMessage.fromJson(messageData);
          _messageReceivedController.add(message);
        } catch (e) {
          _log('Error parsing message: $e');
        }
      }
    });

    // Handle user online status
    _hubConnection!.on('UserOnline', (arguments) {
      _log('User online: $arguments');
      if (arguments != null && arguments.length >= 2) {
        _userOnlineController.add({
          'staffId': arguments[0],
          'isOnline': arguments[1],
        });
      }
    });

    // Handle typing indicator
    _hubConnection!.on('UserTyping', (arguments) {
      if (arguments != null && arguments.length >= 3) {
        _userTypingController.add({
          'roomId': arguments[0],
          'staffId': arguments[1],
          'isTyping': arguments[2],
        });
      }
    });

    // Handle reconnection
    _hubConnection!.onreconnecting(({error}) {
      _log('Reconnecting...');
      _isConnected = false;
      _connectionStateController.add(false);
    });

    _hubConnection!.onreconnected(({connectionId}) {
      _log('Reconnected: $connectionId');
      _isConnected = true;
      _connectionStateController.add(true);
    });

    _hubConnection!.onclose(({error}) {
      _log('Connection closed: $error');
      _isConnected = false;
      _connectionStateController.add(false);
    });
  }

  /// Join a chat room
  Future<void> joinRoom(int roomId) async {
    if (!_isConnected || _hubConnection == null) {
      _log('Not connected, cannot join room');
      return;
    }

    try {
      await _hubConnection!.invoke('JoinChatRoom', args: [roomId]);
      _log('Joined room: $roomId');
    } catch (e) {
      _log('Error joining room: $e');
    }
  }

  /// Leave a chat room
  Future<void> leaveRoom(int roomId) async {
    if (!_isConnected || _hubConnection == null) return;

    try {
      await _hubConnection!.invoke('LeaveChatRoom', args: [roomId]);
      _log('Left room: $roomId');
    } catch (e) {
      _log('Error leaving room: $e');
    }
  }

  /// Send typing indicator
  Future<void> sendTyping(int roomId, bool isTyping) async {
    if (!_isConnected || _hubConnection == null) return;

    try {
      await _hubConnection!.invoke('SendTyping', args: [roomId, isTyping]);
    } catch (e) {
      _log('Error sending typing: $e');
    }
  }

  /// Disconnect from hub
  Future<void> disconnect() async {
    try {
      if (_hubConnection != null) {
        await _hubConnection!.stop();
        _hubConnection = null;
      }
      _isConnected = false;
      _connectionStateController.add(false);
      _log('Disconnected');
    } catch (e) {
      _log('Error disconnecting: $e');
    }
  }

  /// Dispose resources
  void dispose() {
    disconnect();
    _messageReceivedController.close();
    _userOnlineController.close();
    _userTypingController.close();
    _connectionStateController.close();
  }
}
