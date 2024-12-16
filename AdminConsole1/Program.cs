using AdminConsole.Data;
using AdminConsole.Data.Authentication;
using AdminConsole.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Configure ApiSettings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// Configure HttpClient with base address
builder.Services.AddScoped(sp =>
{
    var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>();
    //Console.WriteLine($"Base URL: {apiSettings.Value.BaseUrl}");
    var httpClient = new HttpClient { BaseAddress = new Uri(apiSettings.Value.BaseUrl) };
    
    return httpClient;
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddBlazoredLocalStorage();

// Add authentication services
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, MockAuthenticationStateProvider>();

// Register ApiService
builder.Services.AddScoped<ApiService>();
//builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
