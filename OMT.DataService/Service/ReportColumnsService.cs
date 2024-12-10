using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class ReportColumnsService : IReportColumnsService
    {
        private readonly OMTDataContext _oMTDataContext;
        public ReportColumnsService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO GetReportColumnlist(int? skillsetid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var reportColumnList = (from mrc in _oMTDataContext.MasterReportColumns
                                        join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                        join ss in _oMTDataContext.SkillSet on rc.SkillSetId equals ss.SkillSetId
                                        join sr in _oMTDataContext.SystemofRecord on rc.SystemOfRecordId equals sr.SystemofRecordId
                                        where (ss.SkillSetId == skillsetid || skillsetid == null) && ss.IsActive == true && rc.IsActive == true && sr.IsActive == true
                                        select new
                                        {
                                            SkillSetName = ss.SkillSetName,
                                            SkillSetId = ss.SkillSetId,
                                            SystemofRecordId = sr.SystemofRecordId,
                                            SystemofRecordName = sr.SystemofRecordName,
                                            ReportColumnName = mrc.ReportColumnName,
                                            MasterReportColumnId = rc.MasterReportColumnId,
                                            ColumnSequence = rc.ColumnSequence,

                                        })
                                        .OrderBy(x => x.SystemofRecordName)
                                        .ThenBy(x => x.SkillSetName)
                                        .ThenBy(x => x.ColumnSequence)
                                        .ToList();

                if (reportColumnList.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of ReportColumns";
                    resultDTO.Data = reportColumnList;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "ReportColumns not found";
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
        public ResultDTO CreateReportColumns(CreateReportColumnsDTO createReportColumnsDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                //Check Skillset Already Exists
                var ExistingSkillset = (from rc in _oMTDataContext.ReportColumns
                                        join ss in _oMTDataContext.SkillSet on rc.SkillSetId equals ss.SkillSetId
                                        where ss.SkillSetId == createReportColumnsDTO.SkillSetId
                                        select rc).FirstOrDefault();

                if (ExistingSkillset != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "ReportColumns are already Configured for this Skillset...Please Update it.";

                    return resultDTO;
                }

                //Find Existing Names
                var existingMasterRepCols = _oMTDataContext.MasterReportColumns
                                        .Where(mrc => createReportColumnsDTO.MasterReportColumnNames.Contains(mrc.ReportColumnName))
                                        .ToDictionary(mrc => mrc.ReportColumnName, mrc => mrc.ReportColumnName);

                var newColumns = createReportColumnsDTO.MasterReportColumnNames.Except(existingMasterRepCols.Keys).ToList();

                //Add 
                if (newColumns.Any())
                {
                    var newMasterCol = newColumns.Select(name => new MasterReportColumns { ReportColumnName = name }).ToList();

                    _oMTDataContext.MasterReportColumns.AddRange(newMasterCol);
                    _oMTDataContext.SaveChanges();
                }

                //Total Id's & Ordering Accordingly
                var masterrepId = _oMTDataContext.MasterReportColumns
                                  .Where(mrc => createReportColumnsDTO.MasterReportColumnNames.Contains(mrc.ReportColumnName))
                                  .ToList();

                var TotalColIds = masterrepId
                                  .OrderBy(mrc => createReportColumnsDTO.MasterReportColumnNames.IndexOf(mrc.ReportColumnName))
                                  .Select(mrc => mrc.MasterReportColumnsId).ToList();

                var newCols = new List<ReportColumns>(); //If no active Cols exist (new list)

                int ColsequenceOrder = 1;

                foreach (var marterReportColumnId in TotalColIds)
                {

                    newCols.Add(new ReportColumns
                    {
                        SkillSetId = createReportColumnsDTO.SkillSetId,
                        SystemOfRecordId = createReportColumnsDTO.SystemofRecordId,
                        MasterReportColumnId = marterReportColumnId,
                        ColumnSequence = ColsequenceOrder++,   //increament
                        IsActive = true
                    });
                }
                _oMTDataContext.ReportColumns.AddRange(newCols);
                _oMTDataContext.SaveChanges();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "ReportColumns Added Successfully";
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