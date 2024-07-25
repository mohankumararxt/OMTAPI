using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IDashboardService
    {
        ResultDTO LiveStatusReport(LiveStatusReportDTO liveStatusReportDTO);
    }
}
