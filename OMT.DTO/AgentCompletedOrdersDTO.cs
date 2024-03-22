﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class AgentCompletedOrdersDTO
    {
        public int UserId { get; set; }
        public int? SkillSetId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set;}
    }
}
