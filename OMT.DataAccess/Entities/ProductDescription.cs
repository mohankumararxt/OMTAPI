using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class ProductDescription
    {
        [Key]
        public int ProductDescriptionId { get; set; }
        public string ProductDescriptionName { get; set;}
        public bool IsActive { get; set; }
    }
}
