using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IDashboardScreensService
    {
        ResultDTO GetTodaysOrders();
        ResultDTO GetVolumeProjection(VolumeProjectionInputDTO volumeProjectionInputDTO);
        ResultDTO GetSorCompletionCount(SorCompletionCountInputDTO sorCompletionCountInputDTO);
       
    }
}
