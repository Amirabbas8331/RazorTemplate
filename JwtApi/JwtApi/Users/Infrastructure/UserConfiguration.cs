using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JwtApi.Api.Users.Infrastructure;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName).HasMaxLength(200);
        builder.Property(u => u.LastName).HasMaxLength(200);
        builder.Property(u => u.Email).HasMaxLength(300);
        builder.Property(u => u.PasswordHash).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
