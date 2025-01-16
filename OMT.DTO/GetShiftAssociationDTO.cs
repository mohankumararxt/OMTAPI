using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class GetShiftAssociationDTO
    {
        public int? AgentEmployeeId { get; set; }
        public int? TlEmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public PaginationInputDTO Pagination { get; set; }
    }
}
    