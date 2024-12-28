using ClassLibrary.DtoModels.Email;

public interface IEmailRepository
{
    Task<bool> SendEmailToStaffAsync(EmailDto emailDto);
} 