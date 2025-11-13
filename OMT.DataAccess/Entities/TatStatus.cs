using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class TatStatus
    {
        [Key]
        public int Id { get; set; }
        public string TatStatus_DropdownName { get; set; }
        public bool TatStatus_Value { get; set; }
        public string TatStatus_Name { get; set; }
        public bool IsActive { get; set; }
    }
}
