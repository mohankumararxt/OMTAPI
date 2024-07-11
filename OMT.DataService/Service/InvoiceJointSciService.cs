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
    public class InvoiceJointSciService : IInvoiceJointSciService
    {
        private readonly OMTDataContext _oMTDataContext;

        public InvoiceJointSciService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateInvoiceJointSci(InvoiceJointSciCreateDTO invoiceJointSciCreateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_ss = _oMTDataContext.InvoiceJointSci.Where(x => x.SkillSetId == invoiceJointSciCreateDTO.SkillSetId).FirstOrDefault();

                if (existing_ss != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill set is already mapped with invoice details.";
                }
                else
                {
                    InvoiceJointSci invoiceJointSci = new InvoiceJointSci()
                    {
                        SystemOfRecordId = 1,
                        SkillSetId = invoiceJointSciCreateDTO.SkillSetId,
                        BusinessGroupId = invoiceJointSciCreateDTO.BusinessGroupId,
                        CustomerId = invoiceJointSciCreateDTO.CustomerId,
                        BusinessId = invoiceJointSciCreateDTO.BusinessId,
                        CostCenterId = invoiceJointSciCreateDTO.CostCenterId,
                        TotalOrderFeesId = invoiceJointSciCreateDTO.TotalOrderFeesId,
                    };

                    _oMTDataContext.InvoiceJointSci.Add(invoiceJointSci);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Skill set is successfully mapped with invoice details";
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

        public ResultDTO GetInvoiceJointSci()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var invjs = (from ijs in _oMTDataContext.InvoiceJointSci
                             join sor in _oMTDataContext.SystemofRecord on ijs.SystemOfRecordId equals sor.SystemofRecordId
                             join ss in _oMTDataContext.SkillSet on ijs.SkillSetId equals ss.SkillSetId
                             join bg in _oMTDataContext.BusinessGroup on ijs.BusinessGroupId equals bg.BusinessGroupId
                             join ct in _oMTDataContext.Customer on ijs.CustomerId equals ct.CustomerId
                             join bs in _oMTDataContext.Business on ijs.BusinessId equals bs.BusinessId
                             join cc in _oMTDataContext.CostCenter on ijs.CostCenterId equals cc.CostCenterId
                             join tof in _oMTDataContext.TotalOrderFees on ijs.TotalOrderFeesId equals tof.TotalOrderFeesId
                             where ss.IsActive == true && sor.IsActive == true && bg.IsActive && ct.IsActive && bs.IsActive
                             orderby ss.SkillSetName
                             select new
                             {
                                 SkillSetId = ss.SkillSetId,
                                 SystemOfRecordId = sor.SystemofRecordId,
                                 InvoiceJointSciId = ijs.InvoiceJointSciId,
                                 SystemOfRecord = sor.SystemofRecordName,
                                 SkillSet = ss.SkillSetName,
                                 BusinessGroup = bg.BusinessGroupName,
                                 Customer = ct.CustomerName,
                                 Business = bs.BusinessName,
                                 CostCenterAmount = cc.CostCenterAmount,
                                 TotalOrderFeesAmount = tof.TotalOrderFeesAmount
                             }).ToList();

                if (invjs != null)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.StatusCode = "200";
                    resultDTO.Data = invjs;
                    resultDTO.Message = "List of SCI Invoice mapping fetched succesfully.";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "SCI Invoice mapping not found.";

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

        public ResultDTO UpdateInvoiceJointSci(InvoiceJointSciResponseDTO invoiceJointSciResponseDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                InvoiceJointSci? invoiceJointsci = _oMTDataContext.InvoiceJointSci.Find(invoiceJointSciResponseDTO.InvoiceJointSciId);

                if (invoiceJointsci != null)
                {
                    invoiceJointsci.BusinessGroupId = invoiceJointSciResponseDTO.BusinessGroupId;
                    invoiceJointsci.CustomerId = invoiceJointSciResponseDTO.CustomerId;
                    invoiceJointsci.BusinessId = invoiceJointSciResponseDTO.BusinessId;
                    invoiceJointsci.CostCenterId = invoiceJointSciResponseDTO.CostCenterId;
                    invoiceJointsci.TotalOrderFeesId = invoiceJointSciResponseDTO.TotalOrderFeesId;

                    _oMTDataContext.InvoiceJointSci.Update(invoiceJointsci);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "SCI Invoice mapping has been updated successfully";

                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Selected skill set is not mapped with invoice details yet";
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
