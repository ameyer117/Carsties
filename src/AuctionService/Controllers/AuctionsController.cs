using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    // GET: api/Auctions
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
        
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    // GET: api/Auctions/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AuctionDto>(auction));
    }
    
    // POST: api/Auctions
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> PostAuction(CreateAuctionDto auctionDto)
    {
        // Create a new auction from the create auction dto
        var auction = _mapper.Map<Auction>(auctionDto);
        
        
        _context.Auctions.Add(auction);
        
        var newAuction = _mapper.Map<AuctionDto>(auction);
        
        // Publish the auction created event
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
    }
    
    // PUT: api/Auctions/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        // Update the auction (Item) with the new values
        _mapper.Map(auctionDto, auction.Item);

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
            
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    // DELETE: api/Auctions/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        _context.Auctions.Remove(auction);
        
        await _publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});
        
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
