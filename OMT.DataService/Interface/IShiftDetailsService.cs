using OMT.DTO;


namespace OMT.DataService.Interface
{
    public interface IShiftDetailsService
    {
        ResultDTO CreateShiftDetails(ShiftDetailsDTO shiftDetailsDTO,int userid);
        ResultDTO GetShiftDetails();
        ResultDTO UpdateShiftDetails(EditShiftDetailsDTO editShiftDetailsDTO,int userid);
        ResultDTO DeleteShiftDetails(int ShiftCodeId);

    }
}
