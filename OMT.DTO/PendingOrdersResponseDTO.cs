using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class PendingOrdersResponseDTO
    {
        public List<Dictionary<string, object>> PendingOrder { get; set; }
        public bool IsPending { get; set; }
        public bool IsTiqe { get; set; }
        public bool IsTrdPending { get; set; }
    }
}
