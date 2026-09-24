using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Options;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

public class ResumeSearchDocument
{
    public string Id { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class SearchIndexService : ISearchIndexService
{
    private readonly AiSearchOptions _options;

    public SearchIndexService(IOptions<AiSearchOptions> options)
    {
        _options = options.Value;
    }

    private SearchIndexClient IndexClient => new(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));
    private SearchClient SearchClient => new(new Uri(_options.Endpoint), _options.IndexName, new AzureKeyCredential(_options.ApiKey));

    public async Task IndexResumeAsync(Guid resumeId, string candidateName, string content, CancellationToken ct = default)
    {
        var doc = new ResumeSearchDocument
        {
            Id = resumeId.ToString(),
            CandidateName = candidateName,
            Content = content
        };

        await SearchClient.MergeOrUploadDocumentsAsync(new[] { doc }, cancellationToken: ct);
    }

    public async Task<List<string>> SearchRelevantChunksAsync(string query, int topK = 5, CancellationToken ct = default)
    {
        var results = await SearchClient.SearchAsync<ResumeSearchDocument>(query, new SearchOptions { Size = topK }, ct);

        var chunks = new List<string>();
        await foreach (var result in results.Value.GetResultsAsync())
        {
            chunks.Add(result.Document.Content);
        }
        return chunks;
    }
}
