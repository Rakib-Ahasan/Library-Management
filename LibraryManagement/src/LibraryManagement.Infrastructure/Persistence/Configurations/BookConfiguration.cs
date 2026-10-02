using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Author)
            .HasMaxLength(150);

        builder.Property(e => e.Isbn)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.Isbn).IsUnique();

        builder.Property(e => e.TotalCopies)
            .IsRequired();

        builder.Property(e => e.AvailableCopies)
            .IsRequired()
            .HasDefaultValue(0);

    }
}
