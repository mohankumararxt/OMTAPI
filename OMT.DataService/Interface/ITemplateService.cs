using OMT.DTO;
using System.Runtime.InteropServices.JavaScript;

namespace OMT.DataService.Interface
{
    public interface ITemplateService
    {
        ResultDTO CreateTemplate(CreateTemplateDTO createTemplateDTO);
        ResultDTO DeleteTemplate(int SkillSetId);
        ResultDTO UploadData(UploadTemplateDTO uploadTemplateDTO);
        ResultDTO ValidateData(UploadTemplateDTO uploadTemplateDTO);
    }
}
