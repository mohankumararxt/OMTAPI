﻿namespace OMT.DTO
{
    public class TeamCompletedOrdersDTO
    {
        public int TeamId { get; set; }
        public int? SkillSetId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

    }
}
