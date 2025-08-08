using Microsoft.Extensions.Options;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Settings;
using OMT.DataService.Utility;
using OMT.DTO;
using System.Text;

namespace OMT.DataService.Service
{
    public class LoginService : ILoginService
    {
        private readonly OMTDataContext _oMTDataContext;
        private readonly IOptions<ResetPasswordSettings> _resetpasswordsettings;
        private readonly IOptions<EmailDetailsSettings> _emailDetailsSettings;
        public LoginService(OMTDataContext oMTDataContext, IOptions<EmailDetailsSettings> emailDetailsSettings, IOptions<ResetPasswordSettings> resetPasswordSettings)
        {
            _oMTDataContext = oMTDataContext;
            _emailDetailsSettings = emailDetailsSettings;
            _resetpasswordsettings = resetPasswordSettings;
        }



        public ResultDTO Login(LoginRequestDTO loginRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                string encryptedPassword = Encryption.EncryptPlainTextToCipherText(loginRequestDTO.Password);

                //var omtmenus = 

                //LoginResponseDTO loginResponseDTO = (from up in _oMTDataContext.UserProfile.Where(x => x.Email == loginRequestDTO.Email && x.Password == encryptedPassword && x.IsActive)
                //                                     join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                //                                     select new LoginResponseDTO()
                //                                     {
                //                                         Email = up.Email,
                //                                         FirstName = up.FirstName,
                //                                         Mobile = up.Mobile,
                //                                         OrganizationId = up.OrganizationId,
                //                                         UserId = up.UserId,
                //                                         RoleId = up.RoleId,
                //                                         Role = r.RoleName,
                //                                         Checked_In = _oMTDataContext.User_Checkin
                //                                        .Any(uc => uc.UserId == up.UserId && uc.Checkin != null && uc.Checkout == null)
                //                                      }).FirstOrDefault();


                var user = (from up in _oMTDataContext.UserProfile
                            join r in _oMTDataContext.Roles on up.RoleId equals r.RoleId
                            where up.Email == loginRequestDTO.Email
                                  && up.Password == encryptedPassword
                                  && up.IsActive && r.IsActive
                            select new
                            {
                                up.Email,
                                up.FirstName,
                                up.Mobile,
                                up.OrganizationId,
                                up.UserId,
                                up.RoleId,
                                Role = r.RoleName
                            }).FirstOrDefault();

                LoginResponseDTO loginResponseDTO = null;

                if (user != null)
                {
                    var menuNames = (from od in _oMTDataContext.OmtMenus_Distribution
                                     join m in _oMTDataContext.OmtMenus on od.OmtMenus_Id equals m.OmtMenus_Id
                                     where od.RoleId == user.RoleId && od.IsActive && m.IsActive
                                     select m.OmtMenus_Name).Distinct().ToList();

                    loginResponseDTO = new LoginResponseDTO
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        Mobile = user.Mobile,
                        OrganizationId = user.OrganizationId,
                        UserId = user.UserId,
                        RoleId = user.RoleId,
                        Role = user.Role,
                        Checked_In = _oMTDataContext.User_Checkin
                            .Any(uc => uc.UserId == user.UserId && uc.Checkin != null && uc.Checkout == null),
                        OmtMenus = menuNames
                    };
                }


