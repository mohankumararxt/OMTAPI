using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateUserTestTimeDTO
    {
        public int Id { get; set; }
        public DateTime datetime { get; set; }
    }
    public class UpdateUserTestsDTO : UpdateUserTestTimeDTO
    {
        public float Accuracy { get; set; }
        public int WPM { get; set; }

    }
}
