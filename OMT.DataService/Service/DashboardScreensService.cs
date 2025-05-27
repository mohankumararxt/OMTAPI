using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                DateTime today_date = DateTime.Now.Date;
                DateTime yeserday_date = today_date.AddDays(-1);

                List<int> SOR = new List<int>()
                {
                    1,2,3,4
                };

                List <SorCountDTO> ordercounts = new List<SorCountDTO>();

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

                SkillSet skillSet = _oMTDataContext.SkillSet.Where(x =>x.SkillSetId == volumeProjectionInputDTO.SkillsetId && x.IsActive).FirstOrDefault();

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
    }
}
