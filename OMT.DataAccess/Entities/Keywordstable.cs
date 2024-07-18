using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class Keywordstable
    {
        [Key]
        public int KeywordsId { get; set; }
        public string Keywordname { get; set; }
        public bool IsActive { get; set; }
    }
}
