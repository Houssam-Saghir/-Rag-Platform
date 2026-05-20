using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rag.Application.Interfaces;
using Rag.Infrastructure.Auth;
using Rag.Infrastructure.Persistence;
using Rag.Infrastructure.Services;
using Rag.Shared.Options;

namespace Rag.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<ChunkingOptions>(configuration.GetSection(ChunkingOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<ITextExtractionService, TextExtractionService>();
        services.AddScoped<IChunkingService, ChunkingService>();
        services.AddScoped<IVectorSearchService, VectorSearchService>();
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddHttpClient<IEmbeddingService, EmbeddingService>(c => c.BaseAddress = new Uri("https://api.openai.com"));
        services.AddHttpClient<IAiChatService, AiChatService>(c => c.BaseAddress = new Uri("https://api.openai.com"));

        return services;
    }
}
