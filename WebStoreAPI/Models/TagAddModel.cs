using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebStoreAPI.Models
{
    public class TagAddModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }

    }
}
