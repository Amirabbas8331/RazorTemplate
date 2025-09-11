using Microsoft.EntityFrameworkCore;
using RazorTemplate.Model;

namespace RazorTemplate.Context;

public class ShopDbConext:DbContext
{
    public DbSet<Product> products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    public ShopDbConext(DbContextOptions<ShopDbConext> options):base(options)
    {
        
    }
  
}
