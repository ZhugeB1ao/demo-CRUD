using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers;

[Route("Cart")]
public class CartController : Controller
{
    private readonly AppDBContext _context;
    private readonly UserManager<AppUser> _userManager;
    private const string CartSessionKey = "ShoppingCart";

    public CartController(AppDBContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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

    /// <summary>
    /// Checkout - requires user to be logged in
    /// Creates an Order and saves to database
    /// </summary>
    [HttpPost("Checkout")]
    public async Task<IActionResult> Checkout()
    {
        // Check if user is logged in
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // Redirect to login page
            return RedirectToAction("Login", "User");
        }

        // Get cart from session
        var cart = GetCart();
        if (cart.Count == 0)
        {
            TempData["Error"] = "Your cart is empty!";
            return RedirectToAction("Index");
        }

        try
        {
            // First, create the order (without items)
            var order = new Order
            {
                UserId = user.Id,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                Total = 0,
                OrderProducts = new List<OrderProduct>()
            };

            // Add order to context first
            _context.Orders.Add(order);
            
            // Save to get the Order ID
            await _context.SaveChangesAsync();

            // Now add order products
            decimal total = 0;
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    decimal price = product.Price;
                    int quantity = item.Quantity.HasValue ? item.Quantity.Value : 1;
                    total += price * quantity;

                    // Create OrderProduct with proper foreign keys
                    var orderProduct = new OrderProduct
                    {
                        OrderId = order.Id ?? 0,  // Now we have the Order ID
                        ProductId = product.Id,
                        Quantity = quantity,
                        Price = (double)price  // Store price at time of purchase
                    };

                    _context.OrderProducts.Add(orderProduct);
                }
            }

            // Update order total
            order.Total = total;
            
            // Save all order products and update order
            await _context.SaveChangesAsync();

            // Clear the cart from session
            HttpContext.Session.Remove(CartSessionKey);

            // Redirect to success page or back to shop
            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("Index", "Shop");
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while placing the order. Please try again.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Get payment history for logged-in user
    /// Returns HTML for the modal
    /// </summary>
    [HttpGet("PaymentHistory")]
    public async Task<IActionResult> PaymentHistory()
    {
        // Check if user is logged in
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        // Get all orders for this user with OrderProducts included
        var orders = await _context.Orders
            .Where(o => o.UserId == user.Id)
            .Include(o => o.OrderProducts)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Load products for each order item
        foreach (var order in orders)
        {
            if (order.OrderProducts != null)
            {
                foreach (var item in order.OrderProducts)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
            }
        }

        return PartialView("_PaymentHistoryPartial", orders);
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