using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class TeamAssociationResponseDTO
    {
        public string FirstName { get; set; }
        public string? TeamName { get; set; }
        public int? ThresholdCount { get; set; }
        public string Description { get; set; }
    }
}
