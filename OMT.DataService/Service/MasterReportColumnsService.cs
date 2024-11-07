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
    public class MasterReportColumnsService : IMasterReportColumnsService
    {
        private readonly OMTDataContext _oMTDataContext;

        public MasterReportColumnsService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO CreateReportColumnNames(MasterReportColumnDTO masterReportColumnDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var MC = _oMTDataContext.MasterReportColumns.Where(x => masterReportColumnDTO.ReportColumnNames.Contains(x.ReportColumnName))
                         .Select(x => x.ReportColumnName).ToList();

                if (MC.Any())
                {
                    resultDTO.Message = "The following Report column name already exists: " + string.Join(", ", MC) + ". Please try again.";
                    resultDTO.IsSuccess = false;
                }
                else
                {
                    var Mcnames = masterReportColumnDTO.ReportColumnNames.Select(x => new MasterReportColumns { ReportColumnName = x }).ToList();

                    _oMTDataContext.MasterReportColumns.AddRange(Mcnames);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Master Report Columnnames added successfully.";
                }
            }
            catch (Exception ex)
            {

                resultDTO.StatusCode = "500";
                resultDTO.IsSuccess = false;
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
        public ResultDTO GetMasterReportColumnList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true , StatusCode = "200" };
            try
            {
                var MasterReportColumnList = _oMTDataContext.MasterReportColumns.Select(x => new { x.ReportColumnName,x.MasterReportColumnsId })
                                             .OrderBy(x => x.ReportColumnName).ToList();

                if (MasterReportColumnList.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Master ReportColumnNames";
                    resultDTO.Data = MasterReportColumnList;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Master ReportColumnNames not found";
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