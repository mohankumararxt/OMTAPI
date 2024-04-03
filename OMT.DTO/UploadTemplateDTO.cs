﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UploadTemplateDTO
    {
        public int SkillsetId { get; set; }
        public string JsonData { get; set; }
        public bool IsPriority { get; set; }
    }

    public class ValidateOrderDTO
    {
        public int SkillsetId { get; set; }
        public string JsonData { get; set; }
    }
}
