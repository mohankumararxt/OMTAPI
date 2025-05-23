﻿using System.ComponentModel.DataAnnotations;

namespace OMT.DataAccess.Entities
{
    public class InvoiceJointTiqe
    {
        [Key]
        public int InvoiceJointTiqeId { get; set; }
        public int SystemOfRecordId { get; set; }
        public int SkillSetId { get; set; }
        public int BusinessGroupId { get; set; }
        public int ProcessTypeId { get; set; }
        public int SourceTypeId { get; set; }
        public int CostCenterId { get; set; }
        public int TotalOrderFeesId { get; set; }
    }
}
