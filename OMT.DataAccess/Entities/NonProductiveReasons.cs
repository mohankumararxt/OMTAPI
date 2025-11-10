using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class NonProductiveReasons
    {
        [Key]
        public int Id { get; set; }
        public string Reasons { get; set; }
        public bool IsActive { get; set; }
    }
}
