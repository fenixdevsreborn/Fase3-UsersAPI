using Fcg.Users.Domain.Entities;
using Fcg.Users.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Users.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Username)
            .HasColumnName("username")
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(e => e.Username).IsUnique();

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();
        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(2000);

        builder.Property(e => e.Bio)
            .HasColumnName("bio")
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
