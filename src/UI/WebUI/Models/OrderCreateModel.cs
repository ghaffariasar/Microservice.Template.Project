using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;

public class OrderCreateModel
{
    [Required]
    public Guid CustomerId { get; set; }

    [MinLength(1, ErrorMessage = "حداقل یک آیتم سفارش لازم است")] 
    public List<OrderItemModel> Items { get; set; } = new();
}

public class OrderItemModel
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}


