using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    [Table("BroadcastAnnouncements")]
    public class BroadCastAnnoncement
    {
        [Key]   
        public int Id { get; set; }
        public string BroadCastMessage { get; set; } = string.Empty;

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }   
        public bool SoftDelete { get; set; }
        public DateTime CreateTimeStamp {  get; set; }  

    }
}
