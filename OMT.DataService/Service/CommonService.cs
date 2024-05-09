using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class CommonService : ICommonService
    {
        private readonly OMTDataContext _oMTDataContext;
        public CommonService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetSORList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<SystemofRecordResponseDTO> orgDTOObj = _oMTDataContext.SystemofRecord.Where(x => x.IsActive).Select(_ => new SystemofRecordResponseDTO() { SystemofRecordId = _.SystemofRecordId, SystemofRecordName = _.SystemofRecordName}).ToList();
                if (orgDTOObj != null)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = orgDTOObj;
                    resultDTO.Message = "System of Record List.";
                }
                else
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = orgDTOObj;
                    resultDTO.Message = "System of Record is empty";
                }
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
