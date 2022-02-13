using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WebStoreAPI.Models
{
    public class PageGetModel
    {
        [Min(1)] [Required] public int PageNumber { get; set; }
        [Min(1)] [Required] public int PageSize { get; set; }
        public bool InAscending { get; set; } = true;
        public PageOrder OrderBy { get; set; } = PageOrder.Cost;
        public List<int> CategoriesId { get; set; } = new List<int>();
        public List<int> StoresId { get; set; } = new List<int>();
    }

    public enum PageOrder
    {
        Cost
    }
}