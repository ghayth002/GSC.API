# GsC API Developer Guide

## Quick Start

### Setup and Run

1. **Clone the repository**
2. **Restore dependencies**
   ```
   dotnet restore
   ```
3. **Update database**
   ```
   dotnet ef database update
   ```
4. **Run the application**
   ```
   dotnet run
   ```
5. **Access Swagger UI**
   - http://localhost:5114/swagger
   - https://localhost:7115/swagger

### Default Admin Credentials

- **Email**: admin@gsc.com
- **Password**: Admin123!

## Common Development Tasks

### Authentication

#### Register a New User

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

#### Using JWT Token

After login, include the JWT token in the Authorization header for subsequent requests:

```http
GET /api/auth/current-user
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### User Management

#### Get All Users (Admin only)

```http
GET /api/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Assign Role to User (Admin only)

```http
POST /api/users/1/roles
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "roleName": "Manager"
}
```

## Project Structure Overview

### Key Folders

- **Controllers**: API endpoints
- **Services**: Business logic
- **Models**: Database entities
- **DTOs**: Data transfer objects
- **Data**: Database context and configuration

## Common Workflows

### User Registration and Authentication

1. User registers via `/api/auth/register`
2. User receives verification email
3. User verifies email via `/api/auth/verify-email`
4. User logs in via `/api/auth/login`
5. System returns JWT token
6. User includes token in subsequent requests

### Password Reset

1. User requests password reset via `/api/auth/forgot-password`
2. User receives password reset email
3. User resets password via `/api/auth/reset-password`

### Role and Permission Management

1. Admin assigns role to user via `/api/users/{id}/roles`
2. System automatically assigns permissions based on role
3. User can access resources based on permissions

## Database Schema

### Core Tables

- **Users**: User accounts and profile information
- **Roles**: User roles (Administrator, Manager, User, Viewer)
- **Permissions**: Granular access controls
- **RolePermissions**: Many-to-many relationship between roles and permissions
- **UserRoles**: Many-to-many relationship between users and roles

## Adding New Features

### Adding a New API Endpoint

1. Create or modify a controller in the Controllers folder
2. Define the endpoint method with appropriate HTTP verb attribute
3. Implement authorization using `[Authorize]` attribute
4. Call appropriate service method
5. Return appropriate response

### Adding a New Entity

1. Create entity class in Models folder
2. Add DbSet to ApplicationDbContext
3. Configure entity in OnModelCreating method
4. Create migration: `dotnet ef migrations add AddNewEntity`
5. Update database: `dotnet ef database update`

### Adding a New Service

1. Define interface in Interfaces folder
2. Implement service in Services folder
3. Register service in Program.cs

## Testing

### Authentication Testing

1. Register a new test user
2. Verify email (or use admin to activate)
3. Login to get JWT token
4. Test protected endpoints with token

### Authorization Testing

1. Create users with different roles
2. Test access to endpoints with different role requirements
3. Verify permission checks work correctly

## Troubleshooting

### Common Issues

1. **401 Unauthorized**: Check JWT token is valid and not expired
2. **403 Forbidden**: Check user has required role/permission
3. **404 Not Found**: Check endpoint URL is correct
4. **500 Internal Server Error**: Check server logs for details

### Logging

The application uses built-in ASP.NET Core logging. Check the console output or configured log files for error details.

## Configuration

### Changing Database Connection

Modify the DefaultConnection string in appsettings.json:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server;Database=your-database;User Id=your-user;Password=your-password;"
}
```

### Changing JWT Settings

Modify the JwtSettings in appsettings.json:

```json
"JwtSettings": {
  "SecretKey": "your-new-secret-key-here",
  "ExpirationInMinutes": 120
}
```

### Changing Email Settings

Modify the SmtpSettings and EmailSettings in appsettings.json:

```json
"SmtpSettings": {
  "Host": "your-smtp-server",
  "Port": 587,
  "Username": "your-email",
  "Password": "your-password"
},
"EmailSettings": {
  "VerificationTokenExpirationHours": 48,
  "BaseUrl": "https://your-domain.com"
}
```

## Best Practices

1. **Security**: Never hardcode sensitive information
2. **Error Handling**: Use try-catch blocks and return appropriate status codes
3. **Validation**: Validate input data using ModelState or FluentValidation
4. **Authorization**: Always check permissions before performing sensitive operations
5. **Logging**: Log important events and errors
6. **Testing**: Write unit tests for critical functionality

This guide provides a quick overview of the GsC API project for developers. For more detailed information, refer to the README-DETAILED.md, ARCHITECTURE-DIAGRAM.md, and CODE-MAP.md files.