# OddsBetting - Friends

An invite-only prediction markets application for friends, similar to Kalshi or Polymarket.

## Features

- **Invite-Only Access**: New users can only register with a valid invite code
- **Create Markets**: Users can create prediction markets with YES/NO outcomes
- **Place Bets**: Bet on market outcomes with virtual currency
- **Market Resolution**: Market creators can resolve markets and distribute winnings
- **Real-time Balance**: Track your balance and betting history

## Technology Stack

- **Frontend**: Vanilla JavaScript, HTML5, CSS3
- **Backend**: ASP.NET Core (.NET 10) Web API
- **Storage**: Session storage (in-memory, designed for future Azure integration)

## Getting Started

### Prerequisites

- .NET 10 SDK
- A modern web browser

### Installation

1. Clone the repository:
```bash
git clone https://github.com/idusortus/oddsbetting-friends.git
cd oddsbetting-friends
```

2. Start the backend API:
```bash
cd backend/OddsBetting.Api
dotnet run
```

The API will start at `http://localhost:5000`

3. Open the frontend:
```bash
cd ../../frontend
# Simply open index.html in your browser, or use a local web server:
python -m http.server 8000
# Or with Node.js:
npx http-server -p 8000
```

Then navigate to `http://localhost:8000` in your browser.

### First User Registration

Use the invite code: **FIRSTUSER** to register the first user. After registration, you'll receive your own invite code to share with friends.

## Usage

1. **Register**: Use an invite code to create an account
2. **Login**: Access your dashboard with your credentials
3. **Create Markets**: Ask questions and create YES/NO prediction markets
4. **Place Bets**: Bet on market outcomes using your balance ($1000 starting balance)
5. **Invite Friends**: Share your invite code with friends
6. **Resolve Markets**: Market creators resolve their markets, distributing winnings to correct bettors

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/logout` - Logout user
- `GET /api/auth/me` - Get current user info

### Markets
- `GET /api/markets` - List all markets
- `GET /api/markets/{id}` - Get market by ID
- `POST /api/markets` - Create new market
- `POST /api/markets/{id}/resolve` - Resolve market

### Bets
- `POST /api/bets` - Place a bet
- `GET /api/bets/my` - Get user's bets

### Invite
- `GET /api/invite/validate/{code}` - Validate invite code

## Future Enhancements

- Azure SQL Database integration
- User authentication with Azure AD
- Real money integration
- Mobile app
- Advanced market types
- Market categories and search
- User profiles and statistics
- Market comments and discussions

## Development Notes

The application currently uses in-memory session storage. All data is lost when the server restarts. The architecture is designed to easily migrate to Azure SQL Database in the future.

## License

MIT License
