import '../../core/network/api_client.dart';
import '../../core/constants/api_constants.dart';
import '../models/chat_message.dart';

class ChatRepository {
  final ApiClient _apiClient;

  ChatRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<ChatMessage> sendMessage(String message, {String? context}) async {
    try {
      final response = await _apiClient.dio.post(
        ApiConstants.chatSend,
        data: {
          'message': message,
          if (context != null) 'context': context,
        },
      );
      return ChatMessage.fromApi(response.data as Map<String, dynamic>);
    } catch (e) {
      throw Exception('Failed to send chat message: $e');
    }
  }

  Future<List<SuggestedQuestion>> getSuggestions() async {
    try {
      final response = await _apiClient.dio.get(ApiConstants.chatSuggestions);
      final list = response.data as List<dynamic>;
      return list
          .map((json) => SuggestedQuestion.fromJson(json as Map<String, dynamic>))
          .toList();
    } catch (e) {
      throw Exception('Failed to fetch suggestions: $e');
    }
  }
}
