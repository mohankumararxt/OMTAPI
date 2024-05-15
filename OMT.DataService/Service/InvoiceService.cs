using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class InvoiceService : IInvoiceService
    {
        private readonly OMTDataContext _oMTDataContext;

        public InvoiceService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetInvoice(GetInvoiceDTO getinvoiceDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var skillSet = (from ss in _oMTDataContext.SkillSet
                                join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                where ss.SkillSetId == getinvoiceDTO.SkillSetId && ss.SystemofRecordId == getinvoiceDTO.SystemofRecordId
                                select new
                                {
                                    SkillSet = ss.SkillSetName,
                                    SystemOfRecord = sor.SystemofRecordName
                                }).FirstOrDefault();

                if (skillSet != null)
                {
                    if (getinvoiceDTO.SystemofRecordId == 1)
                    {
                        var invoiceDump = _oMTDataContext.InvoiceDump.Where(x => x.SkillSet == skillSet.SkillSet && x.SystemOfRecord == skillSet.SystemOfRecord && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date)
                                                                      .OrderBy(x => x.CompletionDate)
                                                                      .Select(_ => new SciInvoiceDTO()
                                                                      {
                                                                          InvoiceDumpId = _.InvoiceDumpId,
                                                                          SkillSet = _.SkillSet,
                                                                          SystemOfRecord = _.SystemOfRecord,
                                                                          OrderId = _.OrderId,
                                                                          ProjectId = _.ProjectId,
                                                                          CompletionDate = _.CompletionDate.ToString("MM/dd/yyyy"),
                                                                          Business = _.Business,
                                                                          BusinessGroup = _.BusinessGroup,
                                                                          Workflowstatus = _.Workflowstatus,
                                                                          CostCenter = _.CostCenter,
                                                                          TotalOrderFees = _.TotalOrderFees,
                                                                          Customer = _.Customer,
                                                                      }).ToList();

                        if (invoiceDump.Count > 0)
                        {
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Invoice details fetched successfully";
                            resultDTO.StatusCode = "200";
                            resultDTO.Data = invoiceDump;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Invoice details doesnt exist for the specified details";
                            resultDTO.StatusCode = "404";
                        }
                    }
                    else if (getinvoiceDTO.SystemofRecordId == 2)
                    {
                        var invoiceDump2 = _oMTDataContext.InvoiceDump.Where(x => x.SkillSet == skillSet.SkillSet && x.SystemOfRecord == skillSet.SystemOfRecord && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date)
                                                                      .OrderBy(x => x.CompletionDate)
                                                                      .Select(_ => new ReswareInvoiceDTO()
                                                                      {
                                                                          InvoiceDumpId = _.InvoiceDumpId,
                                                                          SkillSet = _.SkillSet,
                                                                          SystemOfRecord = _.SystemOfRecord,
                                                                          OrderId = _.OrderId,
                                                                          ProcessType = _.ProcessType,
                                                                          CompletionDate = _.CompletionDate.ToString("MM/dd/yyyy"),
                                                                          SourceType = _.SourceType,
                                                                          BusinessGroup = _.BusinessGroup,
                                                                          County = _.County,
                                                                          CostCenter = _.CostCenter,
                                                                          CustomerId = _.CustomerId,
                                                                          TotalOrderFees = _.TotalOrderFees,
                                                                          ProductDescription = _.ProductDescription,
                                                                          State = _.State,
                                                                      }).ToList();

                        if (invoiceDump2.Count > 0)
                        {
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Invoice details fetched successfully";
                            resultDTO.StatusCode = "200";
                            resultDTO.Data = invoiceDump2;
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Invoice details doesnt exist for the specified details";
                            resultDTO.StatusCode = "404";
                        }
                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "System of Record or Skill Set doesnt exist";
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
