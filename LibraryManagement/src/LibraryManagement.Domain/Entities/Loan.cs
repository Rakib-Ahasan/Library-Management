using LibraryManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public DateTime BorrowedOn { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedOn { get; set; }

        public bool IsOverdue(DateTime now) => ReturnedOn == null && DueDate < now;

        public void MarkReturned(DateTime now)
        {
            if (ReturnedOn != null)
                throw new BusinessRuleException("This book has already been returned.");

            ReturnedOn = now;
            Book.ReturnCopy();
        }
    }
}
