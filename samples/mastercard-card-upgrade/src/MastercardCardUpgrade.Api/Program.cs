using System.Text.Json;
using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Options;
using MastercardCardUpgrade.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Sandbox"))
    builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Mastercard Card Upgrade",
        Version = "v1",
        Description = "End-to-end Product Graduation Plus. All Mastercard URLs are BaseUrl + Paths from configuration. AuthMode is OAuth1 or Bearer (Token)."
    });
});

builder.Services.Configure<MastercardOptions>(
    builder.Configuration.GetSection(MastercardOptions.SectionName));
builder.Services.Configure<ProductCatalogOptions>(
    builder.Configuration.GetSection(ProductCatalogOptions.SectionName));

builder.Services.AddSingleton<MastercardSigningKeyHolder>();
builder.Services.AddSingleton<MastercardJweService>();
builder.Services.AddTransient<MastercardOAuthHandler>();
builder.Services.AddSingleton<ICardStore, InMemoryCardStore>();
builder.Services.AddSingleton<IProductCatalog, ProductCatalog>();
builder.Services.AddSingleton<LocalAcsClient>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<ICardLifecycleService, CardLifecycleService>();
builder.Services.AddScoped<IMastercardUpgradeService, MastercardUpgradeService>();
builder.Services.AddHostedService<AcsReconcileWorker>();

builder.Services.AddHttpClient<IMastercardBinLookupClient, MastercardBinLookupClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<MastercardOptions>>().Value;
    client.BaseAddress = options.BaseUri;
    client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
}).AddHttpMessageHandler<MastercardOAuthHandler>();
builder.Services.AddHttpClient<MastercardAcsClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<MastercardOptions>>().Value;
    client.BaseAddress = options.BaseUri;
    client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
}).AddHttpMessageHandler<MastercardOAuthHandler>();

builder.Services.AddScoped<IAcsClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MastercardOptions>>().Value;
    return options.UseLiveMastercardAlm
        ? sp.GetRequiredService<MastercardAcsClient>()
        : sp.GetRequiredService<LocalAcsClient>();
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = ex switch
        {
            KillSwitchException => StatusCodes.Status503ServiceUnavailable,
            IdempotencyConflictException => StatusCodes.Status409Conflict,
            AcsAmbiguousOutcomeException => StatusCodes.Status504GatewayTimeout,
            MastercardApiException api => api.StatusCode is >= 400 and < 600 ? api.StatusCode : 502,
            EligibilityException => StatusCodes.Status422UnprocessableEntity,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException or FileNotFoundException or ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        object body = ex is MastercardApiException mastercard
            ? new
            {
                title = mastercard.Operation,
                status = context.Response.StatusCode,
                detail = mastercard.Message,
                mastercardBody = mastercard.ResponseBody
            }
            : new
            {
                title = ex switch
                {
                    KillSwitchException => "Writes disabled",
                    IdempotencyConflictException => "Idempotency conflict",
                    AcsAmbiguousOutcomeException => "ACS outcome unknown",
                    EligibilityException => "Eligibility failed",
                    _ => "Mastercard Card Upgrade error"
                },
                status = context.Response.StatusCode,
                detail = PanRedactor.Redact(ex?.Message)
            };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body), context.RequestAborted);
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mastercard Card Upgrade v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/mastercard/sandbox/status", (
        IOptions<MastercardOptions> options,
        IAcsClient acs,
        IHostEnvironment env) =>
    {
        var cfg = options.Value;
        var p12 = !string.IsNullOrWhiteSpace(cfg.SigningKeyP12Path) && File.Exists(cfg.SigningKeyP12Path);
        var pem = !string.IsNullOrWhiteSpace(cfg.PrivateKeyPemPath) && File.Exists(cfg.PrivateKeyPemPath);
        var kind = cfg.UseBearerAuth
            ? (cfg.HasBearerToken ? "bearer" : "missing")
            : p12 ? "p12" : pem ? "pem" : "missing";

        var next = cfg.LiveAcsReady
            ? "Live ACS ready: POST /api/demo/e2e."
            : cfg.UseLiveMastercardAlm
                ? "AlmMode=Mastercard but credentials or JWE certs are missing."
                : "Local ACS mode: POST /api/demo/e2e to create, register and upgrade a card.";

        return Results.Ok(new SandboxStatusResponse(
            env.EnvironmentName,
            cfg.BaseUrl.TrimEnd('/'),
            cfg.AuthMode,
            cfg.HasCredentials,
            cfg.UseBearerAuth ? cfg.HasBearerToken : p12 || pem,
            kind,
            cfg.UseLiveMastercardAlm,
            next)
        {
            AlmMode = acs.Mode,
            BinLookupUrl = cfg.Url(cfg.Paths.BinLookup).ToString(),
            AcsRegistrationsUrl = cfg.Url(cfg.Paths.AcsRegistrations).ToString(),
            AcsDeleteRegistrationsUrl = cfg.Url(cfg.Paths.AcsDeleteRegistrations).ToString(),
            WritesEnabled = cfg.WritesEnabled,
            JweConfigured = cfg.HasJweMaterial,
            LiveAcsReady = cfg.LiveAcsReady,
            CardStorePath = string.IsNullOrWhiteSpace(cfg.CardStorePath) ? null : cfg.CardStorePath,
            TestData = new MastercardTestDataResponse(
                cfg.SandboxSampleAccountRange,
                cfg.SandboxSamplePan,
                PanRules.Mask(cfg.SandboxSamplePan),
                cfg.SandboxSampleExpiryMmYy,
                cfg.SandboxSourceProductCode,
                MastercardTestData.SwaggerProductCode,
                cfg.SandboxTargetProductCode,
                MastercardTestData.BinLookupProductCode,
                cfg.AlmServiceCode,
                MastercardTestData.RequestId,
                MastercardTestData.EffectiveDate)
        });
    })
    .WithName("SandboxStatus")
    .WithTags("Sandbox");

app.MapControllers();
app.Run();

public partial class Program
{
}
