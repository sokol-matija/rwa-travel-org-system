let editMode = false;
let originalData = {};

// Toggle between view and edit mode
function toggleEditMode() {
    editMode = true;
    
    // Store original data for cancel functionality
    originalData = {};
    originalData.email = document.getElementById('emailInput').value;
    originalData.firstName = document.getElementById('firstNameInput').value;
    originalData.lastName = document.getElementById('lastNameInput').value;
    originalData.phoneNumber = document.getElementById('phoneInput').value;
    originalData.address = document.getElementById('addressInput').value;

    // Hide display elements and show input elements
    document.getElementById('emailDisplay').classList.add('d-none');
    document.getElementById('emailInput').classList.remove('d-none');
    document.getElementById('firstNameDisplay').classList.add('d-none');
    document.getElementById('firstNameInput').classList.remove('d-none');
    document.getElementById('lastNameDisplay').classList.add('d-none');
    document.getElementById('lastNameInput').classList.remove('d-none');
    document.getElementById('phoneDisplay').classList.add('d-none');
    document.getElementById('phoneInput').classList.remove('d-none');
    document.getElementById('addressDisplay').classList.add('d-none');
    document.getElementById('addressInput').classList.remove('d-none');

    // Toggle buttons
    document.getElementById('editBtn').classList.add('d-none');
    document.getElementById('cancelBtn').classList.remove('d-none');
    document.getElementById('saveBtn').classList.remove('d-none');

    // Focus on first editable field
    document.getElementById('emailInput').focus();
}

// Cancel edit and restore original values
function cancelEdit() {
    editMode = false;

    // Restore original values
    document.getElementById('emailInput').value = originalData.email;
    document.getElementById('firstNameInput').value = originalData.firstName;
    document.getElementById('lastNameInput').value = originalData.lastName;
    document.getElementById('phoneInput').value = originalData.phoneNumber;
    document.getElementById('addressInput').value = originalData.address;

    // Clear validation errors
    clearValidationErrors();

    // Hide input elements and show display elements
    document.getElementById('emailInput').classList.add('d-none');
    document.getElementById('emailDisplay').classList.remove('d-none');
    document.getElementById('firstNameInput').classList.add('d-none');
    document.getElementById('firstNameDisplay').classList.remove('d-none');
    document.getElementById('lastNameInput').classList.add('d-none');
    document.getElementById('lastNameDisplay').classList.remove('d-none');
    document.getElementById('phoneInput').classList.add('d-none');
    document.getElementById('phoneDisplay').classList.remove('d-none');
    document.getElementById('addressInput').classList.add('d-none');
    document.getElementById('addressDisplay').classList.remove('d-none');

    // Toggle buttons
    document.getElementById('editBtn').classList.remove('d-none');
    document.getElementById('cancelBtn').classList.add('d-none');
    document.getElementById('saveBtn').classList.add('d-none');
}

// Save profile using AJAX
async function saveProfile() {
    if (!validateForm()) {
        return;
    }

    // Show loading spinner
    document.getElementById('loadingSpinner').classList.remove('d-none');
    document.getElementById('saveBtn').disabled = true;

    const profileData = {};
    profileData.Email = document.getElementById('emailInput').value;
    profileData.FirstName = document.getElementById('firstNameInput').value || null;
    profileData.LastName = document.getElementById('lastNameInput').value || null;
    profileData.PhoneNumber = document.getElementById('phoneInput').value || null;
    profileData.Address = document.getElementById('addressInput').value || null;

    try {
        const response = await fetch('/api/profile', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include', // Include cookies for authentication
            body: JSON.stringify(profileData)
        });

        let result;
        try {
            result = await response.json();
        } catch (parseError) {
            console.error('Failed to parse response as JSON:', parseError);
            throw new Error('Invalid response from server');
        }

        if (response.ok) {
            // Update display elements with new values
            updateDisplayValues(result);
            
            // Exit edit mode
            editMode = false;
            exitEditMode();
            
            // Show success message
            showMessage('Profile updated successfully!', 'success');
        } else {
            // Handle validation errors
            console.error('Profile update failed:', response.status, result);
            if (result.errors) {
                displayValidationErrors(result.errors);
            } else {
                showMessage(result.message || `Failed to update profile (${response.status}). Please try again.`, 'error');
            }
        }
    } catch (error) {
        console.error('Error updating profile:', error);
        showMessage(`Network error: ${error.message}. Please check if you're logged in and try again.`, 'error');
    } finally {
        // Hide loading spinner
        document.getElementById('loadingSpinner').classList.add('d-none');
        document.getElementById('saveBtn').disabled = false;
    }
}

