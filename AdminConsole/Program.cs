using AdminConsole.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using AdminConsole.Data.Authentication;
using Microsoft.Extensions.Options;
using AdminConsole.IService;
using AdminConsole.Services;

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

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ILocationService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<LocationService>>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new LocationService(httpClient, logger, localStorage);
});

// Add authentication services
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, MockAuthenticationStateProvider>();

builder.Services.AddScoped<IAgencyService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<AgencyService>>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new AgencyService(httpClient, logger, localStorage);
});

builder.Services.AddScoped<IDepartmentService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<DepartmentService>>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new DepartmentService(httpClient, logger, localStorage);
});

builder.Services.AddScoped<IScreenService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<ScreenService>>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new ScreenService(httpClient, logger, localStorage);
});

builder.Services.AddScoped<IEmployeeService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<EmployeeService>>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new EmployeeService(httpClient, logger, localStorage);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
