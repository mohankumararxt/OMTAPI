using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class MasterProjectName
    {
        [Key]
        public int MasterProjectNameId { get; set; }
        public int SkillSetId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool IsActive { get; set; }

    }
}
