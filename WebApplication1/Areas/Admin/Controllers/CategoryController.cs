using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDBContext _context;
        private const int PageSize = 10;

        public CategoryController(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.Categories
                .Include(c => c.Parent)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Name!.Contains(searchString));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var categories = await query
                .OrderBy(c => c.ParentId == null ? 0 : 1)
                .ThenBy(c => c.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var paginatedList = new PaginatedList<Category>(categories, totalCount, page, PageSize);

            ViewData["SearchString"] = searchString;

            return View(paginatedList);
        }

        public IActionResult Create()
        {
            // Only parent categories (ParentId == null) can be selected as parent
            ViewData["ParentId"] = new SelectList(
                _context.Categories.Where(c => c.ParentId == null), 
                "Id", 
                "Name"
            );
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            // Remove validation for navigation properties
            ModelState.Remove("Parent");
            ModelState.Remove("Children");
            ModelState.Remove("Products");

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Category", new { area = "Admin" });
            }

            ViewData["ParentId"] = new SelectList(
                _context.Categories.Where(c => c.ParentId == null),
                "Id",
                "Name",
                category.ParentId
            );
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Parent categories dropdown - exclude itself and its children
            ViewData["ParentId"] = new SelectList(
                _context.Categories.Where(c => c.ParentId == null && c.Id != id),
                "Id",
                "Name",
                category.ParentId
            );
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            // Remove validation for navigation properties
            ModelState.Remove("Parent");
            ModelState.Remove("Children");
            ModelState.Remove("Products");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Category", new { area = "Admin" });
            }

            ViewData["ParentId"] = new SelectList(
                _context.Categories.Where(c => c.ParentId == null && c.Id != id),
                "Id",
                "Name",
                category.ParentId
            );
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                // Check if category has children or products
                if (category.Children?.Any() == true)
                {
                    TempData["Error"] = "Cannot delete category with subcategories. Please delete subcategories first.";
                    return RedirectToAction("Index", "Category", new { area = "Admin" });
                }

                if (category.Products?.Any() == true)
                {
                    TempData["Error"] = "Cannot delete category with products. Please remove or reassign products first.";
                    return RedirectToAction("Index", "Category", new { area = "Admin" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully.";
            }

            return RedirectToAction("Index", "Category", new { area = "Admin" });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
