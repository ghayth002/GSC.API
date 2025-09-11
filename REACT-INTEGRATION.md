# React Integration Examples

## 🚀 Quick Setup for React Frontend

### 1. Install Dependencies

```bash
npm install axios react-router-dom
# or
npm install fetch
```

### 2. Environment Variables (.env)

```env
REACT_APP_API_BASE_URL=http://localhost:5114
```

---

## 🔧 API Service Setup

### Complete API Service (apiService.js)

```javascript
const API_BASE_URL =
  process.env.REACT_APP_API_BASE_URL || "http://localhost:5114";

class ApiService {
  constructor() {
    this.baseURL = API_BASE_URL;
  }

  // Get auth token from localStorage
  getAuthHeader() {
    const token = localStorage.getItem("token");
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  // Generic request handler
  async request(endpoint, options = {}) {
    const url = `${this.baseURL}${endpoint}`;
    const config = {
      headers: {
        "Content-Type": "application/json",
        ...this.getAuthHeader(),
        ...options.headers,
      },
      ...options,
    };

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(
          errorData.message || `HTTP error! status: ${response.status}`
        );
      }

      return await response.json();
    } catch (error) {
      console.error("API Request failed:", error);
      throw error;
    }
  }

  // Authentication methods
  async login(email, password, rememberMe = false) {
    const data = await this.request("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password, rememberMe }),
    });

    if (data.success) {
      localStorage.setItem("token", data.token);
      localStorage.setItem("user", JSON.stringify(data.user));
    }

    return data;
  }

  async register(userData) {
    return this.request("/api/auth/register", {
      method: "POST",
      body: JSON.stringify(userData),
    });
  }

  async getCurrentUser() {
    return this.request("/api/auth/me");
  }

  async changePassword(passwordData) {
    return this.request("/api/auth/change-password", {
      method: "POST",
      body: JSON.stringify(passwordData),
    });
  }

  logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    // Optional: call backend logout
    this.request("/api/auth/logout", { method: "POST" }).catch(() => {});
  }

  // User management methods
  async getUsers() {
    return this.request("/api/users");
  }

  async getUserById(id) {
    return this.request(`/api/users/${id}`);
  }

  async createUser(userData) {
    return this.request("/api/users", {
      method: "POST",
      body: JSON.stringify(userData),
    });
  }

  async updateUser(id, userData) {
    return this.request(`/api/users/${id}`, {
      method: "PUT",
      body: JSON.stringify(userData),
    });
  }

  async deleteUser(id) {
    return this.request(`/api/users/${id}`, {
      method: "DELETE",
    });
  }

  async assignRole(userId, roleName) {
    return this.request(`/api/users/${userId}/assign-role`, {
      method: "POST",
      body: JSON.stringify({ roleName }),
    });
  }

  async removeRole(userId, roleName) {
    return this.request(`/api/users/${userId}/remove-role`, {
      method: "POST",
      body: JSON.stringify({ roleName }),
    });
  }

  async getUserRoles(userId) {
    return this.request(`/api/users/${userId}/roles`);
  }

  async checkPermission(userId, permission) {
    return this.request(`/api/users/${userId}/permissions/${permission}`);
  }
}

export default new ApiService();
```

---

## 🔐 Authentication Context

### AuthContext.js

```javascript
import React, { createContext, useContext, useReducer, useEffect } from "react";
import apiService from "./services/apiService";

const AuthContext = createContext();

const initialState = {
  isAuthenticated: false,
  user: null,
  token: null,
  loading: true,
};

const authReducer = (state, action) => {
  switch (action.type) {
    case "LOGIN_SUCCESS":
      return {
        ...state,
        isAuthenticated: true,
        user: action.payload.user,
        token: action.payload.token,
        loading: false,
      };
    case "LOGOUT":
      return {
        ...state,
        isAuthenticated: false,
        user: null,
        token: null,
        loading: false,
      };
    case "SET_LOADING":
      return {
        ...state,
        loading: action.payload,
      };
    case "UPDATE_USER":
      return {
        ...state,
        user: { ...state.user, ...action.payload },
      };
    default:
      return state;
  }
};

export const AuthProvider = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  useEffect(() => {
    const initializeAuth = async () => {
      const token = localStorage.getItem("token");
      const userData = localStorage.getItem("user");

      if (token && userData) {
        try {
          // Verify token is still valid
          const currentUser = await apiService.getCurrentUser();
          dispatch({
            type: "LOGIN_SUCCESS",
            payload: {
              token,
              user: currentUser,
            },
          });
        } catch (error) {
          // Token invalid, clear storage
          localStorage.removeItem("token");
          localStorage.removeItem("user");
          dispatch({ type: "SET_LOADING", payload: false });
        }
      } else {
        dispatch({ type: "SET_LOADING", payload: false });
      }
    };

    initializeAuth();
  }, []);

  const login = async (email, password, rememberMe = false) => {
    try {
      dispatch({ type: "SET_LOADING", payload: true });
      const data = await apiService.login(email, password, rememberMe);

      if (data.success) {
        dispatch({
          type: "LOGIN_SUCCESS",
          payload: {
            token: data.token,
            user: data.user,
          },
        });
        return { success: true };
      }

      return { success: false, message: data.message };
    } catch (error) {
      dispatch({ type: "SET_LOADING", payload: false });
      return { success: false, message: error.message };
    }
  };

  const logout = () => {
    apiService.logout();
    dispatch({ type: "LOGOUT" });
  };

  const updateUser = (userData) => {
    dispatch({ type: "UPDATE_USER", payload: userData });
  };

  const hasRole = (role) => {
    return state.user?.roles?.includes(role) || false;
  };

  const isAdmin = () => hasRole("Administrator");
  const isManager = () => hasRole("Manager") || isAdmin();

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login,
        logout,
        updateUser,
        hasRole,
        isAdmin,
        isManager,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
```

