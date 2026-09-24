using System.Text.RegularExpressions;
using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.Infrastructure.Services;

public class ResponsibleAiService : IResponsibleAiService
{
    private static readonly string[] InjectionPatterns =
    {
        "ignore previous instructions",
        "disregard the system prompt",
        "you are now",
        "act as if",
        "reveal your prompt"
    };

    private static readonly string[] UnsafeKeywords =
    {
        "self-harm", "violence", "hate speech"
    };

    public Task<bool> IsContentSafeAsync(string text, CancellationToken ct = default)
    {
        var isSafe = !UnsafeKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(isSafe);
    }

    public bool DetectPromptInjection(string text)
    {
        return InjectionPatterns.Any(p => text.Contains(p, StringComparison.OrdinalIgnoreCase))
            || Regex.IsMatch(text, "system\\s*:\\s*", RegexOptions.IgnoreCase);
    }
}
