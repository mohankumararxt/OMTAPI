using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataAccess.Entities
{
    public class SORWisePriorityOrderCount
    {
        [Key]
        public int Id { get; set; }
        public int Sor_Id { get; set; }
        public string? Sor_Name { get; set; }
        public DateTime AllocationDate { get; set; }
        public int TotalCount { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
