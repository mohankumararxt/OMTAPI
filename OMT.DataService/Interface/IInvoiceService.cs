using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IInvoiceService
    {
        ResultDTO GetInvoice(GetInvoiceDTO getinvoiceDTO);
    }
}
