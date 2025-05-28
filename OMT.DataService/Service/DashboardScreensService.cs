using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Data;
using System.Data.SqlClient;

namespace OMT.DataService.Service
{
    public class DashboardScreensService : IDashboardScreensService
    {
        private readonly OMTDataContext _oMTDataContext;

        public DashboardScreensService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetTodaysOrders()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                DateTime today_date = DateTime.UtcNow.Date;
                DateTime yeserday_date = today_date.AddDays(-1);

                List<int> SOR = new List<int>()
                {
                    1,2,3,4
                };

                List<SorCountDTO> ordercounts = new List<SorCountDTO>();

                foreach (int i in SOR)
                {
                    var count = _oMTDataContext.DailyCount_SOR.Where(x => x.Date == today_date && x.SystemofRecordId == i).Select(x => x.Count).FirstOrDefault();
                    var diff = (count - _oMTDataContext.DailyCount_SOR.Where(x => x.Date == yeserday_date && x.SystemofRecordId == i).Select(x => x.Count).FirstOrDefault()) / 100;
                    var sorname = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == i && x.IsActive).Select(x => x.SystemofRecordName).FirstOrDefault();

                    SorCountDTO sorCount = new SorCountDTO()
                    {
                        SorId = i,
                        SorName = sorname,
                        Count = _oMTDataContext.DailyCount_SOR.Where(x => x.Date == today_date && x.SystemofRecordId == i).Select(x => x.Count).FirstOrDefault(),
                        Difference = diff,
                    };

                    ordercounts.Add(sorCount);

                }

                if (ordercounts.Count > 0)
                {
                    resultDTO.Data = ordercounts;
                    resultDTO.Message = "Todays order details fetched successfully.";
                    resultDTO.IsSuccess = true;
                }
                else
                {
                    resultDTO.Message = "No details found.";
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
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

        public ResultDTO GetVolumeProjection(VolumeProjectionInputDTO volumeProjectionInputDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);
                connection.Open();

                DateTime today_date = DateTime.UtcNow.Date;

                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == volumeProjectionInputDTO.SkillsetId && x.IsActive).FirstOrDefault();

                var received = _oMTDataContext.DailyCount_SkillSet.Where(x => x.SystemofRecordId == volumeProjectionInputDTO.SystemOfRecordId && x.SkillSetId == volumeProjectionInputDTO.SkillsetId && x.Date == today_date).Select(x => x.Count).FirstOrDefault();

                string sql = $"SELECT COUNT(*) FROM {skillSet.SkillSetName} WHERE Userid IS NULL AND Status IS NULL";

                using SqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = sql;

                var not_Assigned = (int)cmd.ExecuteScalar();

                VolumeProjectionOutputDTO volumeProjectionOutputDTO = new VolumeProjectionOutputDTO()
                {
                    Received = received,
                    Completed = received - not_Assigned,
                    Not_Assigned = not_Assigned,
                };

                resultDTO.Data = volumeProjectionOutputDTO;
                resultDTO.IsSuccess = true;
                resultDTO.Message = "Volume Projection details fetched successfully.";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetSorCompletionCount(SorCompletionCountInputDTO sorCompletionCountInputDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var pagination = sorCompletionCountInputDTO.Pagination;

                var skillsets = _oMTDataContext.SkillSet.Where(x => x.SystemofRecordId == sorCompletionCountInputDTO.SystemOfRecordId && x.IsActive).ToList();
                var counts = _oMTDataContext.Daily_Status_Count.Where(x => x.SystemofRecordId == sorCompletionCountInputDTO.SystemOfRecordId && x.Date >= sorCompletionCountInputDTO.FromDate && x.Date <= sorCompletionCountInputDTO.ToDate).ToList();
               
                List<SorCompletionCountDTO> sorCompletionCountDTOs = new List<SorCompletionCountDTO>();

                foreach (var skillset in skillsets)
                {
                    SorCompletionCountDTO sorCompletionCountDTO = new SorCompletionCountDTO()
                    {
                        SkillsetId = skillset.SkillSetId,
                        SkillsetName = skillset.SkillSetName,
                        Count = counts.Where(x => x.SkillSetId == skillset.SkillSetId).Sum(x => x.Count),
                    };

                    sorCompletionCountDTOs.Add(sorCompletionCountDTO);

                }

                if (sorCompletionCountDTOs.Count > 0)
                {
                    if (pagination.IsPagination)
                    {
                        var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                        var paginatedData = sorCompletionCountDTOs.Skip(skip).Take(pagination.NoOfRecords).ToList();
                        var totalRecords = sorCompletionCountDTOs.Count;
                        var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.NoOfRecords);

                        var paginationOutput = new PaginationOutputDTO
                        {
                            Records = paginatedData.Cast<object>().ToList(),
                            PageNo = pagination.PageNo,
                            NoOfPages = totalPages,
                            TotalCount = totalRecords,

                        };

                        resultDTO.Data = paginationOutput;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Sor completion count fetched successfully";
                    }
                    else
                    {

                        resultDTO.IsSuccess = true;
                        resultDTO.Data = sorCompletionCountDTOs;
                        resultDTO.Message = "Sor completion count fetched successfully";
                    }
                   
                }
                else
                {
                    resultDTO.Message = "Sor completion count not found.";
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
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
