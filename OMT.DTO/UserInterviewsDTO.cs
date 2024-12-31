using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UserInterviewsDTO
    {
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Firstname should contain alphabets only.")]
        public string Firstname { get; set; } = string.Empty;
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Lastname should contain alphabets only.")]
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        [Range(0, 960, ErrorMessage = "Experience must be between 0 and 960.")]
        public int Experience { get; set; }

        //public DateTime DOB { get; set; }

        //public DateTime DOBOnly => DOB.Date;

    }
}
