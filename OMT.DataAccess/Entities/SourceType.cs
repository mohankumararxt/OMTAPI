using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class SourceType
    {
        [Key] 
        public int SourceTypeId { get; set; }
        public string SourceTypeName { get; set; }
        public bool IsActive { get; set; }
    }
}
