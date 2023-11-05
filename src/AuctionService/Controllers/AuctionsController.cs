using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuctionDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions
                .OrderBy(x => x.Item.Make)
                .AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            return _mapper.Map<AuctionDTO>(auction);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO create)
        {
            var auction = _mapper.Map<Auction>(create);

            auction.Seller = User.Identity.Name;

            _context.Auctions.Add(auction);

            var newAuction = _mapper.Map<AuctionDTO>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Problem creating auction");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO update)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();

            auction.Item.Make = update.Make ?? auction.Item.Make;
            auction.Item.Model = update.Model ?? auction.Item.Model;
            auction.Item.Color = update.Color ?? auction.Item.Color;
            auction.Item.Mileage = update.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = update.Year ?? auction.Item.Year;

            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            return BadRequest("Problem updating auction");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();

            _context.Auctions.Remove(auction);

            await _publishEndpoint.Publish(new AuctionDeleted { Id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Problem deleting auction");

            return Ok();
        }
    }
}