using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class ResWareProductDescriptionsService : IResWareProductDescriptionsService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ResWareProductDescriptionsService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        //public ResultDTO CreateResWareProductDescriptions(ResWareProductDescriptionsDTO resWareProductDescriptionsDTO)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
               
        //       ResWareProductDescriptions res = _oMTDataContext.ResWareProductDescriptions.Where(x => x.IsActive && resWareProductDescriptionsDTO.ResWareProductDescriptionNames.Contains(x.ResWareProductDescriptionName)).FirstOrDefault();

        //       if (res != null)
        //       {
        //           resultDTO.Message = res.ResWareProductDescriptionName + " already exists. Please try again.";
        //           resultDTO.IsSuccess = false;
        //       }
        //       else
        //       {
        //            resultDTO.IsSuccess = true;
        //       }
                



        //    }
        //    catch (Exception ex)
        //    {
        //        resultDTO.IsSuccess = false;
        //        resultDTO.StatusCode = "500";
        //        resultDTO.Message = ex.Message;
        //    }
        //    return resultDTO;
        //}

        //public ResultDTO DeleteResWareProductDescriptions(int rprodid)
        //{
        //    throw new NotImplementedException();
        //}

        public ResultDTO GetResWareProductDescriptions()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var resWareProductDescriptions = _oMTDataContext.ResWareProductDescriptions.Where(x =>x.IsActive).Select(_ => new { _.ResWareProductDescriptionName,_.ResWareProductDescriptionId }).ToList();

                if (resWareProductDescriptions.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of ResWare Product Descriptions";
                    resultDTO.Data = resWareProductDescriptions;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "ResWare Product Descriptions not found";
                    resultDTO.StatusCode = "404";
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

        //public ResultDTO GetResWareProductDescriptionsMap(int productId)
        //{
        //    throw new NotImplementedException();
        //}

        //public ResultDTO UpdateResWareProductDescriptions()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
