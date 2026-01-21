using VacationPlanner.Components;
using VacationPlanner.Application.Interfaces;
using VacationPlanner.Application.Services;
using VacationPlanner.Infrastructure.ApiClients;

var builder = WebApplication.CreateBuilder(args);

// Razor/Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient
builder.Services.AddHttpClient();

// Options
var otmOptions = builder.Configuration
    .GetSection("OpenTripMap")
    .Get<OpenTripMapOptions>() ?? new OpenTripMapOptions();

builder.Services.AddSingleton(otmOptions);

// API clients
builder.Services.AddScoped<WikipediaClient>();
builder.Services.AddScoped<GeoCodingClient>();
builder.Services.AddScoped<OpenTripMapClient>();

// App services
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IPlaceSearchService, PlaceSearchService>();
builder.Services.AddScoped<IVacationPlannerService, VacationPlannerService>();
builder.Services.AddScoped<IVoiceCommandParser, VoiceCommandParser>();
builder.Services.AddScoped<OverpassClient>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
