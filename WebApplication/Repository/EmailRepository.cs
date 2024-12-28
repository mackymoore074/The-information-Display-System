using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using ClassLibrary.DtoModels.Email;
using ClassLibrary.Models;

namespace WebApplication.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ClassDBContext _context;
        private readonly ILogger<EmailRepository> _logger;

        public EmailRepository(ClassDBContext context, IConfiguration configuration, ILogger<EmailRepository> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailToStaffAsync(EmailDto emailDto)
        {
            try
            {
                var employeesWithEmail = await _context.Employees
                    .Where(e => !string.IsNullOrEmpty(e.Email))
                    .Select(e => e.Email)
                    .ToListAsync();

                if (!employeesWithEmail.Any())
                {
                    _logger.LogWarning("No employees with email addresses found");
                    return false;
                }

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
                    Subject = emailDto.Subject,
                    Body = emailDto.Body,
                    IsBodyHtml = true
                };

                foreach (var email in employeesWithEmail)
                {
                    mailMessage.To.Add(email);
                }

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {employeesWithEmail.Count} recipients");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
} 