---

## 🧩 React Components Examples

### Login Component

```javascript
import React, { useState } from "react";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate } from "react-router-dom";

const Login = () => {
  const [formData, setFormData] = useState({
    email: "",
    password: "",
    rememberMe: false,
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    const result = await login(
      formData.email,
      formData.password,
      formData.rememberMe
    );

    if (result.success) {
      navigate("/dashboard");
    } else {
      setError(result.message);
    }

    setLoading(false);
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  return (
    <div className="login-container">
      <form onSubmit={handleSubmit}>
        <h2>Login to GsC</h2>

        {error && <div className="error">{error}</div>}

        <div>
          <label>Email:</label>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            required
          />
        </div>

        <div>
          <label>Password:</label>
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            required
          />
        </div>

        <div>
          <label>
            <input
              type="checkbox"
              name="rememberMe"
              checked={formData.rememberMe}
              onChange={handleChange}
            />
            Remember Me
          </label>
        </div>

        <button type="submit" disabled={loading}>
          {loading ? "Logging in..." : "Login"}
        </button>
      </form>
    </div>
  );
};

export default Login;
```

### Users List Component

```javascript
import React, { useState, useEffect } from "react";
import { useAuth } from "../contexts/AuthContext";
import apiService from "../services/apiService";

const UsersList = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const { isManager } = useAuth();

  useEffect(() => {
    if (!isManager()) {
      setError("You do not have permission to view users");
      setLoading(false);
      return;
    }

    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      const data = await apiService.getUsers();
      setUsers(data);
    } catch (error) {
      setError(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteUser = async (userId) => {
    if (window.confirm("Are you sure you want to delete this user?")) {
      try {
        await apiService.deleteUser(userId);
        setUsers(users.filter((user) => user.id !== userId));
      } catch (error) {
        setError(error.message);
      }
    }
  };

  if (loading) return <div>Loading users...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="users-list">
      <h2>Users Management</h2>

      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Username</th>
            <th>Email</th>
            <th>Name</th>
            <th>Roles</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>{user.id}</td>
              <td>{user.userName}</td>
              <td>{user.email}</td>
              <td>
                {user.firstName} {user.lastName}
              </td>
              <td>{user.roles.join(", ")}</td>
              <td>{user.isActive ? "Active" : "Inactive"}</td>
              <td>
                <button onClick={() => handleDeleteUser(user.id)}>
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default UsersList;
```

### Protected Route Component

```javascript
import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

const ProtectedRoute = ({ children, requiredRole = null }) => {
  const { isAuthenticated, hasRole, loading } = useAuth();

  if (loading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && !hasRole(requiredRole)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return children;
};

export default ProtectedRoute;
```

---

## 🎯 App.js Setup Example

```javascript
import React from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Login from "./components/Login";
import Dashboard from "./components/Dashboard";
import UsersList from "./components/UsersList";
import Unauthorized from "./components/Unauthorized";

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="App">
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/unauthorized" element={<Unauthorized />} />

            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <Dashboard />
                </ProtectedRoute>
              }
            />

            <Route
              path="/users"
              element={
                <ProtectedRoute requiredRole="Manager">
                  <UsersList />
                </ProtectedRoute>
              }
            />

            <Route path="/" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
```

---

## 🔧 Custom Hooks

### useApi Hook

```javascript
import { useState, useEffect } from "react";
import apiService from "../services/apiService";

export const useApi = (apiCall, dependencies = []) => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await apiCall();
        setData(result);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, dependencies);

  return { data, loading, error, refetch: () => fetchData() };
};

// Usage example:
// const { data: users, loading, error } = useApi(() => apiService.getUsers());
```

---

## 🎨 TypeScript Interfaces (Optional)

```typescript
// types/api.ts
export interface User {
  id: number;
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
  isActive: boolean;
  createdAt: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  expirationDate?: string;
  user?: User;
}

export interface CreateUserRequest {
  userName: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  roles: string[];
}
```

This setup provides everything your React frontend team needs to integrate with the GsC API! 🚀
