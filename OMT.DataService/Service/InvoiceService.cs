using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
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

                if (getinvoiceDTO.SkillSetId != null)
                {
                    InvoiceSkillSet isinvoiceskillset = _oMTDataContext.InvoiceSkillSet.Where(x => x.SkillSetName == getinvoiceDTO.SkillSetName && x.IsActive).FirstOrDefault();


                    if (isinvoiceskillset == null)
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
                            if (getinvoiceDTO.SystemofRecordId == 1 || getinvoiceDTO.SystemofRecordId == 3)
                            {
                                var invoiceDump = _oMTDataContext.InvoiceDump.Where(x => x.SkillSet == skillSet.SkillSet && x.SystemOfRecord == skillSet.SystemOfRecord && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date)
                                                                              .OrderBy(x => x.CompletionDate)
                                                                              .Select(_ => new SciInvoiceDTO()
                                                                              {
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
                                                                                  PropertyState = _.PropertyState,
                                                                                  OrderFees = _.OrderFees,
                                                                                  AOLFees = _.AOLFees,
                                                                                  CopyFees = _.CopyFees,
                                                                                  CertifiedCopyFees = _.CertifiedCopyFees,
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
                    else
                    {
                        var systemofrecordname = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == isinvoiceskillset.SystemofRecordId).Select(_ => _.SystemofRecordName).FirstOrDefault();

                        var invoices = _oMTDataContext.InvoiceSkillSet
                                       .Where(x => x.IsActive == true && !string.IsNullOrEmpty(x.MergeSkillSets) && !string.IsNullOrEmpty(x.CompareSkillSets) && x.SkillSetName == getinvoiceDTO.SkillSetName)
                                       .Select(x => new { x.MergeSkillSets, x.CompareSkillSets })
                                       .FirstOrDefault();

                        var skillSets = _oMTDataContext.SkillSet.Where(x => x.IsActive).ToList();

                        var mergeSkillSetNames = invoices.MergeSkillSets
                                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(int.Parse)
                                                .Join(skillSets, id => id, ss => ss.SkillSetId, (id, ss) => ss.SkillSetName)
                                                .Distinct()
                                                .ToList();

                        var compareskillsetnames = invoices.CompareSkillSets
                                                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(int.Parse)
                                                   .Join(skillSets, id => id, ss => ss.SkillSetId, (id, ss) => ss.SkillSetName)
                                                   .Distinct()
                                                   .ToList();

                        if (mergeSkillSetNames != null && compareskillsetnames != null)
                        {
                            var commonmergerecords = _oMTDataContext.InvoiceDump.Where(x => mergeSkillSetNames.Contains(x.SkillSet) && x.SystemOfRecord == systemofrecordname
                                                                         && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date).Select(_ => new
                                                                         {
                                                                             _.SkillSet,
                                                                             _.SystemOfRecord,
                                                                             _.OrderId,
                                                                             _.ProcessType,
                                                                             _.CompletionDate,
                                                                             _.SourceType,
                                                                             _.CostCenter,
                                                                             _.TotalOrderFees,
                                                                             _.BusinessGroup,
                                                                             _.County,
                                                                             _.CustomerId,
                                                                             _.ProductDescription,
                                                                             _.PropertyState,
                                                                             _.OrderFees,
                                                                             _.AOLFees,
                                                                             _.CopyFees,
                                                                             _.CertifiedCopyFees,
                                                                             //_.Business,
                                                                             //_.Workflowstatus,      add these for sci
                                                                             //_.Customer,
                                                                             //_.ProjectId,

                                                                         }).ToList();

                            var commoncombrecords = _oMTDataContext.InvoiceDump.Where(x => compareskillsetnames.Contains(x.SkillSet) && x.SystemOfRecord == systemofrecordname
                                                                          && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date).Select(_ => new
                                                                          {
                                                                              _.SkillSet,
                                                                              _.SystemOfRecord,
                                                                              _.OrderId,
                                                                              _.ProcessType,
                                                                              _.CompletionDate,
                                                                              _.SourceType,
                                                                              _.CostCenter,
                                                                              _.TotalOrderFees,
                                                                              _.BusinessGroup,
                                                                              _.County,
                                                                              _.CustomerId,
                                                                              _.ProductDescription,
                                                                              _.PropertyState,
                                                                              _.OrderFees,
                                                                              _.AOLFees,
                                                                              _.CopyFees,
                                                                              _.CertifiedCopyFees,
                                                                              //_.Business,
                                                                              //_.Workflowstatus,  // add these for sci
                                                                              //_.Customer,
                                                                              //_.ProjectId,
                                                                          }).ToList();



                            if (isinvoiceskillset.OperationType == 1)
                            {
                                if (getinvoiceDTO.SystemofRecordId == 1 || getinvoiceDTO.SystemofRecordId == 3)
                                {
                                    // fetch records for SCI 
                                }
                                else if (getinvoiceDTO.SystemofRecordId == 2)
                                {

                                    var invoiceDump2 = (from cmn1 in commonmergerecords
                                                        join cmn2 in commoncombrecords on cmn1.OrderId equals cmn2.OrderId
                                                        select new ReswareInvoiceDTO()
                                                        {
                                                            SkillSet = getinvoiceDTO.SkillSetName,
                                                            SystemOfRecord = cmn1.SystemOfRecord,
                                                            OrderId = cmn1.OrderId,
                                                            ProcessType = "Verify-PP",
                                                            CompletionDate = cmn1.CompletionDate.ToString("MM/dd/yyyy"),
                                                            SourceType = cmn1.SourceType,
                                                            BusinessGroup = cmn1.BusinessGroup,
                                                            County = cmn1.County,
                                                            CostCenter = cmn1.CostCenter,
                                                            CustomerId = cmn1.CustomerId,
                                                            TotalOrderFees = cmn1.TotalOrderFees,
                                                            ProductDescription = cmn1.ProductDescription,
                                                            PropertyState = cmn1.PropertyState,
                                                            OrderFees = cmn1.OrderFees,
                                                            AOLFees = cmn1.AOLFees,
                                                            CertifiedCopyFees = cmn1.CertifiedCopyFees,
                                                            CopyFees = cmn1.CopyFees,
                                                        }).OrderBy(x => x.CompletionDate).ToList();

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
                                        resultDTO.Message = "Invoice details not found for the specified details";
                                        resultDTO.StatusCode = "404";
                                    }
                                }
                            }
                            else if (isinvoiceskillset.OperationType == 2)
                            {
                                if (getinvoiceDTO.SystemofRecordId == 1 || getinvoiceDTO.SystemofRecordId == 3)
                                {
                                    // fetch records for SCI 
                                }
                                else if (getinvoiceDTO.SystemofRecordId == 2)
                                {
                                    var invoiceDump2 = (from cmn1 in commonmergerecords
                                                        join cmn2 in commoncombrecords on cmn1.OrderId equals cmn2.OrderId into cmn3
                                                        from cmn4 in cmn3.DefaultIfEmpty()
                                                        where cmn4 == null
                                                        select new ReswareInvoiceDTO()
                                                        {
                                                            SkillSet = getinvoiceDTO.SkillSetName,
                                                            SystemOfRecord = cmn1.SystemOfRecord,
                                                            OrderId = cmn1.OrderId,
                                                            ProcessType = cmn1.ProcessType,
                                                            CompletionDate = cmn1.CompletionDate.ToString("MM/dd/yyyy"),
                                                            SourceType = cmn1.SourceType,
                                                            BusinessGroup = cmn1.BusinessGroup,
                                                            County = cmn1.County,
                                                            CostCenter = cmn1.CostCenter,
                                                            CustomerId = cmn1.CustomerId,
                                                            TotalOrderFees = cmn1.TotalOrderFees,
                                                            ProductDescription = cmn1.ProductDescription,
                                                            PropertyState = cmn1.PropertyState,
                                                            OrderFees = cmn1.OrderFees,
                                                            AOLFees = cmn1.AOLFees,
                                                            CertifiedCopyFees = cmn1.CertifiedCopyFees,
                                                            CopyFees = cmn1.CopyFees,
                                                        }).OrderBy(x => x.CompletionDate).ToList();

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
                                        resultDTO.Message = "Invoice details not found for the specified details";
                                        resultDTO.StatusCode = "404";
                                    }
                                }

                            }
                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "Required resource not found";
                            resultDTO.StatusCode = "404";
                        }
                    }
                }
                else
                {
                    List<SkillSet> avalskillset = new List<SkillSet> ();

                    var sorininv = _oMTDataContext.InvoiceSkillSet.Where(x => x.SystemofRecordId == getinvoiceDTO.SystemofRecordId).FirstOrDefault();

                    if (sorininv == null)
                    {
                         avalskillset = (from ss in _oMTDataContext.SkillSet
                                            where ss.SystemofRecordId == getinvoiceDTO.SystemofRecordId && ss.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                            select new SkillSet
                                            {
                                                SkillSetName = ss.SkillSetName,
                                                SkillSetId = ss.SkillSetId,
                                                SystemofRecordId = ss.SystemofRecordId,
                                            }).ToList();
                    }
                    else if (sorininv != null)
                    {
                        List<SkillSet> ListofSkillSets1 = (from sor in _oMTDataContext.SystemofRecord
                                                                             join ss in _oMTDataContext.SkillSet on sor.SystemofRecordId equals ss.SystemofRecordId
                                                                             where ss.IsActive == true && sor.SystemofRecordId == getinvoiceDTO.SystemofRecordId && sor.IsActive
                                                                             orderby sor.SystemofRecordName, ss.SkillSetName
                                                                             select new SkillSet
                                                                             {
                                                                                 SkillSetName = ss.SkillSetName,
                                                                                 SkillSetId = ss.SkillSetId,
                                                                                 SystemofRecordId = ss.SystemofRecordId,
                                                                             }).ToList();

                        int skillsetidcounter = -1;

                        var rawlist = (from inv in _oMTDataContext.InvoiceSkillSet
                                       join sor in _oMTDataContext.SystemofRecord on inv.SystemofRecordId equals sor.SystemofRecordId
                                       join ss in _oMTDataContext.SkillSet on inv.SkillSetName equals ss.SkillSetName into ssJoin
                                       from subSS in ssJoin.DefaultIfEmpty()
                                       where inv.ShowInInvoice == true && subSS == null && sor.SystemofRecordId == getinvoiceDTO.SystemofRecordId
                                       orderby inv.SkillSetName
                                       select new
                                       {
                                           SkillSetName = inv.SkillSetName,
                                           SystemofRecordName = sor.SystemofRecordName,
                                           SystemofRecordId = sor.SystemofRecordId,
                                       }).ToList();

                        List<SkillSet> ListofSkillSets2 = rawlist.Select(x => new SkillSet
                        {
                            SkillSetName = x.SkillSetName,
                            SkillSetId = skillsetidcounter--,
                            SystemofRecordId = x.SystemofRecordId,
                            
                        }).ToList();

                        avalskillset = ListofSkillSets1.Union(ListofSkillSets2).OrderBy(s => s.SkillSetName).ToList();

                    }

                    List<SciInvoiceDTO> invscitrd = new List<SciInvoiceDTO>();
                    List<ReswareInvoiceDTO> invres = new List<ReswareInvoiceDTO>();

                    foreach (var sk in avalskillset)
                    {
                        InvoiceSkillSet isinvoiceskillset = _oMTDataContext.InvoiceSkillSet.Where(x => x.SkillSetName == sk.SkillSetName && x.IsActive).FirstOrDefault();

                        if (isinvoiceskillset == null)
                        {
                            var skillSet = (from ss in _oMTDataContext.SkillSet
                                            join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                            where ss.SkillSetId == sk.SkillSetId && ss.SystemofRecordId == sk.SystemofRecordId
                                            select new
                                            {
                                                SkillSet = ss.SkillSetName,
                                                SystemOfRecord = sor.SystemofRecordName
                                            }).FirstOrDefault();

                            if (skillSet != null)
                            {
                                if (getinvoiceDTO.SystemofRecordId == 1 || getinvoiceDTO.SystemofRecordId == 3)
                                {
                                    var invoiceDump = _oMTDataContext.InvoiceDump.Where(x => x.SkillSet == skillSet.SkillSet && x.SystemOfRecord == skillSet.SystemOfRecord && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date)
                                                                                  .OrderBy(x => x.CompletionDate)
                                                                                  .Select(_ => new SciInvoiceDTO()
                                                                                  {
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
                                        invscitrd.AddRange(invoiceDump);
                                        //resultDTO.IsSuccess = true;
                                        //resultDTO.Message = "Invoice details fetched successfully";
                                        //resultDTO.StatusCode = "200";
                                        //resultDTO.Data = invoiceDump;
                                    }
                                    //else
                                    //{
                                    //    resultDTO.IsSuccess = false;
                                    //    resultDTO.Message = "Invoice details doesnt exist for the specified details";
                                    //    resultDTO.StatusCode = "404";
                                    //}
                                }
                                else if (getinvoiceDTO.SystemofRecordId == 2)
                                {
                                    var invoiceDump2 = _oMTDataContext.InvoiceDump.Where(x => x.SkillSet == skillSet.SkillSet && x.SystemOfRecord == skillSet.SystemOfRecord && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date)
                                                                                  .OrderBy(x => x.CompletionDate)
                                                                                  .Select(_ => new ReswareInvoiceDTO()
                                                                                  {
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
                                                                                      PropertyState = _.PropertyState,
                                                                                      OrderFees = _.OrderFees,
                                                                                      AOLFees = _.AOLFees,
                                                                                      CopyFees = _.CopyFees,
                                                                                      CertifiedCopyFees = _.CertifiedCopyFees,
                                                                                  }).ToList();

                                    if (invoiceDump2.Count > 0)
                                    {
                                        invres.AddRange(invoiceDump2);
                                        //resultDTO.IsSuccess = true;
                                        //resultDTO.Message = "Invoice details fetched successfully";
                                        //resultDTO.StatusCode = "200";
                                        //resultDTO.Data = invoiceDump2;
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
                        else
                        {
                            var systemofrecordname = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == isinvoiceskillset.SystemofRecordId).Select(_ => _.SystemofRecordName).FirstOrDefault();

                            var invoices = _oMTDataContext.InvoiceSkillSet
                                           .Where(x => x.IsActive == true && !string.IsNullOrEmpty(x.MergeSkillSets) && !string.IsNullOrEmpty(x.CompareSkillSets) && x.SkillSetName == sk.SkillSetName)
                                           .Select(x => new { x.MergeSkillSets, x.CompareSkillSets })
                                           .FirstOrDefault();

                            var skillSets = _oMTDataContext.SkillSet.Where(x => x.IsActive).ToList();

                            var mergeSkillSetNames = invoices.MergeSkillSets
                                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(int.Parse)
                                                    .Join(skillSets, id => id, ss => ss.SkillSetId, (id, ss) => ss.SkillSetName)
                                                    .Distinct()
                                                    .ToList();

                            var compareskillsetnames = invoices.CompareSkillSets
                                                       .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(int.Parse)
                                                       .Join(skillSets, id => id, ss => ss.SkillSetId, (id, ss) => ss.SkillSetName)
                                                       .Distinct()
                                                       .ToList();

                            if (mergeSkillSetNames != null && compareskillsetnames != null)
                            {
                                var commonmergerecords = _oMTDataContext.InvoiceDump.Where(x => mergeSkillSetNames.Contains(x.SkillSet) && x.SystemOfRecord == systemofrecordname
                                                                             && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date).Select(_ => new
                                                                             {
                                                                                 _.SkillSet,
                                                                                 _.SystemOfRecord,
                                                                                 _.OrderId,
                                                                                 _.ProcessType,
                                                                                 _.CompletionDate,
                                                                                 _.SourceType,
                                                                                 _.CostCenter,
                                                                                 _.TotalOrderFees,
                                                                                 _.BusinessGroup,
                                                                                 _.County,
                                                                                 _.CustomerId,
                                                                                 _.ProductDescription,
                                                                                 _.PropertyState,
                                                                                 _.OrderFees,
                                                                                 _.AOLFees,
                                                                                 _.CopyFees,
                                                                                 _.CertifiedCopyFees,
                                                                                 //_.Business,
                                                                                 //_.Workflowstatus,      add these for sci
                                                                                 //_.Customer,
                                                                                 //_.ProjectId,

                                                                             }).ToList();

                                var commoncombrecords = _oMTDataContext.InvoiceDump.Where(x => compareskillsetnames.Contains(x.SkillSet) && x.SystemOfRecord == systemofrecordname
                                                                              && x.CompletionDate.Date >= getinvoiceDTO.StartDate.Date && x.CompletionDate.Date <= getinvoiceDTO.EndDate.Date).Select(_ => new
                                                                              {
                                                                                  _.SkillSet,
                                                                                  _.SystemOfRecord,
                                                                                  _.OrderId,
                                                                                  _.ProcessType,
                                                                                  _.CompletionDate,
                                                                                  _.SourceType,
                                                                                  _.CostCenter,
                                                                                  _.TotalOrderFees,
                                                                                  _.BusinessGroup,
                                                                                  _.County,
                                                                                  _.CustomerId,
                                                                                  _.ProductDescription,
                                                                                  _.PropertyState,
                                                                                  _.OrderFees,
                                                                                  _.AOLFees,
                                                                                  _.CopyFees,
                                                                                  _.CertifiedCopyFees,
                                                                                  //_.Business,
                                                                                  //_.Workflowstatus,  // add these for sci
                                                                                  //_.Customer,
                                                                                  //_.ProjectId,
                                                                              }).ToList();



                                if (isinvoiceskillset.OperationType == 1)
                                {
                                    if (getinvoiceDTO.SystemofRecordId == 1 || getinvoiceDTO.SystemofRecordId == 3)
                                    {
                                        // fetch records for SCI 
                                    }
                                    else if (getinvoiceDTO.SystemofRecordId == 2)
                                    {

                                        var invoiceDump2 = (from cmn1 in commonmergerecords
                                                            join cmn2 in commoncombrecords on cmn1.OrderId equals cmn2.OrderId
                                                            select new ReswareInvoiceDTO()
                                                            {
                                                                SkillSet = sk.SkillSetName,
                                                                SystemOfRecord = cmn1.SystemOfRecord,
                                                                OrderId = cmn1.OrderId,
                                                                ProcessType = "Verify-PP",
                                                                CompletionDate = cmn1.CompletionDate.ToString("MM/dd/yyyy"),
                                                                SourceType = cmn1.SourceType,
                                                                BusinessGroup = cmn1.BusinessGroup,
                                                                County = cmn1.County,
                                                                CostCenter = cmn1.CostCenter,
                                                                CustomerId = cmn1.CustomerId,
                                                                TotalOrderFees = cmn1.TotalOrderFees,
                                                                ProductDescription = cmn1.ProductDescription,
                                                                PropertyState = cmn1.PropertyState,
                                                                OrderFees = cmn1.OrderFees,
                                                                AOLFees = cmn1.AOLFees,
                                                                CertifiedCopyFees = cmn1.CertifiedCopyFees,
                                                                CopyFees = cmn1.CopyFees,
                                                            }).OrderBy(x => x.CompletionDate).ToList();

                                        if (invoiceDump2.Count > 0)
                                        {
                                            invres.AddRange(invoiceDump2);
                                            //resultDTO.IsSuccess = true;
                                            //resultDTO.Message = "Invoice details fetched successfully";
                                            //resultDTO.StatusCode = "200";
                                            //resultDTO.Data = invoiceDump2;
                                        }
                                        //else
                                        //{
                                        //    resultDTO.IsSuccess = false;
                                        //    resultDTO.Message = "Invoice details not found for the specified details";
                                        //    resultDTO.StatusCode = "404";
                                        //}
                                    }
                                }
                                else if (isinvoiceskillset.OperationType == 2)
                                {
                                    if (getinvoiceDTO.SystemofRecordId == 1 || getinvoiceDTO.SystemofRecordId == 3)
                                    {
                                        // fetch records for SCI 
                                    }
                                    else if (getinvoiceDTO.SystemofRecordId == 2)
                                    {
                                        var invoiceDump2 = (from cmn1 in commonmergerecords
                                                            join cmn2 in commoncombrecords on cmn1.OrderId equals cmn2.OrderId into cmn3
                                                            from cmn4 in cmn3.DefaultIfEmpty()
                                                            where cmn4 == null
                                                            select new ReswareInvoiceDTO()
                                                            {
                                                                SkillSet = sk.SkillSetName,
                                                                SystemOfRecord = cmn1.SystemOfRecord,
                                                                OrderId = cmn1.OrderId,
                                                                ProcessType = cmn1.ProcessType,
                                                                CompletionDate = cmn1.CompletionDate.ToString("MM/dd/yyyy"),
                                                                SourceType = cmn1.SourceType,
                                                                BusinessGroup = cmn1.BusinessGroup,
                                                                County = cmn1.County,
                                                                CostCenter = cmn1.CostCenter,
                                                                CustomerId = cmn1.CustomerId,
                                                                TotalOrderFees = cmn1.TotalOrderFees,
                                                                ProductDescription = cmn1.ProductDescription,
                                                                PropertyState = cmn1.PropertyState,
                                                                OrderFees = cmn1.OrderFees,
                                                                AOLFees = cmn1.AOLFees,
                                                                CertifiedCopyFees = cmn1.CertifiedCopyFees,
                                                                CopyFees = cmn1.CopyFees,
                                                            }).OrderBy(x => x.CompletionDate).ToList();

                                        if (invoiceDump2.Count > 0)
                                        {
                                            invres.AddRange(invoiceDump2);
                                            //resultDTO.IsSuccess = true;
                                            //resultDTO.Message = "Invoice details fetched successfully";
                                            //resultDTO.StatusCode = "200";
                                            //resultDTO.Data = invoiceDump2;
                                        }
                                        //else
                                        //{
                                        //    resultDTO.IsSuccess = false;
                                        //    resultDTO.Message = "Invoice details not found for the specified details";
                                        //    resultDTO.StatusCode = "404";
                                        //}
                                    }

                                }
                            }
                            else
                            {
                                resultDTO.IsSuccess = false;
                                resultDTO.Message = "Required resource not found";
                                resultDTO.StatusCode = "404";
                            }
                        }
                    }
                    if (invres.Count > 0)
                    {
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Invoice details fetched successfully";
                        resultDTO.StatusCode = "200";
                        resultDTO.Data = invres;
                    }
                    else if (invscitrd.Count > 0)
                    {
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Invoice details fetched successfully";
                        resultDTO.StatusCode = "200";
                        resultDTO.Data = invscitrd;
                    }
                    else if (invres.Count == 0 || invscitrd.Count == 0) 
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Invoice details not found";
                    }
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

        public ResultDTO GetInvoiceSkillSetList(int sorid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<InvoiceSkillSetResponseDTO> ListofSkillSets1 = (from sor in _oMTDataContext.SystemofRecord
                                                                     join ss in _oMTDataContext.SkillSet on sor.SystemofRecordId equals ss.SystemofRecordId
                                                                     where ss.IsActive == true && sor.SystemofRecordId == sorid && sor.IsActive
                                                                     orderby sor.SystemofRecordName, ss.SkillSetName
                                                                     select new InvoiceSkillSetResponseDTO
                                                                     {
                                                                         SkillSetName = ss.SkillSetName,
                                                                         SkillSetId = ss.SkillSetId,
                                                                         SystemofRecordName = sor.SystemofRecordName,
                                                                         SystemofRecordId = ss.SystemofRecordId,
                                                                     }).ToList();

                int skillsetidcounter = -1;

                var rawlist = (from inv in _oMTDataContext.InvoiceSkillSet
                               join sor in _oMTDataContext.SystemofRecord on inv.SystemofRecordId equals sor.SystemofRecordId
                               join ss in _oMTDataContext.SkillSet on inv.SkillSetName equals ss.SkillSetName into ssJoin
                               from subSS in ssJoin.DefaultIfEmpty()
                               where inv.ShowInInvoice == true && subSS == null && sor.SystemofRecordId == sorid
                               orderby inv.SkillSetName
                               select new
                               {
                                   SkillSetName = inv.SkillSetName,
                                   SystemofRecordName = sor.SystemofRecordName,
                                   SystemofRecordId = sor.SystemofRecordId,
                               }).ToList();

                List<InvoiceSkillSetResponseDTO> ListofSkillSets2 = rawlist.Select(x => new InvoiceSkillSetResponseDTO
                {
                    SkillSetName = x.SkillSetName,
                    SkillSetId = skillsetidcounter--,
                    SystemofRecordId = x.SystemofRecordId,
                    SystemofRecordName = x.SystemofRecordName,
                }).ToList();

                var ListofSkillSets = ListofSkillSets1.Union(ListofSkillSets2).OrderBy(s => s.SkillSetName).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of SkillSets";
                resultDTO.Data = ListofSkillSets;
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
