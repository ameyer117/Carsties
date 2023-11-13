using MongoDB.Entities;

namespace SearchService.Models;

public class Item : Entity
{
    // Auction
    public int ReservePrice { get; set; }
    public string? Seller { get; set; }
    public string? Winner { get; set; }
    public int SoldAmount { get; set; }
    public int CurrentHighBid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string? Status { get; set; }

    // Item
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
    public required string ImageUrl { get; set; }
}