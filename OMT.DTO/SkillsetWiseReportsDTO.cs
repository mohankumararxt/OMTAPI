﻿namespace OMT.DTO
{
    public class SkillsetWiseReportsDTO
    {
        public int SystemOfRecordId { get; set; }
        public int? SkillSetId { get; set; }
        public List<int>? StatusId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
