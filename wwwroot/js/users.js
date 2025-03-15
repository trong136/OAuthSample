document.addEventListener('DOMContentLoaded', function() {
    // Check if the user is logged in and has appropriate permissions
    checkAuth();

    // Set up event listeners
    document.getElementById('logoutBtn').addEventListener('click', logout);
    document.getElementById('addUserBtn').addEventListener('click', showAddUserModal);
    document.getElementById('userForm').addEventListener('submit', saveUser);
    document.getElementById('searchBtn').addEventListener('click', searchUsers);
    document.getElementById('searchInput').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            searchUsers();
        }
    });
    document.getElementById('cancelBtn').addEventListener('click', closeModal);
    document.getElementById('closeRolesBtn').addEventListener('click', closeRolesModal);

    // Close modal when clicking the X or outside the modal
    const closeButtons = document.querySelectorAll('.close');
    closeButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            document.getElementById('userModal').style.display = 'none';
            document.getElementById('rolesModal').style.display = 'none';
        });
    });

    window.addEventListener('click', function(event) {
        const userModal = document.getElementById('userModal');
        const rolesModal = document.getElementById('rolesModal');
        if (event.target === userModal) {
            userModal.style.display = 'none';
        }
        if (event.target === rolesModal) {
            rolesModal.style.display = 'none';
        }
    });
});

// Check if user is authenticated and has permissions
async function checkAuth() {
    try {
        // First check if the user is authenticated using the standard auth check endpoint
        const authResponse = await fetch('/api/AuthStatus/check');
        const authData = await authResponse.json();
        
        if (!authData.isAuthenticated) {
            // Redirect to login page if not authenticated
            window.location.href = '/index.html';
            return;
        }
        
        // Then get user permissions from our backend
        // Since we don't have a dedicated permissions endpoint yet, we'll try to load users
        // which will fail with 403 if the user doesn't have permission
        try {
            console.log('Checking user permissions...');
            const response = await fetch('/api/users', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });
            
            if (response.status === 403) {
                // User doesn't have permission
                document.getElementById('permissionError').style.display = 'block';
                document.getElementById('userManagementContent').style.display = 'none';
                return;
            }
            
            if (!response.ok) {
                throw new Error('Failed to check permissions');
            }
            
            // If we got here, the user has permission to view users
            const users = await response.json();
            displayUsers(users);
            
            // Load roles for use in the add/edit forms
            loadRoles();
            
        } catch (error) {
            console.error('Error checking permissions:', error);
            // Show permission error if access is denied
            document.getElementById('permissionError').style.display = 'block';
            document.getElementById('userManagementContent').style.display = 'none';
        }

    } catch (error) {
        console.error('Error checking authentication:', error);
        window.location.href = '/index.html';
    }
}

// Load all users
async function loadUsers() {
    try {
        console.log('Loading users...');
        const response = await fetch('/api/users', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load users');
        }

        const users = await response.json();
        displayUsers(users);
        return users;

    } catch (error) {
        console.error('Error loading users:', error);
        return [];
    }
}

