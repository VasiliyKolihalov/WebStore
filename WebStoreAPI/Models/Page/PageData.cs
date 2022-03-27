using System;
using System.Collections.Generic;

namespace WebStoreAPI.Models
{
    public class PageData
    {
        public int PageNumber { get; }
        public int TotalPages { get; }
        public bool InAscending { get; set; }
        public PageOrder OrderBy { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
        public List<StorePutModel> Stores { get; set; }
        
        
        public PageData(int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int) Math.Ceiling(count / (double) pageSize);
        }
        
        public bool HasPreviousPage => (PageNumber > 1);
        public bool HasNextPage => (PageNumber < TotalPages);
    }
}