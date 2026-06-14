using ApexCharts;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using KenmanDesignStudio.Infrastructure.Data;
using KenmanDesignStudio.Infrastructure.Seeding;
using KenmanDesignStudio.Web.Components;
using KenmanDesignStudio.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor / Blazor Interactive Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MudBlazor + ApexCharts
builder.Services.AddMudServices();
builder.Services.AddApexCharts();

// EF Core — factory pattern for Blazor Server safety
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=KenmanDesignStudio;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// App services (each resolves a short-lived context from the factory)
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<MarketingService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ShowcaseService>();
builder.Services.AddScoped<ImageResolver>();

// ---- Observability: export OpenTelemetry (traces, metrics, logs) to the Aspire Dashboard ----
var otel = builder.Services.AddOpenTelemetry();
otel.ConfigureResource(r => r.AddService(
    serviceName: builder.Configuration["OTEL_SERVICE_NAME"] ?? "kenmandesignstudio"));
otel.WithTracing(t => t
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation());
otel.WithMetrics(m => m
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddRuntimeInstrumentation());
builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeFormattedMessage = true;
    o.IncludeScopes = true;
});
// Only wire the OTLP exporter when an endpoint is configured (OTEL_EXPORTER_OTLP_ENDPOINT),
// so local runs without a collector don't emit export errors.
if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
    otel.UseOtlpExporter();
}

// Liveness endpoint for uptime checks.
builder.Services.AddHealthChecks();

var app = builder.Build();

// ---- Database migrate + seed on startup ----
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        if (config.GetValue("Seed:AutoMigrate", true))
        {
            logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync();
        }

        if (config.GetValue("Seed:AutoSeed", true))
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            logger.LogInformation("Seeding demo data (if empty)...");
            await DataSeeder.SeedAsync(db, webRoot);
            logger.LogInformation("Seed check complete.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialisation failed. Verify the connection string in appsettings.json.");
    }
}

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
