// Application State
let currentUser = null;
let selectedPosition = null;
let selectedResolution = null;

// Page Navigation
function showPage(pageId) {
    document.querySelectorAll('.page').forEach(page => {
        page.classList.remove('active');
    });
    document.getElementById(pageId).classList.add('active');
}

// Initialize App
async function init() {
    const token = getSessionToken();
    
    if (token) {
        try {
            currentUser = await authAPI.getCurrentUser();
            showDashboard();
        } catch (error) {
            console.error('Session expired or invalid');
            showPage('loginPage');
        }
    } else {
        showPage('loginPage');
    }
    
    setupEventListeners();
}

// Setup Event Listeners
function setupEventListeners() {
    // Auth form toggles
    document.getElementById('showRegister').addEventListener('click', (e) => {
        e.preventDefault();
        document.getElementById('loginForm').classList.remove('active');
        document.getElementById('registerForm').classList.add('active');
    });

    document.getElementById('showLogin').addEventListener('click', (e) => {
        e.preventDefault();
        document.getElementById('registerForm').classList.remove('active');
        document.getElementById('loginForm').classList.add('active');
    });

    // Login form
    document.getElementById('loginFormElement').addEventListener('submit', async (e) => {
        e.preventDefault();
        const email = document.getElementById('loginEmail').value;
        const password = document.getElementById('loginPassword').value;

        try {
            const response = await authAPI.login(email, password);
            currentUser = response.user;
            showDashboard();
        } catch (error) {
            alert('Login failed. Please check your credentials.');
        }
    });

    // Register form
    document.getElementById('registerFormElement').addEventListener('submit', async (e) => {
        e.preventDefault();
        const username = document.getElementById('registerUsername').value;
        const email = document.getElementById('registerEmail').value;
        const password = document.getElementById('registerPassword').value;
        const inviteCode = document.getElementById('registerInviteCode').value;

        try {
            await authAPI.register(username, email, password, inviteCode);
            alert('Registration successful! Please login.');
            document.getElementById('showLogin').click();
        } catch (error) {
            alert('Registration failed. Please check your invite code.');
        }
    });

    // Logout
    document.getElementById('logoutBtn').addEventListener('click', async () => {
        await authAPI.logout();
        currentUser = null;
        showPage('loginPage');
    });

    // Tabs
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            const tabName = btn.dataset.tab;
            
            document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            
            document.querySelectorAll('.tab-content').forEach(content => {
                content.classList.remove('active');
            });
            
            if (tabName === 'all') {
                document.getElementById('allMarketsTab').classList.add('active');
                loadMarkets();
            } else if (tabName === 'my-bets') {
                document.getElementById('myBetsTab').classList.add('active');
                loadMyBets();
            }
        });
    });

    // Create Market
    document.getElementById('createMarketBtn').addEventListener('click', () => {
        openModal('createMarketModal');
    });

    document.getElementById('createMarketForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const question = document.getElementById('marketQuestion').value;
        const description = document.getElementById('marketDescription').value;
        const closeDate = document.getElementById('marketCloseDate').value || null;

        try {
            await marketsAPI.create(question, description, closeDate);
            closeModal('createMarketModal');
            document.getElementById('createMarketForm').reset();
            loadMarkets();
        } catch (error) {
            alert('Failed to create market');
        }
    });

    // Place Bet Form
    document.querySelectorAll('.position-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.position-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            selectedPosition = btn.dataset.position;
        });
    });

    document.getElementById('placeBetForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        if (!selectedPosition) {
            alert('Please select a position (YES or NO)');
            return;
        }

        const marketId = document.getElementById('betMarketId').value;
        const amount = parseFloat(document.getElementById('betAmount').value);

        try {
            await betsAPI.place(marketId, selectedPosition, amount);
            closeModal('placeBetModal');
            document.getElementById('placeBetForm').reset();
            selectedPosition = null;
            document.querySelectorAll('.position-btn').forEach(b => b.classList.remove('active'));
            loadMarkets();
            loadMyBets();
            await updateUserInfo();
        } catch (error) {
            alert('Failed to place bet. Check your balance and try again.');
        }
    });

    // Resolve Market Form
    document.querySelectorAll('.resolution-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.resolution-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            selectedResolution = btn.dataset.resolution;
        });
    });

    document.getElementById('resolveMarketForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        
        if (!selectedResolution) {
            alert('Please select a resolution (YES or NO)');
            return;
        }

        const marketId = document.getElementById('resolveMarketId').value;

        try {
            await marketsAPI.resolve(marketId, selectedResolution);
            closeModal('resolveMarketModal');
            selectedResolution = null;
            document.querySelectorAll('.resolution-btn').forEach(b => b.classList.remove('active'));
            loadMarkets();
            loadMyBets();
            await updateUserInfo();
        } catch (error) {
            alert('Failed to resolve market');
        }
    });

    // Show Invite Code
    document.getElementById('showInviteCodeBtn').addEventListener('click', () => {
        document.getElementById('inviteCodeDisplay').textContent = currentUser.inviteCode;
        openModal('inviteCodeModal');
    });

    document.getElementById('copyInviteCode').addEventListener('click', () => {
        const code = currentUser.inviteCode;
        navigator.clipboard.writeText(code).then(() => {
            alert('Invite code copied to clipboard!');
        });
    });

    // Modal close buttons
    document.querySelectorAll('.close').forEach(closeBtn => {
        closeBtn.addEventListener('click', () => {
            closeBtn.closest('.modal').classList.remove('active');
        });
    });

    // Close modal when clicking outside
    window.addEventListener('click', (e) => {
        if (e.target.classList.contains('modal')) {
            e.target.classList.remove('active');
        }
    });
}

