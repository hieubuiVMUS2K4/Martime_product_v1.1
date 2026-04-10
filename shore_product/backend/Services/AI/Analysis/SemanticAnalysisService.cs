namespace ProductApi.Services.AI.Analysis
{
    /// <summary>
    /// Semantic analysis service for intent detection and question similarity
    /// </summary>
    public interface ISemanticAnalysisService
    {
        (string Intent, double Confidence) DetectIntent(string question);
        double CalculateSimilarity(string question1, string question2);
        List<string> ExtractKeywords(string question);
        string NormalizeQuestion(string question);
    }

    public class SemanticAnalysisService : ISemanticAnalysisService
    {
        private readonly ILogger<SemanticAnalysisService> _logger;

        // Domain-specific keywords for maritime operations
        private readonly Dictionary<string, string[]> _intentKeywordMap = new()
        {
            { "fuel_analysis", new[] { "fuel", "tiêu hao", "dầu diesel", "dầu HFO", "ROB", "consumption" } },
            { "speed_analysis", new[] { "tốc độ", "speed", "SOG", "knot", "velocity" } },
            { "engine_status", new[] { "máy", "engine", "RPM", "vòng quay", "temperature", "nhiệt độ" } },
            { "weather_impact", new[] { "thời tiết", "weather", "sóng", "wave", "gió", "wind", "sea state", "tình hình biển" } },
            { "anomaly_detection", new[] { "bất thường", "anomaly", "lạ", "unusual", "khác thường", "problem", "vấn đề", "đột biến", "outlier", "số liệu lạ", "ngày lạ" } },
            { "trend_analysis", new[] { "xu hướng", "trend", "so sánh", "giảm", "tăng", "increase", "decrease", "pattern" } },
            { "equipment_check", new[] { "kiểm tra", "check", "tình trạng", "status", "performance", "hiệu suất" } },
            { "operational_report", new[] { "báo cáo", "report", "tóm tắt", "summary", "overview" } },
            { "eta_voyage_risk", new[] { "eta", "đến cảng", "arrival", "chậm", "trễ", "hành trình", "voyage", "distance to go" } },
            { "fuel_efficiency", new[] { "suất tiêu hao", "efficiency", "fuel per mile", "nm", "tiết kiệm nhiên liệu", "định mức" } },
            { "safety_compliance", new[] { "an toàn", "safety", "incident", "drill", "compliance", "tuân thủ", "cảnh báo" } },
            { "maintenance_material_risk", new[] { "bảo trì", "maintenance", "pms", "work order", "vật tư", "inventory", "rob thấp", "thiếu vật tư" } }
        };

        public SemanticAnalysisService(ILogger<SemanticAnalysisService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Detect the intent of a question based on keywords
        /// </summary>
        public (string Intent, double Confidence) DetectIntent(string question)
        {
            var normalized = NormalizeQuestion(question);
            var keywords = ExtractKeywords(normalized);

            var intentScores = new Dictionary<string, int>();

            foreach (var (intent, intentKeywords) in _intentKeywordMap)
            {
                var matches = intentKeywords.Count(kw =>
                    keywords.Contains(kw, StringComparer.OrdinalIgnoreCase)
                    || normalized.Contains(kw, StringComparison.OrdinalIgnoreCase));
                if (matches > 0)
                {
                    intentScores[intent] = matches;
                }
            }

            if (intentScores.Count == 0)
            {
                _logger.LogDebug($"No specific intent detected for: {question}");
                return ("general_query", 0.3);
            }

            var topIntent = intentScores.OrderByDescending(x => x.Value).First();
            var confidence = Math.Min(1.0, topIntent.Value / 3.0); // Normalize to max 1.0

            _logger.LogInformation($"Detected intent: {topIntent.Key} (confidence: {confidence:P})");

            return (topIntent.Key, confidence);
        }

        /// <summary>
        /// Calculate similarity between two questions (0-1)
        /// Simple implementation using keyword overlap
        /// </summary>
        public double CalculateSimilarity(string question1, string question2)
        {
            var keywords1 = ExtractKeywords(NormalizeQuestion(question1));
            var keywords2 = ExtractKeywords(NormalizeQuestion(question2));

            if (keywords1.Count == 0 || keywords2.Count == 0)
                return 0.0;

            // Calculate Jaccard similarity
            var intersection = keywords1.Intersect(keywords2).Count();
            var union = keywords1.Union(keywords2).Count();

            return union == 0 ? 0.0 : (double)intersection / union;
        }

        /// <summary>
        /// Extract keywords from question (simple implementation)
        /// </summary>
        public List<string> ExtractKeywords(string question)
        {
            var stopwords = new[] { "là", "cái", "của", "và", "hay", "hoặc", "có", "được", "không", "phải", "do", "để", "từ", "ở", "trong", "với", "như", "an", "at", "the", "a", "an", "is", "are", "or", "and", "it" };
            var minLength = 2;

            var words = question.Split(new[] { ' ', ',', '.', '?', '!', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= minLength && !stopwords.Contains(w.ToLowerInvariant()))
                .Select(w => w.ToLowerInvariant())
                .Distinct()
                .ToList();

            return words;
        }

        /// <summary>
        /// Normalize question for comparison
        /// </summary>
        public string NormalizeQuestion(string question)
        {
            return question
                .ToLowerInvariant()
                .Trim()
                .Replace("?", "")
                .Replace("!", "")
                .Replace(",", " ")
                .Replace(".", " ")
                .ReplaceWhitespace(" "); // Remove extra spaces
        }
    }

    public static class StringExtensions
    {
        public static string ReplaceWhitespace(this string input, string replacement)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"\s+");
            return regex.Replace(input, replacement);
        }
    }
}
