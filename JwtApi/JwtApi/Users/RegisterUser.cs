using JwtApi.Api.Database;
using JwtApi.Api.Users.Infrastructure;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JwtApi.Api.Users;

public sealed class RegisterUser(
    AppDbContext context,
    PasswordHasher passwordHasher,
    IFluentEmail fluentEmail,
    EmailVerificationLinkFactory emailVerificationLinkFactory)
{
    public sealed record Request(string Email, string FirstName, string LastName, string Password);

    public async Task<User> Handle(Request request)
    {
        if (await context.Users.Exists(request.Email))
        {
            throw new Exception("The email is already in use");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        context.Users.Add(user);

        DateTime utcNow = DateTime.UtcNow;
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedOnUtc = utcNow,
            ExpiresOnUtc = utcNow.AddDays(1)
        };

        context.EmailVerificationTokens.Add(verificationToken);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
            when (e.InnerException is NpgsqlException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new Exception("The email is already in use", e);
        }

        // Email verification?
        string verificationLink = emailVerificationLinkFactory.Create(verificationToken);

       var response= await fluentEmail
            .To(user.Email)
            .Subject("Email verification for Amirabbas")
            .Body($"To verify your email address <a href='{verificationLink}'>click here</a>", isHtml: true)
            .SendAsync();
        if (!response.Successful)
        {
            throw new Exception($"Email sending failed: {string.Join(", ", response.ErrorMessages)}");
        }
        return user;
    }
}
