using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class TemplateColumns
    {
        [Key]
        public int ColumnId { get; set; }
        public int SkillSetId { get; set; }
        public int SystemOfRecordId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnAliasName { get; set; }
        public string ColumnDataType { get; set; }
        public bool IsDuplicateCheck { get; set; }
        public bool IsGetOrderColumn { get; set; }
    }
}
