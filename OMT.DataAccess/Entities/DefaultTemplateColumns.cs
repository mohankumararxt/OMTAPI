using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class DefaultTemplateColumns
    {
        [Key]
        public int Id { get; set; }
        public int SystemOfRecordId { get; set; }
        public string DefaultColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDuplicateCheck { get; set; }
        public bool IsGetOrderColumn { get; set; }
        public bool IsDefOnlyColumn { get; set; }
        public bool IsMandatoryColumn { get; set; }
    }
}
