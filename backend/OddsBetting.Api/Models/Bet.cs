namespace OddsBetting.Api.Models;

public class Bet
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MarketId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty; // "YES" or "NO"
    public decimal Amount { get; set; }
    public decimal PotentialPayout { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public BetStatus Status { get; set; } = BetStatus.Active;
    public decimal? Payout { get; set; }
}

public enum BetStatus
{
    Active,
    Won,
    Lost
}

public class PlaceBetRequest
{
    public string MarketId { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty; // "YES" or "NO"
    public decimal Amount { get; set; }
}
