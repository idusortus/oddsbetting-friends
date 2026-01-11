using OddsBetting.Api.Models;

namespace OddsBetting.Api.Services;

public class SessionStorageService
{
    private readonly Dictionary<string, User> _users = new();
    private readonly Dictionary<string, Market> _markets = new();
    private readonly Dictionary<string, Bet> _bets = new();
    private readonly Dictionary<string, string> _inviteCodes = new(); // inviteCode -> userId
    private readonly Dictionary<string, string> _sessions = new(); // sessionToken -> userId

    // User methods
    public User? GetUserById(string userId)
    {
        return _users.TryGetValue(userId, out var user) ? user : null;
    }

    public User? GetUserByEmail(string email)
    {
        return _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public void AddUser(User user)
    {
        _users[user.Id] = user;
        if (!string.IsNullOrEmpty(user.InviteCode))
        {
            _inviteCodes[user.InviteCode] = user.Id;
        }
    }

    public void UpdateUser(User user)
    {
        _users[user.Id] = user;
    }

    public void InitializeDefaultInviteCode(string code)
    {
        if (!_inviteCodes.ContainsKey(code))
        {
            _inviteCodes[code] = "system"; // System-generated code
        }
    }

    public bool ValidateInviteCode(string inviteCode)
    {
        return _inviteCodes.ContainsKey(inviteCode);
    }

    // Session methods
    public void CreateSession(string sessionToken, string userId)
    {
        _sessions[sessionToken] = userId;
    }

    public string? GetUserIdBySession(string sessionToken)
    {
        return _sessions.TryGetValue(sessionToken, out var userId) ? userId : null;
    }

    public void DeleteSession(string sessionToken)
    {
        _sessions.Remove(sessionToken);
    }

    // Market methods
    public void AddMarket(Market market)
    {
        _markets[market.Id] = market;
    }

    public Market? GetMarket(string marketId)
    {
        return _markets.TryGetValue(marketId, out var market) ? market : null;
    }

    public List<Market> GetAllMarkets()
    {
        return _markets.Values.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public void UpdateMarket(Market market)
    {
        _markets[market.Id] = market;
    }

    // Bet methods
    public void AddBet(Bet bet)
    {
        _bets[bet.Id] = bet;
    }

    public Bet? GetBet(string betId)
    {
        return _bets.TryGetValue(betId, out var bet) ? bet : null;
    }

    public List<Bet> GetBetsByUser(string userId)
    {
        return _bets.Values.Where(b => b.UserId == userId).OrderByDescending(b => b.PlacedAt).ToList();
    }

    public List<Bet> GetBetsByMarket(string marketId)
    {
        return _bets.Values.Where(b => b.MarketId == marketId).ToList();
    }

    public void UpdateBet(Bet bet)
    {
        _bets[bet.Id] = bet;
    }
}
