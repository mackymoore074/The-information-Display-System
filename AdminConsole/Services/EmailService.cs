using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILocalStorageService localStorage,
        ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task SendEmailToStaffAsync(string subject, string body)
    {
        try
        {
            // Get all employees with emails
            var token = await _localStorage.GetItemAsync<string>("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/Employee/with-emails");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<string>>>();
                if (result?.Success == true && result.Data?.Any() == true)
                {
                    using var smtpClient = new SmtpClient();
                    smtpClient.Host = _configuration["EmailSettings:SmtpHost"];
                    smtpClient.Port = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                    smtpClient.EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);
                    smtpClient.Credentials = new NetworkCredential(
                        _configuration["EmailSettings:Username"],
                        _configuration["EmailSettings:Password"]
                    );

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_configuration["EmailSettings:FromEmail"]),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    // Add all employee emails
                    foreach (var email in result.Data)
                    {
                        mailMessage.To.Add(email);
                    }

                    await smtpClient.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email sent successfully to {result.Data.Count} recipients");
                }
                else
                {
                    _logger.LogWarning("No employee emails found or failed to get employees");
                }
            }
            else
            {
                _logger.LogWarning($"Failed to get employee emails. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email: {ex.Message}");
        }
    }
} 