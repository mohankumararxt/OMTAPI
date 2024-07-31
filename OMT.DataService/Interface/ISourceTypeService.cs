using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface ISourceTypeService
    {
        ResultDTO CreateSourceType(string SourceTypeName);
        ResultDTO UpdateSourceType(SourceTypeDTO sourceTypeDTO);
      //  ResultDTO DeleteSourceType(int stid);
        ResultDTO GetSourceType();
    }
}
