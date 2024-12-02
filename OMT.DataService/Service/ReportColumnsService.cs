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
                                        where (ss.SkillSetId == skillsetid || skillsetid == null) && ss.IsActive==true && rc.IsActive==true && sr.IsActive==true 
                                        select new
                                        {
                                            SkillSetName = ss.SkillSetName,
                                            SkillSetId = ss.SkillSetId,
                                            SystemofRecordId = sr.SystemofRecordId,
                                            SystemofRecordName = sr.SystemofRecordName,
                                            ReportColumnName = mrc.ReportColumnName,  
                                            MasterReportColumnId = rc.MasterReportColumnId,
                                            ColumnSequence=rc.ColumnSequence,

                                        })
                                        .OrderBy(x => x.SkillSetName)  
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
                //all Exisiting cols
                var existingColumns = (from mrc in _oMTDataContext.MasterReportColumns
                                       join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                       join ss in _oMTDataContext.SkillSet on rc.SkillSetId equals ss.SkillSetId
                                       join sr in _oMTDataContext.SystemofRecord on rc.SystemOfRecordId equals sr.SystemofRecordId
                                       where rc.SkillSetId == createReportColumnsDTO.SkillSetId && rc.SystemOfRecordId == createReportColumnsDTO.SystemofRecordId
                                       && createReportColumnsDTO.MasterReportColumnId.Contains(rc.MasterReportColumnId) && rc.IsActive
                                       select new
                                       {
                                           SkillSetName = ss.SkillSetName,
                                           SkillSetId = ss.SkillSetId,
                                           SystemofRecordId = sr.SystemofRecordId,
                                           SystemofRecordName = sr.SystemofRecordName,
                                           ReportColumnName = mrc.ReportColumnName,
                                           ReportColumnId = rc.ReportColumnsId,
                                           IsActive = rc.IsActive
                                       }).ToList();

                if (existingColumns.Any())
                {
                    //if any IsActive=1
                    var skillsetName = existingColumns.First().SkillSetName;
                    var RepColName = string.Join(",", existingColumns.Select(x => x.ReportColumnName));
                    resultDTO.Message = $"The Following Columns are Already Mapped to {skillsetName}->{RepColName}, Please try again..";
                    resultDTO.IsSuccess = false;
                    return resultDTO;
                }

                var newCols = new List<ReportColumns>(); //If no active Cols exist (new list)

                int ColsequenceOrder = 1;

                foreach (var marterReportColumnId in createReportColumnsDTO.MasterReportColumnId)
                {
                    var existingColumn = _oMTDataContext.ReportColumns.FirstOrDefault
                                        (rc => rc.SkillSetId == createReportColumnsDTO.SkillSetId &&
                                        rc.SystemOfRecordId == createReportColumnsDTO.SystemofRecordId &&
                                        rc.MasterReportColumnId == marterReportColumnId);

                    if (existingColumn == null)  //Add new
                    {
                        newCols.Add(new ReportColumns
                        {
                            SkillSetId = createReportColumnsDTO.SkillSetId,
                            SystemOfRecordId = createReportColumnsDTO.SystemofRecordId,
                            MasterReportColumnId = marterReportColumnId,
                            ColumnSequence= ColsequenceOrder++,   //increament
                            IsActive = true
                        });
                    }
                    else //Reactivate
                    {
                        existingColumn.IsActive = true;
                        existingColumn.ColumnSequence = ColsequenceOrder++;
                        _oMTDataContext.ReportColumns.Update(existingColumn);
                        _oMTDataContext.SaveChanges();
                    }
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