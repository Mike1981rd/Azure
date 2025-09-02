using System.Collections.Generic;

namespace WebsiteBuilderAPI.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        
        public PagedResult()
        {
            Items = new List<T>();
        }
        
        public PagedResult(List<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
        }
        
        // Helper properties
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int? NextPage => HasNextPage ? Page + 1 : null;
        public int? PreviousPage => HasPreviousPage ? Page - 1 : null;
    }
}