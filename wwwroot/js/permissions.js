document.addEventListener('DOMContentLoaded', function() {
    // Check if the user is logged in and has appropriate permissions
    checkAuth();

    // Set up event listeners
    document.getElementById('logoutBtn').addEventListener('click', logout);
    document.getElementById('addPermissionBtn').addEventListener('click', showAddPermissionModal);
    document.getElementById('permissionForm').addEventListener('submit', savePermission);
    document.getElementById('searchBtn').addEventListener('click', searchPermissions);
    document.getElementById('searchInput').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            searchPermissions();
        }
    });
    document.getElementById('cancelBtn').addEventListener('click', closeModal);

    // Close modal when clicking the X or outside the modal
    const closeButtons = document.querySelectorAll('.close');
    closeButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            document.getElementById('permissionModal').style.display = 'none';
        });
    });

    window.addEventListener('click', function(event) {
        const permissionModal = document.getElementById('permissionModal');
        if (event.target === permissionModal) {
            permissionModal.style.display = 'none';
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
        // Since we don't have a dedicated permissions check endpoint yet, we'll try to load permissions
        // which will fail with 403 if the user doesn't have permission
        try {
            console.log('Checking user permissions...');
            const response = await fetch('/api/permissions', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });
            
            if (response.status === 403) {
                // User doesn't have permission
                document.getElementById('permissionError').style.display = 'block';
                document.getElementById('permissionManagementContent').style.display = 'none';
                return;
            }
            
            if (!response.ok) {
                throw new Error('Failed to check permissions');
            }
            
            // If we got here, the user has permission to view permissions
            const permissions = await response.json();
            displayPermissions(permissions);
            
        } catch (error) {
            console.error('Error checking permissions:', error);
            // Show permission error if access is denied
            document.getElementById('permissionError').style.display = 'block';
            document.getElementById('permissionManagementContent').style.display = 'none';
        }

    } catch (error) {
        console.error('Error checking authentication:', error);
        window.location.href = '/index.html';
    }
}

// Load all permissions
async function loadPermissions() {
    try {
        console.log('Loading permissions...');
        const response = await fetch('/api/permissions', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to load permissions');
        }

        const permissions = await response.json();
        displayPermissions(permissions);
        return permissions;

    } catch (error) {
        console.error('Error loading permissions:', error);
        return [];
    }
}

// Search permissions
function searchPermissions() {
    const searchTerm = document.getElementById('searchInput').value.trim().toLowerCase();
    const rows = document.querySelectorAll('#permissionsTableBody tr');
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        if (text.includes(searchTerm)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

// Display permissions in the table
function displayPermissions(permissions) {
    const tableBody = document.getElementById('permissionsTableBody');
    tableBody.innerHTML = '';

    if (!permissions || permissions.length === 0) {
        const row = document.createElement('tr');
        row.innerHTML = `<td colspan="5" style="text-align: center;">No permissions found</td>`;
        tableBody.appendChild(row);
        return;
    }

    permissions.forEach(permission => {
        const row = document.createElement('tr');
        
        // Format the date
        const createdAt = permission.createdAt ? new Date(permission.createdAt).toLocaleDateString() : 'N/A';
        
        row.innerHTML = `
            <td>${permission.name || ''}</td>
            <td>${permission.resource || ''}</td>
            <td>${permission.description || ''}</td>
            <td>${createdAt}</td>
            <td>
                <button class="action-btn btn-success" onclick="editPermission('${permission.id}')">Edit</button>
                <button class="action-btn btn-danger" onclick="deletePermission('${permission.id}')">Delete</button>
            </td>
        `;
        
        tableBody.appendChild(row);
    });
}

// Show modal to add a new permission
function showAddPermissionModal() {
    document.getElementById('modalTitle').textContent = 'Add New Permission';
    document.getElementById('permissionForm').reset();
    document.getElementById('permissionId').value = '';
    
    // Clear any previous error message
    document.getElementById('permissionFormError').style.display = 'none';
    
    document.getElementById('permissionModal').style.display = 'block';
}

// Edit an existing permission
async function editPermission(permissionId) {
    try {
        const response = await fetch(`/api/permissions/${permissionId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to get permission details');
        }

        const permission = await response.json();
        
        // Set form values
        document.getElementById('modalTitle').textContent = 'Edit Permission';
        document.getElementById('permissionId').value = permission.id;
        document.getElementById('name').value = permission.name;
        document.getElementById('resource').value = permission.resource;
        document.getElementById('description').value = permission.description;
        
        document.getElementById('permissionModal').style.display = 'block';

    } catch (error) {
        console.error('Error loading permission details:', error);
    }
}

// Save permission (create or update)
async function savePermission(event) {
    event.preventDefault();
    
    // Clear previous error
    const errorElement = document.getElementById('permissionFormError');
    errorElement.style.display = 'none';
    
    const permissionId = document.getElementById('permissionId').value;
    const isEditMode = permissionId !== '';
    
    const permissionData = {
        name: document.getElementById('name').value,
        resource: document.getElementById('resource').value,
        description: document.getElementById('description').value
    };
    
    try {
        // Create or update permission
        const url = isEditMode ? `/api/permissions/${permissionId}` : '/api/permissions';
        const method = isEditMode ? 'PUT' : 'POST';
        
        console.log(`Saving permission data to ${url} using ${method}`);
        
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(permissionData),
            credentials: 'include'
        });
        
        if (!response.ok) {
            let errorMessage = 'Failed to save permission';
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorMessage;
            } catch {
                // If we can't parse the JSON, just use the status text
                errorMessage = `Error: ${response.status} ${response.statusText}`;
            }
            throw new Error(errorMessage);
        }
        
        const savedPermission = await response.json();
        console.log('Permission saved successfully:', savedPermission);
        
        // Reload permissions and close the modal
        await loadPermissions();
        closeModal();
        
    } catch (error) {
        console.error('Error saving permission:', error);
        errorElement.textContent = error.message || 'An error occurred while saving the permission.';
        errorElement.style.display = 'block';
    }
}

// Delete a permission
async function deletePermission(permissionId) {
    if (!confirm('Are you sure you want to delete this permission? This will remove the permission from all roles.')) {
        return;
    }
    
    try {
        const response = await fetch(`/api/permissions/${permissionId}`, {
            method: 'DELETE',
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Failed to delete permission');
        }
        
        // Reload permissions
        loadPermissions();
        
    } catch (error) {
        console.error('Error deleting permission:', error);
        alert('Error deleting permission: ' + error.message);
    }
}

// Close the permission modal
function closeModal() {
    document.getElementById('permissionModal').style.display = 'none';
} 