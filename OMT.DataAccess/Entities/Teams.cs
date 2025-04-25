using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class Teams
    {
        [Key]
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TL_Userid { get; set; }

    }
}
