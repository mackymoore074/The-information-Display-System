using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailToStaffAsync(string subject, string body)
    {
        try
        {
            using var smtpClient = new SmtpClient();
            smtpClient.Host = _configuration["EmailSettings:SmtpHost"];
            smtpClient.Port = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            smtpClient.EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);
            smtpClient.Credentials = new System.Net.NetworkCredential(
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

            // Add staff email addresses from configuration
            var staffEmails = _configuration.GetSection("EmailSettings:StaffEmails").Get<List<string>>();
            foreach (var email in staffEmails)
            {
                mailMessage.To.Add(email);
            }

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - we don't want to break the main functionality
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }
} 