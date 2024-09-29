using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateUserPasswordByHrDTO
    {
        public int UserId {  get; set; }
        public string NewPassword { get; set; }

    }
}
