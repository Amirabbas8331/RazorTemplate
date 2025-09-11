using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorTemplate.Context;

namespace RazorTemplate.Pages
{
    public class CategoryModel : PageModel
    {
        private readonly ShopDbConext _dbConext;
        public string? Name { get; set; }

        public CategoryModel(ShopDbConext dbConext)
        {
            _dbConext = dbConext;
        }
        public async Task<IActionResult> OnGet(int? id)
        {
            if (!id.HasValue)
            {
                // No id provided — skip DB call
                return Page();
            }

            var result = await _dbConext.Categories
                .FirstOrDefaultAsync(x => x.Id == id.Value);
            if (result is null) return NotFound();
            Name = result.Name;
            return Page();
        }

    }
}
