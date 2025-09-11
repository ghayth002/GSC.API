# GsC API Database Schema

## Core Authentication Tables

### Users

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| Id                     | int              | Primary key                          |
| UserName               | nvarchar(256)    | Unique username                      |
| NormalizedUserName     | nvarchar(256)    | Uppercase username for indexing      |
| Email                  | nvarchar(256)    | User email address                   |
| NormalizedEmail        | nvarchar(256)    | Uppercase email for indexing         |
| EmailConfirmed         | bit              | Email verification status            |
| PasswordHash           | nvarchar(max)    | Hashed password                      |
| SecurityStamp          | nvarchar(max)    | Used for invalidating credentials    |
| ConcurrencyStamp       | nvarchar(max)    | Optimistic concurrency token         |
| PhoneNumber            | nvarchar(max)    | User phone number                    |
| PhoneNumberConfirmed   | bit              | Phone verification status            |
| TwoFactorEnabled      | bit              | Two-factor auth enabled status       |
| LockoutEnd             | datetimeoffset   | Account lockout expiration           |
| LockoutEnabled         | bit              | Account can be locked out            |
| AccessFailedCount      | int              | Failed login attempts                |
| FirstName              | nvarchar(100)    | User's first name                    |
| LastName               | nvarchar(100)    | User's last name                     |
| CreatedAt              | datetime2        | Account creation timestamp           |
| UpdatedAt              | datetime2        | Account update timestamp             |
| IsActive               | bit              | Account active status                |
| EmailVerificationToken | nvarchar(max)    | Token for email verification         |
| EmailVerificationTokenExpiry | datetime2 | Token expiration timestamp           |
| PasswordResetToken     | nvarchar(max)    | Token for password reset             |
| PasswordResetTokenExpiry | datetime2     | Token expiration timestamp           |
+------------------------+------------------+--------------------------------------+
```

### Roles

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| Id                     | int              | Primary key                          |
| Name                   | nvarchar(256)    | Role name                            |
| NormalizedName         | nvarchar(256)    | Uppercase name for indexing          |
| ConcurrencyStamp       | nvarchar(max)    | Optimistic concurrency token         |
| Description            | nvarchar(max)    | Role description                     |
+------------------------+------------------+--------------------------------------+
```

### UserRoles

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| UserId                 | int              | Foreign key to Users                 |
| RoleId                 | int              | Foreign key to Roles                 |
+------------------------+------------------+--------------------------------------+
```

### Permissions

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| Id                     | int              | Primary key                          |
| Name                   | nvarchar(100)    | Permission name                      |
| Module                 | nvarchar(100)    | Related module/feature               |
| Description            | nvarchar(max)    | Permission description               |
+------------------------+------------------+--------------------------------------+
```

### RolePermissions

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| RoleId                 | int              | Foreign key to Roles                 |
| PermissionId           | int              | Foreign key to Permissions           |
+------------------------+------------------+--------------------------------------+
```

## ASP.NET Identity Tables

### UserClaims

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| Id                     | int              | Primary key                          |
| UserId                 | int              | Foreign key to Users                 |
| ClaimType              | nvarchar(max)    | Type of claim                        |
| ClaimValue             | nvarchar(max)    | Value of claim                       |
+------------------------+------------------+--------------------------------------+
```

### UserLogins

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| LoginProvider          | nvarchar(450)    | External login provider              |
| ProviderKey            | nvarchar(450)    | Provider's key for the user          |
| ProviderDisplayName    | nvarchar(max)    | Display name for the provider        |
| UserId                 | int              | Foreign key to Users                 |
+------------------------+------------------+--------------------------------------+
```

### UserTokens

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| UserId                 | int              | Foreign key to Users                 |
| LoginProvider          | nvarchar(450)    | Provider that generated the token    |
| Name                   | nvarchar(450)    | Token name/purpose                   |
| Value                  | nvarchar(max)    | Token value                          |
+------------------------+------------------+--------------------------------------+
```

### RoleClaims

```
+------------------------+------------------+--------------------------------------+
| Column                 | Type             | Description                          |
+------------------------+------------------+--------------------------------------+
| Id                     | int              | Primary key                          |
| RoleId                 | int              | Foreign key to Roles                 |
| ClaimType              | nvarchar(max)    | Type of claim                        |
| ClaimValue             | nvarchar(max)    | Value of claim                       |
+------------------------+------------------+--------------------------------------+
```

## Entity Relationships

### User Relationships

- **User → UserRoles**: One-to-many relationship
  - A user can have multiple roles
  - Cascade delete: When a user is deleted, all associated UserRoles are deleted

- **User → UserClaims**: One-to-many relationship
  - A user can have multiple claims
  - Cascade delete: When a user is deleted, all associated UserClaims are deleted

- **User → UserLogins**: One-to-many relationship
  - A user can have multiple external logins
  - Cascade delete: When a user is deleted, all associated UserLogins are deleted

- **User → UserTokens**: One-to-many relationship
  - A user can have multiple tokens
  - Cascade delete: When a user is deleted, all associated UserTokens are deleted

### Role Relationships

- **Role → UserRoles**: One-to-many relationship
  - A role can be assigned to multiple users
  - Cascade delete: When a role is deleted, all associated UserRoles are deleted

- **Role → RoleClaims**: One-to-many relationship
  - A role can have multiple claims
  - Cascade delete: When a role is deleted, all associated RoleClaims are deleted

- **Role → RolePermissions**: One-to-many relationship
  - A role can have multiple permissions
  - Cascade delete: When a role is deleted, all associated RolePermissions are deleted

### Permission Relationships

- **Permission → RolePermissions**: One-to-many relationship
  - A permission can be assigned to multiple roles
  - Cascade delete: When a permission is deleted, all associated RolePermissions are deleted

## Database Indexes

- **Users.NormalizedUserName**: Unique index for fast username lookups
- **Users.NormalizedEmail**: Index for fast email lookups
- **Roles.NormalizedName**: Unique index for fast role name lookups
- **UserRoles**: Composite index on (UserId, RoleId) for fast role lookups
- **RolePermissions**: Composite index on (RoleId, PermissionId) for fast permission lookups

## Default Data

### Default Roles

- **Administrator**: Full system access
- **Manager**: User management access
- **User**: Basic user access
- **Viewer**: Read-only access

### Default Permissions

- **Users.Create**: Create users
- **Users.Read**: View users
- **Users.Update**: Update users
- **Users.Delete**: Delete users
- **Roles.Manage**: Manage roles

### Default Admin User

- **Email**: admin@gsc.com
- **Password**: Admin123!
- **Roles**: Administrator

## Database Migrations

The database schema is managed through Entity Framework Core migrations. To update the database schema:

```
dotnet ef migrations add MigrationName
dotnet ef database update
```

This will apply any pending migrations to the database.