using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity type configuration for the Member entity.
/// </summary>
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.JoinedOn)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

    }
}