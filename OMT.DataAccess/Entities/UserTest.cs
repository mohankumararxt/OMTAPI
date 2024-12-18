using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    [Table("User_Test")]
    public class UserTest
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int TestId { get; set; }

        public double? Accuracy { get; set; }

        public int? WPM { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime CreateTimestamp { get; set; }
    }

}
