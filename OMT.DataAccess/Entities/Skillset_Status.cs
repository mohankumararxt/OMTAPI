using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Skillset_Status
    {
        [Key]
        public int Skillset_StatusId { get; set; }
        public int SystemofRecordId { get; set; }
        public int StatusId { get; set; }   
        public bool IsActive { get; set; }

    }
}
