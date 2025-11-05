using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using WebUI.Models;

namespace WebUI.Controllers;

/// <summary>
/// کنترلر محصولات برای Web UI
/// </summary>
public class ProductsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IHttpClientFactory httpClientFactory, ILogger<ProductsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.GetAsync("/api/products");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ProductViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProductViewModel>();
                return View(products);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products");
        }

        return View(new List<ProductViewModel>());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] ProductCreateModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var payload = new
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                InitialStock = model.InitialStock
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/products", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "محصول با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"خطا در ایجاد محصول: {errorContent}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            TempData["ErrorMessage"] = $"خطا در ایجاد محصول: {ex.Message}";
        }

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.GetAsync($"/api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(product);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product details");
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.GetAsync($"/api/products/{id}");
            if (!response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var model = new ProductCreateModel
            {
                Name = product?.Name ?? string.Empty,
                Description = product?.Description ?? string.Empty,
                Price = product?.Price ?? 0,
                InitialStock = product?.StockQuantity ?? 0
            };
            ViewBag.ProductId = id;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product for edit");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] ProductCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = id;
            return View(model);
        }
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var payload = new { Name = model.Name, Description = model.Description, Price = model.Price };
            var json = JsonSerializer.Serialize(payload);
            var response = await client.PutAsync($"/api/products/{id}", new StringContent(json, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "محصول با موفقیت ویرایش شد";
                return RedirectToAction(nameof(Index));
            }
            var err = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"خطا در ویرایش محصول: {err}";
            ViewBag.ProductId = id;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing product");
            TempData["ErrorMessage"] = $"خطا در ویرایش محصول: {ex.Message}";
            ViewBag.ProductId = id;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DefaultClient");
            var response = await client.DeleteAsync($"/api/products/{id}");
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "محصول حذف شد";
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"خطا در حذف محصول: {err}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            TempData["ErrorMessage"] = $"خطا در حذف محصول: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}

