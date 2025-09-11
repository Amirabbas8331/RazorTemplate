using JwtApi.Api.Database;
using JwtApi.Api.Users.Infrastructure;

namespace JwtApi.Api.Users;

public sealed class LoginUser(AppDbContext context,PasswordHasher passwordHasher, TokenProvider tokenProvider)
{
    public sealed record Request(string Email, string Password);

    public async Task<string> Handle(Request request)
    {
        User? user = await context.Users.GetByEmail(request.Email);

        if (user is null || !user.EmailVerified)
        {
            throw new Exception("The user was not found");
        }

        bool verified = passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!verified)
        {
            throw new Exception("The password is incorrect");
        }

        string token = tokenProvider.Create(user);

        return token;
    }
}