// Search users
function searchUsers() {
    const searchTerm = document.getElementById('searchInput').value.trim().toLowerCase();
    const rows = document.querySelectorAll('#usersTableBody tr');
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        if (text.includes(searchTerm)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

// Display users in the table
function displayUsers(users) {
    const tableBody = document.getElementById('usersTableBody');
    tableBody.innerHTML = '';

    if (!users || users.length === 0) {
        const row = document.createElement('tr');
        row.innerHTML = `<td colspan="6" style="text-align: center;">No users found</td>`;
        tableBody.appendChild(row);
        return;
    }

    users.forEach(user => {
        const row = document.createElement('tr');
        
        // Format the date
        const createdAt = user.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'N/A';
        
        row.innerHTML = `
            <td>${user.username || ''}</td>
            <td>${user.email || ''}</td>
            <td>${(user.firstName || '') + ' ' + (user.lastName || '')}</td>
            <td><span class="status ${user.isActive ? 'active' : 'inactive'}">${user.isActive ? 'Active' : 'Inactive'}</span></td>
            <td>${createdAt}</td>
            <td>
                <button class="action-btn btn-primary" onclick="viewUserRoles('${user.id}')">Roles</button>
                <button class="action-btn btn-success" onclick="editUser('${user.id}')">Edit</button>
                <button class="action-btn btn-danger" onclick="deleteUser('${user.id}')">Delete</button>
            </td>
        `;
        
        tableBody.appendChild(row);
    });
}

// Load all roles
async function loadRoles() {
    try {
        const response = await fetch('/api/roles', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load roles');
        }

        const roles = await response.json();
        window.allRoles = roles; // Store roles for later use

    } catch (error) {
        console.error('Error loading roles:', error);
    }
}

// Show modal to add a new user
function showAddUserModal() {
    document.getElementById('modalTitle').textContent = 'Add New User';
    document.getElementById('userForm').reset();
    document.getElementById('userId').value = '';
    document.getElementById('passwordGroup').style.display = 'block';
    
    // Clear any previous error message
    document.getElementById('userFormError').style.display = 'none';
    
    // Load roles as checkboxes
    const rolesContainer = document.getElementById('rolesCheckboxes');
    rolesContainer.innerHTML = '';
    
    if (window.allRoles && window.allRoles.length > 0) {
        window.allRoles.forEach(role => {
            const div = document.createElement('div');
            div.innerHTML = `
                <input type="checkbox" id="role_${role.id}" name="roles" value="${role.id}">
                <label for="role_${role.id}">${role.name}</label>
            `;
            rolesContainer.appendChild(div);
        });
    } else {
        rolesContainer.innerHTML = '<p>No roles available</p>';
    }
    
    document.getElementById('userModal').style.display = 'block';
}

// Edit an existing user
async function editUser(userId) {
    try {
        const response = await fetch(`/api/users/${userId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to get user details');
        }

        const user = await response.json();
        
        // Set form values
        document.getElementById('modalTitle').textContent = 'Edit User';
        document.getElementById('userId').value = user.id;
        document.getElementById('username').value = user.username;
        document.getElementById('email').value = user.email;
        document.getElementById('firstName').value = user.firstName;
        document.getElementById('lastName').value = user.lastName;
        document.getElementById('isActive').value = user.isActive.toString();
        
        // Password field is optional when editing
        document.getElementById('passwordGroup').style.display = 'none';
        document.getElementById('password').removeAttribute('required');
        
        // Load roles as checkboxes
        const rolesContainer = document.getElementById('rolesCheckboxes');
        rolesContainer.innerHTML = '';
        
        if (window.allRoles && window.allRoles.length > 0) {
            // Fetch user roles
            const rolesResponse = await fetch(`/api/users/${userId}/roles`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });
            
            let userRoles = [];
            if (rolesResponse.ok) {
                userRoles = await rolesResponse.json();
            }
            
            window.allRoles.forEach(role => {
                const isChecked = userRoles.some(userRole => userRole.id === role.id);
                const div = document.createElement('div');
                div.innerHTML = `
                    <input type="checkbox" id="role_${role.id}" name="roles" value="${role.id}" ${isChecked ? 'checked' : ''}>
                    <label for="role_${role.id}">${role.name}</label>
                `;
                rolesContainer.appendChild(div);
            });
        } else {
            rolesContainer.innerHTML = '<p>No roles available</p>';
        }
        
        document.getElementById('userModal').style.display = 'block';

    } catch (error) {
        console.error('Error loading user details:', error);
    }
}

// Save user (create or update)
async function saveUser(event) {
    event.preventDefault();
    
    // Clear previous error
    const errorElement = document.getElementById('userFormError');
    errorElement.style.display = 'none';
    
    const userId = document.getElementById('userId').value;
    const isEditMode = userId !== '';
    
    const userData = {
        username: document.getElementById('username').value,
        email: document.getElementById('email').value,
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        isActive: document.getElementById('isActive').value === 'true'
    };
    
    if (!isEditMode || document.getElementById('password').value) {
        userData.password = document.getElementById('password').value;
    }
    
    try {
        // Create or update user
        const url = isEditMode ? `/api/users/${userId}` : '/api/users';
        const method = isEditMode ? 'PUT' : 'POST';
        
        console.log(`Saving user data to ${url} using ${method}`);
        
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData),
            credentials: 'include'
        });
        
        if (!response.ok) {
            let errorMessage = 'Failed to save user';
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorMessage;
            } catch {
                // If we can't parse the JSON, just use the status text
                errorMessage = `Error: ${response.status} ${response.statusText}`;
            }
            throw new Error(errorMessage);
        }
        
        const savedUser = await response.json();
        console.log('User saved successfully:', savedUser);
        
        // Handle roles assignment
        const selectedRoles = Array.from(document.querySelectorAll('input[name="roles"]:checked'))
            .map(checkbox => checkbox.value);
        
        if (isEditMode) {
            // First, get current roles
            const rolesResponse = await fetch(`/api/users/${userId}/roles`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });
            
            let currentRoles = [];
            if (rolesResponse.ok) {
                currentRoles = await rolesResponse.json();
            }
            
            // Remove roles that are no longer selected
            for (const role of currentRoles) {
                if (!selectedRoles.includes(role.id)) {
                    await fetch(`/api/users/${userId}/roles/${role.id}`, {
                        method: 'DELETE',
                        credentials: 'include'
                    });
                }
            }
            
            // Add newly selected roles
            for (const roleId of selectedRoles) {
                if (!currentRoles.some(role => role.id === roleId)) {
                    await fetch(`/api/users/${userId}/roles/${roleId}`, {
                        method: 'POST',
                        credentials: 'include'
                    });
                }
            }
        } else {
            // For new users, add all selected roles
            for (const roleId of selectedRoles) {
                await fetch(`/api/users/${savedUser.id}/roles/${roleId}`, {
                    method: 'POST',
                    credentials: 'include'
                });
            }
        }
        
        // Reload users and close the modal
        await loadUsers();
        closeModal();
        
    } catch (error) {
        console.error('Error saving user:', error);
        errorElement.textContent = error.message || 'An error occurred while saving the user.';
        errorElement.style.display = 'block';
    }
}

// Delete a user
async function deleteUser(userId) {
    if (!confirm('Are you sure you want to delete this user?')) {
        return;
    }
    
    try {
        const response = await fetch(`/api/users/${userId}`, {
            method: 'DELETE',
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Failed to delete user');
        }
        
        // Reload users
        loadUsers();
        
    } catch (error) {
        console.error('Error deleting user:', error);
        alert('Error deleting user: ' + error.message);
    }
}

// View user roles
async function viewUserRoles(userId) {
    try {
        const [userResponse, rolesResponse] = await Promise.all([
            fetch(`/api/users/${userId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            }),
            fetch(`/api/users/${userId}/roles`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            })
        ]);
        
        if (!userResponse.ok || !rolesResponse.ok) {
            throw new Error('Failed to load user roles');
        }
        
        const user = await userResponse.json();
        const roles = await rolesResponse.json();
        
        const rolesInfo = document.getElementById('userRolesInfo');
        rolesInfo.innerHTML = `
            <h3>${user.firstName} ${user.lastName} (${user.username})</h3>
            <p>Email: ${user.email}</p>
            <h4>Assigned Roles:</h4>
            <ul>
                ${roles.length > 0 
                  ? roles.map(role => `<li>${role.name}: ${role.description}</li>`).join('') 
                  : '<li>No roles assigned</li>'}
            </ul>
        `;
        
        document.getElementById('rolesModal').style.display = 'block';
        
    } catch (error) {
        console.error('Error loading user roles:', error);
        const errorElement = document.getElementById('rolesError');
        errorElement.textContent = 'An error occurred while loading user roles.';
        errorElement.style.display = 'block';
    }
}

// Close the user modal
function closeModal() {
    document.getElementById('userModal').style.display = 'none';
}

// Close the roles modal
function closeRolesModal() {
    document.getElementById('rolesModal').style.display = 'none';
} 