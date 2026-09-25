using TalentMindAI.Application.Interfaces;

namespace TalentMindAI.Infrastructure.Services;

/// <summary>
/// Placeholder implementation of the Food Knowledge Base RAG extension point.
/// Returns no chunks until a Food-specific Azure AI Search index (nutrition PDFs,
/// recipe documents, etc) is provisioned. Swap this implementation in DI once
/// <see cref="Configuration.FoodKnowledgeSearchOptions"/> points to a real index.
/// </summary>
public class FoodKnowledgeRetriever : IFoodKnowledgeRetriever
{
    public Task<List<string>> SearchRelevantChunksAsync(string query, int topK = 5, CancellationToken ct = default)
    {
        return Task.FromResult(new List<string>());
    }
}
