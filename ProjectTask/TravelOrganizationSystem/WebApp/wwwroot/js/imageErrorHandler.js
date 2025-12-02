/**
 * Reliable Image Error Handler
 * Automatically replaces broken images with working fallbacks
 */

// Reliable fallback image collections
const fallbackImages = {
    rome: [
        'https://images.unsplash.com/photo-1515542622106-78bda8ba0e5b?w=1080&q=80&fit=max&auto=format',
        'https://images.unsplash.com/photo-1529260830199-42c24126f198?w=1080&q=80&fit=max&auto=format'
    ],
    paris: [
        'https://images.unsplash.com/photo-1502602898536-47ad22581b52?w=1080&q=80&fit=max&auto=format',
        'https://images.unsplash.com/photo-1499678329028-101435549a4e?w=1080&q=80&fit=max&auto=format'
    ],
    barcelona: [
        'https://images.unsplash.com/photo-1539650116574-75c0c6d45d2e?w=1080&q=80&fit=max&auto=format',
        'https://images.unsplash.com/photo-1511527844068-006b95d162c2?w=1080&q=80&fit=max&auto=format'
    ],
    london: [
        'https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?w=1080&q=80&fit=max&auto=format',
        'https://images.unsplash.com/photo-1533929736458-ca588d08c8be?w=1080&q=80&fit=max&auto=format'
    ],
    tokyo: [
        'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=1080&q=80&fit=max&auto=format',
        'https://images.unsplash.com/photo-1542051841857-5f90071e7989?w=1080&q=80&fit=max&auto=format'
    ],
    travel: [
        'https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=1080&q=80&fit=max&auto=format',
        'https://images.unsplash.com/photo-1488646953014-85cb44e25828?w=1080&q=80&fit=max&auto=format'
    ]
};

/**
 * Handle image loading errors with smart fallbacks
 * @param {HTMLImageElement} img - The failed image element
 */
function handleImageError(img) {
    // Prevent infinite loops
    if (img.dataset.fallbackAttempted === 'true') {
        console.warn('Image fallback already attempted for:', img.src);
        return;
    }
    
    img.dataset.fallbackAttempted = 'true';
    
    // Determine category from trip name or image alt text
    const tripName = (img.alt || img.title || '').toLowerCase();
    let category = 'travel'; // default
    
    if (tripName.includes('rome')) category = 'rome';
    else if (tripName.includes('paris')) category = 'paris';
    else if (tripName.includes('barcelona')) category = 'barcelona';
    else if (tripName.includes('london')) category = 'london';
    else if (tripName.includes('tokyo')) category = 'tokyo';
    
    // Get random fallback from appropriate category
    const categoryImages = fallbackImages[category];
    const randomFallback = categoryImages[Math.floor(Math.random() * categoryImages.length)];
    
    console.log(`🔄 Replacing broken image with ${category} fallback:`, randomFallback);
    
    // Replace with fallback image
    img.src = randomFallback;
    img.style.filter = 'brightness(0.95)'; // Slight dim to indicate it's a fallback
    img.title = `Fallback image for ${tripName || 'this trip'}`;
}

/**
 * Initialize image error handling for the page
 */
function initializeImageErrorHandling() {
    // Handle existing images
    document.querySelectorAll('img').forEach(img => {
        img.addEventListener('error', () => handleImageError(img));
    });
    
    // Handle dynamically added images
    const observer = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType === 1) { // Element node
                    if (node.tagName === 'IMG') {
                        node.addEventListener('error', () => handleImageError(node));
                    }
                    // Handle images within added elements
                    node.querySelectorAll?.('img').forEach(img => {
                        img.addEventListener('error', () => handleImageError(img));
                    });
                }
            });
        });
    });
    
    observer.observe(document.body, { childList: true, subtree: true });
    
    console.log('🛡️ Image error handling initialized - broken images will be automatically replaced');
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeImageErrorHandling);
} else {
    initializeImageErrorHandling();
}

// Export for manual use
window.handleImageError = handleImageError; 