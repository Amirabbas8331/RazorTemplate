using JwtApi.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace JwtApi.Api.Extensions;

internal static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

     ApplyMigration<AppDbContext>(scope);
    }
    private static void ApplyMigration<TDbContext>(IServiceScope scope)
       where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }

}
