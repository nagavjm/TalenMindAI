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
