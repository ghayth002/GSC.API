# GsC API Code Map

## Core Components and Their Relationships

### Authentication System

```
AuthController
    ↓ uses
IAuthService
    ↓ implemented by
AuthService
    ↓ uses
ApplicationDbContext
    ↓ manages
User, Role, Permission entities
```

### User Management System

```
UsersController
    ↓ uses
IAuthService
    ↓ implemented by
AuthService
    ↓ uses
ApplicationDbContext
    ↓ manages
User entity
```

### Email System

```
AuthService
    ↓ uses
IEmailService
    ↓ implemented by
EmailService
    ↓ configured with
SmtpSettings, EmailSettings
```

### Database Context

```
ApplicationDbContext
    ↓ inherits from
IdentityDbContext<User, Role, int>
    ↓ manages
DbSet<User>
DbSet<Role>
DbSet<Permission>
DbSet<RolePermission>
```

### Entity Relationships

```
User
    ↓ has many
UserRole
    ↓ belongs to
Role
    ↓ has many
RolePermission
    ↓ belongs to
Permission
```

## Key Classes and Their Properties

### User Class

```
User : IdentityUser<int>
{
    int Id
    string FirstName
    string LastName
    DateTime CreatedAt
    DateTime? UpdatedAt
    bool IsActive
    string? EmailVerificationToken
    DateTime? EmailVerificationTokenExpiry
    string? PasswordResetToken
    DateTime? PasswordResetTokenExpiry
    ICollection<UserRole> UserRoles
}
```

### Role Class

```
Role : IdentityRole<int>
{
    int Id
    string Name
    string? Description
    ICollection<UserRole> UserRoles
    ICollection<RolePermission> RolePermissions
}
```

### Permission Class

```
Permission
{
    int Id
    string Name
    string Module
    string? Description
    ICollection<RolePermission> RolePermissions
}
```

## Data Transfer Objects (DTOs)

### Authentication DTOs

```
LoginDto
{
    string Email
    string Password
}

RegisterDto
{
    string Email
    string Password
    string ConfirmPassword
    string FirstName
    string LastName
}

AuthResponseDto
{
    bool Success
    string Message
    string? Token
    UserDto? User
}

UserDto
{
    int Id
    string Email
    string FirstName
    string LastName
    bool IsActive
    DateTime CreatedAt
    DateTime? UpdatedAt
    bool EmailConfirmed
}
```

## Service Interfaces

### IAuthService

```
IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto model)
    Task<AuthResponseDto> RegisterAsync(RegisterDto model)
    Task LogoutAsync()
    Task<UserDto?> GetCurrentUserAsync()
    Task<IEnumerable<UserDto>> GetAllUsersAsync()
    Task<UserDto?> GetUserByIdAsync(int id)
    Task<AuthResponseDto> CreateUserAsync(RegisterDto model)
    Task<AuthResponseDto> UpdateUserAsync(int id, UserDto model)
    Task<AuthResponseDto> DeleteUserAsync(int id)
    Task<AuthResponseDto> AssignRoleToUserAsync(int userId, string roleName)
    Task<AuthResponseDto> RemoveRoleFromUserAsync(int userId, string roleName)
    Task<IList<string>> GetUserRolesAsync(int userId)
    Task<bool> CheckUserPermissionAsync(int userId, string permission)
    Task<AuthResponseDto> ChangePasswordAsync(ChangePasswordDto model)
    Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto model)
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model)
    Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto model)
    Task<AuthResponseDto> ResendVerificationEmailAsync(string email)
}
```

### IEmailService

```
IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    Task SendVerificationEmailAsync(User user, string token)
    Task SendPasswordResetEmailAsync(User user, string token)
}
```

## Request Processing Pipeline

```
Program.cs
    ↓ configures
Middleware Pipeline
    ↓ includes
1. Exception Handling
2. HTTPS Redirection
3. Static Files
4. Routing
5. CORS
6. Authentication
7. Authorization
8. Endpoint Routing
```

## API Endpoints and Controllers

```
/api/auth
    ↓ handled by
AuthController
    ↓ methods
    - Login
    - Register
    - Logout
    - GetCurrentUser
    - VerifyEmail
    - ResendVerificationEmail
    - ForgotPassword
    - ResetPassword
    - ChangePassword

/api/users
    ↓ handled by
UsersController
    ↓ methods
    - GetAll
    - GetById
    - Create
    - Update
    - Delete
    - AssignRole
    - RemoveRole
    - GetRoles
    - HasPermission
```

## Database Initialization and Seeding

```
Program.cs
    ↓ calls
InitializeDatabase extension method
    ↓ creates
Default Roles (Administrator, Manager, User, Viewer)
    ↓ creates
Default Admin User (admin@gsc.com)
    ↓ assigns
Administrator role to Admin User
```

This code map provides a visual representation of the key components and their relationships in the GsC API project, helping developers understand the structure and flow of the application.