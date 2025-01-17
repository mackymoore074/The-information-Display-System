using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using DsplayScreen.Auth;
using Microsoft.Extensions.Options;
using DsplayScreen.IService;
using DsplayScreen.Services;
using DsplayScreen.Data;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Configure ApiSettings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// Configure HttpClient with base address
builder.Services.AddScoped(sp =>
{
    var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>();
    return new HttpClient { BaseAddress = new Uri(apiSettings.Value.BaseUrl) };
});

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredLocalStorage();

// Add Screen Service
builder.Services.AddScoped<IScreenService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<ScreenService>>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new ScreenService(httpClient, logger, localStorage);
});

// Add authentication services
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    { "DisplaySettings:RefreshIntervalInSeconds", "30" }
});
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddScoped<IPublicDisplayService, PublicDisplayService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
