using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class OmtMenus
    {
        [Key]
        public int OmtMenus_Id { get; set; }
        public string OmtMenus_Name { get; set; }
        public bool IsActive { get; set; }


    }
}
