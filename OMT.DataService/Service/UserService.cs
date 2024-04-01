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
        //private readonly OMTDataContext _oMTDataContext;
        public UserService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO CreateUser(CreateUserDTO createUserDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                string existingUserCheck = _oMTDataContext.UserProfile.Where(x => x.Email == createUserDTO.Email).Select(_ => _.Email).FirstOrDefault();
                if (existingUserCheck != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The user already exists. Please try to login with your credentials.";
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
                UserProfile? user = _oMTDataContext.UserProfile.Where(x =>x.UserId == userid && x.IsActive).FirstOrDefault();

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
                List<UserListResponseDTO> userListResponseDTOs = _oMTDataContext.UserProfile.Where(x=>x.IsActive).Select(_  => new UserListResponseDTO
                {
                    Email = _.Email,
                    EmployeeId = _.EmployeeId,
                    FirstName = _.FirstName,
                    LastName = _.LastName,
                    UserId = _.UserId,
                    UserName = (_.FirstName??"") +' ' + (_.LastName?? "") +'(' + _.Email + ')'
                }).ToList();
                resultDTO.Data = userListResponseDTOs;
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
