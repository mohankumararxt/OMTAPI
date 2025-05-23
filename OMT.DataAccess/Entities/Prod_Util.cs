﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Prod_Util
    {
        [Key]
        public int Prod_UtilId { get; set; }
        public int AgentUserId{ get; set; }
        public int Total_Shift_Hours { get; set; }
        public decimal Productive_Hours { get; set; }
        public decimal Cross_Utilized_Hours { get; set; }
        public int Productivity_Percentage { get; set; }
        public int Utilization_Percentage { get; set; }
        public int CrossUtilization_Percentage { get; set; }
        public DateTime Createddate { get; set; }
        public int TL_Userid { get; set; }  


    }
}
