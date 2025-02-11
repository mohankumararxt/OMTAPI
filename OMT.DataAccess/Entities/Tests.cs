using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    [Table("Tests")]
    public class Tests
    {
        [Key]
        public int Id { get; set; }
        public string Test_text { get; set; } = string.Empty;
        public int Duration { get; set; }

        public bool IsSample { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateTimestamp { get; set; }

        public int DifficultyLevel { get; set; }
    }
}
