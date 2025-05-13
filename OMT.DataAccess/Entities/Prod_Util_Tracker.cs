using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Prod_Util_Tracker
    {
        [Key]
        public int Prod_Util_Tracker_Id { get; set; }
        public int UserId { get; set; }
        public string OrderId { get; set; }
        public int SkillSetId { get; set; }
        public int SystemofRecordId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public DateTime Productivity_Date { get; set; }


    }
}
