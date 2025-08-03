using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class MonthlyCount_SOR
    {
        [Key]
        public int MonthlyCount_SORId { get; set; }
        public int SystemofRecordId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
    }
}
