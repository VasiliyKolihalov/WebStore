using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebStoreAPI.Models
{
    public class Base64ImagePutModel
    {
        [Required] public long Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string ImageData { get; set; }
    }
}