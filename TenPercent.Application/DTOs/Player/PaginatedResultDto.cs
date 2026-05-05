namespace TenPercent.Application.DTOs
{
    using System.Collections.Generic;

    public class PaginatedResultDto<T>
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }

}