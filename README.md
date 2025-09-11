# GsC API - Catering Subcontracting Management System

## Overview

GsC API is a comprehensive .NET Core Web API project designed for catering subcontracting management. It provides a robust backend system with user authentication, role-based authorization, and various business functionalities.

## Documentation Index

This project includes several documentation files to help you understand its structure and functionality:

1. **[README-DETAILED.md](./README-DETAILED.md)** - Comprehensive project documentation with detailed explanations of architecture, components, and workflows
2. **[ARCHITECTURE-DIAGRAM.md](./ARCHITECTURE-DIAGRAM.md)** - Visual representation of the system architecture and relationships
3. **[CODE-MAP.md](./CODE-MAP.md)** - Detailed mapping of code components and their relationships
4. **[DEVELOPER-GUIDE.md](./DEVELOPER-GUIDE.md)** - Quick start guide for developers working with the project

## Technology Stack

- **Framework**: ASP.NET Core 6.0
- **Database**: Microsoft SQL Server (LocalDB)
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Documentation**: Swagger/OpenAPI
- **Email Service**: SMTP (configured for Gmail)

## Key Features

- **User Authentication**: JWT-based authentication system
- **Role-Based Authorization**: Predefined roles with specific permissions
- **User Management**: CRUD operations for user accounts
- **Email Verification**: Email verification for new accounts
- **Password Management**: Forgot password and reset functionality
- **API Documentation**: Swagger UI for API exploration

## Project Structure

The project follows a layered architecture pattern:

- **Presentation Layer**: API Controllers
- **Business Logic Layer**: Services
- **Data Access Layer**: DbContext and Repositories
- **Domain Layer**: Models

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

## API Endpoints

The API provides endpoints for:

- Authentication (login, register, logout)
- User management (CRUD operations)
- Role management (assign, remove roles)
- Permission checking

For a complete list of endpoints, refer to the Swagger documentation when running the application.

## Configuration

The application is configured through `appsettings.json`, which includes:

- Database connection string
- JWT authentication settings
- SMTP settings for email service
- Logging configuration

## Security

- JWT tokens are signed with a secret key
- Passwords are hashed using ASP.NET Core Identity
- Email verification is required for new accounts
- Role-based and permission-based authorization
- HTTPS is enforced in production

## For More Information

Please refer to the detailed documentation files listed in the Documentation Index section for in-depth information about the project architecture, code structure, and workflows.
