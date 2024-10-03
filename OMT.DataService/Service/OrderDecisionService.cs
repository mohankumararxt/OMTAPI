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
                var users = _oMTDataContext.UserProfile.Where(x => x.IsActive).Select(_ => _.UserId).ToList();


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
            return resultDTO;
        }
    }
}
