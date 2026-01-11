// API Configuration
const API_BASE_URL = 'http://localhost:5000/api';

// Session management
function getSessionToken() {
    return localStorage.getItem('sessionToken');
}

function setSessionToken(token) {
    localStorage.setItem('sessionToken', token);
}

function clearSessionToken() {
    localStorage.removeItem('sessionToken');
}

// Helper function for API calls
async function apiCall(endpoint, method = 'GET', body = null, requiresAuth = false) {
    const headers = {
        'Content-Type': 'application/json'
    };

    if (requiresAuth) {
        const token = getSessionToken();
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }
    }

    const options = {
        method,
        headers
    };

    if (body && method !== 'GET') {
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, options);
        
        if (!response.ok) {
            if (response.status === 401) {
                clearSessionToken();
                throw new Error('Unauthorized');
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('API call error:', error);
        throw error;
    }
}

// Auth API
const authAPI = {
    async register(username, email, password, inviteCode) {
        const data = await apiCall('/auth/register', 'POST', {
            username,
            email,
            password,
            inviteCode
        });
        return data;
    },

    async login(email, password) {
        const data = await apiCall('/auth/login', 'POST', {
            email,
            password
        });
        
        if (data.sessionToken) {
            setSessionToken(data.sessionToken);
        }
        
        return data;
    },

    async logout() {
        await apiCall('/auth/logout', 'POST', null, true);
        clearSessionToken();
    },

    async getCurrentUser() {
        return await apiCall('/auth/me', 'GET', null, true);
    }
};

// Markets API
const marketsAPI = {
    async getAll() {
        return await apiCall('/markets', 'GET');
    },

    async getById(id) {
        return await apiCall(`/markets/${id}`, 'GET');
    },

    async create(question, description, closeDate = null) {
        return await apiCall('/markets', 'POST', {
            question,
            description,
            closeDate
        }, true);
    },

    async resolve(marketId, resolution) {
        return await apiCall(`/markets/${marketId}/resolve`, 'POST', {
            resolution
        }, true);
    }
};

// Bets API
const betsAPI = {
    async place(marketId, position, amount) {
        return await apiCall('/bets', 'POST', {
            marketId,
            position,
            amount
        }, true);
    },

    async getMyBets() {
        return await apiCall('/bets/my', 'GET', null, true);
    }
};

// Invite API
const inviteAPI = {
    async validate(code) {
        return await apiCall(`/invite/validate/${code}`, 'GET');
    }
};
