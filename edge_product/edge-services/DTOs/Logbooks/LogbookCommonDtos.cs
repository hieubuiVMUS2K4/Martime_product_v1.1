using System;

namespace MaritimeEdge.DTOs.Logbooks
{
    public class LogbookPaginationDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class PaginatedLogbookResponseDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
