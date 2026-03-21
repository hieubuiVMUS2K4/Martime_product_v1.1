class ChatMessage {
  final String id;
  final String text;
  final bool isUser;
  final DateTime timestamp;
  final String? source; // 'gemini', 'knowledge_base', 'fallback', 'user'

  ChatMessage({
    required this.id,
    required this.text,
    required this.isUser,
    required this.timestamp,
    this.source,
  });

  factory ChatMessage.fromUser(String text) {
    return ChatMessage(
      id: DateTime.now().millisecondsSinceEpoch.toString(),
      text: text,
      isUser: true,
      timestamp: DateTime.now(),
      source: 'user',
    );
  }

  factory ChatMessage.fromApi(Map<String, dynamic> json) {
    return ChatMessage(
      id: DateTime.now().millisecondsSinceEpoch.toString(),
      text: json['reply'] as String? ?? '',
      isUser: false,
      timestamp: DateTime.now(),
      source: json['source'] as String?,
    );
  }
}

class SuggestedQuestion {
  final String question;
  final String category;

  SuggestedQuestion({required this.question, required this.category});

  factory SuggestedQuestion.fromJson(Map<String, dynamic> json) {
    return SuggestedQuestion(
      question: json['question'] as String? ?? '',
      category: json['category'] as String? ?? '',
    );
  }
}
