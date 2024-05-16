using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class UserSkillSetService : IUserSkillSetService
    {

        private readonly OMTDataContext _oMTDataContext;
        public UserSkillSetService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetUserSkillSetList(int? userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                if (userid == null)
                {
                    List<UserSkillSetResponseDTO> listofuserskillsets = (from up in _oMTDataContext.UserProfile
                                                                         join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                                                         join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                         where up.IsActive == true && uss.IsActive == true
                                                                         orderby up.FirstName, ss.SkillSetName
                                                                         select new UserSkillSetResponseDTO
                                                                         {
                                                                             UserName = up.FirstName,
                                                                             UserId = uss.UserId,
                                                                             UserSkillSetId = uss.UserSkillSetId,
                                                                             SkillSetName = ss.SkillSetName,
                                                                             SkillSetId = uss.SkillSetId,
                                                                             Percentage = uss.Percentage,
                                                                             IsPrimary = uss.IsPrimary,
                                                                             IsHardStateUser = uss.IsHardStateUser,
                                                                             HardStateName = uss.HardStateName,
                                                                         }).ToList();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of User_SkillSets";
                    resultDTO.Data = listofuserskillsets;
                }
                else
                {
                    List<UserSkillSetResponseDTO> listofuserskillsets1 = (from up in _oMTDataContext.UserProfile
                                                                         join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                                                         join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                         where up.UserId == userid && up.IsActive == true && uss.IsActive == true
                                                                         orderby up.FirstName, ss.SkillSetName
                                                                         select new UserSkillSetResponseDTO
                                                                         {
                                                                             UserName = up.FirstName,
                                                                             UserId = uss.UserId,
                                                                             UserSkillSetId = uss.UserSkillSetId,
                                                                             SkillSetName = ss.SkillSetName,
                                                                             SkillSetId = uss.SkillSetId,
                                                                             Percentage = uss.Percentage,
                                                                             IsPrimary = uss.IsPrimary,
                                                                             IsHardStateUser = uss.IsHardStateUser,
                                                                             HardStateName = uss.HardStateName,

                                                                         }).ToList();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of User_SkillSets";
                    resultDTO.Data = listofuserskillsets1;
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

        public ResultDTO AddUserSkillSet(UserSkillSetCreateDTO userSkillSetCreateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                var existing_UserSkillSetId = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userSkillSetCreateDTO.UserId && x.SkillSetId == userSkillSetCreateDTO.SkillSetId && x.IsActive).FirstOrDefault();

                if (existing_UserSkillSetId != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill set already exists in this profile";
                }
                else
                {
                    UserSkillSet userSkillSet = new UserSkillSet()
                    {
                        UserId = userSkillSetCreateDTO.UserId,
                        SkillSetId = userSkillSetCreateDTO.SkillSetId,
                        Percentage = userSkillSetCreateDTO.Percentage,
                        IsPrimary = userSkillSetCreateDTO.IsPrimary,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        IsHardStateUser = userSkillSetCreateDTO.IsHardStateUser,
                        HardStateName = userSkillSetCreateDTO.HardStateName != null ? string.Join(",", userSkillSetCreateDTO.HardStateName) : null,
                };
                    if (userSkillSetCreateDTO.IsPrimary == true)
                    {
                        var existing_IsPrimary = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userSkillSetCreateDTO.UserId && x.IsPrimary == true && x.IsActive).FirstOrDefault();
                        if (existing_IsPrimary != null)
                        {
                            existing_IsPrimary.IsPrimary = false;
                            _oMTDataContext.UserSkillSet.Update(existing_IsPrimary);
                            _oMTDataContext.SaveChanges();
                        }
                    }

                    _oMTDataContext.UserSkillSet.Add(userSkillSet);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "User skill set has been added succcessfully";

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
        public ResultDTO DeleteUserSkillSet(int userskillsetId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                UserSkillSet? userSkillSet = _oMTDataContext.UserSkillSet.Where(x => x.UserSkillSetId == userskillsetId && x.IsActive).FirstOrDefault();

                if (userSkillSet != null)
                {
                    userSkillSet.IsActive = false;
                    _oMTDataContext.UserSkillSet.Update(userSkillSet);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Skill set deleted successfully";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Selected Skill set is not found in your profile";
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

        public ResultDTO UpdateUserSkillSet(UserSkillSetUpdateDTO userSkillSetUpdateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                UserSkillSet? userSkillSet = _oMTDataContext.UserSkillSet.Find(userSkillSetUpdateDTO.UserSkillSetId);
                if (userSkillSet != null)
                {
                    bool isUpdatingCurrentPrimary = userSkillSet.IsPrimary && userSkillSetUpdateDTO.IsPrimary;

                    // If the current skill set is primary and we're updating to be primary, do not unmark it
                    if (userSkillSetUpdateDTO.IsPrimary && !isUpdatingCurrentPrimary)
                    {
                        // Find existing primary skill set for the user
                        var existingPrimary = _oMTDataContext.UserSkillSet
                            .Where(x => x.IsPrimary && x.UserId == userSkillSetUpdateDTO.UserId && x.IsActive)
                            .FirstOrDefault();

                        if (existingPrimary != null && existingPrimary.UserSkillSetId != userSkillSet.UserSkillSetId)
                        {
                            existingPrimary.IsPrimary = false;
                            _oMTDataContext.UserSkillSet.Update(existingPrimary);
                        }
                    }

                    // Update the current skill set with the new values
                    userSkillSet.SkillSetId = userSkillSetUpdateDTO.SkillSetId;
                    userSkillSet.Percentage = userSkillSetUpdateDTO.Percentage;
                    userSkillSet.IsPrimary = userSkillSetUpdateDTO.IsPrimary;
                    userSkillSet.IsHardStateUser = userSkillSetUpdateDTO.IsHardStateUser;
                    userSkillSet.HardStateName = userSkillSetUpdateDTO.HardStateName != null ? string.Join(",", userSkillSetUpdateDTO.HardStateName) : null;

                    _oMTDataContext.UserSkillSet.Update(userSkillSet);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Your skill set has been updated successfully";
                    resultDTO.Data = userSkillSet;
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Selected skill set doesn't exist in your profile";
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
