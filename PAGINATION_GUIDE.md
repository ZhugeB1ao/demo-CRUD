# Dynamic Pagination Feature Guide

## Overview
A complete, reusable dynamic pagination system has been implemented across your ASP.NET Core e-commerce application.

## What Was Created

### 1. **PaginatedList<T> Helper Class**
   - File: `Helpers/PaginatedList.cs`
   - Handles pagination logic (skip, take, calculate total pages)
   - Provides both sync and async methods
   - Properties:
     - `Items` - Current page items
     - `PageIndex` - Current page number
     - `TotalPages` - Total number of pages
     - `PageSize` - Items per page
     - `TotalCount` - Total number of items
     - `HasPreviousPage` / `HasNextPage` - Navigation helpers

### 2. **PaginationViewModel**
   - File: `ViewModels/PaginationViewModel.cs`
   - Alternative view model for pagination data
   - Includes page size configuration
   - Useful for complex scenarios

### 3. **Reusable Pagination Partial View**
   - File: `Views/Shared/_Pagination.cshtml`
   - Displays page numbers, prev/next buttons
   - Smart page number display (shows ... for gaps)
   - Preserves search filters when navigating
   - Works for both main site and admin areas
   - Shows pagination info (page X of Y, total items, range)

### 4. **CSS Styling**
   - Added to `wwwroot/css/site.css`
   - Modern Bootstrap-compatible pagination styles
   - Hover effects and active state styling
   - Responsive design

## Implementation Details

### Updated Controllers

#### ShopController (`Controllers/ShopController.cs`)
```csharp
public IActionResult Index(int page = 1)
{
    // Page size: 12 items per page
    // Supports dynamic page parameter
}
```

#### Admin ProductController (`Areas/Admin/Controllers/ProductController.cs`)
```csharp
public async Task<IActionResult> Index(string searchString, int page = 1)
{
    // Page size: 10 items per page
    // Supports search + pagination combined
    // Preserves search string across pages
}
```

#### ProductController (`Controllers/ProductController.cs`)
```csharp
public IActionResult ListPro(int id, int page = 1)
{
    // Category-based product listing with pagination
    // Page size: 12 items per page
}
```

### Updated Views

#### Shop Index (`Views/Shop/Index.cshtml`)
- Changed from `List<Product>` to `PaginatedList<Product>`
- Loops through `Model.Items` instead of `Model`
- Renders `_Pagination` partial

#### Admin Product Index (`Areas/Admin/Views/Product/Index.cshtml`)
- Changed from `IEnumerable<Product>` to `PaginatedList<Product>`
- Search form now preserves search string
- Renders pagination partial
- Loops through `Model.Items`

## Usage

### Default Implementation
The pagination is automatically applied to:
- **Shop page** (`/Shop`) - 12 items per page
- **Admin products** (`/Admin/Product`) - 10 items per page
- **Category products** (`/Product/ListPro/{id}`) - 12 items per page

### URL Format
```
/Shop?page=1
/Admin/Product?page=2&searchString=laptop
/Product/ListPro/5?page=3
```

### Customizing Page Size
To change the number of items per page, modify the `PageSize` constant in each controller:

```csharp
private const int PageSize = 20; // Change to 20 items per page
```

## Features

✅ **Dynamic Pagination** - Automatically calculates pages based on data  
✅ **Search Integration** - Search filters work across pagination  
✅ **Smart Page Display** - Shows page range with ... for gaps  
✅ **Responsive** - Works on mobile and desktop  
✅ **Reusable Partial** - One pagination component for entire app  
✅ **Bootstrap Compatible** - Uses Bootstrap classes  
✅ **Page Info** - Shows current page, total pages, item count  
✅ **Disabled States** - Previous/Next buttons disabled when not available  
✅ **Area Support** - Works for both main site and admin area  

## Styling Customization

The pagination styles in `site.css` can be customized:

```css
.pagination-container {
  /* Main container styling */
}

.pagination .page-link {
  /* Individual page link styling */
}

.pagination .page-item.active .page-link {
  /* Active page styling */
}
```

## Example: Adding Pagination to Another Page

1. **Update Controller:**
```csharp
private const int PageSize = 15;

public IActionResult MyIndex(int page = 1)
{
    if (page < 1) page = 1;
    
    var query = _context.MyData.AsQueryable();
    var totalCount = query.Count();
    var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
    if (page > totalPages && totalPages > 0) page = totalPages;
    
    var items = query
        .Skip((page - 1) * PageSize)
        .Take(PageSize)
        .ToList();
    
    var paginated = new PaginatedList<MyModel>(items, totalCount, page, PageSize);
    return View(paginated);
}
```

2. **Update View Model:**
```html
@model PaginatedList<MyModel>

@foreach(var item in Model.Items)
{
    <!-- Display item -->
}

@await Html.PartialAsync("_Pagination", Model)
```

## Testing

Test the pagination by:
1. Navigate to `/Shop` - Try clicking page numbers
2. Go to `/Shop?page=2` - Verify page 2 loads
3. Search for a product in Admin, then paginate - Verify search is preserved
4. Try accessing an invalid page number - Should redirect to last page
5. Try page 0 - Should redirect to page 1

## Performance Considerations

- Uses `Skip().Take()` for database queries (efficient)
- Async methods available for database operations
- Only fetches required items per page
- Search filters applied before pagination

## Future Enhancements

- Add page size selector (12, 24, 48 items per page)
- Add sort options with pagination
- Add JSON API response format
- Implement caching for frequently accessed pages
- Add AJAX/infinite scroll option
