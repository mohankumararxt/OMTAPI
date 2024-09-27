using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class MasterReportColumns
    {
        [Key]
        public int MasterReportColumnsId { get; set; }
        public string ReportColumnName { get; set; }
    }
}
