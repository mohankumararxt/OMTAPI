using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class TotalOrderFees
    {
        [Key]
        public int TotalOrderFeesId { get; set; }
        public string TotalOrderFeesAmount { get; set; }
    }
}
