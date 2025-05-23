using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using McGurkin.Api.Features.Iam;
using McGurkin.Api.Features.Iam.Data;
using McGurkin.Api.Features.Kpis;
using McGurkin.Api.Features.Kpis.Data;
using McGurkin.Api.Features.Kv;
using McGurkin.Api.Features.Kv.Data;
using McGurkin.Api.Features.Tmdb;
using McGurkin.Api.Features.Utilities;
using McGurkin.Net.Http;
using McGurkin.Runtime.Serialization;
using McGurkin.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config =>
{
    config.SetAzureTokenCredential(new DefaultAzureCredential());
});

builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsightsConnectionString"];
});

var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
var kpiServiceConfig = KpiServiceConfig.FromConfiguration(builder.Configuration);
var kvServiceConfig = KvServiceConfig.FromConfiguration(builder.Configuration);
var iamServiceConfig = IamServiceConfig.FromConfiguration(builder.Configuration);
if (useInMemoryDatabase)
{
    builder.Services.AddDbContext<KpiDbContext>(options =>
    {
        options.UseInMemoryDatabase("KpiServiceDb");
    });
    builder.Services.AddDbContext<KvDbContext>(options =>
    {
        options.UseInMemoryDatabase("KvServiceDb");
    });
    builder.Services.AddDbContext<IamDbContext>(options =>
    {
        options.UseInMemoryDatabase("IamServiceDb");
    });
}
else
{
    builder.Services.AddDbContext<IamDbContext>(options =>
    {
        options.UseSqlServer(iamServiceConfig.DbConnectionString,
            opt => opt.MigrationsHistoryTable(IamServiceConfig.HISTORY_TABLE, IamServiceConfig.SCHEMA)
        );
    });
    builder.Services.AddDbContext<KpiDbContext>(options =>
    {
        options.UseSqlServer(kpiServiceConfig.DbConnectionString,
            opt => opt.MigrationsHistoryTable(KpiServiceConfig.HISTORY_TABLE, KpiServiceConfig.SCHEMA)
        );
    });
    builder.Services.AddDbContext<KvDbContext>(options =>
    {
        options.UseSqlServer(kvServiceConfig.DbConnectionString,
            opt => opt.MigrationsHistoryTable(KvServiceConfig.HISTORY_TABLE, KvServiceConfig.SCHEMA)
        );
    });
}

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
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
    "http://localhost:5173",
    "https://localhost:7062",
    "http://localhost:8000",
    "http://localhost:9000",
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
builder.Services.AddScoped<IKpiService, KpiService>();
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = iamServiceConfig.Issuer,
        ValidAudience = iamServiceConfig.Issuer,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(iamServiceConfig.IssuerKey)),
    };

    // For SignalR, we have to hook the OnMessageReceived event in order to
    // allow the JWT authentication handler to read the access token from the 
    // query string when a WebSocket or Server-Sent Events request comes in.
    //options.Events = new JwtBearerEvents
    //{
    //    OnMessageReceived = context =>
    //    {
    //        var path = context.HttpContext.Request.Path;

    //        if (path.StartsWithSegments($"/{notificationHubUrl}"))
    //        {
    //            Console.WriteLine("Request path: {0}", path);
    //            var accessToken = context.Request.Query["access_token"];
    //            if (!string.IsNullOrWhiteSpace(accessToken))
    //                context.Token = accessToken;
    //        }

    //        return Task.CompletedTask;
    //    }
    //};
});
builder.Services.AddAuthorization();

// ============================================================================
// APP
// ============================================================================

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

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
app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();

app.MapApiHealthRoutes();
app.MapTmdbRoutes();
app.MapKpiRoutes();
app.MapKvRoutes();
app.MapIamRoutes();
app.MapUtilitiesRoutes();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
await IamDbContext.SeedAsync(dbContext, userManager, builder.Configuration);

app.Run();
