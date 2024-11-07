using OMT.DTO;

namespace OMT.DataService.Interface
{

    public interface IMasterReportColumnsService
    {
        ResultDTO GetMasterReportColumnList();
        ResultDTO CreateReportColumnNames(MasterReportColumnDTO masterReportColumnDTO);
    }
}
