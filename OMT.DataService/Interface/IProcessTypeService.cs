using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IProcessTypeService
    {
        ResultDTO CreateProcessType(string ProcessType);
        ResultDTO UpdateProcessType(ProcessTypeDTO processTypeDTO);
        ResultDTO DeleteProcessType(int id);
        ResultDTO GetProcessType();
    }
}
