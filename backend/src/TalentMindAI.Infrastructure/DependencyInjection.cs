using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;
using TalentMindAI.Infrastructure.Data;
using TalentMindAI.Infrastructure.Security;
using TalentMindAI.Infrastructure.Services;

namespace TalentMindAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));
        services.Configure<AiVisionOptions>(configuration.GetSection(AiVisionOptions.SectionName));
        services.Configure<AiFoundryOptions>(configuration.GetSection(AiFoundryOptions.SectionName));
        services.Configure<AiSearchOptions>(configuration.GetSection(AiSearchOptions.SectionName));
        services.Configure<AiLanguageOptions>(configuration.GetSection(AiLanguageOptions.SectionName));
        services.Configure<FeatureFlagsOptions>(configuration.GetSection(FeatureFlagsOptions.SectionName));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IOcrService, OcrService>();
        services.AddScoped<IAiFoundryService, AiFoundryService>();
        services.AddScoped<ISearchIndexService, SearchIndexService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IResponsibleAiService, ResponsibleAiService>();
        services.AddScoped<IPromptLogService, PromptLogService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IResumeService, ResumeService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
