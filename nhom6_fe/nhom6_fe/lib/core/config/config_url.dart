import 'dart:developer' as developer;
import 'package:flutter_dotenv/flutter_dotenv.dart';

class ConfigUrl {
  static String get baseUrl {
    // Thử đọc API_URL trước (có /api), nếu không có thì dùng API_BASE_URL
    var url = dotenv.env['API_URL'];
    if (url != null && url.isNotEmpty) {
      // Đảm bảo URL kết thúc bằng /
      if (!url.endsWith('/')) url = '$url/';
      return url;
    }

    url = dotenv.env['BASE_URL'];
    if (url != null && url.isNotEmpty) {
      if (!url.endsWith('/')) url = '$url/';
      return url;
    }

    developer.log(
      'API URL is not set in the .env file. Using default URL.',
      name: 'ConfigUrl',
    );
    return "http://localhost:5256/api/";
  }

  // Auth endpoints
  static String get loginUrl => "${baseUrl}Authenticate/login";
  static String get registerUrl => "${baseUrl}Authenticate/register";
  static String get profileUrl => "${baseUrl}Authenticate/profile";

  // Product endpoints
  static String get productsUrl => "${baseUrl}ProductApi";
}
