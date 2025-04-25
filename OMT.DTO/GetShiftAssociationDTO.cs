using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class GetShiftAssociationDTO
    {
        public string? AgentEmployeeId { get; set; }
        public string? TlEmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public PaginationInputDTO Pagination { get; set; }
    }
}
    