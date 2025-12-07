using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace GsC.API.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class EmailSettings
    {
        public int VerificationTokenExpirationHours { get; set; }
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<SmtpSettings> smtpSettings,
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailVerificationAsync(string email, string firstName, string verificationToken)
        {
            try
            {
                var verificationUrl = $"{_emailSettings.BaseUrl}/api/auth/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";
                
                var subject = "Verify Your Email - GsC Catering Management";
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <h1 style='color: #2c5aa0;'>GsC Catering Management</h1>
                            </div>
                            
                            <h2 style='color: #2c5aa0;'>Welcome {firstName}!</h2>
                            
                            <p>Thank you for registering with GsC Catering Management System. To complete your registration, please verify your email address by clicking the button below:</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{verificationUrl}' 
                                   style='background-color: #2c5aa0; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                   Verify Email Address
                                </a>
                            </div>
                            
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background-color: #f5f5f5; padding: 10px; border-radius: 3px;'>{verificationUrl}</p>
                            
                            <p><strong>This verification link will expire in {_emailSettings.VerificationTokenExpirationHours} hours.</strong></p>
                            
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                            
                            <p style='font-size: 12px; color: #666;'>
                                If you didn't create an account with GsC Catering Management, please ignore this email.
                            </p>
                            
                            <p style='font-size: 12px; color: #666;'>
                                Best regards,<br>
                                GsC Catering Management Team
                            </p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(string email, string firstName, string resetToken)
        {
            try
            {
                var resetUrl = $"{_emailSettings.BaseUrl}/api/auth/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

                var subject = "Password Reset - GsC Catering Management";
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <h1 style='color: #2c5aa0;'>GsC Catering Management</h1>
                            </div>

                            <h2 style='color: #2c5aa0;'>Password Reset Request</h2>

                            <p>Hello {firstName},</p>

                            <p>We received a request to reset your password for your GsC Catering Management account. Click the button below to reset your password:</p>

                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetUrl}'
                                   style='background-color: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                   Reset Password
                                </a>
                            </div>

                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background-color: #f5f5f5; padding: 10px; border-radius: 3px;'>{resetUrl}</p>

                            <p><strong>This reset link will expire in {_emailSettings.VerificationTokenExpirationHours} hours.</strong></p>

                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>

                            <p style='font-size: 12px; color: #666;'>
                                If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
                            </p>

                            <p style='font-size: 12px; color: #666;'>
                                Best regards,<br>
                                GsC Catering Management Team
                            </p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetCodeAsync(string email, string firstName, string verificationCode)
        {
            try
            {
                var subject = "Password Reset Code - GsC Catering Management";
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <h1 style='color: #2c5aa0;'>GsC Catering Management</h1>
                            </div>

                            <h2 style='color: #2c5aa0;'>Password Reset Code</h2>

                            <p>Hello {firstName},</p>

                            <p>We received a request to reset your password for your GsC Catering Management account. Please use the verification code below in the app to reset your password:</p>

                            <div style='text-align: center; margin: 30px 0;'>
                                <div style='background-color: #f8f9fa; border: 2px solid #2c5aa0; border-radius: 10px; padding: 20px; display: inline-block;'>
                                    <h1 style='color: #2c5aa0; margin: 0; font-size: 36px; letter-spacing: 5px; font-family: monospace;'>{verificationCode}</h1>
                                </div>
                            </div>

                            <p style='text-align: center;'><strong>Enter this code in the app to reset your password.</strong></p>

                            <p><strong>This verification code will expire in 15 minutes.</strong></p>

                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>

                            <p style='font-size: 12px; color: #666;'>
                                If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
                            </p>

                            <p style='font-size: 12px; color: #666;'>
                                Best regards,<br>
                                GsC Catering Management Team
                            </p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                var subject = "Welcome to GsC Catering Management!";
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <h1 style='color: #2c5aa0;'>GsC Catering Management</h1>
                            </div>
                            
                            <h2 style='color: #2c5aa0;'>Welcome to GsC!</h2>
                            
                            <p>Hello {firstName},</p>
                            
                            <p>Your email has been successfully verified and your account is now active! Welcome to the GsC Catering Management System.</p>
                            
                            <p>You can now access all the features of our platform:</p>
                            <ul>
                                <li>Manage catering operations</li>
                                <li>Track orders and deliveries</li>
                                <li>Access reporting and analytics</li>
                                <li>Collaborate with your team</li>
                            </ul>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{_emailSettings.BaseUrl}' 
                                   style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                   Access Your Account
                                </a>
                            </div>
                            
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                            
                            <p style='font-size: 12px; color: #666;'>
                                If you have any questions or need assistance, please don't hesitate to contact our support team.
                            </p>
                            
                            <p style='font-size: 12px; color: #666;'>
                                Best regards,<br>
                                GsC Catering Management Team
                            </p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendSupplierCredentialsAsync(string email, string firstName, string password, string companyName)
        {
            try
            {
                var subject = "Compte Fournisseur Créé - GsC Catering Management";
                var htmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <h1 style='color: #2c5aa0;'>GsC Catering Management</h1>
                            </div>
                            
                            <h2 style='color: #2c5aa0;'>Bienvenue {firstName}!</h2>
                            
                            <p>Un compte fournisseur a été créé pour votre entreprise <strong>{companyName}</strong> sur la plateforme GsC Catering Management.</p>
                            
                            <p>Voici vos identifiants de connexion :</p>
                            
                            <div style='background-color: #f8f9fa; border-left: 4px solid #2c5aa0; padding: 15px; margin: 20px 0;'>
                                <p style='margin: 5px 0;'><strong>Email :</strong> {email}</p>
                                <p style='margin: 5px 0;'><strong>Mot de passe :</strong> {password}</p>
                            </div>
                            
                            <p>Veuillez vous connecter et changer votre mot de passe dès que possible.</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{_emailSettings.BaseUrl}' 
                                   style='background-color: #2c5aa0; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                   Accéder à mon compte
                                </a>
                            </div>
                            
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                            
                            <p style='font-size: 12px; color: #666;'>
                                Si vous avez des questions, n'hésitez pas à contacter l'administrateur.
                            </p>
                            
                            <p style='font-size: 12px; color: #666;'>
                                Cordialement,<br>
                                L'équipe GsC Catering Management
                            </p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending supplier credentials email to {Email}", email);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }
    }
}
