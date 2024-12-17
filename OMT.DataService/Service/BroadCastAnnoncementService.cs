using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class BroadCastAnnoncementService : IBroadCastAnnoncementService
    {
        private readonly OMTDataContext _oMTDataContext;
        public BroadCastAnnoncementService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO GetBroadCastAnnoncementMessages()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var broadCostList = _oMTDataContext.BroadCastAnnoncement.ToList();
                if (broadCostList != null)
                {

                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and update the result DTO with error details
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = $"An error occurred: {ex.Message}";
            }
            return resultDTO;
        }
    }
}
