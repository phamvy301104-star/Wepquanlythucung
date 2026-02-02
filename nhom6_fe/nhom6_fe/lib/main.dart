import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'core/constants/constants.dart';
import 'shared/widgets/widgets.dart';
import 'features/home/home.dart';
import 'features/services/services.dart';
import 'features/chatbot/chatbot.dart';
import 'features/orders/orders.dart';
import 'features/profile/profile.dart';
import 'features/auth/auth.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Load environment variables
  await dotenv.load(fileName: ".env");

  // ⚠️ DEVELOPMENT ONLY: Bypass SSL certificate verification
  // This allows HTTP requests to self-signed HTTPS servers (ngrok)
  // REMOVE THIS IN PRODUCTION!
  HttpOverrides.global = DevHttpOverrides();

  // Initialize date formatting for Vietnamese locale
  await initializeDateFormatting('vi', null);

  // Set system UI overlay style
  SystemChrome.setSystemUIOverlayStyle(
    const SystemUiOverlayStyle(
      statusBarColor: Colors.transparent,
      statusBarIconBrightness: Brightness.dark,
      systemNavigationBarColor: AppColors.white,
      systemNavigationBarIconBrightness: Brightness.dark,
    ),
  );

  // Wrap app with ProviderScope for Riverpod state management
  // Required for Staff module which uses ConsumerStatefulWidget
  runApp(const ProviderScope(child: UmeApp()));
}

class UmeApp extends StatelessWidget {
  const UmeApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'UME - Đặt lịch cắt tóc',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        fontFamily: 'SF Pro Display',
        scaffoldBackgroundColor: AppColors.background,
        colorScheme: ColorScheme.fromSeed(
          seedColor: AppColors.primary,
          brightness: Brightness.light,
        ),
        appBarTheme: const AppBarTheme(
          backgroundColor: AppColors.white,
          foregroundColor: AppColors.textPrimary,
          elevation: 0,
          centerTitle: true,
          titleTextStyle: AppTextStyles.h4,
        ),
      ),
      home: const LoginScreen(), // Start with LoginScreen
    );
  }
}

/// Main Screen with Bottom Navigation
class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _currentIndex = 0;

  final List<Widget> _screens = [
    const HomeScreen(),
    const ServicesScreen(),
    const AiChatScreen(), // Changed: Dùng AI Chat mới thay vì ChatbotScreen cũ
    const OrdersScreen(),
    const ProfileScreen(),
  ];

  void _onNavTap(int index) {
    setState(() {
      _currentIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBody: true,
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 300),
        switchInCurve: Curves.easeInOut,
        switchOutCurve: Curves.easeInOut,
        transitionBuilder: (Widget child, Animation<double> animation) {
          return FadeTransition(
            opacity: animation,
            child: SlideTransition(
              position: Tween<Offset>(
                begin: const Offset(0.05, 0),
                end: Offset.zero,
              ).animate(animation),
              child: child,
            ),
          );
        },
        child: IndexedStack(
          key: ValueKey<int>(_currentIndex),
          index: _currentIndex,
          children: _screens,
        ),
      ),
      bottomNavigationBar: BottomNavBar(
        currentIndex: _currentIndex,
        onTap: _onNavTap,
      ),
    );
  }
}

/// ⚠️ DEVELOPMENT ONLY: Custom HttpOverrides to bypass SSL certificate verification
/// This allows HTTP requests to self-signed HTTPS servers (ngrok)
/// REMOVE THIS IN PRODUCTION!
class DevHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback = (X509Certificate cert, String host, int port) {
        return true; // Accept all certificates in development
      };
  }
}
