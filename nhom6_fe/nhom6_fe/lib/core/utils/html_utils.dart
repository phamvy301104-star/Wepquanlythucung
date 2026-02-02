import 'package:html/parser.dart' as html_parser;
import 'package:html/dom.dart' as dom;

/// Utility class for HTML operations
class HtmlUtils {
  /// Strip all HTML tags and return plain text
  /// Converts <p>, <br>, <li> to newlines for better readability
  static String stripHtml(String? htmlString) {
    if (htmlString == null || htmlString.isEmpty) {
      return '';
    }

    // Parse HTML
    final document = html_parser.parse(htmlString);

    // Convert list items to bullet points
    document.querySelectorAll('li').forEach((element) {
      element.text = '• ${element.text}\n';
    });

    // Convert <br> and <p> to newlines
    document.querySelectorAll('br').forEach((element) {
      element.replaceWith(dom.Text('\n'));
    });

    document.querySelectorAll('p').forEach((element) {
      element.append(dom.Text('\n'));
    });

    // Get text content
    String text = document.body?.text ?? '';

    // Clean up excessive whitespace and newlines
    text = text
        .replaceAll(RegExp(r'\n\s*\n\s*\n'), '\n\n') // Max 2 newlines
        .replaceAll(RegExp(r'[ \t]+'), ' ') // Multiple spaces to single space
        .trim();

    return text;
  }

  /// Strip HTML but preserve basic structure (for display in cards/lists)
  /// Returns first 100 characters with ellipsis
  static String stripHtmlPreview(String? htmlString, {int maxLength = 100}) {
    final plainText = stripHtml(htmlString);

    if (plainText.length <= maxLength) {
      return plainText;
    }

    return '${plainText.substring(0, maxLength)}...';
  }
}
