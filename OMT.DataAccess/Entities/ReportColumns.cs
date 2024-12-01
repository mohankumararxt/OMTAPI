using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class ReportColumns
    {
        [Key]
        public int ReportColumnsId { get; set; }
        public bool IsActive { get; set; }
        public int SystemOfRecordId { get; set; }
        public int SkillSetId { get; set; }
        public int MasterReportColumnId { get; set; }
        public int ColumnSequence { get; set; }
    }
}
