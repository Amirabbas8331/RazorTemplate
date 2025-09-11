using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorTemplate.Context;
using RazorTemplate.Model;
using System;

namespace RazorTemplate.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ShopDbConext _context;

        public IndexModel(ShopDbConext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        public List<Product> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _context.products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                query = query.Where(p => p.Name.StartsWith(SearchName));
            }

            Products = await query.ToListAsync();
        }
    }


}

