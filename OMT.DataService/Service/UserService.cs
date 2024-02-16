﻿using OMT.DataAccess.Context;
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
                string existingUserCheck = _oMTDataContext.UserProfile.Where(x => x.Email ==  createUserDTO.Email).Select(_ => _.Email).FirstOrDefault();
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
                        Is_Active = true,
                        FirstName = createUserDTO.FirstName,
                        LastName = createUserDTO.LastName,
                        CreatedDate = DateTime.Now,
                        Email = createUserDTO.Email,
                        Mobile = createUserDTO.MobileNumber,
                        OrganizationId = createUserDTO.OrganizationId,
                        Password = encryptedPassword,
                        Last_Login = null,
                        RoleId = createUserDTO.RoleId
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
    }
}
