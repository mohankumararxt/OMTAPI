using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IEmailDetailsService
    {
        ResultDTO SendMail(SendEmailDTO sendEmailDTO);

    }
}
