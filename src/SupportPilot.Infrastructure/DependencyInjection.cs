using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportPilot.Application.Interfaces;
using SupportPilot.Application.Services;
using SupportPilot.Infrastructure.AI;
using SupportPilot.Infrastructure.Configuration;
using SupportPilot.Infrastructure.Embeddings;
using SupportPilot.Infrastructure.VectorStore;
using SupportPilot.Infrastructure.Tickets;
using Microsoft.SemanticKernel;
using SupportPilot.Infrastructure.Agents;
using Microsoft.EntityFrameworkCore;
using SupportPilot.Infrastructure.Persistence;
using SupportPilot.Infrastructure.Conversations;
using SupportPilot.Infrastructure.Sentiment;
using SupportPilot.Application.Settings;
using SupportPilot.Application.Contracts;
using SupportPilot.Infrastructure.Auth;

namespace SupportPilot.Infrastructure;

/// <summary>
/// Registers all Infrastructure services in one place. The Api project calls
/// a single extension method and stays unaware of how AI providers, settings,
/// or vector storage are wired internally.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind appsettings.json + user secrets + env vars into strongly-typed settings.
        AppSettings settings = new();
        configuration.Bind(settings);
        services.AddSingleton(settings);


        // Register individual settings sections so services can inject exactly
        // what they need instead of taking a dependency on the whole AppSettings.
        services.AddSingleton(settings.OpenAI);
        services.AddSingleton(settings.Anthropic);
        services.AddSingleton(settings.Qdrant);
        services.AddSingleton(settings.Embedding);
        services.AddSingleton(settings.Retry);

        // Plugin holds the tool implementations; registered so DI can inject its dependencies.
        services.AddSingleton<SupportPlugin>();

        // Kernel is built once per app lifetime — it wires the OpenAI connector
        // and registers our plugin so the LLM knows search_kb and create_ticket exist.
        services.AddSingleton<Kernel>(sp =>
        {
            var openAiSettings = sp.GetRequiredService<OpenAISettings>();
            var plugin = sp.GetRequiredService<SupportPlugin>();

            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                modelId: openAiSettings.Model,
                apiKey: openAiSettings.ApiKey);

            Kernel kernel = builder.Build();
            kernel.Plugins.AddFromObject(plugin, "Support");

            return kernel;
        });

        // Factory needs settings + a logger factory; both come from DI.
        services.AddSingleton<ChatProviderFactory>();

        // The active provider is resolved through the factory, so callers inject
        // IChatProvider and never know which concrete provider they received.
        services.AddSingleton<IChatProvider>(sp =>
            sp.GetRequiredService<ChatProviderFactory>().Create());

        // Embeddings and vector storage — registered against their interfaces
        // so Application services never reference OpenAI or Qdrant directly.
        services.AddSingleton<IEmbeddingService, EmbeddingService>();
        services.AddSingleton<IVectorStore, QdrantService>();

        services.AddSingleton<IRagOptions>(settings.Rag);
        services.AddScoped<IRagService, RagService>();

        // Scoped — matches DbContext lifetime. A repository holding a Scoped DbContext
        // must itself be Scoped or narrower, never Singleton (see lifetime note below).
        services.AddScoped<ITicketRepository, EfTicketRepository>();

        services.AddSingleton(settings.Sentiment);

        services.AddScoped<IConversationRepository, EfConversationRepository>();
        services.AddScoped<ISentimentAnalyzer, SentimentAnalyzer>();
        services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
        services.AddScoped<IMetricsService, MetricsService>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        // EF Core + PostgreSQL. DbContext is Scoped by design (one per request) —
        // it holds a live DB connection and change tracker that must not be shared
        // across requests or threads.
        services.AddDbContext<SupportPilotDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        return services;
    }
}