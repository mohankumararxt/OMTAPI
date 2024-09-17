using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using OMT.DataService.Settings;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Net;
using System.Net.Mail;

namespace OMT.DataService.Utility
{
    public class EmailDetailsService : IEmailDetailsService
    {
        private readonly IOptions<EmailDetailsSettings> _authSettings;
        public EmailDetailsService(IOptions<EmailDetailsSettings> authSettings)
        {
            _authSettings = authSettings;
        }

        public ResultDTO SendMail(SendEmailDTO sendEmailDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {

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
