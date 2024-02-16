using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class TeamAssociation
    {
        [Key]
        public int AssociationId { get; set; }
        public int UserId { get; set; }
        public int TeamId { get; set; }
        public int ThresholdCount { get; set; }
    }
}
