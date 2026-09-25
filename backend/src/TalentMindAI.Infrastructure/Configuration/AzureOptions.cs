namespace TalentMindAI.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}

public class BlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "resumes";
}

public class AiVisionOptions
{
    public const string SectionName = "AzureAIVision";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class AiFoundryOptions
{
    public const string SectionName = "AzureAIFoundry";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "phi-4-mini-instruct";
}

public class AiSearchOptions
{
    public const string SectionName = "AzureAISearch";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = "resumes-index";
}

public class AiLanguageOptions
{
    public const string SectionName = "AzureAILanguage";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Dedicated AI configuration for TalentMind NutriAI (Food module). Intentionally separate
/// from <see cref="AiFoundryOptions"/> (used by the Resume Assistant) so the two assistants
/// can use different endpoints/models/keys and evolve independently.
/// </summary>
public class FoodAIOptions
{
    public const string SectionName = "FoodAI";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "food-ai-model";
}

/// <summary>
/// Future extension point for a Food Knowledge Base (Azure AI Search index over
/// nutrition/recipe PDFs). Not wired to a live index yet.
/// </summary>
public class FoodKnowledgeSearchOptions
{
    public const string SectionName = "FoodKnowledgeSearch";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = "food-knowledge-index";
}

/// <summary>
/// Dedicated Azure AI Content Safety configuration for TalentMind NutriAI (Food module).
/// Powers Prompt Shields (jailbreak/injection detection) and Content Safety (harmful content
/// moderation) for the Food module only - completely separate from the Resume Assistant's
/// (regex-based) <see cref="TalentMindAI.Application.Interfaces.IResponsibleAiService"/>.
/// If Endpoint/ApiKey are left as placeholders, checks are skipped gracefully (fail-open)
/// until the resource is provisioned.
/// </summary>
public class FoodContentSafetyOptions
{
    public const string SectionName = "FoodContentSafety";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public enum AuthMode
{
    Basic,
    Sso
}

public class FeatureFlagsOptions
{
    public const string SectionName = "FeatureFlags";

    /// <summary>
    /// Selects the single active authentication mode for the app: "Basic" (username/password)
    /// or "Sso" (implemented later). Only one mode is active at a time.
    /// </summary>
    public AuthMode AuthMode { get; set; } = AuthMode.Basic;
}
