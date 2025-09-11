# GsC API - Quick Reference

## 🔗 Base URL

```
http://localhost:5114
```

## 🔑 Authentication

```
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

---

## 📡 All API Endpoints

### **Authentication**

| Method | Endpoint                        | Auth | Description               |
| ------ | ------------------------------- | ---- | ------------------------- |
| `POST` | `/api/auth/login`               | ❌   | User login                |
| `POST` | `/api/auth/register`            | ❌   | User registration         |
| `POST` | `/api/auth/logout`              | ✅   | User logout               |
| `GET`  | `/api/auth/me`                  | ✅   | Get current user          |
| `POST` | `/api/auth/change-password`     | ✅   | Change password           |
| `GET`  | `/api/auth/verify-email`        | ❌   | Verify email address      |
| `POST` | `/api/auth/resend-verification` | ❌   | Resend verification email |
| `POST` | `/api/auth/forgot-password`     | ❌   | Request password reset    |
| `POST` | `/api/auth/reset-password`      | ❌   | Reset password            |

### **User Management**

| Method   | Endpoint                                   | Auth | Role          | Description      |
| -------- | ------------------------------------------ | ---- | ------------- | ---------------- |
| `GET`    | `/api/users`                               | ✅   | Admin/Manager | Get all users    |
| `GET`    | `/api/users/{id}`                          | ✅   | Admin/Manager | Get user by ID   |
| `POST`   | `/api/users`                               | ✅   | Admin         | Create user      |
| `PUT`    | `/api/users/{id}`                          | ✅   | Admin         | Update user      |
| `DELETE` | `/api/users/{id}`                          | ✅   | Admin         | Delete user      |
| `POST`   | `/api/users/{id}/assign-role`              | ✅   | Admin         | Assign role      |
| `POST`   | `/api/users/{id}/remove-role`              | ✅   | Admin         | Remove role      |
| `GET`    | `/api/users/{id}/roles`                    | ✅   | Admin/Manager | Get user roles   |
| `GET`    | `/api/users/{id}/permissions/{permission}` | ✅   | Any           | Check permission |

### **Test**

| Method | Endpoint           | Auth | Description   |
| ------ | ------------------ | ---- | ------------- |
| `GET`  | `/weatherforecast` | ❌   | Test endpoint |

---

## 🎯 Quick Start

### 1. Login

```javascript
POST /api/auth/login
{
  "email": "admin@gsc.com",
  "password": "Admin123!",
  "rememberMe": false
}
```

### 2. Get Users

```javascript
GET /api/users
Authorization: Bearer YOUR_TOKEN
```

### 3. Create User

```javascript
POST /api/users
Authorization: Bearer YOUR_TOKEN
{
  "userName": "john.doe",
  "email": "john@example.com",
  "password": "User123!",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["User"]
}
```

---

## 🏷️ Available Roles

- `Administrator` - Full access
- `Manager` - User management
- `User` - Basic access
- `Viewer` - Read-only

## 🔐 Default Admin

```
Email: admin@gsc.com
Password: Admin123!
```

## 📖 Full Documentation

See `README.md` for complete documentation with examples and React integration guide.
