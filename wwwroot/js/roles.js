document.addEventListener('DOMContentLoaded', function() {
    // Check if the user is logged in and has appropriate permissions
    checkAuth();

    // Set up event listeners
    document.getElementById('logoutBtn').addEventListener('click', logout);
    document.getElementById('addRoleBtn').addEventListener('click', showAddRoleModal);
    document.getElementById('roleForm').addEventListener('submit', saveRole);
    document.getElementById('searchBtn').addEventListener('click', searchRoles);
    document.getElementById('searchInput').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            searchRoles();
        }
    });
    document.getElementById('cancelBtn').addEventListener('click', closeModal);
    document.getElementById('closePermissionsBtn').addEventListener('click', closePermissionsModal);

    // Close modal when clicking the X or outside the modal
    const closeButtons = document.querySelectorAll('.close');
    closeButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            document.getElementById('roleModal').style.display = 'none';
            document.getElementById('permissionsModal').style.display = 'none';
        });
    });

    window.addEventListener('click', function(event) {
        const roleModal = document.getElementById('roleModal');
        const permissionsModal = document.getElementById('permissionsModal');
        if (event.target === roleModal) {
            roleModal.style.display = 'none';
        }
        if (event.target === permissionsModal) {
            permissionsModal.style.display = 'none';
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
        // Since we don't have a dedicated permissions endpoint yet, we'll try to load roles
        // which will fail with 403 if the user doesn't have permission
        try {
            console.log('Checking user permissions...');
            const response = await fetch('/api/roles', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });
            
            if (response.status === 403) {
                // User doesn't have permission
                document.getElementById('permissionError').style.display = 'block';
                document.getElementById('roleManagementContent').style.display = 'none';
                return;
            }
            
            if (!response.ok) {
                throw new Error('Failed to check permissions');
            }
            
            // If we got here, the user has permission to view roles
            const roles = await response.json();
            displayRoles(roles);
            
            // Load permissions for use in the add/edit forms
            loadPermissions();
            
        } catch (error) {
            console.error('Error checking permissions:', error);
            // Show permission error if access is denied
            document.getElementById('permissionError').style.display = 'block';
            document.getElementById('roleManagementContent').style.display = 'none';
        }

    } catch (error) {
        console.error('Error checking authentication:', error);
        window.location.href = '/index.html';
    }
}

// Load all roles
async function loadRoles() {
    try {
        console.log('Loading roles...');
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
        displayRoles(roles);
        return roles;

    } catch (error) {
        console.error('Error loading roles:', error);
        return [];
    }
}

