using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Order_History
    {
        [Key]
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string ProjectId { get; set; }
        public int SkillSetId { get; set; }
        public int UserId { get; set; }
        public int SystemofRecordId { get; set; }
        public int  Status { get; set; }
        public dynamic OrderDetails { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; } 
    }
}
