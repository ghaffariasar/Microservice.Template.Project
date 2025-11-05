using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappings;

/// <summary>
/// AutoMapper Profile برای Order و OrderItem Entities
/// </summary>
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>();
    }
}

