using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IDateAndNphService
    {
        ResultDTO GetCheckinDetails(GetCheckinDetailsDTO getCheckinDetailsDTO, int userid);
        ResultDTO UpdateCheckinDetails(UpdateCheckinDetailsDTO updateCheckinDetailsDTO);

    }
}
