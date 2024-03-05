using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class TeamAssociationCreateDTO
    {
        public int? UserId { get; set; }
        public int? TeamId { get; set; }
        public int? ThresholdCount { get; set; }
        public string Description { get; set; }
    }
}
