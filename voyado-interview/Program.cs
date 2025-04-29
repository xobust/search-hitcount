using interview;
using interview.SearchEngines;
using interview.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;


var builder = WebApplication.CreateBuilder(args);

// Setup Basic logging and tracing
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});
builder.Logging.AddConsole();
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource(builder.Environment.ApplicationName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    })
    .UseOtlpExporter();

// Some extra services (not really necessary for demo)s
builder.Services.AddHealthChecks();
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();

builder.Services.ConfigureHttpClientDefaults(config => 
    config.ConfigureHttpClient(client => client.DefaultRequestVersion = HttpVersion.Version30));

// Configure Search Engines and aggregator
// All need to have names to resolve the correct client
// see: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory-troubleshooting#different-typed-clients-are-registered-on-a-common-interface
builder.Services.AddHttpClient<ISearchEngine, GoogleCustomSearchEngine>(nameof(GoogleCustomSearchEngine), client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/customsearch/v1");
});

builder.Services.AddHttpClient<ISearchEngine, SearchApiIoEngine>("youtube", factory: (client, engine) =>
{
    client.BaseAddress = new Uri("https://www.searchapi.io/api/v1/search");
    return new SearchApiIoEngine(client, builder.Configuration, "youtube");
});

builder.Services.AddHttpClient<ISearchEngine, MojeekSearchEngine>(nameof(MojeekSearchEngine), client =>
{
    client.BaseAddress = new Uri("https://www.mojeek.com/search");
});


builder.Services.AddScoped<ISearchAgregator, SearchAgregator>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

// Serve static files 
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseResponseCaching();
app.UseResponseCompression();

// Map out api routes
app.MapGet("/hello", () => "Hello Voyado!");
app.MapHealthChecks("/health");
app.MapGet("/version", () => Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "Unkown")
    .WithMetadata(new ResponseCacheAttribute
    {
        Duration = 360, 
        Location = ResponseCacheLocation.Any 
    });
app.MapGet("/search", async (string searchQuery, ISearchAgregator searchAgregator, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(searchQuery))
    {
        return Results.BadRequest("Search query cannot be empty.");
    }
    var result = await searchAgregator.GetAgregateSearchResult(searchQuery, cancellation);
    return Results.Ok(result);
}).WithName("GetSearchResults")
  .Produces<AgregateSearchResult>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status400BadRequest)
  .WithMetadata(new ResponseCacheAttribute
  {
      Duration = 120,
      Location = ResponseCacheLocation.Client,
      VaryByQueryKeys = ["searchQuery"]
  });

// Start our app
app.UseHttpsRedirection();
app.Run();
