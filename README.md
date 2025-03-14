# OAuth Sample Application

## Project Overview

This is a demonstration ASP.NET Core application showcasing modern authentication and authorization practices using JWT (JSON Web Tokens) and HttpOnly cookies. The application is built with .NET 8 and implements a token-based authentication system with refresh tokens to provide secure, persistent sessions.

## Key Features

- **Secure JWT Authentication**: Using short-lived JWT tokens and long-lived refresh tokens
- **HttpOnly Cookies**: Storing tokens securely in HttpOnly cookies to prevent XSS attacks
- **Session Management**: Admin dashboard for viewing and revoking user sessions
- **Automatic Token Refresh**: Client-side handling of token refresh for seamless user experience
- **Swagger Integration**: API documentation with Swagger UI including JWT authentication

## Architecture

### Application Structure

The application follows a layered architecture:

- **Controllers**: Handle API requests and responses
  - `AuthController`: Manages login, logout, token refresh, and session management
  - `AuthStatusController`: Provides authentication status information
  - `WeatherForecastController`: Demo controller showing protected data

- **Services**: Implement business logic
  - `AuthService`: Handles authentication operations (scoped per request)
  - `TokenRepository`: Manages refresh token storage (singleton)

- **Models**: Define data structures
  - `User`: User credential model
  - `RefreshToken`: Represents a refresh token with session information
  - `JwtConfig`: Configuration settings for JWT

- **Frontend**: Simple HTML/JavaScript UI
  - Login page with authentication
  - Dashboard with protected data display
  - Admin page for session management

### Authentication Flow

1. User logs in with credentials
2. Server validates credentials and issues:
   - JWT token (short-lived, 15 minutes)
   - Refresh token (long-lived, 7 days)
3. Both tokens are stored in HttpOnly cookies
4. Client accesses protected resources using JWT
5. When JWT expires, client automatically requests token refresh
6. Refresh token is validated and a new JWT is issued

### Session Management

- Each refresh token has a unique session ID
- Admin users can view all active sessions
- Sessions can be individually revoked or all revoked at once

### Technical Implementation Details

- **Token Storage**: The application uses a singleton `TokenRepository` to simulate persistent storage of refresh tokens
- **Service Separation**: Authentication logic is contained in a scoped `AuthService` while token storage is handled by a singleton repository
- **JWT Configuration**: JWT settings are externalized in appsettings.json

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- A modern web browser

### Running the Application

1. Clone the repository
2. Navigate to the project directory
3. Run the application:
   ```
   dotnet run --urls=http://localhost:5075
   ```
4. Open a browser and navigate to `http://localhost:5075`

### Default Credentials

- Username: `admin`
- Password: `password123`

## Using the Application

1. **Login**: Navigate to the homepage and enter credentials
2. **Dashboard**: After login, you'll be redirected to the dashboard showing weather data
3. **Session Management**: Click "Manage Sessions" to view and manage active sessions
4. **Logout**: Click the "Logout" button to end your session

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/login` | POST | Authenticate user and issue tokens |
| `/api/auth/refresh-token` | POST | Refresh an expired JWT |
| `/api/auth/logout` | POST | Logout and revoke current token |
| `/api/auth/sessions` | GET | Get all active sessions for current user |
| `/api/auth/revoke-session/{sessionId}` | POST | Revoke a specific session |
| `/api/auth/revoke-all-sessions` | POST | Revoke all sessions for current user |
| `/api/authstatus/check` | GET | Check current authentication status |
| `/api/weatherforecast` | GET | Get sample protected data |

## Security Considerations

- JWT tokens are short-lived to minimize risk if compromised
- All cookies are HttpOnly, Secure, and use SameSite=Strict
- Refresh tokens can be revoked at any time
- Each device/browser creates a unique session
- Session management provides control over active sessions

## Project Enhancements

The project was enhanced with the following improvements:

1. **Separation of Concerns**:
   - Created a `TokenRepository` to separate token storage from authentication logic
   - Made `TokenRepository` a singleton for persistent token storage
   - Kept `AuthService` as scoped (per-request) for better resource management

2. **User Experience**:
   - Added shared authentication JavaScript for consistent behavior
   - Implemented automatic token refresh for seamless experience
   - Created an admin dashboard for session management

3. **Security**:
   - Implemented proper session identification
   - Added session revocation capabilities
   - Improved authentication status checking 