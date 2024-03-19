using OMT.DTO;
using System.Runtime.InteropServices.JavaScript;

namespace OMT.DataService.Interface
{
    public interface ITemplateService
    {
        ResultDTO CreateTemplate(CreateTemplateDTO createTemplateDTO);
        ResultDTO DeleteTemplate(int SkillSetId);
        ResultDTO UploadOrders(UploadTemplateDTO uploadTemplateDTO);
        ResultDTO ValidateOrders(UploadTemplateDTO uploadTemplateDTO);
        ResultDTO GetOrders(int userid);
        ResultDTO GetTemplateList();
    }
}
