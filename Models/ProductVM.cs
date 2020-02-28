using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HumanReadableLinks.Models
{
    public class ProductVM
    {

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [StringLength(1024, MinimumLength = 3)]
        public string Description { get; set; }


        [Required]
        [Display(Name ="Image Upload")]
        public IFormFile ImageFile { get; set; }

        [Required]
        public double Price { get; set; }


        public string Slug { get; set; }

        public string Image { get; set; }
    }
}
