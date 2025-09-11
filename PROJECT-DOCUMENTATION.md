# GsC API - Detailed Project Documentation

## 📋 Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Database Design](#database-design)
5. [Authentication & Authorization](#authentication--authorization)
6. [API Endpoints](#api-endpoints)
7. [Workflow](#workflow)
8. [File & Folder Explanations](#file--folder-explanations)

## 🎯 Project Overview

**GsC (Gestion de la Sous-traitance du Catering)** is a comprehensive catering subcontractor management system designed for airlines. The backend API provides complete user management functionality with JWT authentication, role-based authorization, and permission management.

The system is built to manage various aspects of airline catering including:
- User management with role-based access control
- Flight information management
- Menu and food item management
- Order processing and tracking
- Medical box management for flights
- Delivery note processing

## 🏗️ Architecture

The project follows a clean architecture pattern with separation of concerns:

### Layers

1. **Presentation Layer** - Controllers
   - Handles HTTP requests and responses
   - Performs input validation
   - Routes requests to appropriate services

2. **Business Logic Layer** - Services
   - Implements business rules and logic
   - Coordinates data access
   - Handles authentication and authorization

3. **Data Access Layer** - DbContext and Models
   - Manages database interactions
   - Defines entity relationships
   - Handles data persistence

### Technologies

- **Framework**: .NET 8 Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens with ASP.NET Core Identity
- **Authorization**: Role-based access control (RBAC) with permissions
- **Documentation**: Swagger/OpenAPI
- **Email**: SMTP-based email service for notifications

## 📁 Project Structure

```
GsC.API/
├── Controllers/           # API endpoints
├── DTOs/                  # Data Transfer Objects
├── Data/                  # Database context and configurations
├── Enums/                 # Enumeration types
├── Migrations/            # EF Core database migrations
├── Models/                # Domain entities
├── Services/              # Business logic implementation
├── Properties/            # Project properties and launch settings
├── Program.cs             # Application entry point and configuration
├── appsettings.json       # Application settings
└── README.md              # Project documentation
```

## 🗄️ Database Design

The database uses Entity Framework Core with SQL Server and includes the following main entities:

### Core Entities

1. **User** - Extends IdentityUser for authentication
   - Custom fields: FirstName, LastName, IsActive, etc.
   - Email verification and password reset functionality

2. **Role** - Extends IdentityRole for authorization
   - Custom fields: Description
   - Many-to-many relationship with Users through UserRoles

3. **Permission** - Granular access control
   - Fields: Name, Description, Module
   - Many-to-many relationship with Roles through RolePermissions

### Business Entities

1. **Flight** - Flight information
2. **Menu** - Food menus for flights
3. **MenuItem** - Individual food items
4. **Order** - Customer orders
5. **OrderLine** - Line items in orders
6. **MedicalBox** - Medical supplies for flights
7. **DeliveryNote** - Delivery documentation

## 🔐 Authentication & Authorization

### Authentication Flow

1. **Registration**
   - User registers with email, password, and personal details
   - System sends verification email
   - User verifies email to activate account

2. **Login**
   - User provides email and password
   - System validates credentials and issues JWT token
   - Token contains user identity and role claims

3. **Token Usage**
   - Client includes token in Authorization header
   - Server validates token for each protected request

### Authorization System

1. **Roles**
   - Administrator: Full system access
   - Manager: User management access
   - User: Basic user access
   - Viewer: Read-only access

2. **Permissions**
   - Fine-grained access control
   - Examples: Users.Create, Users.Read, Users.Update, Users.Delete
   - Assigned to roles, which are assigned to users

## 📡 API Endpoints

### Authentication Endpoints

- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/auth/me` - Get current user info
- `POST /api/auth/change-password` - Change password
- `POST /api/auth/logout` - User logout
- `GET /api/auth/verify-email` - Verify email address
- `POST /api/auth/resend-verification` - Resend verification email
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token

### User Management Endpoints

- `GET /api/users` - Get all users (Admin/Manager)
- `GET /api/users/{id}` - Get user by ID (Admin/Manager)
- `POST /api/users` - Create user (Admin)
- `PUT /api/users/{id}` - Update user (Admin)
- `DELETE /api/users/{id}` - Delete user (Admin)
- `POST /api/users/{userId}/assign-role` - Assign role to user (Admin)
- `POST /api/users/{userId}/remove-role` - Remove role from user (Admin)
- `GET /api/users/{userId}/roles` - Get user roles (Admin/Manager)
- `GET /api/users/{userId}/permissions/{permission}` - Check user permission

## 🔄 Workflow

### User Registration and Authentication

1. User registers through the `/api/auth/register` endpoint
2. System creates user account and sends verification email
3. User verifies email through the verification link
4. User logs in through the `/api/auth/login` endpoint
5. System issues JWT token for authenticated requests

### User Management

1. Admin creates users with specific roles
2. Admin assigns permissions to roles
3. Users access system based on their roles and permissions

## 📂 File & Folder Explanations

### Controllers

- **AuthController.cs** - Handles authentication operations (login, register, etc.)
- **UsersController.cs** - Manages user operations (CRUD, role assignment)

### DTOs (Data Transfer Objects)

- **AuthDTOs.cs** - Objects for authentication operations
- **FlightDTOs.cs** - Objects for flight operations
- **MenuDTOs.cs** - Objects for menu operations
- **OrderDTOs.cs** - Objects for order operations

### Data

- **ApplicationDbContext.cs** - EF Core database context with entity configurations

### Enums

- **DeliveryNoteStatus.cs** - Status values for delivery notes
- **MedicalBoxType.cs** - Types of medical boxes
- **MenuClass.cs** - Classes of menus (Economy, Business, etc.)
- **OrderStatus.cs** - Status values for orders
- **OrderType.cs** - Types of orders

### Models

- **User.cs** - User entity extending IdentityUser
- **Role.cs** - Role entity extending IdentityRole
- **Permission.cs** - Permission entity for access control
- **Flight.cs** - Flight information
- **Menu.cs** - Menu information
- **MenuItem.cs** - Menu item information
- **Order.cs** - Order information
- **OrderLine.cs** - Order line item information
- **MedicalBox.cs** - Medical box information
- **DeliveryNote.cs** - Delivery note information

### Services

- **AuthService.cs** - Authentication and user management logic
- **EmailService.cs** - Email sending functionality
- **IAuthService.cs** - Interface for authentication service
- **IEmailService.cs** - Interface for email service
- **IFlightService.cs** - Interface for flight service
- **IMedicalBoxService.cs** - Interface for medical box service
- **IMenuService.cs** - Interface for menu service
- **IOrderService.cs** - Interface for order service

### Configuration Files

- **Program.cs** - Application startup and configuration
- **appsettings.json** - Application settings (connection strings, JWT, email)
- **launchSettings.json** - Development environment settings

### Documentation

- **README.md** - Project overview and setup instructions
- **API-REFERENCE.md** - Detailed API documentation
- **REACT-INTEGRATION.md** - Guide for frontend integration

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Running the Application

1. Clone the repository
2. Update the connection string in `appsettings.json` if needed
3. Run the application using `dotnet run` or through Visual Studio
4. Access the Swagger documentation at `https://localhost:7115/swagger`

### Default Admin Account

- Email: admin@gsc.com
- Password: Admin123!

## 🔧 Configuration

### Database Connection

The application uses SQL Server LocalDB by default. The connection string is defined in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GsCDatabase;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

### JWT Authentication

JWT settings are configured in `appsettings.json`:

```json
"JwtSettings": {
  "SecretKey": "GsC-Super-Secret-Key-For-Development-Only-Change-In-Production-2024-User-Management",
  "Issuer": "GsC.API",
  "Audience": "GsC.Client",
  "ExpirationInMinutes": 60
}
```

### Email Settings

Email configuration for sending verification emails and password resets:

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "GsC Catering Management"
}
```