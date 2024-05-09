using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class ProductDescription
    {
        [Key]
        public int ProductDescriptionId { get; set; }
        public string ProductDescriptionName { get; set;}
        public bool IsActive { get; set; }
    }
}
