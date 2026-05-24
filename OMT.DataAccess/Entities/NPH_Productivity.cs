using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class NPH_Productivity
    {
        [Key]
        public int NPH_ProductivityId { get; set; }
        public int UserId { get; set; }
        public int TlUserId { get; set; }
        public DateTime Productivity_Date { get; set; }
        public decimal Applied_Hours { get; set; }
        public decimal Pending_Orders_Hours { get; set; }
        public int Productivity_Percentage { get; set; }
        public int NPH_Productivity_Percentage { get; set; }
        public string Remarks { get; set; }
    }
}
    