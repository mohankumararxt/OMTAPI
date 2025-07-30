using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class GetOrderResponseDTO
    {
        public string AssignedOrder { get; set; }
        public bool IsTiqe { get; set; }

        public bool IsTrdPending { get; set; }
        public bool IsAutomaticFlow { get; set; }
    }

   
}
