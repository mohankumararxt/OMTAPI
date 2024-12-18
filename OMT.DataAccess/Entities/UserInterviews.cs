using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    [Table("UserInterviews")]
    public class UserInterviews
    {
        [Key]
        public int Id { get; set; }

        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        
        //public DateOnly DOB { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateTimestamp { get; set; }
        [Range(0, 80, ErrorMessage = "Experience must be between 0 and 80.")]
        public int Experience { get; set; }
    }
}
