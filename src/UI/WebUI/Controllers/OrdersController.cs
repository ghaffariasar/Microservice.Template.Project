using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;
using WebUI.Models;

namespace WebUI.Controllers;

/// <summary>
/// کنترلر سفارش‌ها برای Web UI
/// </summary>
public class OrdersController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IHttpClientFactory httpClientFactory, ILogger<OrdersController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.GetAsync("/api/orders");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<OrderViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<OrderViewModel>();
                return View(orders);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders");
        }

        return View(new List<OrderViewModel>());
    }

    public async Task<IActionResult> Create()
    {
        await LoadProductsForViewAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] OrderCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadProductsForViewAsync();
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var payload = new
            {
                CustomerId = model.CustomerId,
                Items = model.Items?.Select(i => new { i.ProductId, i.ProductName, i.Quantity, i.UnitPrice }).ToList(),
                IdempotencyKey = Guid.NewGuid().ToString()
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/orders", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "سفارش با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"خطا در ایجاد سفارش: {errorContent}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            TempData["ErrorMessage"] = $"خطا در ایجاد سفارش: {ex.Message}";
            }

            await LoadProductsForViewAsync();
            return View(model);
    }

    private async Task LoadProductsForViewAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.GetAsync("/api/products");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ProductViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProductViewModel>();
                ViewBag.Products = products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
                ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(p => new { id = p.Id, name = p.Name }));
            }
            else
            {
                ViewBag.Products = new List<SelectListItem>();
                ViewBag.ProductsJson = "[]";
            }
        }
        catch
        {
            ViewBag.Products = new List<SelectListItem>();
            ViewBag.ProductsJson = "[]";
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var idempotencyKey = Guid.NewGuid().ToString();
            client.DefaultRequestHeaders.Remove(Shared.Common.HeaderNames.IdempotencyKey);
            client.DefaultRequestHeaders.Add(Shared.Common.HeaderNames.IdempotencyKey, idempotencyKey);

            var response = await client.PostAsync($"/api/orders/{id}/confirm-payment", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "سفارش با موفقیت نهایی شد و موجودی ثبت گردید";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"خطا در نهایی‌سازی سفارش: {error}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for order {OrderId}", id);
            TempData["ErrorMessage"] = $"خطا در نهایی‌سازی سفارش: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.DeleteAsync($"/api/orders/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "سفارش حذف شد";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"خطا در حذف سفارش: {error}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            TempData["ErrorMessage"] = $"خطا در حذف سفارش: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.GetAsync($"/api/orders/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var order = JsonSerializer.Deserialize<OrderViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(order);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order details");
        }

        return RedirectToAction(nameof(Index));
    }
}

