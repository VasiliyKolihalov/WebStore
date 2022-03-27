using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations;

namespace WebStoreAPI.Models
{
    public class CatergoryAddModel
    {
        [Required] public string Name { get; set; }
        public int ParentId { get; set; }
    }
}