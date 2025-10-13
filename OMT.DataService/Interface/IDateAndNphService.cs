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
        ResultDTO GetNonProductiveReasons();
        ResultDTO ApplyNonProductiveHours(ApplyNonProductiveHoursDTO applyNonProductiveHoursDTO, int userid);
        ResultDTO GetNonProductiveRegularizations_Agent(PaginationInputDTO paginationInputDTO,int userid);
        ResultDTO GetRegularizationStatus();
        ResultDTO GetNonProductiveRegularizations(GetCheckinDetailsDTO getCheckinDetailsDTO, int userid);
        ResultDTO UpdateRegularizations(UpdateRegularizationsDTO updateRegularizationsDTO, int userid);

    }
}
