using System;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;
[ApiController]
[Route("api/auctions")]
public class AuctionController:ControllerBase
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;

    public AuctionController(AuctionDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>>  GetAllAuctions(string date){

        var query = _context.Auctions.OrderBy(x=>x.Item.Make).AsQueryable();
        if(!string.IsNullOrEmpty(date)){
            query = query.Where(x=>x.LastUpdatedAt.CompareTo(DateTime.Parse(date))>0) ;
        }  
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        // var list = await _context.Auctions.Include(c=>c.Item).OrderBy(c=>c.Item.Make).ToListAsync();
        // return _mapper.Map<List<AuctionDto>>(list);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id){
        var item = await _context.Auctions.Include(c=>c.Item).FirstOrDefaultAsync(c => c.Id == id);
        if (item == null)   return NotFound();  
        return _mapper.Map<AuctionDto>(item) ;
    }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto){
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync()>0;
        if(!result) return  BadRequest("Could not save changes to DB");
        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id,UpdateAuctionDto auctionDto){
        var auction = await _context.Auctions.Include(c=>c.Item).FirstOrDefaultAsync(c => c.Id == id);
        auction.Item.Make = auctionDto.Make??auction.Item.Make;
        auction.Item.Model = auctionDto.Model??auction.Item.Model;
        auction.Item.Year = auctionDto.Year??auction.Item.Year;
        auction.Item.Mileage = auctionDto.Mileage??auction.Item.Mileage;
        auction.Item.Color = auctionDto.Color??auction.Item.Color;
        var result = await _context.SaveChangesAsync()>0;
        if(!result) return  BadRequest("Could not save changes to DB");
        return  Ok(result);
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id){
        var auction = await _context.Auctions.Include(c=>c.Item).FirstOrDefaultAsync(c => c.Id == id);
        if(auction == null)return NotFound();

        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync()>0;
        if(!result) return  BadRequest("Could not update DB");
        return  Ok();
    }


}
