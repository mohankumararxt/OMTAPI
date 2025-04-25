using OMT.DTO;

namespace OMT.DataService.Interface
{
    public interface IInvoiceJointReswareService
    {
        ResultDTO CreateInvoiceJointResware(InvoiceJointReswareCreateDTO invoiceJointReswareCreateDTO);
        ResultDTO UpdateInvoiceJointResware(InvoiceJointReswareUpdateDTO invoiceJointReswareUpdateDTO);
        ResultDTO GetInvoiceJointResware();
        ResultDTO CreateInvoiceJointTiqe(InvoiceJointReswareCreateDTO invoiceJointReswareCreateDTO);
        ResultDTO UpdateInvoiceJointTiqe(InvoiceJointTiqeUpdateDTO invoiceJointTiqeUpdateDTO);
        ResultDTO GetInvoiceJointTiqe();
    }
}
