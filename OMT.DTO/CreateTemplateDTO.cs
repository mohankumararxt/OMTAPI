using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DTO
{
    public class CreateTemplateDTO
    {
        public int SkillsetId { get; set; }
        public int SystemofRecordId { get; set; }
        public List<TemplateColumnDTO> TemplateColumns { get; set; }

        public CreateTemplateDTO()
        {
            TemplateColumns = new List<TemplateColumnDTO>();
        }


    }

    public class TemplateColumnDTO
    {
        public string ColumnName { get; set; }
        public string ColumnDataType { get; set; }
    }
}
