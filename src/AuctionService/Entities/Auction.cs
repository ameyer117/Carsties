namespace AuctionService.Entities;

public class Auction
{
    public Guid Id { get; set; }

    public int ReservePrice { get; set; }

    public string? Seller { get; set; }

    public string? Winner { get; set; }

    public int? SoldAmount { get; set; }

    public int? CurrentHighBid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime AuctionEnd { get; set; }

    public StatusType Status { get; set; }

    // Navigation properties
    public Item Item { get; set; } = default!;
}

public enum StatusType
{
    Live = 1,
    Finished = 2,
    ReserveNotMet = 3
}
