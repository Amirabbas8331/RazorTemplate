using JwtApi.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace JwtApi.Api.Users;

public sealed class VerifyEmail(AppDbContext context)
{
    public async Task<bool> Handle(Guid tokenId)
    {
        EmailVerificationToken? token = await context.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == tokenId);

        if (token is null || token.ExpiresOnUtc < DateTime.UtcNow || token.User.EmailVerified)
        {
            return false;
        }

        token.User.EmailVerified = true;

        context.EmailVerificationTokens.Remove(token);

        await context.SaveChangesAsync();

        return true;
    }
}
