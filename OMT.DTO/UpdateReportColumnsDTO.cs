using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateReportColumnsDTO
    {
        public int SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public List<string> MasterReportColumnNames { get; set; }
    }
}
