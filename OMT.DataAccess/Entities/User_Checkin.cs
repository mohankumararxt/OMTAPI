using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class User_Checkin
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public bool Prod_Util_Calculated { get; set; }
    }
}
