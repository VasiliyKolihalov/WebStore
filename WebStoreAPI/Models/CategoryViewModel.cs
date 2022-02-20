using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace WebStoreAPI.Models
{
    public class CategoryViewModel
    {
        [Required] public int Id { get; set; }
        [Required] public string Name { get; set; }
        public int ParentId { get; set; }
    }
}