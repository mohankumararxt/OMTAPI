using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class ResWareProductDescriptionMap
    {
        [Key]
        public int ResWareProductDescriptionMapId { get; set; }
        public int ResWareProductDescriptionId { get; set; }
        public int ProductDescriptionId { get; set; }

    }
}
