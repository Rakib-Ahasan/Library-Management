using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity type configuration for the Loan entity.
/// </summary>
public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BorrowedOn)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.DueDate)
            .IsRequired();

        builder.Property(e => e.ReturnedOn)
            .HasColumnType("datetime2");

        builder.HasOne(e => e.Book)
            .WithMany(e => e.Loans)
            .HasForeignKey(e => e.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Member)
            .WithMany(e => e.Loans)
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}