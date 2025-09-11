using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorTemplate.Context;
using RazorTemplate.Model;
using System;
using System.Threading.Tasks;

namespace RazorTemplate.Pages
{
    public class ProductModel : PageModel
    {
        private readonly ShopDbConext _dbContext;

        public ProductModel(ShopDbConext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public string? SearchName { get; set; }

        [BindProperty]
        public int? SearchCategoryId { get; set; }

        public List<Product> Products { get; set; } = new();
        public int Pages { get; set; } = 1; // default page
        public string? Name { get; set; }
        public int PageSize { get; set; } = 10;
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var query = _dbContext.products
                .Include(x => x.category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                query = query.Where(x => x.Name.Contains(SearchName));
            }

            if (SearchCategoryId.HasValue)
            {
                query = query.Where(x => x.category.Id == SearchCategoryId.Value);
            }
            Products = await query
        .OrderBy(x => x.Name)
        .Skip((Pages - 1) * PageSize)
        .Take(PageSize)
        .ToListAsync();

            return Page();
        }
    }
}
