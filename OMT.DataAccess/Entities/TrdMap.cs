using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class TrdMap
    {
        [Key]
        public int TrdMapId { get; set; }
        public string ProjectId { get; set; }
        public int DoctypeId { get; set; }
        public int SkillSetId { get; set; }
        public int SystemOfRecordId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
