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

                string senderEmail = _authSettings.Value.FromEmailId;
                string password = _authSettings.Value.Password;
                string toemails = string.Join(',', sendEmailDTO.ToEmailIds);
                string host = _authSettings.Value.Host;
                int port = _authSettings.Value.Port;
                bool enablessl = _authSettings.Value.EnableSSL;

                MailMessage message = new MailMessage();

                message.From = new MailAddress(senderEmail);

                // Add multiple recipients to the "To" field (Comma-separated email addresses)
                message.To.Add(toemails);
                message.Subject = sendEmailDTO.Subject;
                message.Body = sendEmailDTO.Body;
                //message.IsBodyHtml = true; // to enable html

                SmtpClient smtpClient = new SmtpClient(host)
                {
                    Port = port, // Common port for TLS
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = enablessl // Enable SSL for secure email sending
                };

                // Send the email
                smtpClient.Send(message);

                resultDTO.Message = "Email sent successfully to multiple recipients!";

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
