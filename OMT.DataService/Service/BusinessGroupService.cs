using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class BusinessGroupService : IBusinessGroupService
    {
        private readonly OMTDataContext _oMTDataContext;
        public BusinessGroupService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateBusinessGroup(string BusinessGroupName)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                BusinessGroup businessgroup = _oMTDataContext.BusinessGroup.Where(x =>x.IsActive && x.BusinessGroupName == BusinessGroupName).FirstOrDefault();

                if (businessgroup != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Business Group already exists. Please try to add different Business Group";
                }
                else
                {
                    BusinessGroup bgroup = new BusinessGroup()
                    {
                        BusinessGroupName = BusinessGroupName,
                        IsActive = true,
                    };

                    _oMTDataContext.BusinessGroup.Add(bgroup);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Business Group created successfully";
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

        //public ResultDTO DeleteBusinessGroup(int id)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        BusinessGroup businessgroup = _oMTDataContext.BusinessGroup.Where(x => x.IsActive && x.BusinessGroupId == id).FirstOrDefault();

        //        if (businessgroup != null)
        //        {
        //            businessgroup.IsActive = false;
        //            _oMTDataContext.BusinessGroup.Update(businessgroup);
        //            _oMTDataContext.SaveChanges();
        //            resultDTO.IsSuccess = true;
        //            resultDTO.Message = "Business Group has been deleted successfully";
        //        }
        //        else
        //        {
        //            resultDTO.StatusCode = "404";
        //            resultDTO.IsSuccess = false;
        //            resultDTO.Message = "Business Group is not found";
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        resultDTO.IsSuccess = false;
        //        resultDTO.StatusCode = "500";
        //        resultDTO.Message = ex.Message;
        //    }
        //    return resultDTO;
        //}

        public ResultDTO GetBusinessGroup()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<BusinessGroup> bg = _oMTDataContext.BusinessGroup.Where(x =>x.IsActive).OrderBy(x =>x.BusinessGroupName).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Business Groups";
                resultDTO.Data = bg;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;

        }

        public ResultDTO UpdateBusinessGroup(BusinessGroupDTO businessGroupDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                BusinessGroup businessGroup = _oMTDataContext.BusinessGroup.Find(businessGroupDTO.BusinessGroupId);

                if (businessGroup != null)
                {
                    businessGroup.BusinessGroupName = businessGroupDTO.BusinessGroupName;
                    _oMTDataContext.BusinessGroup.Update(businessGroup);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Business Group updated successfully";
                    
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Business Group not found";
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
