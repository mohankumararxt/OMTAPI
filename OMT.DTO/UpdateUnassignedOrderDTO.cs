using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateUnassignedOrderDTO
    {
        public List<int> Id { get; set; }
        public int Status { get; set; }
        public string Remarks { get; set; }
        public int SkillsetId { get; set; }
        public int SystemofRecordId { get; set; }
    }
}
