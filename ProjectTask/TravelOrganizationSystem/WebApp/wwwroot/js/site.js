// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Newsletter subscription functionality
function subscribeNewsletter() {
    const emailInput = document.getElementById('newsletterEmail');
    const email = emailInput.value.trim();
    
    // Clear any previous error styling
    emailInput.classList.remove('is-invalid');
    
    // Validate email format
    if (!email) {
        showEmailError(emailInput, 'Please enter your email address.');
        return;
    }
    
    if (!isValidEmail(email)) {
        showEmailError(emailInput, 'Please enter a valid email address.');
        return;
    }
    
    // Add loading state to button
    const button = document.querySelector('.btn-newsletter');
    const originalHTML = button.innerHTML;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    button.disabled = true;
    
    // Simulate API call (you can replace this with actual backend call)
    setTimeout(() => {
        // Reset button
        button.innerHTML = originalHTML;
        button.disabled = false;
        
        // Clear the input
        emailInput.value = '';
        emailInput.classList.remove('is-invalid');
        
        // Show success modal
        const modal = new bootstrap.Modal(document.getElementById('newsletterModal'));
        modal.show();
        
        // Optional: Store email for future use (you can add backend integration here)
        console.log('Newsletter subscription for:', email);
        
    }, 1500); // Simulate 1.5 second loading time
}

// Email validation function
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Show email validation error
function showEmailError(input, message) {
    input.classList.add('is-invalid');
    
    // Remove existing error message
    const existingError = input.parentNode.querySelector('.invalid-feedback');
    if (existingError) {
        existingError.remove();
    }
    
    // Add error message
    const errorDiv = document.createElement('div');
    errorDiv.className = 'invalid-feedback';
    errorDiv.textContent = message;
    input.parentNode.appendChild(errorDiv);
    
    // Remove error after 5 seconds
    setTimeout(() => {
        input.classList.remove('is-invalid');
        if (errorDiv) {
            errorDiv.remove();
        }
    }, 5000);
}

// Allow Enter key to submit newsletter
document.addEventListener('DOMContentLoaded', function() {
    const emailInput = document.getElementById('newsletterEmail');
    if (emailInput) {
        emailInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                subscribeNewsletter();
            }
        });
    }
    
    // Modern navbar scroll effect
    initializeNavbarScrollEffect();
    
    // Initialize active nav link highlighting
    initializeActiveNavLinks();
});

// Navbar scroll effect - Sticky behavior with smart hiding
function initializeNavbarScrollEffect() {
    const navbar = document.querySelector('.modern-navbar');
    if (!navbar) return;
    
    let lastScrollTop = 0;
    let ticking = false;
    
    function updateNavbar() {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        
        // Apply scrolled state for enhanced styling
        if (scrollTop > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
        
        // Smart navbar visibility
        if (scrollTop > 100) {
            if (scrollTop > lastScrollTop && scrollTop > 200) {
                // Scrolling down - hide navbar
                navbar.style.transform = 'translateY(-100%)';
            } else {
                // Scrolling up - show navbar
                navbar.style.transform = 'translateY(0)';
            }
        } else {
            // Always show navbar at top
            navbar.style.transform = 'translateY(0)';
        }
        
        lastScrollTop = scrollTop;
        ticking = false;
    }
    
    function requestTick() {
        if (!ticking) {
            requestAnimationFrame(updateNavbar);
            ticking = true;
        }
    }
    
    window.addEventListener('scroll', requestTick);
}

// Active nav link highlighting - Fixed for specific path matching
function initializeActiveNavLinks() {
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.modern-nav-link');
    
    // Remove all active classes first
    navLinks.forEach(link => link.classList.remove('active'));
    
    // Find exact matches first, then partial matches
    let exactMatch = null;
    let partialMatches = [];
    
    navLinks.forEach(link => {
        const linkPath = link.getAttribute('href') || link.getAttribute('asp-page');
        if (linkPath) {
            const normalizedLinkPath = linkPath.toLowerCase();
            
            // Exact match has highest priority
            if (currentPath === normalizedLinkPath) {
                exactMatch = link;
            }
            // Partial match (but not root path)
            else if (normalizedLinkPath !== '/' && currentPath.startsWith(normalizedLinkPath)) {
                partialMatches.push({
                    link: link,
                    path: normalizedLinkPath,
                    length: normalizedLinkPath.length
                });
            }
        }
    });
    
    // Apply active class to exact match or most specific partial match
    if (exactMatch) {
        exactMatch.classList.add('active');
    } else if (partialMatches.length > 0) {
        // Sort by path length (longest first) to get most specific match
        partialMatches.sort((a, b) => b.length - a.length);
        partialMatches[0].link.classList.add('active');
    }
}

// Smooth scrolling for anchor links
document.addEventListener('click', function(e) {
    if (e.target.matches('a[href^="#"]')) {
        e.preventDefault();
        const target = document.querySelector(e.target.getAttribute('href'));
        if (target) {
            const offsetTop = target.getBoundingClientRect().top + window.pageYOffset - 100;
            window.scrollTo({
                top: offsetTop,
                behavior: 'smooth'
            });
        }
    }
});
