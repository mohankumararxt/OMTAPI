using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class AutomaticFlow
    {
        [Key]
        public int AutomaticFlow_Id { get; set; }
        public int FromSkillSetId { get; set; }
        public int ToSkillSetId { get; set; }
        public bool PriorityOrders_Only { get; set; }
        public bool IsActive { get; set; }
    }
}
