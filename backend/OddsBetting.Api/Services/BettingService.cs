using OddsBetting.Api.Models;

namespace OddsBetting.Api.Services;

public class BettingService
{
    private readonly SessionStorageService _storage;

    public BettingService(SessionStorageService storage)
    {
        _storage = storage;
    }

    public Market CreateMarket(CreateMarketRequest request, string userId)
    {
        var market = new Market
        {
            Question = request.Question,
            Description = request.Description,
            CreatedBy = userId,
            CloseDate = request.CloseDate
        };

        _storage.AddMarket(market);
        return market;
    }

    public List<Market> GetAllMarkets()
    {
        return _storage.GetAllMarkets();
    }

    public Market? GetMarket(string marketId)
    {
        return _storage.GetMarket(marketId);
    }

    public Bet? PlaceBet(PlaceBetRequest request, string userId)
    {
        var market = _storage.GetMarket(request.MarketId);
        if (market == null || market.Status != MarketStatus.Open)
        {
            return null;
        }

        var user = _storage.GetUserById(userId);
        if (user == null || user.Balance < request.Amount)
        {
            return null;
        }

        // Calculate potential payout based on current pool sizes
        var totalPool = market.YesPool + market.NoPool + request.Amount;
        var positionPool = request.Position.ToUpper() == "YES" ? market.YesPool + request.Amount : market.NoPool + request.Amount;
        var potentialPayout = totalPool * (request.Amount / positionPool);

        var bet = new Bet
        {
            MarketId = request.MarketId,
            UserId = userId,
            Position = request.Position.ToUpper(),
            Amount = request.Amount,
            PotentialPayout = potentialPayout
        };

        // Update user balance
        user.Balance -= request.Amount;
        _storage.UpdateUser(user);

        // Update market pools
        if (request.Position.ToUpper() == "YES")
        {
            market.YesPool += request.Amount;
        }
        else
        {
            market.NoPool += request.Amount;
        }
        _storage.UpdateMarket(market);

        _storage.AddBet(bet);
        return bet;
    }

    public List<Bet> GetUserBets(string userId)
    {
        return _storage.GetBetsByUser(userId);
    }

    public bool ResolveMarket(string marketId, string resolution, string userId)
    {
        var market = _storage.GetMarket(marketId);
        if (market == null || market.CreatedBy != userId || market.Status == MarketStatus.Resolved)
        {
            return false;
        }

        resolution = resolution.ToUpper();
        if (resolution != "YES" && resolution != "NO")
        {
            return false;
        }

        market.Status = MarketStatus.Resolved;
        market.Resolution = resolution;
        market.ResolvedAt = DateTime.UtcNow;
        _storage.UpdateMarket(market);

        // Process all bets for this market
        var bets = _storage.GetBetsByMarket(marketId);
        var totalPool = market.YesPool + market.NoPool;
        var winningPool = resolution == "YES" ? market.YesPool : market.NoPool;

        foreach (var bet in bets)
        {
            if (bet.Position == resolution)
            {
                // Winner
                var payout = (bet.Amount / winningPool) * totalPool;
                bet.Status = BetStatus.Won;
                bet.Payout = payout;

                var user = _storage.GetUserById(bet.UserId);
                if (user != null)
                {
                    user.Balance += payout;
                    _storage.UpdateUser(user);
                }
            }
            else
            {
                // Loser
                bet.Status = BetStatus.Lost;
                bet.Payout = 0;
            }
            _storage.UpdateBet(bet);
        }

        return true;
    }
}
