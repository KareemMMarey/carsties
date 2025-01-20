using System;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelper;

namespace SearchService.Controllers;
[ApiController]
[Route("api/search")]
public class SearchController: ControllerBase
{
    [HttpGet]   
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams){
        //var query = DB.Find<Item>();  

        var query = DB.PagedSearch<Item,Item>(); // For pagination
        //query.Sort(x=>x.Ascending(a=>a.Make));
        if(!string.IsNullOrEmpty(searchParams.SearchTerm)){
            query.Match(Search.Full,searchTerm:searchParams.SearchTerm).SortByTextScore();
        }
        if(!string.IsNullOrEmpty(searchParams.Seller)){
            query.Match(x=>x.Seller==searchParams.Seller);
        }
         if(!string.IsNullOrEmpty(searchParams.Winner)){
            query.Match(x=>x.Winner==searchParams.Winner);
        }
        query = searchParams.OrderBy switch{
            "make" => query.Sort(c=>c.Ascending(b=>b.Make)),
            "new" => query.Sort(c=>c.Descending(b=>b.CreatedAt)),
           _ => query.Sort(c=>c.Ascending(b=>b.AuctionEnd)),
        };
        query = searchParams.FilterBy switch{
            "finished" => query.Match(c=>c.AuctionEnd<DateTime.UtcNow),
            "endingSoon" => query.Match(c=>c.AuctionEnd<DateTime.UtcNow.AddHours(6)&& c.AuctionEnd>DateTime.UtcNow),
           _ => query.Match(c=>c.AuctionEnd>DateTime.UtcNow),
        };
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync(); 
        return Ok(new {results=result.Results,pageCount= result.PageCount, totalCount= result.TotalCount});
    }

}
