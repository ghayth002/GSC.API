namespace GsC.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerificationAsync(string email, string firstName, string verificationToken);
        Task<bool> SendPasswordResetAsync(string email, string firstName, string resetToken);
        Task<bool> SendPasswordResetCodeAsync(string email, string firstName, string verificationCode);
        Task<bool> SendWelcomeEmailAsync(string email, string firstName);
        Task<bool> SendSupplierCredentialsAsync(string email, string firstName, string password, string companyName);
    }
}
