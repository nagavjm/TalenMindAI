namespace TalentMindAI.Application.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string blobUri, CancellationToken ct = default);
}

public interface IOcrService
{
    Task<string> ExtractTextFromImageAsync(Stream imageStream, CancellationToken ct = default);
}

public interface IAiFoundryService
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, bool jsonMode = false, CancellationToken ct = default);
}

public interface ISearchIndexService
{
    Task IndexResumeAsync(Guid resumeId, string candidateName, string content, CancellationToken ct = default);
    Task<List<string>> SearchRelevantChunksAsync(string query, int topK = 5, CancellationToken ct = default);
}

public interface ILanguageService
{
    Task<List<string>> ExtractKeyPhrasesAsync(string text, CancellationToken ct = default);
    Task<List<string>> RecognizeEntitiesAsync(string text, CancellationToken ct = default);
    Task<string> MaskPiiAsync(string text, CancellationToken ct = default);
}

public interface IResponsibleAiService
{
    Task<bool> IsContentSafeAsync(string text, CancellationToken ct = default);
    bool DetectPromptInjection(string text);
}

public interface IPromptLogService
{
    Task LogAsync(Guid? userId, string promptType, string prompt, string response, bool containedPii, bool failedSafety, bool injectionDetected, int latencyMs, CancellationToken ct = default);
}