// Dashboard Functions
async function showDashboard() {
    showPage('dashboardPage');
    await updateUserInfo();
    await loadMarkets();
}

async function updateUserInfo() {
    try {
        currentUser = await authAPI.getCurrentUser();
        document.getElementById('userName').textContent = currentUser.username;
        document.getElementById('userBalance').textContent = `Balance: $${currentUser.balance.toFixed(2)}`;
    } catch (error) {
        console.error('Failed to update user info');
    }
}

async function loadMarkets() {
    const marketsList = document.getElementById('marketsList');
    marketsList.innerHTML = '<p class="loading">Loading markets...</p>';

    try {
        const markets = await marketsAPI.getAll();
        
        if (markets.length === 0) {
            marketsList.innerHTML = '<p class="loading">No markets yet. Create one!</p>';
            return;
        }

        marketsList.innerHTML = markets.map(market => createMarketCard(market)).join('');
    } catch (error) {
        marketsList.innerHTML = '<p class="error">Failed to load markets</p>';
    }
}

function createMarketCard(market) {
    const statusMap = {0: 'open', 1: 'closed', 2: 'resolved'};
    const statusClass = statusMap[market.status] || 'open';
    const canBet = market.status === 0; // Open
    const canResolve = currentUser && market.createdBy === currentUser.id && market.status !== 2; // Not resolved and created by user
    
    const totalPool = market.yesPool + market.noPool;
    const yesPercent = totalPool > 0 ? ((market.yesPool / totalPool) * 100).toFixed(0) : 50;
    const noPercent = totalPool > 0 ? ((market.noPool / totalPool) * 100).toFixed(0) : 50;

    let statusText = 'Open';
    if (market.status === 1) statusText = 'Closed';
    if (market.status === 2) statusText = `Resolved: ${market.resolution}`;

    return `
        <div class="market-card">
            <div class="market-header">
                <h3 class="market-question">${escapeHtml(market.question)}</h3>
                <span class="market-status status-${statusClass}">${statusText}</span>
            </div>
            <p class="market-description">${escapeHtml(market.description)}</p>
            <div class="market-pools">
                <div class="pool pool-yes">
                    <div class="pool-label">YES ${yesPercent}%</div>
                    <div class="pool-amount">$${market.yesPool.toFixed(2)}</div>
                </div>
                <div class="pool pool-no">
                    <div class="pool-label">NO ${noPercent}%</div>
                    <div class="pool-amount">$${market.noPool.toFixed(2)}</div>
                </div>
            </div>
            <div class="market-actions">
                ${canBet ? `<button class="btn btn-primary" onclick="openBetModal('${market.id}')">Place Bet</button>` : ''}
                ${canResolve ? `<button class="btn btn-secondary" onclick="openResolveModal('${market.id}')">Resolve</button>` : ''}
            </div>
            <div class="market-meta">
                Created ${formatDate(market.createdAt)}
                ${market.closeDate ? ` • Closes ${formatDate(market.closeDate)}` : ''}
            </div>
        </div>
    `;
}

async function loadMyBets() {
    const betsList = document.getElementById('betsList');
    betsList.innerHTML = '<p class="loading">Loading bets...</p>';

    try {
        const bets = await betsAPI.getMyBets();
        
        if (bets.length === 0) {
            betsList.innerHTML = '<p class="loading">No bets yet. Place one!</p>';
            return;
        }

        // Get market details for each bet
        const markets = await marketsAPI.getAll();
        const marketMap = {};
        markets.forEach(m => marketMap[m.id] = m);

        betsList.innerHTML = bets.map(bet => createBetCard(bet, marketMap[bet.marketId])).join('');
    } catch (error) {
        betsList.innerHTML = '<p class="error">Failed to load bets</p>';
    }
}

function createBetCard(bet, market) {
    let statusText = 'Active';
    if (bet.status === 1) statusText = 'Won';
    if (bet.status === 2) statusText = 'Lost';

    return `
        <div class="bet-card">
            <div class="bet-header">
                <h3>${escapeHtml(market?.question || 'Unknown Market')}</h3>
                <span class="bet-position position-${bet.position.toLowerCase()}">${bet.position}</span>
            </div>
            <div class="bet-details">
                <div class="bet-detail">
                    <div class="bet-detail-label">Amount</div>
                    <div class="bet-detail-value">$${bet.amount.toFixed(2)}</div>
                </div>
                <div class="bet-detail">
                    <div class="bet-detail-label">Status</div>
                    <div class="bet-detail-value">${statusText}</div>
                </div>
                <div class="bet-detail">
                    <div class="bet-detail-label">Potential/Payout</div>
                    <div class="bet-detail-value">$${(bet.payout || bet.potentialPayout).toFixed(2)}</div>
                </div>
            </div>
            <div class="market-meta">Placed ${formatDate(bet.placedAt)}</div>
        </div>
    `;
}

// Modal Functions
function openModal(modalId) {
    document.getElementById(modalId).classList.add('active');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

async function openBetModal(marketId) {
    const market = await marketsAPI.getById(marketId);
    
    document.getElementById('betMarketId').value = marketId;
    document.getElementById('betMarketInfo').innerHTML = `
        <h3>${escapeHtml(market.question)}</h3>
        <p>${escapeHtml(market.description)}</p>
    `;
    
    openModal('placeBetModal');
}

async function openResolveModal(marketId) {
    const market = await marketsAPI.getById(marketId);
    
    document.getElementById('resolveMarketId').value = marketId;
    document.getElementById('resolveMarketInfo').innerHTML = `
        <h3>${escapeHtml(market.question)}</h3>
        <p>${escapeHtml(market.description)}</p>
    `;
    
    openModal('resolveMarketModal');
}

// Utility Functions
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', init);
