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
                List<int> SOR = _oMTDataContext.SystemofRecord.Where(x => x.IsActive).Select(x => x.SystemofRecordId).ToList();

                List<SorCountDTO> ordercounts = new List<SorCountDTO>();

                foreach (int i in SOR)
                {
                    // capture count of uploaded orders 

                    DateTime today_date = DateTime.Now.Date;
                    DateTime uploading_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                    Daywise_Cutoff_Timing cutoff_time = _oMTDataContext.Daywise_Cutoff_Timing.Where(x => x.SystemOfRecordId == i && x.IsActive).FirstOrDefault();

                    DateTime yesterday_dt = uploading_time.Date.AddDays(-1);
                    DateTime cutoff_start_dt = yesterday_dt + cutoff_time.StartTime;
                    DateTime cutoff_end_dt = uploading_time.Date + cutoff_time.EndTime;


                    if (uploading_time >= cutoff_start_dt && uploading_time <= cutoff_end_dt)
                    {
                        today_date = uploading_time.Date.AddDays(-1);
                    }

                    else if (uploading_time > cutoff_end_dt)
                    {
                        today_date = uploading_time.Date;
                    }

                    DateTime yeserday_date = today_date.AddDays(-1);

                    var count = _oMTDataContext.DailyCount_SOR.Where(x => x.Date == today_date && x.SystemofRecordId == i).Select(x => x.Count).FirstOrDefault();
                    var diff = (count - _oMTDataContext.DailyCount_SOR.Where(x => x.Date == yeserday_date && x.SystemofRecordId == i).Select(x => x.Count).FirstOrDefault()) / 100;
                    var sorname = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == i && x.IsActive).Select(x => x.SystemofRecordName).FirstOrDefault();

                    SorCountDTO sorCount = new SorCountDTO()
                    {
                        SorId = i,
                        SorName = sorname,
                        Count = count,
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

                DateTime today_date = DateTime.Now.Date;
                DateTime uploading_time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                Daywise_Cutoff_Timing cutoff_time = _oMTDataContext.Daywise_Cutoff_Timing.Where(x => x.SystemOfRecordId == volumeProjectionInputDTO.SystemOfRecordId && x.IsActive).FirstOrDefault();

                DateTime yesterday_dt = uploading_time.Date.AddDays(-1);
                DateTime cutoff_start_dt = yesterday_dt + cutoff_time.StartTime;
                DateTime cutoff_end_dt = uploading_time.Date + cutoff_time.EndTime;
                DateTime tomorrow_dt = uploading_time.Date.AddDays(1) + cutoff_time.EndTime;

                DateTime cutoff_start_dt_utc = DateTime.UtcNow;
                DateTime cutoff_end_dt_utc = DateTime.UtcNow;

                if (uploading_time >= cutoff_start_dt && uploading_time <= cutoff_end_dt)
                {
                    today_date = uploading_time.Date.AddDays(-1);
                    cutoff_start_dt_utc = TimeZoneInfo.ConvertTimeToUtc(cutoff_start_dt, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    cutoff_end_dt_utc = TimeZoneInfo.ConvertTimeToUtc(cutoff_end_dt, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                }

                else if (uploading_time > cutoff_end_dt)
                {
                    today_date = uploading_time.Date;
                    cutoff_start_dt_utc = TimeZoneInfo.ConvertTimeToUtc(cutoff_end_dt, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    cutoff_end_dt_utc = TimeZoneInfo.ConvertTimeToUtc(tomorrow_dt, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                }

                string skillsetnames = (from ss in _oMTDataContext.SkillSet
                                        where ss.SkillSetId == volumeProjectionInputDTO.SkillsetId && ss.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                        select ss.SkillSetName).FirstOrDefault();

                if (skillsetnames == null)
                {
                    VolumeProjectionOutputDTO volumeProjectionOutputDTO = new VolumeProjectionOutputDTO()
                    {
                        Received = 0,
                        Completed = 0,
                        Not_Assigned = 0,
                    };

                    resultDTO.Data = volumeProjectionOutputDTO;
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Template does not exist for the skillset.";
                }
                else
                {
                    SkillSet skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == volumeProjectionInputDTO.SkillsetId && x.IsActive).FirstOrDefault();

                    var received = _oMTDataContext.DailyCount_SkillSet.Where(x => x.SystemofRecordId == volumeProjectionInputDTO.SystemOfRecordId && x.SkillSetId == volumeProjectionInputDTO.SkillsetId && x.Date == today_date).Select(x => x.Count).FirstOrDefault();

                    string sql = $"SELECT COUNT(*) FROM {skillSet.SkillSetName} WHERE Userid IS NULL AND Status IS NULL";

                    var not_Assigned = 0;

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        not_Assigned = (int)cmd.ExecuteScalar();
                    }

                    //get completed count

                    string sql2 = $"SELECT COUNT(*) FROM {skillSet.SkillSetName} WHERE Status IS NOT NULL AND UserId IS NOT NULL AND CompletionDate BETWEEN @FromDate AND @ToDate";

                    var completed = 0;

                    using (SqlCommand command = new SqlCommand(sql2, connection))
                    {
                        command.Parameters.AddWithValue("@FromDate", cutoff_start_dt_utc);
                        command.Parameters.AddWithValue("@ToDate", cutoff_end_dt_utc);

                        completed = (int)command.ExecuteScalar();
                    }

                    VolumeProjectionOutputDTO volumeProjectionOutputDTO = new VolumeProjectionOutputDTO()
                    {
                        Received = received,
                        Completed = completed,
                        Not_Assigned = not_Assigned,
                    };

                    resultDTO.Data = volumeProjectionOutputDTO;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Volume Projection details fetched successfully.";
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

        public ResultDTO GetWeeklyCompletion(WeeklyCompletionDTO weeklyCompletionDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var statusid = _oMTDataContext.Skillset_Status.Where(x => x.SystemofRecordId == weeklyCompletionDTO.SystemOfRecordId && x.IsActive).Select(x => x.StatusId).ToList();

                var counts = _oMTDataContext.Daily_Status_Count.Where(x => x.SystemofRecordId == weeklyCompletionDTO.SystemOfRecordId && x.SkillSetId == weeklyCompletionDTO.SkillsetId && x.Date >= weeklyCompletionDTO.FromDate && x.Date <= weeklyCompletionDTO.ToDate).ToList();

                var dates = counts.Select(x => x.Date).Distinct().OrderBy(x => x.Date).ToList();

                var uploaded = _oMTDataContext.DailyCount_SkillSet.Where(x => x.SkillSetId == weeklyCompletionDTO.SkillsetId && x.Date >= weeklyCompletionDTO.FromDate && x.Date <= weeklyCompletionDTO.ToDate).ToList();

                var weeklyCount = new List<WeeklyCompletionResponseDTO>();

                foreach (var date in dates)
                {
                    var statusCount = new Dictionary<string, int>();

                    var assigned = uploaded.Where(x => x.Date == date).Sum(x => x.Count);

                    foreach (var sid in statusid)
                    {
                        var statusname = _oMTDataContext.ProcessStatus.Where(x => x.Id == sid).Select(x => x.Status).FirstOrDefault();

                        var cnt = counts.Where(x => x.Status == sid && x.Date == date).Sum(x => x.Count);

                        statusCount[statusname] = cnt;

                    }

                    statusCount["Assigned"] = assigned;

                    weeklyCount.Add(new WeeklyCompletionResponseDTO
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        StatusCount = statusCount
                    });
                }

                if (weeklyCount.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = weeklyCount;
                    resultDTO.Message = "Weekly completion count fetched successfully";
                }
                else
                {
                    resultDTO.Message = "Weekly completion count not found.";
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

        public ResultDTO GetMonthlyVolumeTrend(MonthlyVolumeTrendDTO monthlyVolumeTrendDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                DateTime today = DateTime.Today;

                DateTime startDate = today.AddMonths(-12).AddDays(1);
                DateTime endDate = today;

                Console.WriteLine($"Start Date: {startDate:yyyy-MM-dd}");
                Console.WriteLine($"End Date: {endDate:yyyy-MM-dd}");

                List<(int Year, int Month)> yearMonthList = new List<(int, int)>();

                DateTime iterDate = new DateTime(startDate.Year, startDate.Month, 1);
                DateTime endMonth = new DateTime(endDate.Year, endDate.Month, 1);

                while (iterDate <= endMonth)
                {
                    yearMonthList.Add((iterDate.Year, iterDate.Month));
                    iterDate = iterDate.AddMonths(1);
                }

                MonthlyVolumeTrendResponseDTO monthlyVolumeTrendResponseDTO = new MonthlyVolumeTrendResponseDTO();
                MonthlyVolumeTrendSkillsetResponseDTO monthlyVolumeTrendSkillsetResponseDTO = new MonthlyVolumeTrendSkillsetResponseDTO();
                List<MonthlyVolumeTrendResponseDTO> monthvt = new List<MonthlyVolumeTrendResponseDTO>();
                List<MonthlyVolumeTrendSkillsetResponseDTO> month_Skillset = new List<MonthlyVolumeTrendSkillsetResponseDTO>();
                MonthlyVolumeTrendSkillsetStatusResponseDTO monthlyVolumeTrendSkillsetStatusResponseDTO = new MonthlyVolumeTrendSkillsetStatusResponseDTO();
                List<MonthlyVolumeTrendSkillsetStatusResponseDTO> month_skillset_status = new List<MonthlyVolumeTrendSkillsetStatusResponseDTO>();

                List<int> SOR = _oMTDataContext.SystemofRecord.Where(x => x.IsActive).Select(x => x.SystemofRecordId).ToList();

                if (monthlyVolumeTrendDTO.SystemOfRecordId == null && monthlyVolumeTrendDTO.SkillsetId == null && monthlyVolumeTrendDTO.StatusId == null)
                {
                    foreach (int sorid in SOR)
                    {
                        var monthly_count = _oMTDataContext.MonthlyCount_SOR
                                                                       .Where(x => x.SystemofRecordId == sorid)
                                                                       .AsEnumerable()
                                                                       .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                                                                       .ToList();


                        var monthlyCounts = monthly_count
                            .GroupBy(x => new { x.Year, x.Month })
                            .Select(g => new
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Count = g.Sum(x => x.Count)
                            })
                            .ToList();

                        monthlyVolumeTrendResponseDTO = new MonthlyVolumeTrendResponseDTO()
                        {
                            SystemOfRecordId = sorid,
                            SystemOfRecordName = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == sorid).Select(x => x.SystemofRecordName).FirstOrDefault(),
                            MonthlyCount = monthlyCounts
                                                                         .Select(m => new MonthCountDTO
                                                                         {
                                                                             Year = m.Year,
                                                                             Month = m.Month,
                                                                             Count = m.Count
                                                                         }).ToList()

                        };

                        monthvt.Add(monthlyVolumeTrendResponseDTO);
                    }

                    if (monthvt.Count > 0)
                    {
                        resultDTO.Data = monthvt;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Monthly volume trend fetched successfully";
                    }

                    else
                    {
                        resultDTO.Message = "Monthly volume trend count not found.";
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
                    }
                }

                else if (monthlyVolumeTrendDTO.SystemOfRecordId.Count > 0 && monthlyVolumeTrendDTO.SkillsetId == null && monthlyVolumeTrendDTO.StatusId == null)
                {
                    //var monthly_count = _oMTDataContext.MonthlyCount_SOR
                    //                                                    .Where(x => x.SystemofRecordId == monthlyVolumeTrendDTO.SystemOfRecordId)
                    //                                                    .AsEnumerable()
                    //                                                    .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                    //                                                    .ToList();


                    //var monthlyCounts = monthly_count
                    //    .GroupBy(x => new { x.Year, x.Month })
                    //    .Select(g => new
                    //    {
                    //        Year = g.Key.Year,
                    //        Month = g.Key.Month,
                    //        Count = g.Sum(x => x.Count)
                    //    })
                    //    .ToList();

                    //monthlyVolumeTrendResponseDTO = new MonthlyVolumeTrendResponseDTO()
                    //{
                    //    SystemOfRecordId = monthlyVolumeTrendDTO.SystemOfRecordId ?? 0,
                    //    SystemOfRecordName = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == monthlyVolumeTrendDTO.SystemOfRecordId).Select(x => x.SystemofRecordName).FirstOrDefault(),
                    //    MonthlyCount = monthlyCounts
                    //                                                 .Select(m => new MonthCountDTO
                    //                                                 {
                    //                                                     Year = m.Year,
                    //                                                     Month = m.Month,
                    //                                                     Count = m.Count
                    //                                                 }).ToList()

                    //};

                    //if (monthlyVolumeTrendResponseDTO != null)
                    //{
                    //    resultDTO.Data = monthlyVolumeTrendResponseDTO;
                    //    resultDTO.IsSuccess = true;
                    //    resultDTO.Message = "Monthly volume trend fetched successfully";
                    //}

                    //else
                    //{
                    //    resultDTO.Message = "Monthly volume trend count not found.";
                    //    resultDTO.StatusCode = "404";
                    //    resultDTO.IsSuccess = false;
                    //}

                    foreach (int sorid in monthlyVolumeTrendDTO.SystemOfRecordId)
                    {
                        var monthly_count = _oMTDataContext.MonthlyCount_SOR
                                                                       .Where(x => x.SystemofRecordId == sorid)
                                                                       .AsEnumerable()
                                                                       .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                                                                       .ToList();


                        var monthlyCounts = monthly_count
                            .GroupBy(x => new { x.Year, x.Month })
                            .Select(g => new
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Count = g.Sum(x => x.Count)
                            })
                            .ToList();

                        monthlyVolumeTrendResponseDTO = new MonthlyVolumeTrendResponseDTO()
                        {
                            SystemOfRecordId = sorid,
                            SystemOfRecordName = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == sorid).Select(x => x.SystemofRecordName).FirstOrDefault(),
                            MonthlyCount = monthlyCounts
                                                                         .Select(m => new MonthCountDTO
                                                                         {
                                                                             Year = m.Year,
                                                                             Month = m.Month,
                                                                             Count = m.Count
                                                                         }).ToList()

                        };

                        monthvt.Add(monthlyVolumeTrendResponseDTO);
                    }

                    if (monthvt.Count > 0)
                    {
                        resultDTO.Data = monthvt;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Monthly volume trend fetched successfully";
                    }

                    else
                    {
                        resultDTO.Message = "Monthly volume trend count not found.";
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
                    }
                }

                else if (monthlyVolumeTrendDTO.SystemOfRecordId != null && monthlyVolumeTrendDTO.SkillsetId != null && monthlyVolumeTrendDTO.StatusId == null)
                {
                    var skillsetnames = (from ss in _oMTDataContext.SkillSet
                                         where ss.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                         && monthlyVolumeTrendDTO.SkillsetId.Contains(ss.SkillSetId)
                                         select new
                                         {
                                             ss.SkillSetId,
                                             ss.SkillSetName
                                         }
                                        ).ToList();

                    List<int> skillsetids = skillsetnames.Select(x => x.SkillSetId).ToList();
                    var notavailable_skillsets = (from ss in _oMTDataContext.SkillSet
                                                  where ss.IsActive && !_oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                  && monthlyVolumeTrendDTO.SkillsetId.Contains(ss.SkillSetId)
                                                  select ss.SkillSetName).ToList();

                    if (skillsetnames.Count == 0)
                    {
                        monthlyVolumeTrendSkillsetResponseDTO = new MonthlyVolumeTrendSkillsetResponseDTO()
                        {
                            SkillsetId = 0,
                            SkillsetName = null,
                            MonthlyCount = new List<int>(),

                        };

                        resultDTO.Data = monthlyVolumeTrendSkillsetResponseDTO;
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Templates does not exist for the skillsets.";
                    }
                    else
                    {
                        foreach (int sorid in monthlyVolumeTrendDTO.SystemOfRecordId)
                        {
                            foreach (int ssid in skillsetids)
                            {
                                var monthly_count = _oMTDataContext.MonthlyCount_SkillSet
                                                                                     .Where(x => x.SystemofRecordId == sorid && x.SkillSetId == ssid)
                                                                                     .AsEnumerable()
                                                                                     .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                                                                                     .ToList();


                                var monthlyCounts = monthly_count
                                    .GroupBy(x => new { x.Year, x.Month })
                                    .Select(g => new
                                    {
                                        Year = g.Key.Year,
                                        Month = g.Key.Month,
                                        Count = g.Sum(x => x.Count)
                                    })
                                    .ToList();

                                monthlyVolumeTrendSkillsetResponseDTO = new MonthlyVolumeTrendSkillsetResponseDTO
                                {
                                    SkillsetId = ssid,
                                    SkillsetName = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == ssid && x.IsActive).Select(_ => _.SkillSetName).FirstOrDefault(),
                                    MonthlyCount = monthlyCounts
                                                                         .Select(m => new MonthCountDTO
                                                                         {
                                                                             Year = m.Year,
                                                                             Month = m.Month,
                                                                             Count = m.Count
                                                                         }).ToList()
                                };

                                month_Skillset.Add(monthlyVolumeTrendSkillsetResponseDTO);
                            }

                        }

                        if (month_Skillset.Count > 0 && notavailable_skillsets.Count == 0)
                        {
                            resultDTO.Data = month_Skillset;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Monthly volume trend fetched successfully";
                        }

                        else if (month_Skillset.Count > 0 && notavailable_skillsets.Count > 0)
                        {
                            resultDTO.Data = month_Skillset;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Monthly volume trend fetched successfully and templates does not exists for the skillsets : " + string.Join(",", notavailable_skillsets) + ".";
                        }
                        //else if (month_Skillset.Count == 0 && notavailable_skillsets.Count == 0)
                        //{
                        //    resultDTO.Message = "Monthly volume trend count not found.";
                        //    resultDTO.StatusCode = "404";
                        //    resultDTO.IsSuccess = false;
                        //}
                    }

                    //string skillsetnames = (from ss in _oMTDataContext.SkillSet
                    //                        where ss.SkillSetId == monthlyVolumeTrendDTO.SkillsetId && ss.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                    //                        select ss.SkillSetName).FirstOrDefault();

                    //if (skillsetnames == null)
                    //{
                    //    monthlyVolumeTrendResponseDTO = new MonthlyVolumeTrendResponseDTO()
                    //    {
                    //        SystemOfRecordId = monthlyVolumeTrendDTO.SystemOfRecordId ?? 0,
                    //        SystemOfRecordName = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == monthlyVolumeTrendDTO.SystemOfRecordId).Select(x => x.SystemofRecordName).FirstOrDefault(),
                    //        MonthlyCount = new List<int>()

                    //    };

                    //    resultDTO.Data = monthlyVolumeTrendResponseDTO;
                    //    resultDTO.StatusCode = "404";
                    //    resultDTO.IsSuccess = false;
                    //    resultDTO.Message = "Template does not exist for the skillset.";
                    //}
                    //else
                    //{
                    //    var monthly_count = _oMTDataContext.MonthlyCount_SkillSet
                    //                                                             .Where(x => x.SystemofRecordId == monthlyVolumeTrendDTO.SystemOfRecordId && x.SkillSetId == monthlyVolumeTrendDTO.SkillsetId)
                    //                                                             .AsEnumerable()
                    //                                                             .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                    //                                                             .ToList();


                    //    var monthlyCounts = monthly_count
                    //        .GroupBy(x => new { x.Year, x.Month })
                    //        .Select(g => new
                    //        {
                    //            Year = g.Key.Year,
                    //            Month = g.Key.Month,
                    //            Count = g.Sum(x => x.Count)
                    //        })
                    //        .ToList();

                    //    monthlyVolumeTrendResponseDTO = new MonthlyVolumeTrendResponseDTO()
                    //    {
                    //        SystemOfRecordId = monthlyVolumeTrendDTO.SystemOfRecordId ?? 0,
                    //        SystemOfRecordName = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == monthlyVolumeTrendDTO.SystemOfRecordId).Select(x => x.SystemofRecordName).FirstOrDefault(),
                    //        MonthlyCount = monthlyCounts
                    //                                                     .Select(m => new MonthCountDTO
                    //                                                     {
                    //                                                         Year = m.Year,
                    //                                                         Month = m.Month,
                    //                                                         Count = m.Count
                    //                                                     }).ToList()

                    //    };

                    //    if (((IEnumerable<object>)monthlyVolumeTrendResponseDTO.MonthlyCount).Any())
                    //    {
                    //        resultDTO.Data = monthlyVolumeTrendResponseDTO;
                    //        resultDTO.IsSuccess = true;
                    //        resultDTO.Message = "Monthly volume trend fetched successfully";
                    //    }

                    //    else
                    //    {
                    //        resultDTO.Data = monthlyVolumeTrendResponseDTO;
                    //        resultDTO.Message = "Monthly volume trend count not found.";
                    //        resultDTO.StatusCode = "404";
                    //        resultDTO.IsSuccess = false;
                    //    }
                    //}




                }

                else if (monthlyVolumeTrendDTO.SystemOfRecordId != null && monthlyVolumeTrendDTO.SkillsetId != null && monthlyVolumeTrendDTO.StatusId != null)
                {
                    var skillsetnames = (from ss in _oMTDataContext.SkillSet
                                         where ss.IsActive && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                         && monthlyVolumeTrendDTO.SkillsetId.Contains(ss.SkillSetId)
                                         select new
                                         {
                                             ss.SkillSetId,
                                             ss.SkillSetName
                                         }
                                        ).ToList();

                    List<int> skillsetids = skillsetnames.Select(x => x.SkillSetId).ToList();
                    var notavailable_skillsets = (from ss in _oMTDataContext.SkillSet
                                                  where ss.IsActive && !_oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                                  && monthlyVolumeTrendDTO.SkillsetId.Contains(ss.SkillSetId)
                                                  select ss.SkillSetName).ToList();

                    if (skillsetnames.Count == 0)
                    {
                        monthlyVolumeTrendSkillsetResponseDTO = new MonthlyVolumeTrendSkillsetResponseDTO()
                        {
                            SkillsetId = 0,
                            SkillsetName = null,
                            MonthlyCount = new List<int>(),

                        };

                        resultDTO.Data = monthlyVolumeTrendSkillsetResponseDTO;
                        resultDTO.IsSuccess = false;
                        resultDTO.StatusCode = "404";
                        resultDTO.Message = "Templates does not exist for the skillsets.";
                    }
                    else
                    {
                        foreach (int sorid in monthlyVolumeTrendDTO.SystemOfRecordId)
                        {
                            foreach (int ssid in skillsetids)
                            {
                                var monthly_count = _oMTDataContext.MonthlyCount_SkillSet
                                                                                     .Where(x => x.SystemofRecordId == sorid && x.SkillSetId == ssid)
                                                                                     .AsEnumerable()
                                                                                     .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                                                                                     .ToList();


                                var monthlyCounts = monthly_count
                                    .GroupBy(x => new { x.Year, x.Month })
                                    .Select(g => new
                                    {
                                        Year = g.Key.Year,
                                        Month = g.Key.Month,
                                        Count = g.Sum(x => x.Count)
                                    })
                                    .ToList();

                                var statusSource = _oMTDataContext.Monthly_Status_Count_SkillSet
                                    .Where(ms => ms.SystemofRecordId == sorid && ms.SkillSetId == ssid &&
                                     monthlyVolumeTrendDTO.StatusId.Contains(ms.Status))
                                    .Join(_oMTDataContext.ProcessStatus,
                                            ms => ms.Status,
                                            ps => ps.Id,
                                            (ms, ps) => new
                                            {
                                                StatusId = ms.Status,
                                                StatusName = ps.Status,
                                                Year = ms.Year,
                                                Month = ms.Month,
                                                Count = ms.Count
                                            })
                                    .AsEnumerable()
                                     .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                                    .ToList();

                                var statusWiseCounts = statusSource
                                                                    .GroupBy(x => new { x.StatusId, x.StatusName })
                                                                    .Select(g => new
                                                                    {
                                                                        StatusId = g.Key.StatusId,
                                                                        StatusName = g.Key.StatusName,
                                                                        MonthlyCount = g.Select(item => new
                                                                        {
                                                                            Year = item.Year,
                                                                            Month = item.Month,
                                                                            Count = item.Count
                                                                        }).ToList()
                                                                    })
                                                                    .ToList();


                                monthlyVolumeTrendSkillsetStatusResponseDTO = new MonthlyVolumeTrendSkillsetStatusResponseDTO
                                {
                                    SkillsetId = ssid,
                                    SkillsetName = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == ssid && x.IsActive).Select(_ => _.SkillSetName).FirstOrDefault(),
                                    MonthlyCount = monthlyCounts
                                                                         .Select(m => new MonthCountDTO
                                                                         {
                                                                             Year = m.Year,
                                                                             Month = m.Month,
                                                                             Count = m.Count
                                                                         }).ToList(),

                                    StatusWiseCounts = statusWiseCounts
                                };

                                month_skillset_status.Add(monthlyVolumeTrendSkillsetStatusResponseDTO);
                            }

                        }

                        if (month_skillset_status.Count > 0 && notavailable_skillsets.Count == 0)
                        {
                            resultDTO.Data = month_skillset_status;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Monthly volume trend VS status trend fetched successfully";
                        }

                        else if (month_skillset_status.Count > 0 && notavailable_skillsets.Count > 0)
                        {
                            resultDTO.Data = month_skillset_status;
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Monthly volume trend VS status trend fetched successfully and templates does not exists for the skillsets : " + string.Join(",", notavailable_skillsets) + ".";
                        }
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

        public ResultDTO GetMonthlyUtilization(MonthlyUtilizationSorDTO monthlyUtilizationSorDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                DateTime today = DateTime.Today;

                DateTime startDate = today.AddMonths(-12).AddDays(1);
                DateTime endDate = today;

                List<(int Year, int Month)> yearMonthList = new List<(int, int)>();

                DateTime iterDate = new DateTime(startDate.Year, startDate.Month, 1);
                DateTime endMonth = new DateTime(endDate.Year, endDate.Month, 1);

                while (iterDate <= endMonth)
                {
                    yearMonthList.Add((iterDate.Year, iterDate.Month));
                    iterDate = iterDate.AddMonths(1);
                }

                MonthlyUtilizationSorResponseDTO monthlyUtilizationSorResponseDTO = new MonthlyUtilizationSorResponseDTO();
                List<MonthlyUtilizationSorResponseDTO> monthly_util = new List<MonthlyUtilizationSorResponseDTO>();

                List<int> SOR = _oMTDataContext.SystemofRecord.Where(x => x.IsActive).Select(x => x.SystemofRecordId).ToList();

                if (monthlyUtilizationSorDTO.SystemOfRecordId == null)
                {
                    foreach (int sorid in SOR)
                    {
                        var monthly_count = _oMTDataContext.Monthly_Utilization_SOR
                                                                       .Where(x => x.SystemofRecordId == sorid)
                                                                       .AsEnumerable()
                                                                       .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                                                                       .ToList();


                        var monthlyUtilization = monthly_count
                            .GroupBy(x => new { x.Year, x.Month })
                            .Select(g => new
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Count = g.Sum(x => x.Utilization)
                            })
                            .ToList();

                        monthlyUtilizationSorResponseDTO = new MonthlyUtilizationSorResponseDTO()
                        {
                            SystemOfRecordId = sorid,
                            SystemOfRecordName = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == sorid).Select(x => x.SystemofRecordName).FirstOrDefault(),
                            Monthly_Utilization = monthlyUtilization
                                                                         .Select(m => new MonthCountDTO
                                                                         {
                                                                             Year = m.Year,
                                                                             Month = m.Month,
                                                                             Count = m.Count
                                                                         }).ToList()

                        };

                        monthly_util.Add(monthlyUtilizationSorResponseDTO);
                    }

                    if (monthly_util.Count > 0)
                    {
                        resultDTO.Data = monthly_util;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Monthly volume trend fetched successfully";
                    }

                    else
                    {
                        resultDTO.Message = "Monthly volume trend count not found.";
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
                    }
                }

                else if (monthlyUtilizationSorDTO.SystemOfRecordId.Count > 0)
                {
                    foreach (int sorid in monthlyUtilizationSorDTO.SystemOfRecordId)
                    {
                        var monthly_count = _oMTDataContext.Monthly_Utilization_SOR
                                                                       .Where(x => x.SystemofRecordId == sorid)
                                                                       .AsEnumerable()
                                                                       .Where(x => yearMonthList.Any(y => y.Year == x.Year && y.Month == x.Month))
                                                                       .ToList();


                        var monthly_utilization = monthly_count
                            .GroupBy(x => new { x.Year, x.Month })
                            .Select(g => new
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Count = g.Sum(x => x.Utilization)
                            })
                            .ToList();

                        monthlyUtilizationSorResponseDTO = new MonthlyUtilizationSorResponseDTO()
                        {
                            SystemOfRecordId = sorid,
                            SystemOfRecordName = _oMTDataContext.SystemofRecord.Where(x => x.SystemofRecordId == sorid).Select(x => x.SystemofRecordName).FirstOrDefault(),
                            Monthly_Utilization = monthly_utilization
                                                                         .Select(m => new MonthCountDTO
                                                                         {
                                                                             Year = m.Year,
                                                                             Month = m.Month,
                                                                             Count = m.Count
                                                                         }).ToList()

                        };

                        monthly_util.Add(monthlyUtilizationSorResponseDTO);
                    }

                    if (monthly_util.Count > 0)
                    {
                        resultDTO.Data = monthly_util;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Monthly volume trend fetched successfully";
                    }

                    else
                    {
                        resultDTO.Message = "Monthly volume trend count not found.";
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
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

        public ResultDTO GetCompletedVsReceived(WeeklyCompletionDTO weeklyCompletionDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var statusid = _oMTDataContext.Skillset_Status.Where(x => x.SystemofRecordId == weeklyCompletionDTO.SystemOfRecordId && x.IsActive).Select(x => x.StatusId).ToList();

                var counts = _oMTDataContext.Daily_Status_Count.Where(x => x.SystemofRecordId == weeklyCompletionDTO.SystemOfRecordId && x.SkillSetId == weeklyCompletionDTO.SkillsetId && x.Date >= weeklyCompletionDTO.FromDate && x.Date <= weeklyCompletionDTO.ToDate).ToList();

                var dates = Enumerable.Range(0, (weeklyCompletionDTO.ToDate.Date - weeklyCompletionDTO.FromDate.Date).Days + 1).Select(i => weeklyCompletionDTO.FromDate.Date.AddDays(i)).ToList();


                var uploaded = _oMTDataContext.DailyCount_SkillSet.Where(x => x.SkillSetId == weeklyCompletionDTO.SkillsetId && x.Date >= weeklyCompletionDTO.FromDate && x.Date <= weeklyCompletionDTO.ToDate).ToList();

                var systempending = _oMTDataContext.Daily_system_pending_Count.Where(x => x.SystemofRecordId == weeklyCompletionDTO.SystemOfRecordId && x.SkillSetId == weeklyCompletionDTO.SkillsetId && x.Date >= weeklyCompletionDTO.FromDate && x.Date <= weeklyCompletionDTO.ToDate).ToList();
                
                GetCompletedVsReceivedDTO getCompletedVsReceivedDTO = new GetCompletedVsReceivedDTO();

                var weeklyCount = new List<DayWiseCompletedDTO>();

                var DailyReceivedcount = new List<DayWiseReceivedDTO>();

                foreach (var date in dates)
                {
                    var statusCount = new Dictionary<string, int>();

                    var ReceivedCount = new Dictionary<string, int>();

                    var grandtotal = uploaded.Where(x => x.Date == date).Sum(x => x.Count);

                    foreach (var sid in statusid)
                    {
                        var statusname = _oMTDataContext.ProcessStatus.Where(x => x.Id == sid).Select(x => x.Status).FirstOrDefault();

                        var cnt = counts.Where(x => x.Status == sid && x.Date == date).Sum(x => x.Count);

                        statusCount[statusname] = cnt;

                    }

                    statusCount["System-Pending"] = systempending.Where(x => x.Date == date).Select(x => x.Count).FirstOrDefault();

                    weeklyCount.Add(new DayWiseCompletedDTO
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        StatusCount = statusCount
                    });


                    ReceivedCount["Grand-Total"] = grandtotal;
                    ReceivedCount["Pre-Day-Sys-Pend"] = systempending.Where(x => x.Date == date).Sum(x => x.Pre_day_count);

                    DailyReceivedcount.Add(new DayWiseReceivedDTO
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        ReceivedCount = ReceivedCount
                    });

                }

                getCompletedVsReceivedDTO = new GetCompletedVsReceivedDTO
                {
                    Completed = weeklyCount,
                    Received = DailyReceivedcount
                };

                if (weeklyCount.Count > 0 || DailyReceivedcount.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Data = getCompletedVsReceivedDTO;
                    resultDTO.Message = "Completion count vs received count fetched successfully";
                }
                else
                {
                    resultDTO.Message = "Completion count vs received count not found.";
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
