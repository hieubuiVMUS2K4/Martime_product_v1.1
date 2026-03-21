import 'package:flutter/foundation.dart';
import '../../data/models/chat_message.dart';
import '../../data/repositories/chat_repository.dart';

class ChatProvider with ChangeNotifier {
  final ChatRepository _repository;

  ChatProvider(this._repository);

  final List<ChatMessage> _messages = [];
  List<SuggestedQuestion> _suggestions = [];
  bool _isLoading = false;
  String? _error;

  List<ChatMessage> get messages => List.unmodifiable(_messages);
  List<SuggestedQuestion> get suggestions => _suggestions;
  bool get isLoading => _isLoading;
  String? get error => _error;

  Future<void> sendMessage(String text) async {
    if (text.trim().isEmpty) return;

    // Add user message immediately
    final userMessage = ChatMessage.fromUser(text.trim());
    _messages.add(userMessage);
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _repository.sendMessage(text.trim());
      _messages.add(response);
    } catch (e) {
      _error = e.toString();
      // Add error message as bot response
      _messages.add(ChatMessage(
        id: DateTime.now().millisecondsSinceEpoch.toString(),
        text: 'Sorry, I could not process your request. Please check your connection and try again.',
        isUser: false,
        timestamp: DateTime.now(),
        source: 'error',
      ));
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> fetchSuggestions() async {
    try {
      _suggestions = await _repository.getSuggestions();
      notifyListeners();
    } catch (e) {
      // Use default suggestions if API fails
      _suggestions = [
        SuggestedQuestion(question: 'What is the fire drill procedure?', category: 'Safety'),
        SuggestedQuestion(question: 'Explain man overboard procedure', category: 'Safety'),
        SuggestedQuestion(question: 'What are COLREGs?', category: 'Navigation'),
        SuggestedQuestion(question: 'MARPOL garbage disposal rules?', category: 'Environmental'),
        SuggestedQuestion(question: 'What are rest hour requirements?', category: 'Crew'),
        SuggestedQuestion(question: 'Navigation lights rules?', category: 'Navigation'),
      ];
      notifyListeners();
    }
  }

  void clearMessages() {
    _messages.clear();
    _error = null;
    notifyListeners();
  }
}
