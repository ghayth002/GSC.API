# GsC API Endpoints Reference Guide

## Authentication Endpoints

| Endpoint | Method | Description | Auth Required | Request Body | Response |
|----------|--------|-------------|--------------|--------------|----------|
| `/api/auth/login` | POST | Authenticate user | No | `{ "email": "string", "password": "string" }` | JWT token and user info |
| `/api/auth/register` | POST | Register new user | No | `{ "email": "string", "password": "string", "confirmPassword": "string", "firstName": "string", "lastName": "string" }` | Success message |
| `/api/auth/logout` | POST | Logout user | Yes | None | Success message |
| `/api/auth/current-user` | GET | Get current user | Yes | None | User info |
| `/api/auth/verify-email` | POST | Verify email | No | `{ "email": "string", "token": "string" }` | Success message |
| `/api/auth/resend-verification` | POST | Resend verification email | No | `{ "email": "string" }` | Success message |
| `/api/auth/forgot-password` | POST | Request password reset | No | `{ "email": "string" }` | Success message |
| `/api/auth/reset-password` | POST | Reset password | No | `{ "email": "string", "token": "string", "newPassword": "string", "confirmPassword": "string" }` | Success message |
| `/api/auth/change-password` | POST | Change password | Yes | `{ "currentPassword": "string", "newPassword": "string", "confirmPassword": "string" }` | Success message |

## User Management Endpoints

| Endpoint | Method | Description | Auth Required | Roles | Request Body | Response |
|----------|--------|-------------|--------------|-------|--------------|----------|
| `/api/users` | GET | Get all users | Yes | Admin, Manager | None | Array of users |
| `/api/users/{id}` | GET | Get user by ID | Yes | Admin, Manager | None | User info |
| `/api/users` | POST | Create user | Yes | Admin | `{ "email": "string", "password": "string", "firstName": "string", "lastName": "string", "roles": ["string"] }` | Created user |
| `/api/users/{id}` | PUT | Update user | Yes | Admin | `{ "firstName": "string", "lastName": "string", "isActive": boolean }` | Updated user |
| `/api/users/{id}` | DELETE | Delete user | Yes | Admin | None | Success message |
| `/api/users/{id}/roles` | POST | Assign role | Yes | Admin | `{ "roleName": "string" }` | Success message |
| `/api/users/{id}/roles/{roleName}` | DELETE | Remove role | Yes | Admin | None | Success message |
| `/api/users/{id}/roles` | GET | Get user roles | Yes | Admin, Manager | None | Array of roles |
| `/api/users/{id}/has-permission` | GET | Check permission | Yes | Any | Query param: `permission` | Boolean result |

## Common Response Formats

### Success Response

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... } // Optional, depends on endpoint
}
```

### Error Response

```json
{
  "success": false,
  "message": "Error message",
  "errors": { ... } // Optional, validation errors
}
```

## Authentication

For protected endpoints, include the JWT token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Status Codes

- `200 OK`: Request succeeded
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request or validation error
- `401 Unauthorized`: Authentication required or failed
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Pagination

For endpoints that return collections, pagination is supported with query parameters:

- `pageNumber`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)

Example: `/api/users?pageNumber=2&pageSize=15`

## Filtering and Sorting

Some endpoints support filtering and sorting with query parameters:

- `search`: Search term
- `sortBy`: Field to sort by
- `sortDirection`: `asc` or `desc`

Example: `/api/users?search=john&sortBy=lastName&sortDirection=asc`

## Rate Limiting

API requests are rate-limited to prevent abuse. The limits are:

- 60 requests per minute for authenticated users
- 30 requests per minute for unauthenticated users

When the limit is exceeded, a `429 Too Many Requests` status code is returned.