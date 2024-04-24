using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class ResWareProductDescriptions
    {
        [Key]
        public int ResWareProductDescriptionId { get; set; }
        public string ResWareProductDescriptionName { get; set; }
        public bool IsActive { get; set; }

    }
}
