using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorTemplate.Context;
using RazorTemplate.Model;

namespace RazorTemplate.Pages
{
    public class AllProductsModel : PageModel
    {

        private readonly ShopDbConext _dbContext;

        public AllProductsModel(ShopDbConext dbContext)
        {
            _dbContext = dbContext;
        }
        public List<Product> Products { get; set; } = new();
        public async Task OnGet(string name)
        {
            var query = _dbContext.products
                  .Include(x => x.category)
                  .AsNoTracking()
                  .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.category.Name.Contains(name));
            }
            Products = await query
        .OrderBy(x => x.Name)
        .ToListAsync();

        }
    }

}
