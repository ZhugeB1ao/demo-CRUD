using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

/// <summary>
/// Represents a customer order
/// 
/// Order Lifecycle:
/// Pending → Paid → Shipped → Delivered (or Cancelled at any stage)
/// </summary>
public class Order
{
    /// <summary>
    /// Unique order identifier
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Foreign key to AppUser - owner of the order
    /// StringLength(450) matches Identity user ID size
    /// </summary>
    [StringLength(450)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Order status: Pending, Paid, Shipped, Delivered, Cancelled
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// When the order was created
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Total order amount (stored at purchase time)
    /// Calculated from sum of (Quantity × Price) for all items
    /// </summary>
    public decimal Total { get; set; } = 0;

    /// <summary>
    /// Order items (products in this order)
    /// Junction table linking Order → Product
    /// </summary>
    public ICollection<OrderProduct>? OrderProducts { get; set; }
}
