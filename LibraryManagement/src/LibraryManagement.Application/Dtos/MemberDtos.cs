using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Application.Dtos
{
    public record MemberRequest(string FullName, string Email, string? Phone);

    public record MemberResponse(int Id, string FullName, string Email, string? Phone, DateTime JoinedOn);
}
