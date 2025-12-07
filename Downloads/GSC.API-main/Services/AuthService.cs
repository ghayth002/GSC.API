using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using Google.Apis.Auth;
using GsC.API.Data;
using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IEmailService emailService,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null || !user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }

                if (!user.EmailConfirmed)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Please verify your email address before logging in. Check your email for the verification link."
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    var token = await GenerateJwtTokenAsync(user);
                    var userRoles = await _userManager.GetRolesAsync(user);

                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Login successful.",
                        Token = token,
                        ExpirationDate = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                        User = new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName!,
                            Email = user.Email!,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            IsActive = user.IsActive,
                            CreatedAt = user.CreatedAt,
                            EmailConfirmed = user.EmailConfirmed,
                            EmailVerifiedAt = user.EmailVerifiedAt,
                            PictureUrl = user.PictureUrl,
                            Roles = userRoles
                        }
                    };
                }
                else if (result.IsLockedOut)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Account is locked out. Please try again later."
                    };
                }
                else
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", loginDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during login."
                };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                if (registerDto.Password != registerDto.ConfirmPassword)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Passwords do not match."
                    };
                }

                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User with this email already exists."
                    };
                }

                // Generate email verification token
                var verificationToken = Guid.NewGuid().ToString();
                var tokenExpiration = DateTime.UtcNow.AddHours(24);

                var user = new User
                {
                    UserName = registerDto.UserName,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    EmailConfirmed = false,
                    EmailVerificationToken = verificationToken,
                    EmailVerificationTokenExpires = tokenExpiration
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Administrator");

                    // Send verification email
                    var emailSent = await _emailService.SendEmailVerificationAsync(
                        user.Email,
                        user.FirstName ?? "User",
                        verificationToken);

                    if (!emailSent)
                    {
                        _logger.LogWarning("Failed to send verification email to {Email}", user.Email);
                    }

                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Registration successful. Please check your email to verify your account before logging in.",
                        User = new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            IsActive = user.IsActive,
                            CreatedAt = user.CreatedAt,
                            EmailConfirmed = user.EmailConfirmed,
                            EmailVerifiedAt = user.EmailVerifiedAt,
                            Roles = await _userManager.GetRolesAsync(user)
                        }
                    };
                }
                else
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", registerDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration."
                };
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                await _signInManager.SignOutAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    EmailConfirmed = user.EmailConfirmed,
                    EmailVerifiedAt = user.EmailVerifiedAt,
                    PictureUrl = user.PictureUrl,
                    Roles = roles
                });
            }

            return userDtos;
        }

        public async Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return null;
                }

                var user = new User
                {
                    UserName = createUserDto.UserName,
                    Email = createUserDto.Email,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                
                if (result.Succeeded)
                {
                    foreach (var role in createUserDto.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }

                    var userRoles = await _userManager.GetRolesAsync(user);
                    return new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        EmailConfirmed = user.EmailConfirmed,
                        EmailVerifiedAt = user.EmailVerifiedAt,
                        PictureUrl = user.PictureUrl,
                        Roles = userRoles
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", createUserDto.Email);
                return null;
            }
        }

        public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return null;

                if (!string.IsNullOrEmpty(updateUserDto.UserName))
                    user.UserName = updateUserDto.UserName;

                if (!string.IsNullOrEmpty(updateUserDto.Email))
                    user.Email = updateUserDto.Email;

                if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                    user.FirstName = updateUserDto.FirstName;

                if (!string.IsNullOrEmpty(updateUserDto.LastName))
                    user.LastName = updateUserDto.LastName;

                if (updateUserDto.IsActive.HasValue)
                    user.IsActive = updateUserDto.IsActive.Value;

                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    if (updateUserDto.Roles != null)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        
                        foreach (var role in updateUserDto.Roles)
                        {
                            if (await _roleManager.RoleExistsAsync(role))
                            {
                                await _userManager.AddToRoleAsync(user, role);
                            }
                        }
                    }

                    var userRoles = await _userManager.GetRolesAsync(user);
                    return new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        EmailConfirmed = user.EmailConfirmed,
                        EmailVerifiedAt = user.EmailVerifiedAt,
                        PictureUrl = user.PictureUrl,
                        Roles = userRoles
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                // Soft delete
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> AssignRoleAsync(int userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists) return false;

                var result = await _userManager.AddToRoleAsync(user, roleName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<bool> RemoveRoleAsync(int userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                var userRoles = await _userManager.GetRolesAsync(user);
                
                var hasPermission = await _context.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .AnyAsync(rp => userRoles.Contains(rp.Role.Name!) && rp.Permission.Name == permission);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
                    return false;

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return false;
            }
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "GsC-Super-Secret-Key-For-Development-Only-Change-In-Production";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var userRoles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!),
                new("FirstName", user.FirstName ?? ""),
                new("LastName", user.LastName ?? "")
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetTokenExpirationMinutes()
        {
            return _configuration.GetSection("JwtSettings").GetValue<int>("ExpirationInMinutes", 60);
        }
        
        public async Task<AuthResponseDto> ExternalLoginAsync(string email, string firstName, string lastName, string provider)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email is required for external authentication."
                    };
                }
                
                // Check if user exists
                var user = await _userManager.FindByEmailAsync(email);
                
                if (user == null)
                {
                    // Create new user
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        EmailConfirmed = true, // External auth providers have already verified the email
                        EmailVerifiedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    var createResult = await _userManager.CreateAsync(user);
                    
                    if (!createResult.Succeeded)
                    {
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Failed to create user account."
                        };
                    }
                    
                    // Assign default role
                    await _userManager.AddToRoleAsync(user, "Administrator");
                }
                else if (!user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Your account has been deactivated. Please contact support."
                    };
                }
                
                // Generate JWT token
                var token = await GenerateJwtTokenAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);
                
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "External login successful.",
                    Token = token,
                    ExpirationDate = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                    User = new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        EmailConfirmed = user.EmailConfirmed,
                        EmailVerifiedAt = user.EmailVerifiedAt,
                        PictureUrl = user.PictureUrl,
                        Roles = userRoles
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in external login");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during external login."
                };
            }
        }
        
        public async Task<AuthResponseDto> GoogleTokenLoginAsync(string idToken)
        {
            try
            {
                // Validate the token
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };
                
                if (_environment.IsDevelopment())
                {
                    // Relax validation for development
                    settings.HostedDomain = null;
                    settings.Clock = null;
                }
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                
                // Process the authenticated user
                return await ExternalLoginAsync(
                    payload.Email,
                    payload.GivenName,
                    payload.FamilyName,
                    "Google");
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid Google token");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid Google authentication token."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Google token login");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during Google authentication."
                };
            }
        }

        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(verifyEmailDto.Email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid verification link."
                    };
                }

                if (user.EmailConfirmed)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email is already verified."
                    };
                }

                if (user.EmailVerificationToken != verifyEmailDto.Token ||
                    user.EmailVerificationTokenExpires == null ||
                    user.EmailVerificationTokenExpires < DateTime.UtcNow)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired verification link."
                    };
                }

                user.EmailConfirmed = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpires = null;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Send welcome email
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName ?? "User");

                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Email verified successfully! You can now log in to your account."
                    };
                }

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Failed to verify email. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for {Email}", verifyEmailDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during email verification."
                };
            }
        }

        public async Task<AuthResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resendDto.Email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                if (user.EmailConfirmed)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email is already verified."
                    };
                }

                // Generate new verification token
                var verificationToken = Guid.NewGuid().ToString();
                var tokenExpiration = DateTime.UtcNow.AddHours(24);

                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpires = tokenExpiration;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var emailSent = await _emailService.SendEmailVerificationAsync(
                        user.Email,
                        user.FirstName ?? "User",
                        verificationToken);

                    if (emailSent)
                    {
                        return new AuthResponseDto
                        {
                            Success = true,
                            Message = "Verification email sent successfully. Please check your email."
                        };
                    }
                }

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Failed to send verification email. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email for {Email}", resendDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred while sending verification email."
                };
            }
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null || !user.EmailConfirmed)
                {
                    // Don't reveal whether user exists or not
                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "If an account with that email exists, a password reset link has been sent."
                    };
                }

                // Generate password reset token
                var resetToken = Guid.NewGuid().ToString();
                var tokenExpiration = DateTime.UtcNow.AddHours(24);

                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpires = tokenExpiration;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _emailService.SendPasswordResetAsync(
                        user.Email,
                        user.FirstName ?? "User",
                        resetToken);
                }

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", forgotPasswordDto.Email);
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Passwords do not match."
                    };
                }

                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid reset link."
                    };
                }

                if (user.PasswordResetToken != resetPasswordDto.Token ||
                    user.PasswordResetTokenExpires == null ||
                    user.PasswordResetTokenExpires < DateTime.UtcNow)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired reset link."
                    };
                }

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    // Clear reset token
                    user.PasswordResetToken = null;
                    user.PasswordResetTokenExpires = null;
                    await _userManager.UpdateAsync(user);

                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Password reset successfully. You can now log in with your new password."
                    };
                }

                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", resetPasswordDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during password reset."
                };
            }
        }

        public async Task<AuthResponseDto> RequestPasswordResetAsync(RequestPasswordResetDto requestDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(requestDto.Email);
                if (user == null || !user.EmailConfirmed)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found or email not verified."
                    };
                }

                // Generate a 6-digit verification code
                var verificationCode = new Random().Next(100000, 999999).ToString();
                var tokenExpiration = DateTime.UtcNow.AddMinutes(15); // 15 minutes expiration

                user.PasswordResetToken = verificationCode;
                user.PasswordResetTokenExpires = tokenExpiration;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Send verification code via email
                    var emailSent = await _emailService.SendPasswordResetCodeAsync(
                        user.Email,
                        user.FirstName ?? "User",
                        verificationCode);

                    if (emailSent)
                    {
                        return new AuthResponseDto
                        {
                            Success = true,
                            Message = "Verification code sent to your email. Please check your email and enter the code."
                        };
                    }
                }

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Failed to send verification code. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset for {Email}", requestDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your request."
                };
            }
        }

        public async Task<AuthResponseDto> ResetPasswordDirectAsync(ResetPasswordDirectDto resetDto)
        {
            try
            {
                // Trim whitespace and validate passwords
                var newPassword = resetDto.NewPassword?.Trim() ?? "";
                var confirmPassword = resetDto.ConfirmPassword?.Trim() ?? "";

                _logger.LogInformation("Password reset attempt - NewPassword length: {NewLength}, ConfirmPassword length: {ConfirmLength}",
                    newPassword.Length, confirmPassword.Length);

                if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Password fields cannot be empty."
                    };
                }

                if (newPassword != confirmPassword)
                {
                    _logger.LogWarning("Password mismatch for email {Email}", resetDto.Email);
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Passwords do not match."
                    };
                }

                var user = await _userManager.FindByEmailAsync(resetDto.Email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                if (user.PasswordResetToken != resetDto.VerificationCode ||
                    user.PasswordResetTokenExpires == null ||
                    user.PasswordResetTokenExpires < DateTime.UtcNow)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired verification code."
                    };
                }

                // Reset password using Identity's built-in method
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    // Clear reset token
                    user.PasswordResetToken = null;
                    user.PasswordResetTokenExpires = null;
                    await _userManager.UpdateAsync(user);

                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Password reset successfully. You can now log in with your new password."
                    };
                }

                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password directly for {Email}", resetDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during password reset."
                };
            }
        }

        // Google authentication methods have been removed
        
        private async Task<UserDto> GetUserDtoAsync(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed,
                EmailVerifiedAt = user.EmailVerifiedAt,
                PictureUrl = user.PictureUrl,
                Roles = userRoles
            };
        }
    }
}