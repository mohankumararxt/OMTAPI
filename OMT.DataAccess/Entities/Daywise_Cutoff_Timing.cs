using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Daywise_Cutoff_Timing
    {
        [Key]

        public int Daywise_Cutoff_TimingId { get; set; }
        public int SystemOfRecordId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
