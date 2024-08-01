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
    public class BusinessService : IBusinessService
    {
        private readonly OMTDataContext _oMTDataContext;

        public BusinessService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO CreateBusiness(string businessname)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                Business business = _oMTDataContext.Business.Where(x => x.IsActive && x.BusinessName == businessname).FirstOrDefault();

                if (business != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Business already exists. Please try to add different Business";
                }
                else
                {
                    Business b = new Business()
                    {
                        BusinessName = businessname,
                        IsActive = true,
                    };

                    _oMTDataContext.Business.Add(b);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Business created successfully";
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

        public ResultDTO GetBusiness()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<Business> b = _oMTDataContext.Business.Where(x => x.IsActive).OrderBy(x => x.BusinessName).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Business";
                resultDTO.Data = b;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO UpdateBusiness(BusinessDTO businessDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                Business business = _oMTDataContext.Business.Find(businessDTO.BusinessId);

                if (business != null)
                {
                    business.BusinessName = businessDTO.BusinessName;
                    _oMTDataContext.Business.Update(business);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Business updated successfully";

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Business not found";
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
    }
}

