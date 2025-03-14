// Helper function to fetch with token refresh
async function fetchWithTokenRefresh(url, options = {}) {
    console.log(`Making request to ${url}`);
    
    // First attempt
    const response = await fetch(url, options);
    
    // If unauthorized, try to refresh the token
    if (response.status === 401) {
        console.log('Received 401 Unauthorized, attempting token refresh');
        try {
            // Attempt to refresh the token
            const refreshResponse = await fetch('/api/auth/refresh-token', {
                method: 'POST',
            });
            
            // If refresh successful, retry the original request
            if (refreshResponse.ok) {
                console.log('Token refreshed successfully, retrying original request');
                return fetch(url, options);
            } else {
                // If refresh failed, redirect to login
                console.error('Token refresh failed');
                window.location.href = '/';
                throw new Error('Authentication failed');
            }
        } catch (error) {
            console.error('Error refreshing token:', error);
            window.location.href = '/';
            throw error;
        }
    }
    
    // Return the original response if not 401 or after successful refresh
    if (!response.ok) {
        const errorText = await response.text();
        console.error(`Request failed with status ${response.status}: ${errorText}`);
        throw new Error(`Request failed with status ${response.status}`);
    }
    
    return response;
}

// Check if user is authenticated
async function checkAuth() {
    try {
        console.log('Checking authentication status');
        const response = await fetch('/api/AuthStatus/check');
        const data = await response.json();
        console.log('Auth status response:', data);
        
        if (!data.isAuthenticated) {
            console.log('Not authenticated, redirecting to login page');
            window.location.href = '/';
            return false;
        }
        
        return true;
    } catch (error) {
        console.error('Authentication check failed:', error);
        window.location.href = '/';
        return false;
    }
}

// Handle logout
async function logout() {
    try {
        console.log('Logging out...');
        const response = await fetch('/api/auth/logout', {
            method: 'POST'
        });
        
        if (response.ok) {
            console.log('Logout successful');
            window.location.href = '/';
        } else {
            console.error('Logout failed');
        }
    } catch (error) {
        console.error('Logout failed:', error);
    }
} 