// Update display values after successful save
function updateDisplayValues(userData) {
    // Update each field individually
    document.getElementById('emailDisplay').innerHTML = userData.email;
    document.getElementById('firstNameDisplay').innerHTML = userData.firstName || '<span class="text-muted">Not provided</span>';
    document.getElementById('lastNameDisplay').innerHTML = userData.lastName || '<span class="text-muted">Not provided</span>';
    document.getElementById('phoneDisplay').innerHTML = userData.phoneNumber || '<span class="text-muted">Not provided</span>';
    document.getElementById('addressDisplay').innerHTML = userData.address || '<span class="text-muted">Not provided</span>';
}

// Exit edit mode and reset UI
function exitEditMode() {
    document.getElementById('emailInput').classList.add('d-none');
    document.getElementById('emailDisplay').classList.remove('d-none');
    document.getElementById('firstNameInput').classList.add('d-none');
    document.getElementById('firstNameDisplay').classList.remove('d-none');
    document.getElementById('lastNameInput').classList.add('d-none');
    document.getElementById('lastNameDisplay').classList.remove('d-none');
    document.getElementById('phoneInput').classList.add('d-none');
    document.getElementById('phoneDisplay').classList.remove('d-none');
    document.getElementById('addressInput').classList.add('d-none');
    document.getElementById('addressDisplay').classList.remove('d-none');

    document.getElementById('editBtn').classList.remove('d-none');
    document.getElementById('cancelBtn').classList.add('d-none');
    document.getElementById('saveBtn').classList.add('d-none');
}

// Basic form validation
function validateForm() {
    clearValidationErrors();
    let isValid = true;

    const email = document.getElementById('emailInput').value;
    if (!email || !isValidEmail(email)) {
        showFieldError('emailInput', 'Please enter a valid email address.');
        isValid = false;
    }

    return isValid;
}

// Email validation helper
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Show field-specific validation error
function showFieldError(fieldId, message) {
    const field = document.getElementById(fieldId);
    const feedback = field.nextElementSibling;
    
    field.classList.add('is-invalid');
    feedback.textContent = message;
}

// Clear all validation errors
function clearValidationErrors() {
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.classList.remove('is-invalid');
        const feedback = input.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = '';
        }
    });
}

// Display server validation errors
function displayValidationErrors(errors) {
    for (const field in errors) {
        const fieldName = field.toLowerCase();
        const input = document.getElementById(fieldName + 'Input');
        if (input) {
            const fieldErrors = errors[field];
            const errorMessage = fieldErrors ? fieldErrors.join(' ') : 'Validation error';
            showFieldError(input.id, errorMessage);
        }
    }
}

// Show success/error messages
function showMessage(message, type) {
    const resultDiv = document.getElementById('updateResult');
    const successAlert = document.getElementById('successAlert');
    const errorAlert = document.getElementById('errorAlert');

    if (type === 'success') {
        document.getElementById('successMessage').textContent = message;
        successAlert.classList.remove('d-none');
        errorAlert.classList.add('d-none');
    } else {
        document.getElementById('errorMessage').textContent = message;
        errorAlert.classList.remove('d-none');
        successAlert.classList.add('d-none');
    }

    resultDiv.classList.remove('d-none');

    // Auto-hide after 5 seconds
    setTimeout(() => {
        hideAlert('updateResult');
    }, 5000);
}

// Hide alert messages
function hideAlert(elementId) {
    document.getElementById(elementId).classList.add('d-none');
}

// Get auth token
function getAuthToken() {
    return sessionStorage.getItem('token') || 
           localStorage.getItem('token') || 
           getCookie('token');
}

// Simple cookie getter
function getCookie(name) {
    const value = '; ' + document.cookie;
    const parts = value.split('; ' + name + '=');
    const expectedLength = 2;
    if (parts.length === expectedLength) return parts.pop().split(';').shift();
} 