using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using DsplayScreen;
using DsplayScreen.Services;
using DsplayScreen.IService;
using DsplayScreen.Auth;
using DsplayScreen.Data;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Configure ApiSettings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Authentication and Authorization
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// Register HttpClient with base URL from configuration
builder.Services.AddHttpClient<IScreenService, ScreenService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
});

// Add routing with explicit configuration
builder.Services.AddRouting(options => 
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add MVC services (this might be needed for proper routing)
builder.Services.AddMvc();

// Add WeatherForecastService to fix the error
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ensure routing is configured before authentication and authorization
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure endpoints
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers(); // Add this if you have any API controllers

app.Run();
