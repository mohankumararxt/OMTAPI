using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class ProcessStatusService : IProcessStatusService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ProcessStatusService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetStatusList(int systemofrecordid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<ProcessStatusResponseDTO> listofstatus = (from sor in _oMTDataContext.SystemofRecord
                                                               join ps in _oMTDataContext.ProcessStatus on sor.SystemofRecordId equals ps.SystemOfRecordId
                                                               where ps.IsActive == true && ps.SystemOfRecordId == systemofrecordid && ps.IsActive
                                                               orderby ps.Status
                                                               select new ProcessStatusResponseDTO
                                                               {
                                                                    SystemofRecordName = sor.SystemofRecordName,
                                                                    Status = ps.Status,
                                                                    SystemofRecordId = ps.SystemOfRecordId,
                                                                    StatusId = ps.Id
                                                               }).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Status";
                resultDTO.Data = listofstatus;
                
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
            
        public ResultDTO UpdateOrderStatusList(int systemofrecordid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                
                List<ProcessStatusResponseDTO> listofstatus = (from sor in _oMTDataContext.SystemofRecord
                                                               join ps in _oMTDataContext.ProcessStatus on sor.SystemofRecordId equals ps.SystemOfRecordId
                                                               where ps.IsActive == true && ps.SystemOfRecordId == systemofrecordid && ps.IsActive
                                                               orderby ps.Status
                                                               select new ProcessStatusResponseDTO
                                                               {
                                                                   SystemofRecordName = sor.SystemofRecordName,
                                                                   Status = ps.Status,
                                                                   SystemofRecordId = ps.SystemOfRecordId,
                                                                   StatusId = ps.Id
                                                               }).ToList();

                if (listofstatus.Any(status => status.Status == "System-Pending"))
                {
                    listofstatus.RemoveAll(status => status.Status == "System-Pending");
                }

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Status";
                resultDTO.Data = listofstatus;

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
    }
}