                if (loginResponseDTO != null)
                {
                    resultDTO.Data = loginResponseDTO;
                    resultDTO.Message = "Logged in successfully";
                    resultDTO.IsSuccess = true;
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

        public ResultDTO Checkin(CheckinoutRequestDTO checkinRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                User_Checkin user_Checkin = _oMTDataContext.User_Checkin
                                                           .Where(x => x.UserId == checkinRequestDTO.UserId)
                                                           .OrderByDescending(x => x.Id)
                                                           .FirstOrDefault();

                if (user_Checkin != null)
                {

                    if (user_Checkin.Checkin != null && user_Checkin.Checkout == null)
                    {
                        user_Checkin.Checkout = checkinRequestDTO.DateTime;
                        _oMTDataContext.User_Checkin.Update(user_Checkin);
                    }
                    else if (user_Checkin.Checkin != null && user_Checkin.Checkout != null)
                    {
                        User_Checkin user_time = new User_Checkin()
                        {
                            UserId = checkinRequestDTO.UserId,
                            Checkin = checkinRequestDTO.DateTime,
                        };

                        _oMTDataContext.User_Checkin.Add(user_time);
                    }

                }
                else
                {
                    User_Checkin user_time = new User_Checkin()
                    {
                        UserId = checkinRequestDTO.UserId,
                        Checkin = checkinRequestDTO.DateTime,

                    };

                    _oMTDataContext.User_Checkin.Add(user_time);

                }

                _oMTDataContext.SaveChanges();

                CheckinResponseDTO responseDTO = new CheckinResponseDTO()
                {
                    Checked_In = _oMTDataContext.User_Checkin.Any(uc => uc.UserId == checkinRequestDTO.UserId && uc.Checkin != null && uc.Checkout == null)
                };

                resultDTO.Data = responseDTO;
                resultDTO.Message = "Checkin / checkout successful";
                resultDTO.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO ForgotPassword(ForgotPasswordRequestDTO forgotPasswordRequestDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var user = _oMTDataContext.UserProfile.Where(x => x.Email == forgotPasswordRequestDTO.EmailId && x.IsActive).FirstOrDefault();

                if (user != null)
                {
                    string token = Guid.NewGuid().ToString();

                    var resetpassword = new PasswordResetTokens
                    {
                        UserId = user.UserId,
                        GuId = token,
                        LinkSentDate = DateTime.UtcNow,
                        IsUsed = false,
                        ResetDate = null
                    };

                    _oMTDataContext.PasswordResetTokens.Add(resetpassword);
                    _oMTDataContext.SaveChanges();


                    string resetUrl1 = _resetpasswordsettings.Value.ResetPasswordURL;

                    string resetUrl2 = resetUrl1 + "{" + token + "}";

                    List<string> toEmailIds1 = new List<string>
                    {
                        forgotPasswordRequestDTO.EmailId
                    };

                    SendEmailDTO sendEmailDTO1 = new SendEmailDTO
                    {
                        ToEmailIds = toEmailIds1,
                        Subject = "Password Reset Link",
                        Body = "Please click the reset link given below to reset your password:\n" + resetUrl2

                    };

                    var url = _emailDetailsSettings.Value.SendEmailURL;

                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailDTO1);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var webApiUrl = new Uri(url);
                            var response = client.PostAsync(webApiUrl, content).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = response.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "We've sent a password reset link to your email address. Please check your inbox to proceed.";


                }
                else
                {
                    resultDTO.Message = "Invalid Email Id. Please check your credentials.";
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

        public ResultDTO ValidateGuId(ValidateGuIdDTO validateGuIdDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var guid_validity = _oMTDataContext.PasswordResetTokens.Where(x => x.GuId == validateGuIdDTO.GuId && !x.IsUsed).FirstOrDefault();

                if (guid_validity != null)
                {
                    var userdetails = _oMTDataContext.UserProfile.Where(x => x.UserId == guid_validity.UserId && x.IsActive).FirstOrDefault();

                    resultDTO.Data = userdetails.Email;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Please fill the required details to reset the password";
                }
                else
                {
                    resultDTO.Message = "This reset password link has expired.";
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

        public ResultDTO ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string encryptedPassword = Encryption.EncryptPlainTextToCipherText(resetPasswordDTO.NewPassword);

                var userdetails = _oMTDataContext.UserProfile.Where(x => x.Email == resetPasswordDTO.Email && x.IsActive).FirstOrDefault();

                if(userdetails != null)
                {
                    userdetails.Password = encryptedPassword;

                    _oMTDataContext.UserProfile.Update(userdetails);
                    _oMTDataContext.SaveChanges();

                    var reset = _oMTDataContext.PasswordResetTokens.Where(x => x.GuId == resetPasswordDTO.GuId && !x.IsUsed).FirstOrDefault();

                    if(reset != null)
                    {
                        reset.ResetDate = DateTime.UtcNow;
                        reset.IsUsed = true;

                        _oMTDataContext.PasswordResetTokens.Update(reset);
                        _oMTDataContext.SaveChanges();
                    }

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Password has been reset successfully.";
                }

                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Error fetching account detials.";
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
