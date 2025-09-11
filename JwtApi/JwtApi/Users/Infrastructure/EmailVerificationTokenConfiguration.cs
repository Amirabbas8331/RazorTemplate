using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JwtApi.Api.Users.Infrastructure;

public sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
    }
}
