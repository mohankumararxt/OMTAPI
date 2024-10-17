using Azure.Identity;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Collections.Generic;

namespace OMT.DataService.Service
{
    public class UserSkillSetService : IUserSkillSetService
    {

        private readonly OMTDataContext _oMTDataContext;
        private readonly IOrderDecisionService _orderDecisionService;

        public UserSkillSetService(OMTDataContext oMTDataContext, IOrderDecisionService orderDecisionService)
        {
            _oMTDataContext = oMTDataContext;
            _orderDecisionService = orderDecisionService;
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
                                                                         where up.IsActive == true && uss.IsActive == true && ss.IsActive == true
                                                                         orderby up.FirstName, ss.SkillSetName
                                                                         select new UserSkillSetResponseDTO
                                                                         {
                                                                             UserName = (up.FirstName ?? "") + ' ' + (up.LastName ?? ""),
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
                                                                          where up.UserId == userid && up.IsActive == true && uss.IsActive == true && ss.IsActive == true
                                                                          orderby up.FirstName, ss.SkillSetName
                                                                          select new UserSkillSetResponseDTO
                                                                          {
                                                                              UserName = (up.FirstName ?? "") + ' ' + (up.LastName ?? ""),
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
                    resultDTO.Message = "User Skillset deleted successfully";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Selected Skill set is not found in the profile";
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
                    resultDTO.Message = "User skillset has been updated successfully";
                    resultDTO.Data = userSkillSet;
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Selected skill set doesn't exist in the profile";
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

        public ResultDTO UpdateUserSkillsetList(UpdateUserSkillsetListDTO updateUserSkillsetListDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                if (updateUserSkillsetListDTO.TeamId != null && updateUserSkillsetListDTO.SystemOfRecordId != null)
                {
                    List<SkillSet> skillSet = _oMTDataContext.SkillSet.Where(x => x.SystemofRecordId == updateUserSkillsetListDTO.SystemOfRecordId && x.IsActive).OrderBy(x => x.SkillSetName).ToList();

                    var users = (from te in _oMTDataContext.Teams
                                 join ta in _oMTDataContext.TeamAssociation on te.TeamId equals ta.TeamId
                                 join up in _oMTDataContext.UserProfile on ta.UserId equals up.UserId
                                 join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                 join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                 join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                 where ta.TeamId == updateUserSkillsetListDTO.TeamId && sor.SystemofRecordId == updateUserSkillsetListDTO.SystemOfRecordId && uss.IsActive && up.IsActive && te.IsActive && ss.IsActive && sor.IsActive
                                 select new
                                 {
                                     UserId = ta.UserId,
                                     UserName = (up.FirstName ?? "") + ' ' + (up.LastName ?? ""),
                                 }).Distinct().ToList();

                    UpdateUserSkillsetListResponseDTO userSkillsetResponse = new UpdateUserSkillsetListResponseDTO();
                    List<userskillsetdetailsDTO> userSkillsetDetailsList = new List<userskillsetdetailsDTO>();

                    foreach (var user in users)
                    {
                        List<UserskillsetAssociationdetailsDTO> details = (from uss in _oMTDataContext.UserSkillSet
                                                                           join up in _oMTDataContext.UserProfile on uss.UserId equals up.UserId
                                                                           join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                           join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                                                           where sor.SystemofRecordId == updateUserSkillsetListDTO.SystemOfRecordId && uss.IsActive && up.IsActive && up.UserId == user.UserId && ss.IsActive && sor.IsActive
                                                                           select new UserskillsetAssociationdetailsDTO
                                                                           {
                                                                               SkillSetId = uss.SkillSetId,
                                                                               SkillSetName = ss.SkillSetName,
                                                                               IsPrimary = uss.IsPrimary,
                                                                               UserSkillSetId = uss.UserSkillSetId,
                                                                           }).ToList();

                        userSkillsetResponse = new UpdateUserSkillsetListResponseDTO
                        {
                            UserId = user.UserId,
                            UserName = user.UserName,
                            UserSkillsetDetails = details
                        };

                        userskillsetdetailsDTO existingUserSkillsetDetails = userSkillsetDetailsList.FirstOrDefault(x => x.SkillSetNames.SequenceEqual(skillSet.Select(d => d.SkillSetName).ToList()));

                        if (existingUserSkillsetDetails != null)
                        {
                            existingUserSkillsetDetails.updateUserSkillsetListResponse.Add(userSkillsetResponse);
                        }
                        else
                        {
                            userskillsetdetailsDTO newUserSkillsetDetails = new userskillsetdetailsDTO
                            {
                                SkillSetNames = skillSet.Select(d => d.SkillSetName).ToList(),
                                updateUserSkillsetListResponse = new List<UpdateUserSkillsetListResponseDTO> { userSkillsetResponse }
                            };

                            userSkillsetDetailsList.Add(newUserSkillsetDetails);
                        }
                    }

                    if (userSkillsetDetailsList.Count > 0)
                    {
                        resultDTO.Data = userSkillsetDetailsList;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "User Skillset details fetched successfully";
                    }
                    else
                    {
                        resultDTO.StatusCode = "404";
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "User Skillset details not found";
                        resultDTO.Data = userSkillsetDetailsList;
                    }

                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Selected team or SystemOfRecord is not found";
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

        public ResultDTO BulkUpdate(BulkUserSkillsetUpdateDTO bulkUserSkillsetUpdateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                if (bulkUserSkillsetUpdateDTO != null)
                {
                    foreach (var details in bulkUserSkillsetUpdateDTO.UserInfoToUpdate)
                    {
                        UserSkillSet userSkillSet = _oMTDataContext.UserSkillSet.Where(x => x.UserSkillSetId == details.UserSkillSetId && x.UserId == details.UserId && x.IsActive).FirstOrDefault();

                        if (userSkillSet != null)
                        {
                            bool isUpdatingCurrentPrimary = userSkillSet.IsPrimary;

                            // If the current skill set is primary and we're updating to be primary, do not unmark it
                            if (!isUpdatingCurrentPrimary)
                            {
                                // Find existing primary skill set for the user
                                var existingPrimary = _oMTDataContext.UserSkillSet
                                                      .Where(x => x.IsPrimary && x.UserId == details.UserId && x.IsActive)
                                                      .FirstOrDefault();

                                if (existingPrimary != null && existingPrimary.UserSkillSetId != userSkillSet.UserSkillSetId)
                                {
                                    existingPrimary.IsPrimary = false;
                                    _oMTDataContext.UserSkillSet.Update(existingPrimary);
                                }

                            }

                            // Update the current skill set with the new values

                            userSkillSet.IsPrimary = true;

                            _oMTDataContext.UserSkillSet.Update(userSkillSet);
                            _oMTDataContext.SaveChanges();

                        }
                        else
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.StatusCode = "404";
                            resultDTO.Message = "User skillsets update failed due to unavailable resource";
                        }
                    }

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "User's skill sets have been updated successfully";

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
        public ResultDTO CreateMultipleUserSkillset(MultipleUserSkillSetCreateDTO multipleUserSkillSetCreateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                var existingUserSkillSet = _oMTDataContext.UserSkillSet.Where(x => x.UserId == multipleUserSkillSetCreateDTO.UserId && x.IsActive).FirstOrDefault();

                if (existingUserSkillSet != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The UserSkillSet already Exists for the User.Please! try to Update.";
                }
                else
                {
                    foreach (var detail in multipleUserSkillSetCreateDTO.FirstCycle)
                    {
                        UserSkillSet userSkillSet = new UserSkillSet
                        {
                            UserId = multipleUserSkillSetCreateDTO.UserId,
                            SkillSetId = detail.SkillSetId,
                            Percentage = (int)detail.Weightage,
                            IsHardStateUser = detail.IsHardStateUser,
                            HardStateName = detail.HardStateName != null && detail.HardStateName.Any() ? string.Join(",", detail.HardStateName) : "",
                            IsActive = true,
                            IsCycle1 = true,
                            CreatedDate = DateTime.Now,
                        };
                        _oMTDataContext.UserSkillSet.Add(userSkillSet);
                        _oMTDataContext.SaveChanges();
                    }
                    foreach (var details in multipleUserSkillSetCreateDTO.SecondCycle)
                    {
                        UserSkillSet userSkillSet2 = new UserSkillSet
                        {
                            UserId = multipleUserSkillSetCreateDTO.UserId,
                            SkillSetId = details.SkillSetId,
                            Percentage = 0,
                            IsHardStateUser = details.IsHardStateUser,
                            HardStateName = details.HardStateName != null && details.HardStateName.Any() ? string.Join(",", details.HardStateName) : "",
                            IsActive = true,
                            IsCycle1 = false,
                            CreatedDate = DateTime.Now,
                        };
                        _oMTDataContext.UserSkillSet.Add(userSkillSet2);
                        _oMTDataContext.SaveChanges();
                    }

                    // after adding userskillset for new user, call the insertintogoc table method 

                    _orderDecisionService.InsertGetOrderCalculation(resultDTO,multipleUserSkillSetCreateDTO.UserId);

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "UserSkillset Added Successfully.";
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

        public ResultDTO ConsolidatedUserSkillSetlist(int? userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<ConsolidatedUserSkillSetlistDTO> allUserSkillSet = new List<ConsolidatedUserSkillSetlistDTO>();

                var USSid = _oMTDataContext.UserSkillSet.Where(uss => uss.IsActive).Select(uss => uss.UserId).Distinct();
                  
                var userskillSetIds = (userid == null) ? USSid.ToList() : new List<int> { userid.Value };

                foreach (var id in userskillSetIds)
                {
                    var userProfile = _oMTDataContext.UserProfile.FirstOrDefault(up => up.UserId == id);
                    var userName = userProfile != null ? userProfile.FirstName + ' ' + userProfile.LastName : "User Not Found";

                    //Cycle1
                    List<UserSkillSetDetailsDTO> FirstCycle1 = (from uss in _oMTDataContext.UserSkillSet
                                                                join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                join up in _oMTDataContext.UserProfile on uss.UserId equals up.UserId
                                                                where uss.IsActive && uss.UserId == id && uss.IsCycle1
                                                                orderby uss.UserId
                                                                select new UserSkillSetDetailsDTO()
                                                                {
                                                                    UserSkillSetId=uss.UserSkillSetId, 
                                                                    SkillSetId = uss.SkillSetId,
                                                                    SkillSetName = ss.SkillSetName,
                                                                    Weightage = uss.Percentage,
                                                                    IsHardStateUser = uss.IsHardStateUser,
                                                                    HardStateName = uss.HardStateName,
                                                                }).ToList();

                    //Cycle2
                    List<UserSkillSetDetailsDTO> SecondCycle2 = (from uss in _oMTDataContext.UserSkillSet
                                                                 join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                 join up in _oMTDataContext.UserProfile on uss.UserId equals up.UserId
                                                                 where uss.IsActive && uss.UserId == id && !uss.IsCycle1
                                                                 orderby uss.UserId
                                                                 select new UserSkillSetDetailsDTO()
                                                                 {
                                                                     UserSkillSetId=uss.UserSkillSetId, 
                                                                     SkillSetId = uss.SkillSetId,
                                                                     SkillSetName = ss.SkillSetName,
                                                                     Weightage = 0,
                                                                     IsHardStateUser = uss.IsHardStateUser,
                                                                     HardStateName = uss.HardStateName,
                                                                 }).ToList();



                    if (FirstCycle1.Count == 0 && SecondCycle2.Count == 0)
                    {
                        resultDTO.IsSuccess = false;
                        resultDTO.Message = "No Consolidated Userskillset details found for this Userid";
                        return resultDTO;
                    }

                    //combine details 
                    ConsolidatedUserSkillSetlistDTO userSkillSetDetailsDTO = new ConsolidatedUserSkillSetlistDTO()
                    {
                        Username = userName,
                        UserId = id,
                        FirstCycle = FirstCycle1,
                        SecondCycle = SecondCycle2
                    };

                    allUserSkillSet.Add(userSkillSetDetailsDTO);
                }
                resultDTO.Data = allUserSkillSet;
                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Consolidated Userskillset Details Successfully Fetched";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }  
        public ResultDTO UpdateUserSkillSetThWt(UpdateUserSkillSetThWtDTO updateUserSkillSetThWtDTO)
        {
            ResultDTO resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "201" };
            try
            {  
                var exisitinguss = _oMTDataContext.UserSkillSet.Where(uss => uss.UserId == updateUserSkillSetThWtDTO.UserId).ToList(); 

                //Disable
                foreach (var USS_SS in exisitinguss)
                {
                    USS_SS.IsActive = false;
                    _oMTDataContext.UserSkillSet.Update(USS_SS);
                    _oMTDataContext.SaveChanges();
                }
                //cycle 1
                foreach (var USS_ss in updateUserSkillSetThWtDTO.FirstCycle)
                {
                    var Uss_Cycle1 = exisitinguss.FirstOrDefault(uss => uss.IsCycle1 && uss.SkillSetId==USS_ss.SkillSetId);
                    if (Uss_Cycle1 != null)
                    {
                        //Activate
                        Uss_Cycle1.Percentage = (int)USS_ss.Weightage;
                        Uss_Cycle1.IsActive = true;
                        _oMTDataContext.UserSkillSet.Update(Uss_Cycle1);
                        _oMTDataContext.SaveChanges();
                    }
                    else
                    {
                        UserSkillSet userSkillSet = new UserSkillSet()
                        {
                            UserId = updateUserSkillSetThWtDTO.UserId,
                            Percentage = (int)USS_ss.Weightage,
                            SkillSetId = USS_ss.SkillSetId,
                            HardStateName = USS_ss.HardStateName != null && USS_ss.HardStateName.Any() ? string.Join(",", USS_ss.HardStateName) : "",
                            IsHardStateUser = USS_ss.IsHardStateUser,
                            IsCycle1 = true,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                        };
                        _oMTDataContext.UserSkillSet.Add(userSkillSet);
                        _oMTDataContext.SaveChanges();
                    }
                }
                //cycle 2
                foreach (var Uss_skillset in updateUserSkillSetThWtDTO.SecondCycle)
                {
                    var Uss_Cycle2 = exisitinguss.FirstOrDefault(uss => uss.IsCycle1==false && uss.SkillSetId == Uss_skillset.SkillSetId);
                    if (Uss_Cycle2 != null)
                    {
                        //Activate
                        Uss_Cycle2.Percentage = (int)Uss_skillset.Weightage;
                        Uss_Cycle2.IsActive = true;
                        _oMTDataContext.UserSkillSet.Update(Uss_Cycle2);
                        _oMTDataContext.SaveChanges();
                    }
                    else
                    {
                        UserSkillSet userSkillSet = new UserSkillSet()
                        {
                            UserId = updateUserSkillSetThWtDTO.UserId,
                            Percentage = (int)Uss_skillset.Weightage,
                            SkillSetId = Uss_skillset.SkillSetId,
                            HardStateName = Uss_skillset.HardStateName != null && Uss_skillset.HardStateName.Any() ? string.Join(",", Uss_skillset.HardStateName) : "",
                            IsHardStateUser = Uss_skillset.IsHardStateUser,
                            IsCycle1 = false,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                        };
                        _oMTDataContext.UserSkillSet.Add(userSkillSet);
                        _oMTDataContext.SaveChanges();
                    }
                }
                resultDTO.IsSuccess = true;
                resultDTO.Message = "UserSkillSet Updated Successfully";
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