using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string ChatMessage { get; set; } = string.Empty;
        public DateTime CreateTimeStamp { get; set; }
        public bool IsRead { get; set; }
        public string Status { get; set; } = string.Empty;

    }
}
