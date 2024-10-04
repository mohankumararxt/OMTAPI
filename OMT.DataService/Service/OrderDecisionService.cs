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

namespace OMT.DataService.Service
{
    public class OrderDecisionService : IOrderDecisionService
    {
        private readonly OMTDataContext _oMTDataContext;

        public OrderDecisionService(OMTDataContext oMTDataContext)
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

                if (existingdetails != null && existingdetails.Any())
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
                                         orderby uss.IsHardStateUser descending, uss.Percentage descending
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
