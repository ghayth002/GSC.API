using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using GsC.API.Services;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.LoginAsync(loginDto);
                
                if (result.Success)
                    return Ok(result);
                
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RegisterAsync(registerDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in register endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userId = User.Identity?.Name ?? "";
                var result = await _authService.LogoutAsync(userId);
                
                if (result)
                    return Ok(new { message = "Logout successful" });
                
                return BadRequest(new { message = "Logout failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in logout endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleResponse)),
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme }
                }
            };
            
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            
            if (!authenticateResult.Succeeded)
                return BadRequest(new { message = "Google authentication failed" });
                
            var googleUser = authenticateResult.Principal;
            var email = googleUser.FindFirstValue(ClaimTypes.Email);
            var name = googleUser.FindFirstValue(ClaimTypes.Name);
            var firstName = googleUser.FindFirstValue(ClaimTypes.GivenName);
            var lastName = googleUser.FindFirstValue(ClaimTypes.Surname);
            
            var result = await _authService.ExternalLoginAsync(email, firstName, lastName, "Google");
            
            if (result.Success)
            {
                // For API clients, return the token
                if (Request.Headers.Accept.Any(h => h.Contains("application/json")))
                {
                    return Ok(result);
                }
                
                // For browser clients, redirect to the frontend with the token
                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
                return Redirect($"{frontendUrl}/auth/callback?token={result.Token}");
            }
            
            return BadRequest(result);
        }
        
        [HttpPost("google-token")]
        public async Task<ActionResult<AuthResponseDto>> GoogleTokenLogin([FromBody] GoogleAuthDto googleAuthDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                    
                var result = await _authService.GoogleTokenLoginAsync(googleAuthDto.IdToken);
                
                if (result.Success)
                    return Ok(result);
                    
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Google token login endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }
        
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound();

                var roles = await _authService.GetUserRolesAsync(userId);

                var userDto = new UserDto
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
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get current user endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
                
                if (result)
                    return Ok(new { message = "Password changed successfully" });
                
                return BadRequest(new { message = "Failed to change password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in change password endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpGet("verify-email")]
        public async Task<ActionResult<AuthResponseDto>> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                    return BadRequest(new { message = "Email and token are required" });

                var verifyEmailDto = new VerifyEmailDto
                {
                    Email = email,
                    Token = token
                };

                var result = await _authService.VerifyEmailAsync(verifyEmailDto);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in verify email endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("resend-verification")]
        public async Task<ActionResult<AuthResponseDto>> ResendVerification([FromBody] ResendVerificationDto resendDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.ResendVerificationEmailAsync(resendDto);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in resend verification endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<AuthResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

                return Ok(result); // Always return success to prevent email enumeration
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.ResetPasswordAsync(resetPasswordDto);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reset password endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("request-password-reset")]
        public async Task<ActionResult<AuthResponseDto>> RequestPasswordReset([FromBody] RequestPasswordResetDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RequestPasswordResetAsync(requestDto);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in request password reset endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("reset-password-direct")]
        public async Task<ActionResult<AuthResponseDto>> ResetPasswordDirect([FromBody] ResetPasswordDirectDto resetDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    _logger.LogWarning("Model validation failed for reset password: {@Errors}", errors);

                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = string.Join("; ", errors.SelectMany(e => e.Errors))
                    });
                }

                _logger.LogInformation("Processing password reset for email: {Email}", resetDto.Email);
                var result = await _authService.ResetPasswordDirectAsync(resetDto);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reset password direct endpoint");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        // Google authentication endpoints have been removed
    }
}