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

        public ResultDTO GetUserSkillSetList(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<UserSkillSetResponseDTO> listofuserskillsets = (from up in _oMTDataContext.UserProfile
                                                                     join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                                                     join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                     where up.UserId == userid && up.Is_Active == true && uss.IsActive == true
                                                                     select new UserSkillSetResponseDTO
                                                                     {
                                                                         UserSkillSetId = uss.UserSkillSetId,
                                                                         SkillSetName = ss.SkillSetName,
                                                                         SkillSetId = uss.SkillSetId,
                                                                         Percentage = uss.Percentage,
                                                                         IsPrimary = uss.IsPrimary
                                                                     }).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of User_SkillSets";
                resultDTO.Data = listofuserskillsets;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO AddUserSkillSet(UserSkillSetCreateDTO userSkillSetCreateDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                var existing_UserSkillSetId = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.SkillSetId == userSkillSetCreateDTO.SkillSetId && x.IsActive).FirstOrDefault();

                if (existing_UserSkillSetId != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill set already exists in this profile";
                }
                else
                {
                    UserSkillSet userSkillSet = new UserSkillSet()
                    {
                        UserId = userid,
                        SkillSetId = userSkillSetCreateDTO.SkillSetId,
                        Percentage = userSkillSetCreateDTO.Percentage,
                        IsPrimary = userSkillSetCreateDTO.IsPrimary,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                    if (userSkillSetCreateDTO.IsPrimary == true)
                    {
                        var existing_IsPrimary = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.IsPrimary == true && x.IsActive).FirstOrDefault();
                        if (existing_IsPrimary != null)
                        {
                            existing_IsPrimary.IsPrimary = false;
                            _oMTDataContext.UserSkillSet.Update(existing_IsPrimary);
                            _oMTDataContext.SaveChanges();
                            _oMTDataContext.UserSkillSet.Update(userSkillSet);
                            _oMTDataContext.SaveChanges();
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Your skill set has been added successfully";
                        }
                        else
                        {
                            _oMTDataContext.UserSkillSet.Add(userSkillSet);
                            _oMTDataContext.SaveChanges();
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Your skill set has been added succcessfully";
                        }
                    }
                    else
                    {
                        _oMTDataContext.UserSkillSet.Add(userSkillSet);
                        _oMTDataContext.SaveChanges();
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Your skill set has been added succcessfully";
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

        public ResultDTO UpdateUserSkillSet(UserSkillSetResponseDTO userskillSetResponseDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                UserSkillSet? userSkillSet = _oMTDataContext.UserSkillSet.Find(userskillSetResponseDTO.UserSkillSetId);
                if (userSkillSet != null)
                {
                    userSkillSet.SkillSetId = userskillSetResponseDTO.SkillSetId;
                    userSkillSet.Percentage = userskillSetResponseDTO.Percentage;
                    userSkillSet.IsPrimary = userskillSetResponseDTO.IsPrimary;
                    if (userskillSetResponseDTO.IsPrimary == true)
                    {
                        var existing_IsPrimary = _oMTDataContext.UserSkillSet.Where(x => x.IsPrimary == true && x.UserId == userid && x.IsActive).First();
                        if (existing_IsPrimary != null)
                        {
                            existing_IsPrimary.IsPrimary = false;
                            _oMTDataContext.UserSkillSet.Update(existing_IsPrimary);
                            _oMTDataContext.SaveChanges();
                            _oMTDataContext.UserSkillSet.Update(userSkillSet);
                            _oMTDataContext.SaveChanges();
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Your skill set has been updated successfully";
                            resultDTO.Data = userSkillSet;
                        }
                        else
                        {
                            _oMTDataContext.UserSkillSet.Update(userSkillSet);
                            _oMTDataContext.SaveChanges();
                            resultDTO.IsSuccess = true;
                            resultDTO.Message = "Your skill set has been updated successfully";
                            resultDTO.Data = userSkillSet;
                        }
                    }
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
                    resultDTO.Message = "Selected skill set doesnt exist in your profile";
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
