using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class SendEmailDTO
    {
        public List<string> ToEmailIds { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
