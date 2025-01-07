using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Utility;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class UserService : IUserService
    {
        private readonly OMTDataContext _oMTDataContext;
        public UserService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO CreateUser(CreateUserDTO createUserDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                string existingUserCheck = _oMTDataContext.UserProfile.Where(x => (x.Email == createUserDTO.Email || x.EmployeeId == createUserDTO.EmployeeId) && x.IsActive).Select(_ => _.Email).FirstOrDefault();
                if (existingUserCheck != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Email Id or Employee Id already exists. Please try to add user with valid details.";
                }
                else
                {
                    string encryptedPassword = Encryption.EncryptPlainTextToCipherText(createUserDTO.Password);
                    UserProfile userProfile = new UserProfile()
                    {
                        Is_Verified = false,
                        IsActive = true,
                        FirstName = createUserDTO.FirstName,
                        LastName = createUserDTO.LastName,
                        CreatedDate = DateTime.Now,
                        Email = createUserDTO.Email,
                        Mobile = createUserDTO.MobileNumber,
                        OrganizationId = createUserDTO.OrganizationId,
                        Password = encryptedPassword,
                        Last_Login = null,
                        RoleId = createUserDTO.RoleId,
                        EmployeeId = createUserDTO.EmployeeId
                    };

                    _oMTDataContext.UserProfile.Add(userProfile);
                    _oMTDataContext.SaveChanges();
                    resultDTO.Message = "User Created Successfully.";
                    resultDTO.IsSuccess = true;
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

        public ResultDTO DeleteUser(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                UserProfile? user = _oMTDataContext.UserProfile.Where(x => x.UserId == userid && x.IsActive).FirstOrDefault();

                if (user != null)
                {
                    user.IsActive = false;
                    _oMTDataContext.UserProfile.Update(user);
                    _oMTDataContext.SaveChanges();
                    resultDTO.Message = "User Deleted Successfully.";
                    resultDTO.IsSuccess = true;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "User not found";
                    resultDTO.StatusCode = "404";
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

        public ResultDTO GetUserList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<UserListResponseDTO> userListResponse = (from up in _oMTDataContext.UserProfile
                                                              join org in _oMTDataContext.Organization on up.OrganizationId equals org.OrganizationId
                                                              join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                                                              where up.IsActive && org.IsActive && r.IsActive
                                                              orderby up.FirstName
                                                              select new UserListResponseDTO()
                                                              {
                                                                  UserName = (up.FirstName ?? "") + ' ' + (up.LastName ?? ""),
                                                                  UserId = up.UserId,
                                                                  OrganizationId = org.OrganizationId,
                                                                  OrganizationName = org.OrganizationName,
                                                                  FirstName = up.FirstName,
                                                                  LastName = up.LastName,
                                                                  Mobile = up.Mobile,
                                                                  Email = up.Email,
                                                                  RoleId = r.RoleId,
                                                                  RoleName = r.RoleName,
                                                                  EmployeeId = up.EmployeeId,
                                                              }).ToList();

                if (userListResponse.Count > 0)
                {
                    resultDTO.Data = userListResponse;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Users";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "List of Users not found";
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
        public ResultDTO UpdateByHR(UpdateUserByHrDTO updateUserDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                UserProfile? user = _oMTDataContext.UserProfile.Where(x => x.UserId == updateUserDTO.UserId && x.IsActive).FirstOrDefault();
                if (user != null)
                {
                    user.OrganizationId = updateUserDTO.OrganizationId;
                    user.RoleId = updateUserDTO.RoleId;
                    user.Mobile = updateUserDTO.Mobile;
                    _oMTDataContext.UserProfile.Update(user);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "User updated successfully";

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "User not found";
                    resultDTO.StatusCode = "404";
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

        public ResultDTO GetuserInfoByUserId(int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                UserProfile? user = _oMTDataContext.UserProfile.Where(x => x.UserId == userid && x.IsActive).FirstOrDefault();
                if (user != null)
                {
                    //User Profile Info
                    List<GetUserProfileInfoDTO> ListofUserInformations = (from up in _oMTDataContext.UserProfile
                                                                          join org in _oMTDataContext.Organization on up.OrganizationId equals org.OrganizationId
                                                                          join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                                                                          where up.IsActive && org.IsActive && r.IsActive && up.UserId == userid
                                                                          orderby up.FirstName
                                                                          select new GetUserProfileInfoDTO()
                                                                          {
                                                                              UserName = (up.FirstName ?? "") + ' ' + (up.LastName ?? ""),
                                                                              UserId = up.UserId,
                                                                              OrganizationId = org.OrganizationId,
                                                                              OrganizationName = org.OrganizationName,
                                                                              FirstName = up.FirstName,
                                                                              LastName = up.LastName,
                                                                              Mobile = up.Mobile,
                                                                              Email = up.Email,
                                                                              Password = up.Password,
                                                                              RoleId = r.RoleId,
                                                                              RoleName = r.RoleName,
                                                                              EmployeeId = up.EmployeeId,
                                                                          }).ToList();

                    //User Skill Set Info
                    List<UserSkillSetResponseDTO> listofuserskillsets = (from up in _oMTDataContext.UserProfile
                                                                         join uss in _oMTDataContext.UserSkillSet on up.UserId equals uss.UserId
                                                                         join ss in _oMTDataContext.SkillSet on uss.SkillSetId equals ss.SkillSetId
                                                                         where up.UserId == userid && up.IsActive == true && uss.IsActive == true && ss.IsActive == true
                                                                         orderby ss.SkillSetName
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

                    // Combining Both
                    UserSkillsetCombinedDTO combinedDTO = new UserSkillsetCombinedDTO()
                    {
                        UserInfo = ListofUserInformations,
                        UserSkillSet = listofuserskillsets
                    };

                    resultDTO.Data = combinedDTO;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List Of User Informations";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "User Informations not found";
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

        public ResultDTO UpdatePasswordByUser(UpdatePasswordByUserDTO updatePasswordByUserDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = false, StatusCode = "201" };
            try
            {
                UserProfile? user = _oMTDataContext.UserProfile.Where(x => x.UserId == updatePasswordByUserDTO.UserId && x.IsActive).FirstOrDefault();
                if (user != null)
                {

                    //Checking Current Password with existing Password
                    string encryptedCurrentPassword = Encryption.EncryptPlainTextToCipherText(updatePasswordByUserDTO.CurrentPassword);
                    if (user.Password != encryptedCurrentPassword)
                    {
                        resultDTO.StatusCode = "401";
                        resultDTO.Message = "Current password is incorrect";
                        return resultDTO;
                    }

                    // Encrypt and update new password
                    string encryptedNewPassword = Encryption.EncryptPlainTextToCipherText(updatePasswordByUserDTO.ConfirmNewPassword);
                    user.Password = encryptedNewPassword;
                    _oMTDataContext.UserProfile.Update(user);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "Password updated successfully";
                }
                else
                {

                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "User not found";
                    resultDTO.StatusCode = "404";
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

        public ResultDTO UpdatePasswordByHR(UpdateUserPasswordByHrDTO updateUserPasswordDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                UserProfile? user = _oMTDataContext.UserProfile.Where(x => x.UserId == updateUserPasswordDTO.UserId && x.IsActive).FirstOrDefault();

                if (user != null)
                {
                    string encryptedNewPassword = Encryption.EncryptPlainTextToCipherText(updateUserPasswordDTO.NewPassword);
                    user.Password = encryptedNewPassword;
                    _oMTDataContext.UserProfile.Update(user);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Password updated successfully";
                }
                else
                {

                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "User not found";
                    resultDTO.StatusCode = "404";
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

        public ResultDTO GetUserExcel()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };

            try
            {
                List<GetUserExcelDTO> TL_Above = (from up in _oMTDataContext.UserProfile
                                                  join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                                                  where up.IsActive && (up.RoleId == 1 || up.RoleId == 4)
                                                  orderby up.FirstName
                                                  select new GetUserExcelDTO
                                                  {
                                                      UserName = up.FirstName + " " + up.LastName,
                                                      Email = up.Email,
                                                      EmployeeId = up.EmployeeId
                                                  }).ToList();

                List<GetUserExcelDTO> agent = (from up in _oMTDataContext.UserProfile
                                               join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                                               where up.IsActive && up.RoleId == 3
                                               orderby up.FirstName
                                               select new GetUserExcelDTO
                                               {
                                                   UserName = up.FirstName + " " + up.LastName,
                                                   Email = up.Email,
                                                   EmployeeId = up.EmployeeId
                                               }).ToList();

                if ((TL_Above.Count > 0 && agent.Count > 0) || TL_Above.Count > 0 || agent.Count > 0)
                {
                    CombinedUsersListDTO combinedUsersListDTO = new CombinedUsersListDTO
                    {
                        TlnAbove = TL_Above,
                        Agent = agent,
                    };

                    resultDTO.Data = combinedUsersListDTO;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of user details fetched successfully.";

                }
                else 
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "List of user details not found";
                    resultDTO.StatusCode = "404";
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
