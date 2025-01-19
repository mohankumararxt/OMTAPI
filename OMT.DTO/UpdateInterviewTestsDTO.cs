using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class UpdateInterviewTestTimeDTO
    {
        public int Id { get; set; }
        public DateTime datetime {  get; set; }
    }
    public class UpdateInterviewTestsDTO : UpdateInterviewTestTimeDTO
    {
        public float Accuracy { get; set; }
        public int WPM { get; set; }

    }
}
