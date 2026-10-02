using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Application.Dtos
{
    public record BorrowRequest(int BookId, int MemberId, int Days = 14);

    public record LoanResponse(
        int Id,
        int BookId,
        string BookTitle,
        int MemberId,
        string MemberName,
        DateTime BorrowedOn,
        DateTime DueDate,
        DateTime? ReturnedOn,
        bool IsOverdue);
}
