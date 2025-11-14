using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ISciExceptionService
    {
        ResultDTO UploadSciExceptionReport(UploadSciExceptionReportDTO uploadSciExceptionReportDTO);
        ResultDTO GetSciExceptionReport(GetSciExceptionReportDTO getSciExceptionReportDTO);
        ResultDTO GetTatStatus();
        ResultDTO GetSciPendingStatusSkillsetsList();
        ResultDTO UpdateTat(UpdateTatDTO updateTatDTO);
    }
}
