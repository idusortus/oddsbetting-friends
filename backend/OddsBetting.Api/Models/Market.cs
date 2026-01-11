namespace OddsBetting.Api.Models;

public class Market
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Question { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CloseDate { get; set; }
    public MarketStatus Status { get; set; } = MarketStatus.Open;
    public string? Resolution { get; set; } // "YES" or "NO"
    public DateTime? ResolvedAt { get; set; }
    public decimal YesPool { get; set; } = 0;
    public decimal NoPool { get; set; } = 0;
}

public enum MarketStatus
{
    Open,
    Closed,
    Resolved
}

public class CreateMarketRequest
{
    public string Question { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? CloseDate { get; set; }
}

public class ResolveMarketRequest
{
    public string Resolution { get; set; } = string.Empty; // "YES" or "NO"
}
