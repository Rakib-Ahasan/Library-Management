using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Application.Dtos
{
    public record BookRequest(string Title, string Author, string Isbn, int TotalCopies);

    public record BookResponse(
        int Id,
        string Title,
        string Author,
        string Isbn,
        int TotalCopies,
        int AvailableCopies);
}
