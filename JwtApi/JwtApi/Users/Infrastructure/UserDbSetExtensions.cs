using Microsoft.EntityFrameworkCore;

namespace JwtApi.Api.Users.Infrastructure;

public static class UserDbSetExtensions
{
    public static async Task<bool> Exists(this DbSet<User> users, string email)
    {
        return await users.AnyAsync(u => u.Email == email);
    }

    public static async Task<User?> GetByEmail(this DbSet<User> users, string email)
    {
        return await users.SingleOrDefaultAsync(u => u.Email == email);
    }
}
