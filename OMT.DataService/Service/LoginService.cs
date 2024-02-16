using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DataService.Utility;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class LoginService : ILoginService
    {
        private readonly OMTDataContext _oMTDataContext;
        public LoginService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO Login(LoginRequestDTO loginRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                string encryptedPassword = Encryption.EncryptPlainTextToCipherText(loginRequestDTO.Password);
                LoginResponseDTO loginResponseDTO = _oMTDataContext.UserProfile.Where(x => x.Email == loginRequestDTO.Email && x.Password == encryptedPassword)
                    .Select(_ => new LoginResponseDTO()
                    {
                        Email = _.Email,
                        FirstName = _.FirstName,
                        Mobile = _.Mobile,
                        OrganizationId = _.OrganizationId,
                        UserId = _.UserId,
                        RoleId = _.RoleId
                    }).FirstOrDefault();

                if (loginResponseDTO != null)
                {
                    resultDTO.Data = loginResponseDTO;
                }
                else
                {
                    resultDTO.Message = "Login Failed. Please check your credentials.";
                    resultDTO.IsSuccess = false;
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
