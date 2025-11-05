namespace WebUI.Models;

public class OrderViewModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();

    public string StatusText => Status switch
    {
        0 => "در انتظار",
        1 => "تایید شده",
        2 => "در حال پردازش",
        3 => "ارسال شده",
        4 => "تحویل شده",
        5 => "تکمیل شده",
        6 => "لغو شده",
        _ => Status.ToString()
    };
}

public class OrderItemViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}


