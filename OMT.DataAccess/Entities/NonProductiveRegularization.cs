using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class NonProductiveRegularization
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TlUserId { get; set; }
        public int Primary_SorId { get; set; }
        public int Reasons { get; set; }
        public string Remarks { get; set; }
        public DateTime NonProductiveHours_Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Applied_Hours { get; set; }
        public int Regularization_Status { get; set; }
        public DateTime Applied_Time { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? TlDescription { get; set; }

    }
}
