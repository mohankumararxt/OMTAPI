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
                                                         UserSkillSetId = x.UserSkillSetId,
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
                    //var PriorityOrder = 1;

                    //int highestCycle1PriorityOrder = 0;
                    //var skillSetPriorityMap_c1 = new Dictionary<int, int>();
                    //var skillSetPriorityMap_c2 = new Dictionary<int, int>();

                    var userskillsets = (from up in _oMTDataContext.UserProfile
                                         join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                         join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                         where up.UserId == user && up.IsActive == true && uss.IsActive == true && ss.IsActive == true
                                         select new
                                         {
                                             UserSkillSet = uss,
                                             TotalPercentage = _oMTDataContext.UserSkillSet.Where(x => x.SkillSetId == uss.SkillSetId && x.UserId == user).Sum(x => x.Percentage)
                                         })
                                         .OrderByDescending(x => x.UserSkillSet.IsCycle1)
                                         .OrderBy(x => x.UserSkillSet.PriorityOrder)
                                         .OrderByDescending(x => x.UserSkillSet.SkillSetId)
                                         .ThenByDescending(x => x.UserSkillSet.IsHardStateUser)
                                         .ThenByDescending(x => x.UserSkillSet.Percentage)
                                         .Select(x => new UserSkillSet
                                         {
                                             UserId = x.UserSkillSet.UserId,
                                             UserSkillSetId = x.UserSkillSet.UserSkillSetId,
                                             SkillSetId = x.UserSkillSet.SkillSetId,
                                             Percentage = x.UserSkillSet.Percentage,
                                             IsActive = x.UserSkillSet.IsActive,
                                             IsHardStateUser = x.UserSkillSet.IsHardStateUser,
                                             IsCycle1 = x.UserSkillSet.IsCycle1,
                                             HardStateName = x.UserSkillSet.HardStateName,
                                             PriorityOrder = x.UserSkillSet.PriorityOrder,

                                         })
                                         .ToList();

                    foreach (var userSkillset in userskillsets)
                    {
                        //int currentPriorityOrder;

                        //if (userSkillset.IsCycle1)
                        //{
                        //    // Handle IsCycle1 = true entries
                        //    if (!skillSetPriorityMap_c1.TryGetValue(userSkillset.SkillSetId, out currentPriorityOrder))
                        //    {
                        //        // Assign the next available PriorityOrder for this SkillSetId
                        //        currentPriorityOrder = PriorityOrder++;
                        //        skillSetPriorityMap_c1[userSkillset.SkillSetId] = currentPriorityOrder;

                        //    }
                        //    // Update highestCycle1PriorityOrder if this is the highest priority seen in Cycle1
                        //    highestCycle1PriorityOrder = Math.Max(highestCycle1PriorityOrder, currentPriorityOrder);
                        //}
                        //else
                        //{
                        //    // For Cycle2 entries, start from highestCycle1PriorityOrder + 1
                        //    if (!skillSetPriorityMap_c2.TryGetValue(userSkillset.SkillSetId, out currentPriorityOrder))
                        //    {
                        //        currentPriorityOrder = highestCycle1PriorityOrder + 1;
                        //        skillSetPriorityMap_c2[userSkillset.SkillSetId] = currentPriorityOrder;
                        //    }
                        //}

                        var skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == userSkillset.SkillSetId && x.IsActive).FirstOrDefault();

                        double totalorders = ((double)userSkillset.Percentage / 100) * skillset.Threshold;

                        int roundedtotalorders = totalorders == 0 ? 0 : (totalorders > 0 && totalorders < 1) ? 1 : (int)Math.Round(totalorders, MidpointRounding.AwayFromZero);

                        GetOrderCalculation getOrderCalculation = new GetOrderCalculation
                        {
                            UserId = userSkillset.UserId,
                            UserSkillSetId = userSkillset.UserSkillSetId,
                            SkillSetId = userSkillset.SkillSetId,
                            TotalOrderstoComplete = userSkillset.IsCycle1 == true ? roundedtotalorders : 0,
                            OrdersCompleted = 0,
                            Weightage = userSkillset.Percentage,
                            PriorityOrder = userSkillset.PriorityOrder,
                            IsActive = true,
                            UpdatedDate = DateTime.Now,
                            IsCycle1 = userSkillset.IsCycle1,
                            IsHardStateUser = userSkillset.IsHardStateUser,
                            Utilized = userSkillset.IsCycle1 == false ? false : roundedtotalorders == 0 ? true : false,
                            HardStateUtilized = false,
                        };

                        _oMTDataContext.GetOrderCalculation.Add(getOrderCalculation);
                        _oMTDataContext.SaveChanges();

                    }

                }

                // update priorityorder in goc table based on priority orders in skillset tables

                // Update_by_priorityOrder(resultDTO, connection, userid);

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
