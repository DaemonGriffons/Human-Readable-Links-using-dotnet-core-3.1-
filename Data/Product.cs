using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HumanReadableLinks.Data
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        [StringLength(1024, MinimumLength = 3)]
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public string Slug { get; set; }
    }
}
