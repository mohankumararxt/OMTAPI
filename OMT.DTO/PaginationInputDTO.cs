using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class PaginationInputDTO
    {
        public int PageNo { get; set; }
        public int NoOfRecords { get; set; } 
        public bool IsPagination { get; set; } 
    }

    public class PaginationOutputDTO
    {
        public List<object> Records { get; set; }    // Paginated data
        public int PageNo { get; set; }         // Current page number
        public int NoOfPages { get; set; }      // Total number of pages
        public int TotalCount { get; set; }     // Total record count
       
    }
}
