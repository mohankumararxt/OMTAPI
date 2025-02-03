using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class MessageRequestDTO
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string ChatMessage { get; set; } = string.Empty;
    }
    public class MessageUpdateDTO
    {
        public List<int> MessageIds {  get; set; }
        public int OutputMessageStatus { get; set; }
    }
}
