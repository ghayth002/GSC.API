# GsC API - Detailed Project Documentation

## Project Overview

GsC API is a .NET Core Web API project designed for catering subcontracting management. It provides a robust backend system with user authentication, role-based authorization, and various business functionalities.

## Technology Stack

- **Framework**: ASP.NET Core 6.0
- **Database**: Microsoft SQL Server (LocalDB)
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Documentation**: Swagger/OpenAPI
- **Email Service**: SMTP (configured for Gmail)

## Project Architecture

The project follows a layered architecture pattern with clear separation of concerns:

### Layers

1. **Presentation Layer** - API Controllers
   - Handles HTTP requests and responses
   - Implements input validation
   - Routes requests to appropriate services

2. **Business Logic Layer** - Services
   - Implements business rules and workflows
   - Handles authentication, authorization logic
   - Processes data before persistence

3. **Data Access Layer** - DbContext and Repositories
   - Manages database connections and operations
   - Implements data access patterns
   - Handles entity relationships

4. **Domain Layer** - Models
   - Defines business entities and relationships
   - Implements domain-specific logic

## Project Structure

### Core Components

- **Controllers**: Handle API endpoints and request processing
- **Services**: Implement business logic and workflows
- **Models**: Define data structures and relationships
- **DTOs**: Data Transfer Objects for API communication
- **Middleware**: Request pipeline components
- **Extensions**: Service configuration and setup

## Database Design

### Core Entities

- **User**: Extends IdentityUser with additional properties
- **Role**: Extends IdentityRole with custom properties
- **Permission**: Defines granular access controls
- **RolePermission**: Many-to-many relationship between roles and permissions

### Business Entities

- **Flight**: Represents flight information
- **Order**: Represents customer orders
- **OrderLine**: Individual items within an order
- **Menu**: Collection of menu items
- **MenuItem**: Individual food/beverage items
- **MedicalBox**: Medical supplies container
- **MedicalBoxItem**: Individual medical items

## Authentication and Authorization

### Authentication Flow

1. User registers with email, password, and personal information
2. User verifies email through a verification token
3. User logs in with credentials
4. System generates JWT token with claims
5. User includes token in subsequent requests

### Authorization System

- **Role-Based Access Control**: Users are assigned roles (Administrator, Manager, User, Viewer)
- **Permission-Based Access Control**: Roles have specific permissions
- **Policy-Based Authorization**: Endpoints are protected by policies

## Key Files and Their Functions

### Configuration and Startup

- **Program.cs**: Application entry point and configuration
  - Configures services (DbContext, Identity, JWT, CORS)
  - Sets up middleware pipeline
  - Initializes database and seeds data

- **appsettings.json**: Application configuration
  - Database connection strings
  - JWT settings (secret key, issuer, audience, expiration)
  - SMTP settings for email service
  - Logging configuration

### Data Access

- **ApplicationDbContext.cs**: EF Core database context
  - Defines DbSets for entities
  - Configures entity relationships
  - Sets up table names and constraints

### Authentication and User Management

- **AuthController.cs**: Authentication endpoints
  - Login, Register, Logout
  - Password management (change, reset)
  - Email verification

- **UsersController.cs**: User management endpoints
  - CRUD operations for users
  - Role assignment
  - Permission checking

- **IAuthService.cs/AuthService.cs**: Authentication business logic
  - User authentication and registration
  - Token generation and validation
  - Password management

### Models

- **User.cs**: User entity definition
  - Personal information (first name, last name)
  - Account status and timestamps
  - Email verification and password reset properties

- **Role.cs**: Role entity definition
  - Role name and description
  - Associated permissions

- **Permission.cs**: Permission entity definition
  - Permission name and module
  - Description and access level

### DTOs

- **AuthDTOs.cs**: Data transfer objects for authentication
  - LoginDto, RegisterDto
  - AuthResponseDto
  - UserDto

## API Endpoints

### Authentication Endpoints

