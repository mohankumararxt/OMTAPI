using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class BusinessGroup
    {
        [Key]
        public int BusinessGroupId { get; set; }
        public string BusinessGroupName { get; set; }
        public bool IsActive { get; set; }

    }
}
