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
    public class InvoiceJointReswareService : IInvoiceJointReswareService
    {
        private readonly OMTDataContext _oMTDataContext;

        public InvoiceJointReswareService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateInvoiceJointResware(InvoiceJointReswareCreateDTO invoiceJointReswareCreateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_ss = _oMTDataContext.InvoiceJointResware.Where(x =>x.SkillSetId == invoiceJointReswareCreateDTO.SkillSetId).FirstOrDefault();

                if (existing_ss != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill set is already mapped with invoice details.";
                }
                else
                {
                    InvoiceJointResware invoiceJointResware = new InvoiceJointResware()
                    {
                        SystemOfRecordId = 2,
                        SkillSetId = invoiceJointReswareCreateDTO.SkillSetId,
                        BusinessGroupId = invoiceJointReswareCreateDTO.BusinessGroupId,
                        ProcessTypeId = invoiceJointReswareCreateDTO.ProcessTypeId,
                        SourceTypeId = invoiceJointReswareCreateDTO.SourceTypeId,
                        CostCenterId = invoiceJointReswareCreateDTO.CostCenterId,
                        TotalOrderFeesId = invoiceJointReswareCreateDTO.TotalOrderFeesId,
                    };

                    _oMTDataContext.InvoiceJointResware.Add(invoiceJointResware);
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

        public ResultDTO GetInvoiceJointResware()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var invjr = (from ijr in _oMTDataContext.InvoiceJointResware
                             join sor in _oMTDataContext.SystemofRecord on ijr.SystemOfRecordId equals sor.SystemofRecordId
                             join ss in _oMTDataContext.SkillSet on ijr.SkillSetId equals ss.SkillSetId
                             join bg in _oMTDataContext.BusinessGroup on ijr.BusinessGroupId equals bg.BusinessGroupId
                             join pt in _oMTDataContext.ProcessType on ijr.ProcessTypeId equals pt.ProcessTypeId
                             join st in _oMTDataContext.SourceType on ijr.SourceTypeId equals st.SourceTypeId
                             join cc in _oMTDataContext.CostCenter on ijr.CostCenterId equals cc.CostCenterId
                             join tof in _oMTDataContext.TotalOrderFees on ijr.TotalOrderFeesId equals tof.TotalOrderFeesId
                             where ss.IsActive == true && sor.IsActive == true && bg.IsActive && pt.IsActive && st.IsActive
                             orderby ss.SkillSetName
                             select new 
                             {
                                 SystemOfRecordName = sor.SystemofRecordName,
                                 SkillSetName = ss.SkillSetName,
                                 BusinessGroupName = bg.BusinessGroupName,
                                 ProcessTypeName = pt.ProcessTypeName,
                                 SourceTypeName = st.SourceTypeName,
                                 CostCenterAmount = cc.CostCenterAmount,
                                 TotalOrderFeesAmount = tof.TotalOrderFeesAmount 
                             }).ToList();

                if (invjr != null)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.StatusCode = "200";
                    resultDTO.Data = invjr;
                    resultDTO.Message = "List of Resware Invoice mapping fetched succesfully.";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message="List of Resware Invoice mapping fetched succesfully.";
                    
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

        public ResultDTO UpdateInvoiceJointResware(InvoiceJointReswareUpdateDTO invoiceJointReswareUpdateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                InvoiceJointResware? invoiceJointResware = _oMTDataContext.InvoiceJointResware.Find(invoiceJointReswareUpdateDTO.InvoiceJointReswareId);

                if (invoiceJointResware != null)
                {
                    invoiceJointResware.BusinessGroupId = invoiceJointReswareUpdateDTO.BusinessGroupId;
                    invoiceJointResware.ProcessTypeId = invoiceJointReswareUpdateDTO.ProcessTypeId;
                    invoiceJointResware.SourceTypeId = invoiceJointReswareUpdateDTO.SourceTypeId;
                    invoiceJointResware.CostCenterId = invoiceJointReswareUpdateDTO.CostCenterId;
                    invoiceJointResware.TotalOrderFeesId = invoiceJointReswareUpdateDTO.TotalOrderFeesId;

                    _oMTDataContext.InvoiceJointResware.Update(invoiceJointResware);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Resware Invoice mapping has been updated successfully";

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
