using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateOrderStatusByTLDTO
    {
        public int SkillSetId { get; set; }
        public string OrderId { get; set; }
        public int Status { get; set; }
        public string TL_Description { get; set; }  
    }
}
