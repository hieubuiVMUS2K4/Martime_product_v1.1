const fs = require('fs');
const file_path = 'f:/NCKH/Product/Martime_product_v1.1/shore_product/backend/Services/AI/GeminiEvaluationService.cs';
let code = fs.readFileSync(file_path, 'utf8');

const oldStr = \                using var jsonDoc = JsonDocument.Parse(responseBody);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0];
                    if (candidate.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].TryGetProperty("text", out var textProp)
                            ? textProp.GetString() ?? "Không thể trả lời."
                            : "Không thể trả lời.";\;

const newStr = \                using var jsonDoc = JsonDocument.Parse(responseBody);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentProp))
                    {
                        var text = contentProp.GetString() ?? "Không thể trả lời.";\;

// In case the word wrap had CR LF
code = code.replace(oldStr.replace(/\r\n/g, '\n'), newStr.replace(/\r\n/g, '\n'));
code = code.replace(oldStr, newStr);

fs.writeFileSync(file_path, code, 'utf8');
