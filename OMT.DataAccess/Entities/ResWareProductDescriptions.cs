using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class ResWareProductDescriptions
    {
        [Key]
        public int ResWareProductDescriptionId { get; set; }
        public string ResWareProductDescriptionName { get; set; }
        public bool IsActive { get; set; }

    }
}
