using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class DefaultTemplateColumns
    {
        public int Id { get; set; }
        public int SystemOfRecordId { get; set; }
        public string DefaultColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDuplicateCheck { get; set; }
        public bool IsGetOrderColumn { get; set; }
    }
}
