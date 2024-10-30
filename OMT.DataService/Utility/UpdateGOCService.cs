using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Utility
{
    public class UpdateGOCService : IUpdateGOCService
    {
        private readonly OMTDataContext _oMTDataContext;
        public UpdateGOCService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO UpdateGetOrderCalculation()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                //check getorder table for any records , if yes put it into backup table, truncate getorder table and call the below method 
                var tablename = "GetOrderCalculation";

                var existingdetails = _oMTDataContext.GetOrderCalculation
                                                     .Select(x => new Utilization
                                                     {
                                                         UserId = x.UserId,
                                                         UserSkillSetId = x.SkillSetId,
                                                         SkillSetId = x.SkillSetId,
                                                         TotalOrderstoComplete = x.TotalOrderstoComplete,
                                                         OrdersCompleted = x.OrdersCompleted,
                                                         Weightage = x.Weightage,
                                                         PriorityOrder = x.PriorityOrder,
                                                         Utilized = x.Utilized,
                                                         IsActive = x.IsActive,
                                                         UpdatedDate = x.UpdatedDate,
                                                         IsCycle1 = x.IsCycle1,
                                                         IsHardStateUser = x.IsHardStateUser,
                                                         HardStateUtilized = x.HardStateUtilized,
                                                     }).ToList();

                if (existingdetails.Count > 0)
                {
                    _oMTDataContext.Utilization.AddRange(existingdetails);
                    _oMTDataContext.SaveChanges();

                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                    using SqlConnection connection = new(connectionstring);
                    using SqlCommand truncatecmd = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = "Truncate table " + tablename,
                    };
                    connection.Open();
                    truncatecmd.ExecuteNonQuery();

                    InsertGetOrderCalculation(resultDTO, 0);
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

        public void InsertGetOrderCalculation(ResultDTO resultDTO, int userid)
        {
            string? connectionstring = _oMTDataContext.Database.GetConnectionString();
            using SqlConnection connection = new(connectionstring);
            connection.Open();

            try
            {
                List<int> users = new List<int>();

                if (userid == 0)
                {
                    users = _oMTDataContext.UserProfile.Where(x => x.IsActive).Select(_ => _.UserId).ToList();
                }
                else
                {
                    users.Add(userid);
                }

                foreach (var user in users)
                {
                    var PriorityOrder = 1;

                    var userskillsets = (from up in _oMTDataContext.UserProfile
                                         join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                         join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                         where up.UserId == user && up.IsActive == true && uss.IsActive == true && ss.IsActive == true
                                         orderby uss.IsCycle1 descending, uss.Percentage descending
                                         select new UserSkillSet
                                         {
                                             UserId = uss.UserId,
                                             UserSkillSetId = uss.UserSkillSetId,
                                             SkillSetId = uss.SkillSetId,
                                             Percentage = uss.Percentage,
                                             IsActive = uss.IsActive,
                                             IsHardStateUser = uss.IsHardStateUser,
                                             IsCycle1 = uss.IsCycle1,
                                             HardStateName = uss.HardStateName,
                                         }).ToList();


                    foreach (var userkillset in userskillsets)
                    {
                        var skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == userkillset.SkillSetId && x.IsActive).FirstOrDefault();

                        double totalorders = ((double)userkillset.Percentage / 100) * skillset.Threshold;

                        int roundedtotalorders = (int)Math.Round(totalorders);

                        GetOrderCalculation getOrderCalculation = new GetOrderCalculation
                        {
                            UserId = userkillset.UserId,
                            UserSkillSetId = userkillset.UserSkillSetId,
                            SkillSetId = userkillset.SkillSetId,
                            TotalOrderstoComplete = roundedtotalorders,
                            OrdersCompleted = 0,
                            Weightage = userkillset.Percentage,
                            PriorityOrder = PriorityOrder++,
                            IsActive = true,
                            UpdatedDate = DateTime.Now,
                            IsCycle1 = userkillset.IsCycle1,
                            IsHardStateUser = userkillset.IsHardStateUser,
                            Utilized = false,
                            HardStateUtilized = false,
                        };

                        _oMTDataContext.GetOrderCalculation.Add(getOrderCalculation);
                        _oMTDataContext.SaveChanges();

                    }

                }

                // update priorityorder in goc table based on priority orders in skillset tables

                Update_by_priorityOrder(resultDTO, connection, userid);

                resultDTO.IsSuccess = true;
                resultDTO.Message = "GetOrderCalculation table updated successfully.";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
        }

        public void Update_by_priorityOrder(ResultDTO resultDTO, SqlConnection connection, int userid)
        {
            try
            {
                List<SkillSet> skillsets = new List<SkillSet>();

                if (userid == 0)
                {
                    skillsets = (from ss in _oMTDataContext.SkillSet
                                 join goc in _oMTDataContext.GetOrderCalculation on ss.SkillSetId equals goc.SkillSetId
                                 where ss.IsActive && goc.IsActive && goc.IsCycle1 && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                 select new SkillSet
                                 {
                                     SkillSetId = ss.SkillSetId,
                                     SkillSetName = ss.SkillSetName,
                                     SystemofRecordId = ss.SystemofRecordId
                                 }).Distinct().ToList();
                }
                else
                {
                    skillsets = (from ss in _oMTDataContext.SkillSet
                                 join goc in _oMTDataContext.GetOrderCalculation on ss.SkillSetId equals goc.SkillSetId
                                 where ss.IsActive && goc.IsActive && goc.IsCycle1 && goc.UserId == userid && _oMTDataContext.TemplateColumns.Any(temp => temp.SkillSetId == ss.SkillSetId)
                                 select new SkillSet
                                 {
                                     SkillSetId = ss.SkillSetId,
                                     SkillSetName = ss.SkillSetName,
                                     SystemofRecordId = ss.SystemofRecordId
                                 }).Distinct().ToList();
                }

                foreach (var ss in skillsets)
                {
                    string tableName = ss.SkillSetName;

                    string query = $@"
                                    SELECT COUNT(*)
                                    FROM {tableName} 
                                    WHERE IsPriority = 1 AND UserId IS NULL AND Status IS NULL";

                    using SqlCommand command = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = query
                    };

                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        using SqlCommand priority = new()
                        {
                            Connection = connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "UpdateGoc_PriorityOrder"
                        };
                        priority.Parameters.AddWithValue("@SkillSetId", ss.SkillSetId);
                        priority.Parameters.AddWithValue("@SystemOfRecordId", ss.SystemofRecordId);
                        priority.Parameters.AddWithValue("@UserId", userid);

                        SqlParameter priority_returnValue = new()
                        {
                            ParameterName = "@RETURN_VALUE_Po",
                            Direction = ParameterDirection.ReturnValue
                        };
                        priority.Parameters.Add(priority_returnValue);

                        priority.ExecuteNonQuery();

                        int priority_returnCode = (int)priority.Parameters["@RETURN_VALUE_Po"].Value;

                        if (priority_returnCode != 1)
                        {
                            throw new InvalidOperationException("Something went wrong while updating GetOrderCalculation table.");
                        }
                    }

                }
                resultDTO.IsSuccess = true;
                resultDTO.Message = "GetOrderCalculation table updated successfully.";
            }
            catch (Exception ex)
            {

                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
        }
    }
}
