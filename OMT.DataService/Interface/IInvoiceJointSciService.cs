using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface IInvoiceJointSciService
    {
        ResultDTO CreateInvoiceJointSci(InvoiceJointSciCreateDTO invoiceJointSciCreateDTO);
        ResultDTO UpdateInvoiceJointSci(InvoiceJointSciResponseDTO invoiceJointSciResponseDTO);
        ResultDTO GetInvoiceJointSci();
    }
}
