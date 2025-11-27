namespace WebApplication1.Models;

/// <summary>
/// Junction table representing items in an order
/// 
/// This is a bridge between Order and Product
/// Why separate table?
/// - One Order can have many Products
/// - One Product can be in many Orders
/// - Stores price at time of purchase (historical record)
/// - Stores quantity for this specific purchase
/// </summary>
public class OrderProduct
{
    /// <summary>
    /// Unique identifier for this order-product relationship
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Product table
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Navigation property to Product
    /// Allows access to product details (name, image, etc.)
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Foreign key to Order table
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to Order
    /// Allows access to order details
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// How many of this product were ordered
    /// Example: Customer ordered 2 iPhones
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Price at time of purchase
    /// IMPORTANT: This is NOT the current product price
    /// It's the price the customer actually paid
    /// 
    /// Why store this?
    /// - Historical accuracy (product price changes over time)
    /// - Invoice accuracy (shows exact price paid)
    /// - Dispute resolution (proof of original price)
    /// - Reporting (analyze price trends)
    /// </summary>
    public double? Price { get; set; }

    /// <summary>
    /// Calculate subtotal for this line item
    /// Subtotal = Quantity × Price at purchase
    /// </summary>
    public decimal GetSubtotal() => ((Quantity ?? 0) * (decimal)(Price ?? 0));
}

