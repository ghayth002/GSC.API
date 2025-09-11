using GsC.API.Models;
using GsC.API.DTOs;

namespace GsC.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> LogoutAsync(string userId);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> AssignRoleAsync(int userId, string roleName);
        Task<bool> RemoveRoleAsync(int userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permission);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

        // Email verification methods
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
        Task<AuthResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        // Direct password reset methods (app-based)
        Task<AuthResponseDto> RequestPasswordResetAsync(RequestPasswordResetDto requestDto);
        Task<AuthResponseDto> ResetPasswordDirectAsync(ResetPasswordDirectDto resetDto);
        
        // External authentication methods
        Task<AuthResponseDto> ExternalLoginAsync(string email, string firstName, string lastName, string provider);
        Task<AuthResponseDto> GoogleTokenLoginAsync(string idToken);
    }
}