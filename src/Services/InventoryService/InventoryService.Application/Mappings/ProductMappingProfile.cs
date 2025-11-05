using AutoMapper;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Mappings;

/// <summary>
/// AutoMapper Profile برای Product Entity
/// </summary>
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.AvailableQuantity));
    }
}

