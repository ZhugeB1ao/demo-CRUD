using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Route("/")]
[Route("Shop")]
public class ShopController : Controller
{
    private readonly ILogger<ShopController> _logger;
    private readonly AppDBContext _context;
    private const int PageSize = 12; // Items per page
    
    public ShopController(ILogger<ShopController> logger, AppDBContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index(int page = 1)
    {
        // Validate page number
        if (page < 1) page = 1;
        
        var query = _context.Products.AsQueryable();
        var totalCount = query.Count();
        
        // Calculate pagination
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (page > totalPages && totalPages > 0) page = totalPages;
        
        var products = query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        var paginatedList = new PaginatedList<Product>(products, totalCount, page, PageSize);
        return View(paginatedList);
    }
    
    

    // [HttpGet("{slug?}")]
    // public IActionResult Index(string? slug)
    // {
    //     var categories = _context.Categories.ToList();
    //
    //     if (string.IsNullOrEmpty(slug))
    //     {
    //         var viewAll = new ShopView
    //         {
    //             Categories = categories,
    //             Products = _context.Products.ToList(),
    //             CurrentCategory = null
    //         };
    //         return View(viewAll);
    //     }
    //
    //     var currentCategory = categories.FirstOrDefault(c => c.Slug == slug);
    //
    //     if (currentCategory == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     // Get id of all children category
    //     var subCategoryIds = categories
    //         .Where(c => c.ParentId == currentCategory.Id)
    //         .Select(c => c.Id)
    //         .ToList();
    //
    //     subCategoryIds.Add(currentCategory.Id);
    //
    //     var products = _context.Products
    //         .Where(p => subCategoryIds.Contains(p.CategoryId))
    //         .ToList();
    //
    //     var view = new ShopView
    //     {
    //         Categories = categories,
    //         Products = products,
    //         // CurrentCategory = currentCategory
    //     };
    //
    //     return View(view);
    // }
}