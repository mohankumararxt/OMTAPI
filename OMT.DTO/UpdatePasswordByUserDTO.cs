using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdatePasswordByUserDTO
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string ConfirmNewPassword { get; set; } 

    }
}
