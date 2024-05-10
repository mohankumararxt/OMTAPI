﻿namespace OMT.DTO
{
    public class ReswareInvoiceDTO
    {
        public int InvoiceDumpId { get; set; }
        public string SystemOfRecord { get; set; }
        public string SkillSet { get; set; }
        public string BusinessGroup { get; set; }
        public string ProcessType { get; set; }
        public string SourceType { get; set; }  
        public string CostCenter { get; set; }
        public string TotalOrderFees { get; set; }
        public string CompletionDate { get; set; }
        public string CustomerId { get; set; }
        public string OrderId { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public string ProductDescription { get; set; }
    }
}