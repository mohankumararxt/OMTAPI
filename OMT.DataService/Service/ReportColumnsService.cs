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
                var groupedColumnList = (from mrc in _oMTDataContext.MasterReportColumns
                                         join rc in _oMTDataContext.ReportColumns on mrc.MasterReportColumnsId equals rc.MasterReportColumnId
                                         join ss in _oMTDataContext.SkillSet on rc.SkillSetId equals ss.SkillSetId
                                         join sr in _oMTDataContext.SystemofRecord on rc.SystemOfRecordId equals sr.SystemofRecordId
                                         where (ss.SkillSetId == skillsetid || skillsetid == null) && ss.IsActive == true && rc.IsActive == true && sr.IsActive == true
                                         select new
                                         {
                                             SkillSetId = ss.SkillSetId,
                                             SystemofRecordId = sr.SystemofRecordId,
                                             ReportColumnName = mrc.ReportColumnName,
                                             ColumnSequence = rc.ColumnSequence
                                         })
                                         .OrderBy(x => x.SystemofRecordId)
                                         .ThenBy(x => x.SkillSetId)
                                         .ThenBy(x => x.ColumnSequence)
                                         .ToList()
                                         .GroupBy(x => new { x.SkillSetId, x.SystemofRecordId })
                                         .Select(g => new
                                         {
                                             SkillSetId = g.Key.SkillSetId,
                                             SystemofRecordId = g.Key.SystemofRecordId,
                                             ColumnDetails = g.Select(c => new
                                             {
                                                 ColumnName = c.ReportColumnName,
                                                 ColumnSequence = c.ColumnSequence
                                             }).ToList()
                                         })
                                         .ToList();

                if (groupedColumnList.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Report Columns";
                    resultDTO.Data = groupedColumnList;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Report Columns not found";
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
        public ResultDTO UpdateReportColumns(UpdateReportColumnsDTO updateReportColumnsDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                //existing MasterReportColumns
                var existingMasterRepCols = _oMTDataContext.MasterReportColumns
                                            .Where(mrc => updateReportColumnsDTO.MasterReportColumnNames.Contains(mrc.ReportColumnName))
                                            .ToDictionary(mrc => mrc.ReportColumnName, mrc => mrc.MasterReportColumnsId);


                var newColumnNames = updateReportColumnsDTO.MasterReportColumnNames
                                    .Except(existingMasterRepCols.Keys)
                                    .ToList();

                //Add new colnames to MasterReportColumns
                if (newColumnNames.Any())
                {
                    var newMasterCols = newColumnNames.Select(name => new MasterReportColumns
                    {
                        ReportColumnName = name
                    }).ToList();

                    _oMTDataContext.MasterReportColumns.AddRange(newMasterCols);
                    _oMTDataContext.SaveChanges();

                    foreach (var newCol in newMasterCols)
                    {
                        existingMasterRepCols.Add(newCol.ReportColumnName, newCol.MasterReportColumnsId);
                    }
                }

                //Rep Col Table
                var existingReportColumns = _oMTDataContext.ReportColumns
                                            .Where(rc => rc.SkillSetId == updateReportColumnsDTO.SkillSetId)
                                            .ToList();

                //Disable 
                if (existingReportColumns.Any())
                {
                    foreach (var column in existingReportColumns)
                    {
                        column.IsActive = false;
                    }
                    _oMTDataContext.ReportColumns.UpdateRange(existingReportColumns);
                    _oMTDataContext.SaveChanges();
                }
                //add in incoming sqnc
                var newCols = new List<ReportColumns>();
                int columnSequenceOrder = 1;

                foreach (var columnName in updateReportColumnsDTO.MasterReportColumnNames)
                {

                    var masterReportColumnId = existingMasterRepCols.Where(id => id.Key == columnName)
                                               .Select(id => id.Value).FirstOrDefault();

                    if (masterReportColumnId != 0)
                    {
                        var existingColumn = existingReportColumns
                                            .FirstOrDefault(rc => rc.MasterReportColumnId == masterReportColumnId);

                        if (existingColumn != null)
                        {
                            // Reactivate 
                            existingColumn.IsActive = true;
                            existingColumn.ColumnSequence = columnSequenceOrder++;

                            _oMTDataContext.ReportColumns.UpdateRange(existingReportColumns);   
                        }
                        else
                        {
                            // Add 
                            newCols.Add(new ReportColumns
                            {
                                SkillSetId = updateReportColumnsDTO.SkillSetId,
                                SystemOfRecordId = updateReportColumnsDTO.SystemofRecordId,
                                MasterReportColumnId = masterReportColumnId,
                                ColumnSequence = columnSequenceOrder++,
                                IsActive = true

                            });
                            _oMTDataContext.ReportColumns.AddRange(newCols);                          
                        }
                    }
                }
                _oMTDataContext.SaveChanges();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "ReportColumns updated successfully.";
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