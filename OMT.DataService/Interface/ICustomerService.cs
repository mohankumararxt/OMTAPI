using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Interface
{
    public interface ICustomerService
    {
        ResultDTO CreateCustomer(string customername);
      //  ResultDTO DeleteCustomer(int customerId);
        ResultDTO GetCustomerList();
        ResultDTO UpdateCustomer(CustomerDTO customerDTO);
    }
}
