using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Business
    {
        [Key]
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public bool IsActive { get; set; }
    }
}
