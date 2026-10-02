using LibraryManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        public List<Loan> Loans { get; set; } = new();

        public int BorrowedCopies => TotalCopies - AvailableCopies;

        public void TakeCopy()
        {
            if (AvailableCopies < 1)
            {
                throw new BusinessRuleException("No copies of this book are available right now.");
            }
            AvailableCopies--;
        }

        public void ReturnCopy()
        {
            if (AvailableCopies >= TotalCopies)
            {
                throw new BusinessRuleException("All copies are already in the library.");
            }
            AvailableCopies++;
        }

        public void ChangeTotalCopies(int newTotal)
        {
            if (newTotal < BorrowedCopies)
            {
                throw new BusinessRuleException(
                    $"{BorrowedCopies} copies are currently borrowed, total copies can't be lower than that.");
            }

            AvailableCopies = newTotal - BorrowedCopies;
            TotalCopies = newTotal;
        }
    }
}
