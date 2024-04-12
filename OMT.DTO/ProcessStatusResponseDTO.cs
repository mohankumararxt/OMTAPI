using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class ProcessStatusResponseDTO
    {
        public string SystemofRecordName { get; set; }
        public int SystemofRecordId { get; set; }
        public string Status {  get; set; }
        public int StatusId { get; set; }
    }
}
