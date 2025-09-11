# Google Authentication Implementation Guide

## Overview

This guide explains how to set up and use Google Authentication in the GsC.API backend and integrate it with a React frontend application.

## Backend Implementation

### Prerequisites

1. Google Cloud Platform account
2. Registered application in Google Cloud Console
3. OAuth 2.0 Client ID and Client Secret

### Configuration Steps

#### 1. Google Cloud Console Setup

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to "APIs & Services" > "Credentials"
4. Click "Create Credentials" > "OAuth client ID"
5. Select "Web application" as the application type
6. Add authorized JavaScript origins (e.g., `http://localhost:3000` for development)
7. Add authorized redirect URIs (e.g., `https://your-api-domain/api/auth/google-callback`)
8. Note down the Client ID and Client Secret

#### 2. Backend Configuration

1. Update `appsettings.json` with your Google credentials:

```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

2. Add a `FrontendUrl` setting to your configuration (for redirect after authentication):

```json
"FrontendUrl": "http://localhost:3000"
```

### Authentication Flow

The backend supports two methods for Google authentication:

#### Server-Side Flow (Cookie-Based)

1. User is redirected to `/api/auth/google-login`
2. After successful Google authentication, user is redirected to `/api/auth/google-response`
3. The backend creates or updates the user account and generates a JWT token
4. User is redirected to the frontend with the token

#### Client-Side Flow (Token-Based)

1. Frontend obtains Google ID token using Google Sign-In button
2. Frontend sends the token to `/api/auth/google-token`
3. Backend validates the token, creates or updates the user account, and returns a JWT token

## Frontend Integration (React)

### Prerequisites

1. React application
2. `@react-oauth/google` package for Google Sign-In button

### Installation

```bash
npm install @react-oauth/google
```

### Implementation

#### 1. Set up Google OAuth Provider

In your main application file (e.g., `index.js` or `App.js`):

```jsx
import { GoogleOAuthProvider } from '@react-oauth/google';

function App() {
  return (
    <GoogleOAuthProvider clientId="YOUR_GOOGLE_CLIENT_ID">
      {/* Your app components */}
    </GoogleOAuthProvider>
  );
}
```

#### 2. Implement Google Sign-In Button

```jsx
import { GoogleLogin } from '@react-oauth/google';
import axios from 'axios';

function LoginPage() {
  const handleGoogleSuccess = async (credentialResponse) => {
    try {
      // Send the ID token to your backend
      const response = await axios.post('https://your-api-domain/api/auth/google-token', {
        idToken: credentialResponse.credential
      });
      
      // Store the JWT token
      localStorage.setItem('token', response.data.token);
      
      // Redirect or update state
      // ...
    } catch (error) {
      console.error('Google authentication failed:', error);
    }
  };

  return (
    <div>
      <h2>Login</h2>
      <GoogleLogin
        onSuccess={handleGoogleSuccess}
        onError={() => console.log('Login Failed')}
        useOneTap
      />
      {/* Other login options */}
    </div>
  );
}
```

#### 3. Implement Auth Callback Handler

Create a component to handle the redirect from the server-side flow:

```jsx
import { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';

function AuthCallback() {
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = params.get('token');
    
    if (token) {
      // Store the JWT token
      localStorage.setItem('token', token);
      
      // Redirect to home or dashboard
      navigate('/');
    } else {
      // Handle error
      navigate('/login?error=authentication_failed');
    }
  }, [location, navigate]);

  return <div>Processing authentication...</div>;
}
```

#### 4. Add the Callback Route

```jsx
import { Routes, Route } from 'react-router-dom';
import AuthCallback from './components/AuthCallback';

function App() {
  return (
    <Routes>
      {/* Other routes */}
      <Route path="/auth/callback" element={<AuthCallback />} />
    </Routes>
  );
}
```

## Testing

1. Start your backend API
2. Start your React frontend
3. Navigate to the login page
4. Click the Google Sign-In button
5. Complete the Google authentication process
6. Verify that you are redirected back to your application with a valid JWT token

## Troubleshooting

### Common Issues

1. **Invalid Client ID**: Ensure your Client ID is correctly configured in both the backend and frontend
2. **Redirect URI Mismatch**: Verify that the redirect URI in Google Cloud Console matches your backend endpoint
3. **CORS Issues**: Check that your API allows requests from your frontend domain
4. **Token Validation Errors**: Ensure the backend can validate the Google ID token

### Debugging

- Check the browser console for frontend errors
- Review the backend logs for authentication issues
- Verify network requests in the browser developer tools

## Security Considerations

1. Always use HTTPS in production
2. Validate tokens on the server side
3. Implement proper CSRF protection
4. Set appropriate token expiration times
5. Use secure cookie settings

## Additional Resources

- [Google Identity Documentation](https://developers.google.com/identity)
- [ASP.NET Core Authentication Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication)
- [@react-oauth/google Documentation](https://github.com/MomenSherif/react-oauth)