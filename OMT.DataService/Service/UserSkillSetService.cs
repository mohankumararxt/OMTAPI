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
    public class UserSkillSetService : IUserSkillSetService
    {

        private readonly OMTDataContext _oMTDataContext;
        public UserSkillSetService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetUserSkillSetList(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200"};
            
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
                                                                         Percentage = uss.Percentage
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
                var existing_UserSkillSetId = _oMTDataContext.UserSkillSet.Where(x => x.UserId == userid && x.SkillSetId == userSkillSetCreateDTO.SkillSetId && x.IsActive && x.Percentage == userSkillSetCreateDTO.Percentage).FirstOrDefault();

                if(existing_UserSkillSetId != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "You have already added this skill set to your profile";
                }

                else
                {
                    UserSkillSet userSkillSet = new UserSkillSet()
                    {
                        UserId = userid,
                        SkillSetId = userSkillSetCreateDTO.SkillSetId,
                        Percentage = userSkillSetCreateDTO.Percentage,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    _oMTDataContext.UserSkillSet.Add(userSkillSet);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Your skill set has been added succcessfully";
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
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200"};

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

        public ResultDTO UpdateUserSkillSet(UserSkillSetResponseDTO userskillSetResponseDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                UserSkillSet? userSkillSet = _oMTDataContext.UserSkillSet.Find(userskillSetResponseDTO.UserSkillSetId);
                if (userSkillSet != null)
                {
                    userSkillSet.SkillSetId = userskillSetResponseDTO.SkillSetId;
                    userSkillSet.Percentage = userskillSetResponseDTO .Percentage;
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
