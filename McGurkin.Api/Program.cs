using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using McGurkin.Api.Features.Iam;
using McGurkin.Api.Features.Iam.Data;
using McGurkin.Api.Features.Kv;
using McGurkin.Api.Features.Kv.Data;
using McGurkin.Api.Features.Tmdb;
using McGurkin.Api.Features.Utilities;
using McGurkin.Net.Http;
using McGurkin.Runtime.Serialization;
using McGurkin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;

// ============================================================================
// BUILDER
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

var vaultUri = Environment.GetEnvironmentVariable("VaultUri");
if (!string.IsNullOrWhiteSpace(vaultUri))
{
    var keyVaultEndpoint = new Uri(vaultUri);
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}

builder.Services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config =>
{
    config.SetAzureTokenCredential(new DefaultAzureCredential());
});

builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsightsConnectionString"];
});

var kvServiceConfig = KvServiceConfig.FromConfiguration(builder.Configuration);
Console.WriteLine($"KvServiceConfig: {kvServiceConfig.DbConnectionString}");
builder.Services.AddDbContext<KvDbContext>(options =>
{
    options.UseSqlServer(kvServiceConfig.DbConnectionString,
        opt => opt.MigrationsHistoryTable(KvServiceConfig.HISTORY_TABLE, KvServiceConfig.SCHEMA)
    );
});

var iamServiceConfig = IamServiceConfig.FromConfiguration(builder.Configuration);
Console.WriteLine($"IamServiceConfig: {iamServiceConfig.DbConnectionString}");
builder.Services.AddDbContext<IamDbContext>(options =>
{
    options.UseSqlServer(iamServiceConfig.DbConnectionString,
        opt => opt.MigrationsHistoryTable(IamServiceConfig.HISTORY_TABLE, IamServiceConfig.SCHEMA)
    );
});

builder.Services.AddDefaultIdentity<IamUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<IamDbContext>();

builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    var options = Serializer.Options;
    foreach (var converter in options.Converters)
        o.SerializerOptions.Converters.Add(converter);
    o.SerializerOptions.DefaultIgnoreCondition = options.DefaultIgnoreCondition;
    o.SerializerOptions.PreferredObjectCreationHandling = options.PreferredObjectCreationHandling;
    o.SerializerOptions.PropertyNamingPolicy = options.PropertyNamingPolicy;
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var policyName = "CorePolicy";
string[] allowedOrigins = [
        "http://localhost:8000",
        "http://localhost:9000",
        "https://localhost:7266",
        "https://www.kixvu.com",
        "https://www.mcgurkin.net",
    ];
builder.Services.AddCors(options =>
{
    options.AddPolicy(policyName, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped<IIamService, IamService>();
builder.Services.AddScoped<ITmdbService, TmdbService>();
builder.Services.AddScoped<IKvService, KvService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "api.mcgurkin.net",
        Version = "v1"
    });
});

// ============================================================================
// APP
// ============================================================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("./v1/swagger.json", "api.mcgurkin.net v1");
        opt.EnableTryItOutByDefault();
        opt.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseCors("CorePolicy");
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseHttpsRedirection();

app.MapApiHealthRoutes();
app.MapTmdbRoutes();
app.MapKvRoutes();
app.MapIamRoutes();
app.MapUtilitiesRoutes();

app.Run();
