using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Collections.Generic;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OMT.DataService.Service
{
    public class UserSkillSetService : IUserSkillSetService
    {

        private readonly OMTDataContext _oMTDataContext;

        private readonly IUpdateGOCService _updateGOCService;

        public UserSkillSetService(OMTDataContext oMTDataContext, IUpdateGOCService updateGOCService)
        {
            _oMTDataContext = oMTDataContext;
            _updateGOCService = updateGOCService;
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
                        if (detail.IsHardStateUser)
                        {
                            foreach (var hsdetails in detail.HardStateDetails)
                            {
                                UserSkillSet hs_userSkillSet = new UserSkillSet
                                {
                                    UserId = multipleUserSkillSetCreateDTO.UserId,
                                    SkillSetId = detail.SkillSetId,
                                    Percentage = hsdetails.Weightage,
                                    IsHardStateUser = detail.IsHardStateUser,
                                    HardStateName = hsdetails.HardStateName,
                                    IsActive = true,
                                    IsCycle1 = true,
                                    CreatedDate = DateTime.Now,
                                };
                                _oMTDataContext.UserSkillSet.Add(hs_userSkillSet);
                                _oMTDataContext.SaveChanges();
                            }

                            UserSkillSet nr_userSkillSet = new UserSkillSet
                            {
                                UserId = multipleUserSkillSetCreateDTO.UserId,
                                SkillSetId = detail.SkillSetId,
                                Percentage = ((int)detail.Weightage != null && (int)detail.Weightage != 0) ? (int)detail.Weightage : 0,
                                IsHardStateUser = false,
                                HardStateName = "",
                                IsActive = true,
                                IsCycle1 = true,
                                CreatedDate = DateTime.Now,
                            };
                            _oMTDataContext.UserSkillSet.Add(nr_userSkillSet);
                            _oMTDataContext.SaveChanges();
                        }
                        else
                        {
                            UserSkillSet userSkillSet = new UserSkillSet
                            {
                                UserId = multipleUserSkillSetCreateDTO.UserId,
                                SkillSetId = detail.SkillSetId,
                                Percentage = ((int)detail.Weightage != null && (int)detail.Weightage != 0) ? (int)detail.Weightage : 0,
                                IsHardStateUser = detail.IsHardStateUser,
                                HardStateName = "",
                                IsActive = true,
                                IsCycle1 = true,
                                CreatedDate = DateTime.Now,
                            };
                            _oMTDataContext.UserSkillSet.Add(userSkillSet);
                            _oMTDataContext.SaveChanges();
                        }
                    }
                    foreach (var details in multipleUserSkillSetCreateDTO.SecondCycle)
                    {
                        if (details.IsHardStateUser)
                        {
                            foreach (var hsdetails in details.HardStateDetails)
                            {
                                UserSkillSet hs_userSkillSet = new UserSkillSet
                                {
                                    UserId = multipleUserSkillSetCreateDTO.UserId,
                                    SkillSetId = details.SkillSetId,
                                    Percentage = hsdetails.Weightage,
                                    IsHardStateUser = details.IsHardStateUser,
                                    HardStateName = hsdetails.HardStateName,
                                    IsActive = true,
                                    IsCycle1 = false,
                                    CreatedDate = DateTime.Now,
                                };
                                _oMTDataContext.UserSkillSet.Add(hs_userSkillSet);
                                _oMTDataContext.SaveChanges();
                            }

                            UserSkillSet nr_userSkillSet = new UserSkillSet
                            {
                                UserId = multipleUserSkillSetCreateDTO.UserId,
                                SkillSetId = details.SkillSetId,
                                Percentage = ((int)details.Weightage != null && (int)details.Weightage != 0) ? (int)details.Weightage : 0,
                                IsHardStateUser = false,
                                HardStateName = "",
                                IsActive = true,
                                IsCycle1 = false,
                                CreatedDate = DateTime.Now,
                            };
                            _oMTDataContext.UserSkillSet.Add(nr_userSkillSet);
                            _oMTDataContext.SaveChanges();
                        }
                        else
                        {
                            UserSkillSet userSkillSet2 = new UserSkillSet
                            {
                                UserId = multipleUserSkillSetCreateDTO.UserId,
                                SkillSetId = details.SkillSetId,
                                Percentage = ((int)details.Weightage != null && (int)details.Weightage != 0) ? (int)details.Weightage : 0,
                                IsHardStateUser = details.IsHardStateUser,
                                HardStateName = "",
                                IsActive = true,
                                IsCycle1 = false,
                                CreatedDate = DateTime.Now,
                            };
                            _oMTDataContext.UserSkillSet.Add(userSkillSet2);
                            _oMTDataContext.SaveChanges();
                        }

                    }

                    // after adding userskillset for new user, call the insertintogoc table method 

                    _updateGOCService.InsertGetOrderCalculation(resultDTO, multipleUserSkillSetCreateDTO.UserId);

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

                var USSid = _oMTDataContext.UserSkillSet.Where(uss => uss.IsActive).Select(uss => uss.UserId).Distinct().ToList();

                var active_users = _oMTDataContext.UserProfile.Where(x => x.IsActive && USSid.Contains(x.UserId)).Select(x => x.UserId).ToList();

                var userids = (userid == null) ? active_users : new List<int> { userid.Value };

                foreach (var id in userids)
                {
                    List<UserSkillSetDetailsDTO> FirstCycle1 = new List<UserSkillSetDetailsDTO>();
                    List<UserSkillSetDetailsDTO> SecondCycle2 = new List<UserSkillSetDetailsDTO>();

                    var userProfile = _oMTDataContext.UserProfile.FirstOrDefault(up => up.UserId == id);
                    var userName = userProfile != null ? userProfile.FirstName + ' ' + userProfile.LastName : "User Not Found";

                    //Cycle1

                    List<int> ssids_c1 = _oMTDataContext.UserSkillSet.Where(x => x.UserId == id && x.IsActive && x.IsCycle1).Select(x => x.SkillSetId).Distinct().ToList();

                    foreach (var ssid in ssids_c1)
                    {
                        List<UPdateHardStateDetailsDTO> details_hs_c1 = new List<UPdateHardStateDetailsDTO>();
                        var skillsetname = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == ssid).Select(x => x.SkillSetName).FirstOrDefault();

                        var hs_ss = _oMTDataContext.UserSkillSet.Where(x => x.UserId == id && x.IsActive && x.IsCycle1 && x.SkillSetId == ssid && x.IsHardStateUser).ToList();

                        var ns_ss = _oMTDataContext.UserSkillSet.Where(x => x.UserId == id && x.IsActive && x.IsCycle1 && x.SkillSetId == ssid && !x.IsHardStateUser).FirstOrDefault();

                        if (hs_ss.Count > 0)
                        {
                            foreach (var h in hs_ss)
                            {
                                var detail_add_hs = new UPdateHardStateDetailsDTO()
                                {
                                    HardStateName = h.HardStateName,
                                    Weightage = h.Percentage,
                                    UserSkillSetId = h.UserSkillSetId,
                                };

                                details_hs_c1.Add(detail_add_hs);

                            }

                            UserSkillSetDetailsDTO c1_hs = new UserSkillSetDetailsDTO()
                            {
                                UserSkillSetId = ns_ss.UserSkillSetId,
                                SkillSetId = ns_ss.SkillSetId,
                                SkillSetName = skillsetname,
                                Weightage = ns_ss.Percentage,
                                IsHardStateUser = true,
                                HardStateDetails = details_hs_c1
                            };

                            FirstCycle1.Add(c1_hs);
                        }
                        else
                        {
                            UserSkillSetDetailsDTO c1_ns = new UserSkillSetDetailsDTO()
                            {
                                UserSkillSetId = ns_ss.UserSkillSetId,
                                SkillSetId = ns_ss.SkillSetId,
                                SkillSetName = skillsetname,
                                Weightage = ns_ss.Percentage,
                                IsHardStateUser = false,
                            };

                            FirstCycle1.Add(c1_ns);
                        }
                    }

                    //Cycle2

                    List<int> ssids_c2 = _oMTDataContext.UserSkillSet.Where(x => x.UserId == id && x.IsActive && !x.IsCycle1).Select(x => x.SkillSetId).Distinct().ToList();

                    foreach (var ssid in ssids_c2)
                    {
                        List<UPdateHardStateDetailsDTO> details_hs_c2 = new List<UPdateHardStateDetailsDTO>();
                        var skillsetname = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == ssid).Select(x => x.SkillSetName).FirstOrDefault();

                        var hs_ss = _oMTDataContext.UserSkillSet.Where(x => x.UserId == id && x.IsActive && !x.IsCycle1 && x.SkillSetId == ssid && x.IsHardStateUser).ToList();

                        var ns_ss = _oMTDataContext.UserSkillSet.Where(x => x.UserId == id && x.IsActive && !x.IsCycle1 && x.SkillSetId == ssid && !x.IsHardStateUser).FirstOrDefault();

                        if (hs_ss.Count > 0)
                        {
                            foreach (var h in hs_ss)
                            {
                                var detail_add_hs_c2 = new UPdateHardStateDetailsDTO()
                                {
                                    HardStateName = h.HardStateName,
                                    Weightage = h.Percentage,
                                    UserSkillSetId = h.UserSkillSetId,
                                };

                                details_hs_c2.Add(detail_add_hs_c2);

                            }

                            UserSkillSetDetailsDTO c2_hs = new UserSkillSetDetailsDTO()
                            {
                                UserSkillSetId = ns_ss.UserSkillSetId,
                                SkillSetId = ns_ss.SkillSetId,
                                SkillSetName = skillsetname,
                                Weightage = ns_ss.Percentage,
                                IsHardStateUser = true,
                                HardStateDetails = details_hs_c2
                            };

                            SecondCycle2.Add(c2_hs);
                        }
                        else
                        {
                            UserSkillSetDetailsDTO c2_ns = new UserSkillSetDetailsDTO()
                            {
                                UserSkillSetId = ns_ss.UserSkillSetId,
                                SkillSetId = ns_ss.SkillSetId,
                                SkillSetName = skillsetname,
                                Weightage = ns_ss.Percentage,
                                IsHardStateUser = ns_ss.IsHardStateUser,
                            };

                            SecondCycle2.Add(c2_ns);
                        }
                    }

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
                resultDTO.Data = allUserSkillSet.OrderBy(x => x.Username);
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
            string? connectionstring = _oMTDataContext.Database.GetConnectionString();
            using SqlConnection connection = new(connectionstring);
            connection.Open();

            ResultDTO resultDTO = new ResultDTO { IsSuccess = true, StatusCode = "201" };
            try
            {
                // check if any to be deleted skillset has any orders in que for the user, if yes dont allow to delete
                var ExistingActiveUSS_Cycle1 = _oMTDataContext.UserSkillSet.Where(uss => uss.UserId == updateUserSkillSetThWtDTO.UserId && uss.IsActive && uss.IsCycle1).Select(x => x.SkillSetId).Distinct().ToList();
                var ExistingActiveUSS_Cycle2 = _oMTDataContext.UserSkillSet.Where(uss => uss.UserId == updateUserSkillSetThWtDTO.UserId && uss.IsActive && !uss.IsCycle1).Select(x => x.SkillSetId).Distinct().ToList();

                var incomingUss_Cycle1 = updateUserSkillSetThWtDTO.FirstCycle.Select(x => x.SkillSetId).ToList();
                var incomingUss_Cycle2 = updateUserSkillSetThWtDTO.SecondCycle.Select(x => x.SkillSetId).ToList();

                var Delete_Cycle1 = ExistingActiveUSS_Cycle1.Except(incomingUss_Cycle1).ToList();
                var Delete_Cycle2 = ExistingActiveUSS_Cycle2.Except(incomingUss_Cycle2).ToList();

                var Delete_Cycle = Delete_Cycle1.Concat(Delete_Cycle2).Distinct().ToList();

                Delete_Cycle = _oMTDataContext.TemplateColumns.Where(x => Delete_Cycle.Contains(x.SkillSetId)).Select(x => x.SkillSetId).ToList();

                List<string> Dont_Delete_Skillset = new List<string>(); // will have the skillsets in which the user has an order to be processed

                // get skillsets which cant be deleted 
                foreach (var dc1 in Delete_Cycle)
                {
                    var skillsetinfo1 = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == dc1).FirstOrDefault();

                    string AssignedOrder = $"SELECT * FROM {skillsetinfo1.SkillSetName} WHERE UserId = @userid AND Status IS NULL AND CompletionDate IS NULL";

                    using SqlCommand AssignedOrder_command = new SqlCommand(AssignedOrder, connection);

                    AssignedOrder_command.Parameters.AddWithValue("@userid", updateUserSkillSetThWtDTO.UserId);

                    using SqlDataAdapter AssignedOrderDA = new SqlDataAdapter(AssignedOrder_command);
                    DataSet AssignedOrderDS = new DataSet();

                    AssignedOrderDA.Fill(AssignedOrderDS);

                    DataTable AssignedOrderDT = AssignedOrderDS.Tables[0];

                    var query_AssignedOrder = AssignedOrderDT.AsEnumerable()
                                     .Select(row => AssignedOrderDT.Columns.Cast<DataColumn>().ToDictionary(
                                         column => column.ColumnName,
                                         column => row[column] == DBNull.Value ? "" : row[column])).ToList();

                    if (query_AssignedOrder.Count > 0)
                    {
                        Dont_Delete_Skillset.Add(skillsetinfo1.SkillSetName);
                    }

                }

                if (Dont_Delete_Skillset.Count > 0)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "The following skillsets cannot be deleted as the user has orders in their queue: " + string.Join(", ", Dont_Delete_Skillset) + ".";

                }

                else
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
                        if (USS_ss.IsHardStateUser)
                        {
                            foreach (var h in USS_ss.HardStateDetails)
                            {
                                var is_hs = exisitinguss.Where(x => x.IsCycle1 && x.IsHardStateUser && x.SkillSetId == USS_ss.SkillSetId && x.HardStateName == h.HardStateName).FirstOrDefault();

                                if (is_hs != null)
                                {
                                    is_hs.IsActive = true;
                                    is_hs.IsCycle1 = true;
                                    is_hs.Percentage = h.Weightage;
                                    is_hs.IsHardStateUser = USS_ss.IsHardStateUser;
                                    is_hs.HardStateName = h.HardStateName;

                                    _oMTDataContext.UserSkillSet.Update(is_hs);
                                    _oMTDataContext.SaveChanges();
                                }
                                else
                                {
                                    UserSkillSet userSkillSet = new UserSkillSet()
                                    {
                                        UserId = updateUserSkillSetThWtDTO.UserId,
                                        Percentage = h.Weightage,
                                        SkillSetId = USS_ss.SkillSetId,
                                        HardStateName = h.HardStateName,
                                        IsHardStateUser = USS_ss.IsHardStateUser,
                                        IsCycle1 = true,
                                        IsActive = true,
                                        CreatedDate = DateTime.Now,
                                    };
                                    _oMTDataContext.UserSkillSet.Add(userSkillSet);
                                    _oMTDataContext.SaveChanges();
                                }
                            }

                        }

                        var not_hs = exisitinguss.Where(x => x.IsCycle1 && !x.IsHardStateUser && x.SkillSetId == USS_ss.SkillSetId).FirstOrDefault();

                        if (not_hs != null)
                        {
                            not_hs.IsActive = true;
                            not_hs.IsCycle1 = true;
                            not_hs.Percentage = (int)USS_ss.Weightage;
                            not_hs.IsHardStateUser = false;
                            not_hs.HardStateName = "";

                            _oMTDataContext.UserSkillSet.Update(not_hs);
                            _oMTDataContext.SaveChanges();
                        }
                        else
                        {
                            UserSkillSet userSkillSet = new UserSkillSet()
                            {
                                UserId = updateUserSkillSetThWtDTO.UserId,
                                Percentage = (int)USS_ss.Weightage,
                                SkillSetId = USS_ss.SkillSetId,
                                HardStateName = "",
                                IsHardStateUser = false,
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
                        if (Uss_skillset.IsHardStateUser)
                        {
                            foreach (var h in Uss_skillset.HardStateDetails)
                            {
                                var is_hs = exisitinguss.Where(x => !x.IsCycle1 && x.IsHardStateUser && x.SkillSetId == Uss_skillset.SkillSetId && x.HardStateName == h.HardStateName).FirstOrDefault();

                                if (is_hs != null)
                                {
                                    is_hs.IsActive = true;
                                    is_hs.IsCycle1 = false;
                                    is_hs.Percentage = h.Weightage;
                                    is_hs.IsHardStateUser = Uss_skillset.IsHardStateUser;
                                    is_hs.HardStateName = h.HardStateName;

                                    _oMTDataContext.UserSkillSet.Update(is_hs);
                                    _oMTDataContext.SaveChanges();
                                }
                                else
                                {
                                    UserSkillSet userSkillSet = new UserSkillSet()
                                    {
                                        UserId = updateUserSkillSetThWtDTO.UserId,
                                        Percentage = h.Weightage,
                                        SkillSetId = Uss_skillset.SkillSetId,
                                        HardStateName = h.HardStateName,
                                        IsHardStateUser = Uss_skillset.IsHardStateUser,
                                        IsCycle1 = false,
                                        IsActive = true,
                                        CreatedDate = DateTime.Now,
                                    };
                                    _oMTDataContext.UserSkillSet.Add(userSkillSet);
                                    _oMTDataContext.SaveChanges();
                                }
                            }

                        }

                        var not_hs = exisitinguss.Where(x => !x.IsCycle1 && !x.IsHardStateUser && x.SkillSetId == Uss_skillset.SkillSetId).FirstOrDefault();

                        if (not_hs != null)
                        {
                            not_hs.IsActive = true;
                            not_hs.IsCycle1 = false;
                            not_hs.Percentage = (int)Uss_skillset.Weightage;
                            not_hs.IsHardStateUser = false;
                            not_hs.HardStateName = "";

                            _oMTDataContext.UserSkillSet.Update(not_hs);
                            _oMTDataContext.SaveChanges();
                        }
                        else
                        {
                            UserSkillSet userSkillSet = new UserSkillSet()
                            {
                                UserId = updateUserSkillSetThWtDTO.UserId,
                                Percentage = (int)Uss_skillset.Weightage,
                                SkillSetId = Uss_skillset.SkillSetId,
                                HardStateName = "",
                                IsHardStateUser = false,
                                IsCycle1 = false,
                                IsActive = true,
                                CreatedDate = DateTime.Now,
                            };
                            _oMTDataContext.UserSkillSet.Add(userSkillSet);
                            _oMTDataContext.SaveChanges();
                        }
                    }

                    //call EditUssInGOC method to update goc table 
                    EditUssInGOC(updateUserSkillSetThWtDTO, resultDTO, connection);

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "UserSkillSet Updated Successfully";
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

        private void EditUssInGOC(UpdateUserSkillSetThWtDTO updateUserSkillSetThWtDTO, ResultDTO resultDTO, SqlConnection connection)
        {
            try
            {
                var exisitinguss = _oMTDataContext.GetOrderCalculation.Where(uss => uss.UserId == updateUserSkillSetThWtDTO.UserId).ToList();

                //Disable all uss of the user
                foreach (var USS_SS in exisitinguss)
                {
                    USS_SS.IsActive = false;
                    USS_SS.PriorityOrder = 0;
                    _oMTDataContext.GetOrderCalculation.Update(USS_SS);
                    _oMTDataContext.SaveChanges();
                }

                //cycle 1
                List<Dictionary<int, int>> ss_per = new List<Dictionary<int, int>>();

                foreach (var s in updateUserSkillSetThWtDTO.FirstCycle)
                {
                    var total_weightage = (int)s.Weightage;
                    foreach (var hs in s.HardStateDetails)
                    {
                        total_weightage += hs.Weightage;
                    }

                    var skillWeightageDict = new Dictionary<int, int>
                    {
                        { s.SkillSetId, total_weightage }
                    };

                    ss_per.Add(skillWeightageDict);
                }

                var sortedList = ss_per.OrderByDescending(dict => dict.Values.First()).Select(dict => dict.Keys.First()).ToList();

                var cycle1_ss = updateUserSkillSetThWtDTO.FirstCycle.OrderBy(s => sortedList.IndexOf(s.SkillSetId)).ToList();

                var PriorityOrder = 1;
                int currentPriorityOrder;
                int highestCycle1PriorityOrder = 0;
                var skillSetPriorityMap_c1 = new Dictionary<int, int>();
                var skillSetPriorityMap_c2 = new Dictionary<int, int>();

                foreach (var USS_ip in cycle1_ss)
                {
                    var threshold = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == USS_ip.SkillSetId && x.IsActive).Select(_ => _.Threshold).FirstOrDefault();

                    if (!skillSetPriorityMap_c1.TryGetValue(USS_ip.SkillSetId, out currentPriorityOrder))
                    {
                        // Assign the next available PriorityOrder for this SkillSetId
                        currentPriorityOrder = PriorityOrder++;
                        skillSetPriorityMap_c1[USS_ip.SkillSetId] = currentPriorityOrder;

                    }
                    highestCycle1PriorityOrder = Math.Max(highestCycle1PriorityOrder, currentPriorityOrder);

                    if (USS_ip.IsHardStateUser)
                    {
                        foreach (var item in USS_ip.HardStateDetails)
                        {
                            var hs_present = exisitinguss.FirstOrDefault(uss => uss.SkillSetId == USS_ip.SkillSetId && uss.IsCycle1 && uss.IsHardStateUser && uss.UserSkillSetId == item.UserSkillSetId);

                            if (hs_present != null)
                            {
                                var ExistingWeightage = hs_present.Weightage;

                                hs_present.IsActive = true;
                                hs_present.IsCycle1 = true;
                                hs_present.UserSkillSetId = item.UserSkillSetId;
                                hs_present.IsHardStateUser = USS_ip.IsHardStateUser;
                                hs_present.Weightage = item.Weightage;
                                hs_present.PriorityOrder = currentPriorityOrder;

                                if (item.Weightage != ExistingWeightage)
                                {
                                    double totalorders = ((double)item.Weightage / 100) * threshold;
                                    int roundedtotalorders = totalorders == 0 ? 0 : (totalorders > 0 && totalorders < 1) ? 1 : (int)Math.Round(totalorders, MidpointRounding.AwayFromZero);

                                    if (hs_present.OrdersCompleted != roundedtotalorders && hs_present.OrdersCompleted < roundedtotalorders && hs_present.IsCycle1)
                                    {
                                        hs_present.Utilized = false;
                                    }
                                    else if (hs_present.OrdersCompleted >= roundedtotalorders && hs_present.IsCycle1)
                                    {
                                        hs_present.Utilized = true;
                                    }

                                    hs_present.TotalOrderstoComplete = roundedtotalorders;
                                }
                                _oMTDataContext.GetOrderCalculation.Update(hs_present);
                                _oMTDataContext.SaveChanges();
                            }
                            else
                            {
                                double totalorders = ((double)item.Weightage / 100) * threshold;
                                int roundedtotalorders = totalorders == 0 ? 0 : (totalorders > 0 && totalorders < 1) ? 1 : (int)Math.Round(totalorders, MidpointRounding.AwayFromZero);

                                var ussid = _oMTDataContext.UserSkillSet.Where(x => x.UserId == updateUserSkillSetThWtDTO.UserId && x.IsActive && x.IsCycle1 && x.SkillSetId == USS_ip.SkillSetId && x.IsHardStateUser && x.HardStateName == item.HardStateName).FirstOrDefault();

                                GetOrderCalculation goc = new GetOrderCalculation()
                                {
                                    UserId = updateUserSkillSetThWtDTO.UserId,
                                    UserSkillSetId = ussid.UserSkillSetId,
                                    SkillSetId = USS_ip.SkillSetId,
                                    TotalOrderstoComplete = roundedtotalorders,
                                    OrdersCompleted = 0,
                                    Weightage = item.Weightage,
                                    PriorityOrder = currentPriorityOrder,
                                    Utilized = roundedtotalorders == 0 ? true : false,
                                    IsActive = true,
                                    UpdatedDate = DateTime.Now,
                                    IsCycle1 = true,
                                    IsHardStateUser = USS_ip.IsHardStateUser,
                                    HardStateUtilized = false,

                                };
                                _oMTDataContext.GetOrderCalculation.Add(goc);
                                _oMTDataContext.SaveChanges();
                            }

                        }


                    }
                    var ns_present = exisitinguss.FirstOrDefault(uss => uss.SkillSetId == USS_ip.SkillSetId && uss.IsCycle1 && !uss.IsHardStateUser && uss.UserSkillSetId == USS_ip.UserSkillSetId);

                    if (ns_present != null)
                    {
                        var ExistingWeightage_n = ns_present.Weightage;

                        ns_present.IsActive = true;
                        ns_present.IsCycle1 = true;
                        ns_present.UserSkillSetId = (int)USS_ip.UserSkillSetId;
                        ns_present.IsHardStateUser = false;
                        ns_present.Weightage = (int)USS_ip.Weightage;
                        ns_present.PriorityOrder = currentPriorityOrder;

                        if (USS_ip.Weightage != ExistingWeightage_n)
                        {
                            double totalorders = ((double)USS_ip.Weightage / 100) * threshold;
                            int roundedtotalorders = totalorders == 0 ? 0 : (totalorders > 0 && totalorders < 1) ? 1 : (int)Math.Round(totalorders, MidpointRounding.AwayFromZero);

                            if (ns_present.OrdersCompleted != roundedtotalorders && ns_present.OrdersCompleted < roundedtotalorders && ns_present.IsCycle1)
                            {
                                ns_present.Utilized = false;
                            }
                            else if (ns_present.OrdersCompleted >= roundedtotalorders && ns_present.IsCycle1)
                            {
                                ns_present.Utilized = true;
                            }

                            ns_present.TotalOrderstoComplete = roundedtotalorders;
                        }
                        _oMTDataContext.GetOrderCalculation.Update(ns_present);
                        _oMTDataContext.SaveChanges();
                    }
                    else
                    {
                        double totalorders = ((double)USS_ip.Weightage / 100) * threshold;
                        int roundedtotalorders = totalorders == 0 ? 0 : (totalorders > 0 && totalorders < 1) ? 1 : (int)Math.Round(totalorders, MidpointRounding.AwayFromZero);

                        var ussid = _oMTDataContext.UserSkillSet.Where(x => x.UserId == updateUserSkillSetThWtDTO.UserId && x.IsActive && x.IsCycle1 && x.SkillSetId == USS_ip.SkillSetId && !x.IsHardStateUser && x.HardStateName == "").FirstOrDefault();

                        GetOrderCalculation goc = new GetOrderCalculation()
                        {
                            UserId = updateUserSkillSetThWtDTO.UserId,
                            UserSkillSetId = ussid.UserSkillSetId,
                            SkillSetId = USS_ip.SkillSetId,
                            TotalOrderstoComplete = roundedtotalorders,
                            OrdersCompleted = 0,
                            Weightage = (int)USS_ip.Weightage,
                            PriorityOrder = currentPriorityOrder,
                            Utilized = roundedtotalorders == 0 ? true : false,
                            IsActive = true,
                            UpdatedDate = DateTime.Now,
                            IsCycle1 = true,
                            IsHardStateUser = false,
                            HardStateUtilized = false,

                        };
                        _oMTDataContext.GetOrderCalculation.Add(goc);
                        _oMTDataContext.SaveChanges();
                    }
                }
                // cycle2
                var cycle2_ss = updateUserSkillSetThWtDTO.SecondCycle.ToList();

                foreach (var USS_ip in cycle2_ss)
                {
                    var threshold = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == USS_ip.SkillSetId && x.IsActive).Select(_ => _.Threshold).FirstOrDefault();

                    if (!skillSetPriorityMap_c2.TryGetValue(USS_ip.SkillSetId, out currentPriorityOrder))
                    {
                        // Assign the next available PriorityOrder for this SkillSetId
                        currentPriorityOrder = highestCycle1PriorityOrder + 1;
                        skillSetPriorityMap_c2[USS_ip.SkillSetId] = currentPriorityOrder;

                    }
                    if (USS_ip.IsHardStateUser)
                    {
                        foreach (var item in USS_ip.HardStateDetails)
                        {
                            var hs_present = exisitinguss.FirstOrDefault(uss => uss.SkillSetId == USS_ip.SkillSetId && !uss.IsCycle1 && uss.IsHardStateUser && uss.UserSkillSetId == item.UserSkillSetId);

                            if (hs_present != null)
                            {
                                hs_present.IsActive = true;
                                hs_present.IsCycle1 = false;
                                hs_present.UserSkillSetId = item.UserSkillSetId;
                                hs_present.IsHardStateUser = USS_ip.IsHardStateUser;
                                hs_present.Weightage = 0; // item.Weightage;
                                hs_present.PriorityOrder = currentPriorityOrder;
                                hs_present.TotalOrderstoComplete = 0;

                                _oMTDataContext.GetOrderCalculation.Update(hs_present);
                                _oMTDataContext.SaveChanges();
                            }
                            else
                            {
                                var ussid = _oMTDataContext.UserSkillSet.Where(x => x.UserId == updateUserSkillSetThWtDTO.UserId && x.IsActive && !x.IsCycle1 && x.SkillSetId == USS_ip.SkillSetId && x.IsHardStateUser && x.HardStateName == item.HardStateName).FirstOrDefault();

                                GetOrderCalculation goc = new GetOrderCalculation()
                                {
                                    UserId = updateUserSkillSetThWtDTO.UserId,
                                    UserSkillSetId = ussid.UserSkillSetId,
                                    SkillSetId = USS_ip.SkillSetId,
                                    TotalOrderstoComplete = 0,
                                    OrdersCompleted = 0,
                                    Weightage = 0,
                                    PriorityOrder = currentPriorityOrder,
                                    Utilized = false,
                                    IsActive = true,
                                    UpdatedDate = DateTime.Now,
                                    IsCycle1 = false,
                                    IsHardStateUser = USS_ip.IsHardStateUser,
                                    HardStateUtilized = false,

                                };
                                _oMTDataContext.GetOrderCalculation.Add(goc);
                                _oMTDataContext.SaveChanges();
                            }

                        }
                    }
                    var ns_present = exisitinguss.FirstOrDefault(uss => uss.SkillSetId == USS_ip.SkillSetId && !uss.IsCycle1 && !uss.IsHardStateUser && uss.UserSkillSetId == USS_ip.UserSkillSetId);

                    if (ns_present != null)
                    {
                        ns_present.IsActive = true;
                        ns_present.IsCycle1 = false;
                        ns_present.UserSkillSetId = (int)USS_ip.UserSkillSetId;
                        ns_present.IsHardStateUser = false;
                        ns_present.Weightage = 0;
                        ns_present.PriorityOrder = currentPriorityOrder;
                        ns_present.TotalOrderstoComplete = 0;

                        _oMTDataContext.GetOrderCalculation.Update(ns_present);
                        _oMTDataContext.SaveChanges();
                    }
                    else
                    {
                        var ussid = _oMTDataContext.UserSkillSet.Where(x => x.UserId == updateUserSkillSetThWtDTO.UserId && x.IsActive && !x.IsCycle1 && x.SkillSetId == USS_ip.SkillSetId && !x.IsHardStateUser && x.HardStateName == "").FirstOrDefault();

                        GetOrderCalculation goc = new GetOrderCalculation()
                        {
                            UserId = updateUserSkillSetThWtDTO.UserId,
                            UserSkillSetId = ussid.UserSkillSetId,
                            SkillSetId = USS_ip.SkillSetId,
                            TotalOrderstoComplete = 0,
                            OrdersCompleted = 0,
                            Weightage = 0,
                            PriorityOrder = currentPriorityOrder,
                            Utilized = false,
                            IsActive = true,
                            UpdatedDate = DateTime.Now,
                            IsCycle1 = false,
                            IsHardStateUser = false,
                            HardStateUtilized = false,

                        };
                        _oMTDataContext.GetOrderCalculation.Add(goc);
                        _oMTDataContext.SaveChanges();
                    }

                }
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