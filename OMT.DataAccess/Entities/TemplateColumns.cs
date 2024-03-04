using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class TemplateColumns
    {
        [Key]
        public int ColumnId { get; set; }
        public int TemplateId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnAliasName { get; set; }
        public string ColumnDataType { get; set; }
    }
}
