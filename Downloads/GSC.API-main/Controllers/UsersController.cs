using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GsC.API.Services;
using GsC.API.DTOs;

namespace GsC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAuthService authService, ILogger<UsersController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                var roles = await _authService.GetUserRolesAsync(id);

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
                _logger.LogError(ex, "Error retrieving user {Id}", id);
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _authService.CreateUserAsync(createUserDto);
                if (user == null)
                    return BadRequest(new { message = "Failed to create user. User with this email may already exist." });

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _authService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound(new { message = "User not found or update failed" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", id);
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _authService.DeleteUserAsync(id);
                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("{userId}/assign-role")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> AssignRole(int userId, [FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                // Override userId from URL
                assignRoleDto.UserId = userId;

                var result = await _authService.AssignRoleAsync(assignRoleDto.UserId, assignRoleDto.RoleName);
                
                if (result)
                    return Ok(new { message = "Role assigned successfully" });
                
                return BadRequest(new { message = "Failed to assign role" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpPost("{userId}/remove-role")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> RemoveRole(int userId, [FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                // Override userId from URL
                assignRoleDto.UserId = userId;

                var result = await _authService.RemoveRoleAsync(assignRoleDto.UserId, assignRoleDto.RoleName);
                
                if (result)
                    return Ok(new { message = "Role removed successfully" });
                
                return BadRequest(new { message = "Failed to remove role" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpGet("{userId}/roles")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(int userId)
        {
            try
            {
                var roles = await _authService.GetUserRolesAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }

        [HttpGet("{userId}/permissions/{permission}")]
        [Authorize]
        public async Task<ActionResult<bool>> HasPermission(int userId, string permission)
        {
            try
            {
                var hasPermission = await _authService.HasPermissionAsync(userId, permission);
                return Ok(hasPermission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission");
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }
    }
}