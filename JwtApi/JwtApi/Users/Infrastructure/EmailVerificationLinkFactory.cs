namespace JwtApi.Api.Users.Infrastructure;

public sealed class EmailVerificationLinkFactory(
    IHttpContextAccessor httpContextAccessor,
    LinkGenerator linkGenerator)
{
    public string Create(EmailVerificationToken emailVerificationToken)
    {
        string? verificationLink = linkGenerator.GetUriByName(
            httpContextAccessor.HttpContext!,
            UserEndpoints.VerifyEmail,
            new { token = emailVerificationToken.Id });

        return verificationLink ?? throw new Exception("Could not create email verification link");
    }
}
