using System;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelper;

public class MappingProfile:Profile
{
    public MappingProfile()
    {
        CreateMap<Auction,AuctionDto>().IncludeMembers(x => x.Item); 
        CreateMap<Item,AuctionDto>();
        CreateMap<CreateAuctionDto,Auction>().ForMember(d=>d.Item, o=>o.MapFrom(x=>x));
        CreateMap<CreateAuctionDto,Item>();
    }

}