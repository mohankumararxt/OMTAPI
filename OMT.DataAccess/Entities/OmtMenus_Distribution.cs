using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class OmtMenus_Distribution
    {
        [Key]
        public int OmtMenus_Distribution_Id { get; set; }
        public int RoleId { get; set; }
        public int OmtMenus_Id { get; set; }
        public bool IsActive { get; set; }

    }
}
