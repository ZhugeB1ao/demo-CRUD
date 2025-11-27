using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Text.Json;

namespace WebApplication1.Controllers;

[Route("Cart")]
public class CartController : Controller
{
    private readonly AppDBContext _context;
    private const string CartSessionKey = "ShoppingCart";

    public CartController(AppDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Display cart page
    /// </summary>

    [HttpGet("")]
    public IActionResult Index()
    {
        var cart = GetCart();
        
        // Create an Order object to represent the cart
        var order = new Order
        {
            OrderProducts = cart
        };

        // Populate Product details for each item
        foreach (var item in order.OrderProducts)
        {
            item.Product = _context.Products.Find(item.ProductId);
        }

        return View(order);
    }

    /// <summary>
    /// Add product to cart
    /// </summary>
    [HttpPost("Add")]
    public IActionResult AddToCart(int productId, int quantity = 1)
    {
        var product = _context.Products.Find(productId);
        
        if (product == null)
        {
            return NotFound("Product not found");
        }

        var cart = GetCart();
        
        // Check if product already exists in cart
        var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);
        
        if (existingItem != null)
        {
            // Update quantity if product already in cart
            existingItem.Quantity = (existingItem.Quantity ?? 0) + quantity;
        }
        else
        {
            // Add new item to cart
            cart.Add(new OrderProduct
            {
                ProductId = productId,
                Quantity = quantity,
                Price = (double)product.Price
            });
        }

        SaveCart(cart);
        
        TempData["SuccessMessage"] = $"Add success full {product.Name}";
        return RedirectToAction("Index", "Shop");
    }

    /// <summary>
    /// Increment quantity by 1
    /// </summary>
    [HttpPost("Increment")]
    public IActionResult IncrementQuantity(int productId)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        
        if (item != null)
        {
            item.Quantity = (item.Quantity ?? 0) + 1;
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Decrement quantity by 1
    /// </summary>
    [HttpPost("Decrement")]
    public IActionResult DecrementQuantity(int productId)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        
        if (item != null)
        {
            var newQuantity = (item.Quantity ?? 0) - 1;
            
            if (newQuantity <= 0)
            {
                // Remove item if quantity becomes 0
                cart.Remove(item);
            }
            else
            {
                item.Quantity = newQuantity;
            }
            
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Update quantity directly
    /// </summary>
    [HttpPost("Update")]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            return RemoveFromCart(productId);
        }

        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        
        if (item != null)
        {
            item.Quantity = quantity;
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Remove product from cart
    /// </summary>
    [HttpPost("Remove")]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        
        if (item != null)
        {
            cart.Remove(item);
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    [HttpPost("Clear")]
    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove(CartSessionKey);
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Get cart item count (for displaying in navbar)
    /// </summary>
    [HttpGet("Count")]
    public IActionResult GetCartCount()
    {
        var cart = GetCart();
        return Json(new { count = cart.Sum(c => c.Quantity ?? 0) });
    }

    #region Helper Methods

    /// <summary>
    /// Get cart from session as List of OrderProduct
    /// </summary>
    private List<OrderProduct> GetCart()
    {
        var cartJson = HttpContext.Session.GetString(CartSessionKey);
        
        if (string.IsNullOrEmpty(cartJson))
        {
            return new List<OrderProduct>();
        }

        return JsonSerializer.Deserialize<List<OrderProduct>>(cartJson) ?? new List<OrderProduct>();
    }

    /// <summary>
    /// Save cart to session
    /// </summary>
    private void SaveCart(List<OrderProduct> cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        HttpContext.Session.SetString(CartSessionKey, cartJson);
    }

    #endregion
}