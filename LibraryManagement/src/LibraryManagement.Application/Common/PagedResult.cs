using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagement.Application.Common
{
    public class PagedQuery
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? Search { get; set; }
    }

    public class PagedResult<T>
    {
        public PagedResult()
        {
        }

        public PagedResult(IEnumerable<T> items, int page, int pageSize, int totalCount)
        {
            Items = items.ToArray();
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
