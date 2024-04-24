using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class SourceType
    {
        [Key] 
        public int SourceTypeId { get; set; }
        public string SourceTypeName { get; set; }
        public bool IsActive { get; set; }
    }
}
