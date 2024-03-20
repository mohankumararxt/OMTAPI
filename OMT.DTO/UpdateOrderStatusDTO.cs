using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateOrderStatusDTO
    {
        public int ID { get; set;}
        public int StatusId { get; set;}
        public string? Remarks { get; set;}
        public int SkillSetId { get; set;}
        

    }
}