// Search roles
function searchRoles() {
    const searchTerm = document.getElementById('searchInput').value.trim().toLowerCase();
    const rows = document.querySelectorAll('#rolesTableBody tr');
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        if (text.includes(searchTerm)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

// Display roles in the table
function displayRoles(roles) {
    const tableBody = document.getElementById('rolesTableBody');
    tableBody.innerHTML = '';

    if (!roles || roles.length === 0) {
        const row = document.createElement('tr');
        row.innerHTML = `<td colspan="5" style="text-align: center;">No roles found</td>`;
        tableBody.appendChild(row);
        return;
    }

    roles.forEach(role => {
        const row = document.createElement('tr');
        
        // Format the date
        const createdAt = role.createdAt ? new Date(role.createdAt).toLocaleDateString() : 'N/A';
        
        row.innerHTML = `
            <td>${role.name || ''}</td>
            <td>${role.description || ''}</td>
            <td><span class="status ${role.isActive ? 'active' : 'inactive'}">${role.isActive ? 'Active' : 'Inactive'}</span></td>
            <td>${createdAt}</td>
            <td>
                <button class="action-btn btn-primary" onclick="viewRolePermissions('${role.id}')">Permissions</button>
                <button class="action-btn btn-success" onclick="editRole('${role.id}')">Edit</button>
                <button class="action-btn btn-danger" onclick="deleteRole('${role.id}')">Delete</button>
            </td>
        `;
        
        tableBody.appendChild(row);
    });
}

// Load all permissions
async function loadPermissions() {
    try {
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
        // Group permissions by resource
        const groupedPermissions = groupPermissionsByResource(permissions);
        window.allPermissions = permissions; // Store permissions for later use
        window.groupedPermissions = groupedPermissions; // Store grouped permissions

    } catch (error) {
        console.error('Error loading permissions:', error);
    }
}

// Group permissions by resource
function groupPermissionsByResource(permissions) {
    const groups = {};
    
    permissions.forEach(permission => {
        if (!groups[permission.resource]) {
            groups[permission.resource] = [];
        }
        groups[permission.resource].push(permission);
    });
    
    return groups;
}

// Show modal to add a new role
function showAddRoleModal() {
    document.getElementById('modalTitle').textContent = 'Add New Role';
    document.getElementById('roleForm').reset();
    document.getElementById('roleId').value = '';
    
    // Clear any previous error message
    document.getElementById('roleFormError').style.display = 'none';
    
    // Load permissions as checkboxes grouped by resource
    loadPermissionsForForm();
    
    document.getElementById('roleModal').style.display = 'block';
}

// Load permissions for the form
function loadPermissionsForForm(selectedPermissionIds = []) {
    const permissionsContainer = document.getElementById('permissionsContainer');
    permissionsContainer.innerHTML = '';
    
    if (!window.groupedPermissions) {
        permissionsContainer.innerHTML = '<p>No permissions available</p>';
        return;
    }
    
    // Sort resources alphabetically
    const resources = Object.keys(window.groupedPermissions).sort();
    
    resources.forEach(resource => {
        const permissionGroup = document.createElement('div');
        permissionGroup.className = 'permission-group';
        
        const resourceTitle = document.createElement('h4');
        resourceTitle.textContent = resource;
        permissionGroup.appendChild(resourceTitle);
        
        const checkboxGroup = document.createElement('div');
        checkboxGroup.className = 'checkbox-group';
        
        window.groupedPermissions[resource].forEach(permission => {
            const checkboxItem = document.createElement('div');
            checkboxItem.className = 'checkbox-item';
            
            const isChecked = selectedPermissionIds.includes(permission.id);
            
            checkboxItem.innerHTML = `
                <input type="checkbox" id="perm_${permission.id}" name="permissions" value="${permission.id}" ${isChecked ? 'checked' : ''}>
                <label for="perm_${permission.id}">${permission.name}: ${permission.description}</label>
            `;
            
            checkboxGroup.appendChild(checkboxItem);
        });
        
        permissionGroup.appendChild(checkboxGroup);
        permissionsContainer.appendChild(permissionGroup);
    });
}

// Edit an existing role
async function editRole(roleId) {
    try {
        const response = await fetch(`/api/roles/${roleId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Failed to get role details');
        }

        const role = await response.json();
        
        // Set form values
        document.getElementById('modalTitle').textContent = 'Edit Role';
        document.getElementById('roleId').value = role.id;
        document.getElementById('name').value = role.name;
        document.getElementById('description').value = role.description;
        document.getElementById('isActive').value = role.isActive.toString();
        
        // Get role permissions
        const selectedPermissionIds = role.permissions ? role.permissions.map(p => p.id) : [];
        
        // Load permissions with selected ones checked
        loadPermissionsForForm(selectedPermissionIds);
        
        document.getElementById('roleModal').style.display = 'block';

    } catch (error) {
        console.error('Error loading role details:', error);
    }
}

// Save role (create or update)
async function saveRole(event) {
    event.preventDefault();
    
    // Clear previous error
    const errorElement = document.getElementById('roleFormError');
    errorElement.style.display = 'none';
    
    const roleId = document.getElementById('roleId').value;
    const isEditMode = roleId !== '';
    
    const roleData = {
        name: document.getElementById('name').value,
        description: document.getElementById('description').value,
        isActive: document.getElementById('isActive').value === 'true'
    };
    
    try {
        // Create or update role
        const url = isEditMode ? `/api/roles/${roleId}` : '/api/roles';
        const method = isEditMode ? 'PUT' : 'POST';
        
        console.log(`Saving role data to ${url} using ${method}`);
        
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(roleData),
            credentials: 'include'
        });
        
        if (!response.ok) {
            let errorMessage = 'Failed to save role';
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorMessage;
            } catch {
                // If we can't parse the JSON, just use the status text
                errorMessage = `Error: ${response.status} ${response.statusText}`;
            }
            throw new Error(errorMessage);
        }
        
        const savedRole = await response.json();
        console.log('Role saved successfully:', savedRole);
        
        // Handle permissions assignment
        const savedRoleId = isEditMode ? roleId : savedRole.id;
        const selectedPermissions = Array.from(document.querySelectorAll('input[name="permissions"]:checked'))
            .map(checkbox => checkbox.value);
        
        // Get current permissions for the role
        const permissionsResponse = await fetch(`/api/roles/${savedRoleId}/permissions`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        let currentPermissions = [];
        if (permissionsResponse.ok) {
            currentPermissions = await permissionsResponse.json();
        }
        
        // Remove permissions that are no longer selected
        for (const permission of currentPermissions) {
            if (!selectedPermissions.includes(permission.id)) {
                await fetch(`/api/roles/${savedRoleId}/permissions/${permission.id}`, {
                    method: 'DELETE',
                    credentials: 'include'
                });
            }
        }
        
        // Add newly selected permissions
        for (const permissionId of selectedPermissions) {
            if (!currentPermissions.some(p => p.id === permissionId)) {
                await fetch(`/api/roles/${savedRoleId}/permissions/${permissionId}`, {
                    method: 'POST',
                    credentials: 'include'
                });
            }
        }
        
        // Reload roles and close the modal
        await loadRoles();
        closeModal();
        
    } catch (error) {
        console.error('Error saving role:', error);
        errorElement.textContent = error.message || 'An error occurred while saving the role.';
        errorElement.style.display = 'block';
    }
}

// Delete a role
async function deleteRole(roleId) {
    if (!confirm('Are you sure you want to delete this role? This will remove the role from all users.')) {
        return;
    }
    
    try {
        const response = await fetch(`/api/roles/${roleId}`, {
            method: 'DELETE',
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Failed to delete role');
        }
        
        // Reload roles
        loadRoles();
        
    } catch (error) {
        console.error('Error deleting role:', error);
        alert('Error deleting role: ' + error.message);
    }
}

// View role permissions
async function viewRolePermissions(roleId) {
    try {
        const [roleResponse, permissionsResponse] = await Promise.all([
            fetch(`/api/roles/${roleId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            }),
            fetch(`/api/roles/${roleId}/permissions`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            })
        ]);
        
        if (!roleResponse.ok || !permissionsResponse.ok) {
            throw new Error('Failed to load role permissions');
        }
        
        const role = await roleResponse.json();
        const permissions = await permissionsResponse.json();
        
        // Group permissions by resource for display
        const groupedPermissions = {};
        permissions.forEach(permission => {
            if (!groupedPermissions[permission.resource]) {
                groupedPermissions[permission.resource] = [];
            }
            groupedPermissions[permission.resource].push(permission);
        });
        
        const permissionsInfo = document.getElementById('rolePermissionsInfo');
        
        // Create the role info HTML
        let html = `
            <h3>${role.name}</h3>
            <p>${role.description}</p>
            <p>Status: <span class="status ${role.isActive ? 'active' : 'inactive'}">${role.isActive ? 'Active' : 'Inactive'}</span></p>
            <h4>Assigned Permissions:</h4>
        `;
        
        if (permissions.length === 0) {
            html += '<p>No permissions assigned to this role.</p>';
        } else {
            // Sort resources alphabetically
            const resources = Object.keys(groupedPermissions).sort();
            
            html += '<div>';
            resources.forEach(resource => {
                html += `
                    <div class="permission-group">
                        <h4>${resource}</h4>
                        <ul>
                            ${groupedPermissions[resource].map(p => `<li>${p.name}: ${p.description}</li>`).join('')}
                        </ul>
                    </div>
                `;
            });
            html += '</div>';
        }
        
        permissionsInfo.innerHTML = html;
        document.getElementById('permissionsModal').style.display = 'block';
        
    } catch (error) {
        console.error('Error loading role permissions:', error);
        const errorElement = document.getElementById('permissionsError');
        errorElement.textContent = 'An error occurred while loading role permissions.';
        errorElement.style.display = 'block';
    }
}

// Close the role modal
function closeModal() {
    document.getElementById('roleModal').style.display = 'none';
}

// Close the permissions modal
function closePermissionsModal() {
    document.getElementById('permissionsModal').style.display = 'none';
} 