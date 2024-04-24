using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class InvoiceJointResware
    {
        [Key]
        public int InvoiceJointReswareId { get; set; }
        public int SystemOfRecordId { get; set; }
        public int SkillSetId { get; set; }
        public int BusinessGroupId { get; set; }
        public int ProcessTypeId { get; set; }
        public int SourceTypeId { get; set; }
        public int CostCenterId { get; set; }
    }
}