- **POST /api/auth/login**: Authenticate user and generate token
- **POST /api/auth/register**: Register new user
- **POST /api/auth/logout**: Invalidate user session
- **GET /api/auth/current-user**: Get current authenticated user
- **POST /api/auth/verify-email**: Verify user email
- **POST /api/auth/resend-verification**: Resend verification email
- **POST /api/auth/forgot-password**: Initiate password reset
- **POST /api/auth/reset-password**: Complete password reset
- **POST /api/auth/change-password**: Change user password

### User Management Endpoints

- **GET /api/users**: Get all users (Admin only)
- **GET /api/users/{id}**: Get user by ID
- **POST /api/users**: Create new user (Admin only)
- **PUT /api/users/{id}**: Update user
- **DELETE /api/users/{id}**: Delete user (Admin only)
- **POST /api/users/{id}/roles**: Assign role to user
- **DELETE /api/users/{id}/roles/{roleId}**: Remove role from user
- **GET /api/users/{id}/roles**: Get user roles
- **GET /api/users/{id}/has-permission**: Check if user has permission

## Code Workflow

### Request Processing Flow

1. **HTTP Request**: Client sends request to API endpoint
2. **Middleware Pipeline**: Request passes through configured middleware
   - Exception handling middleware catches errors
   - Authentication middleware validates JWT token
   - Authorization middleware checks user permissions
3. **Controller Action**: Appropriate controller method handles request
   - Validates input data
   - Calls service methods
4. **Service Layer**: Business logic is executed
   - Implements business rules
   - Interacts with database through DbContext
5. **Database Operations**: Data is retrieved or modified
6. **Response Generation**: Controller formats service result
7. **HTTP Response**: Response is sent back to client

### Authentication Workflow

1. **Registration**:
   - Client sends registration data
   - Server validates data
   - Creates user account (inactive)
   - Generates verification token
   - Sends verification email

2. **Email Verification**:
   - User clicks verification link
   - Server validates token
   - Activates user account

3. **Login**:
   - Client sends credentials
   - Server validates credentials
   - Generates JWT token with claims
   - Returns token to client

4. **Protected Resource Access**:
   - Client includes token in request header
   - Server validates token
   - Checks user permissions
   - Grants or denies access

## Getting Started

### Prerequisites

- .NET 6.0 SDK
- SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Running the Application

1. Clone the repository
2. Navigate to the project directory
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet ef database update` to create/update the database
5. Run `dotnet run` to start the application
6. Access Swagger UI at `https://localhost:7115/swagger` or `http://localhost:5114/swagger`

### Default Admin Account

- Email: `admin@gsc.com`
- Password: `Admin123!`

## Configuration

### Database Connection

The application uses SQL Server LocalDB by default. Connection string is configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GsCDatabase;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### JWT Authentication

JWT settings are configured in `appsettings.json`:

```json
"JwtSettings": {
  "SecretKey": "your-secret-key-here",
  "Issuer": "GsC.API",
  "Audience": "GsC.Client",
  "ExpirationInMinutes": 60
}
```

### Email Service

Email settings are configured in `appsettings.json`:

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password"
},
"EmailSettings": {
  "VerificationTokenExpirationHours": 24,
  "BaseUrl": "https://localhost:7115"
}
```

## Folder Structure Explanation

- **/Controllers**: Contains API controllers that handle HTTP requests
- **/Models**: Contains domain models and database entities
- **/DTOs**: Contains Data Transfer Objects for API communication
- **/Services**: Contains business logic implementation
- **/Interfaces**: Contains service interfaces
- **/Data**: Contains database context and configurations
- **/Migrations**: Contains EF Core database migrations
- **/Extensions**: Contains extension methods for service configuration
- **/Middleware**: Contains custom middleware components
- **/Helpers**: Contains utility classes and helper methods

## Security Considerations

- JWT tokens are signed with a secret key
- Passwords are hashed using ASP.NET Core Identity
- Email verification is required for new accounts
- Role-based and permission-based authorization
- HTTPS is enforced in production

## Conclusion

The GsC API provides a comprehensive backend solution for catering subcontracting management with robust authentication, authorization, and business functionality. The layered architecture ensures separation of concerns and maintainability, while the use of modern technologies like ASP.NET Core, Entity Framework Core, and JWT authentication provides security and scalability.