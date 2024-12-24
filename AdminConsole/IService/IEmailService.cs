public interface IEmailService
{
    Task SendEmailToStaffAsync(string subject, string body);
} 