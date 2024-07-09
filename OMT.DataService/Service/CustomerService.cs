using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly OMTDataContext _oMTDataContext;
        public CustomerService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateCustomer(string customername)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_customer = _oMTDataContext.Customer.Where(x => x.CustomerName == customername).FirstOrDefault();

                if (existing_customer != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Customer already exists. Please try to add different Customer";
                }
                else
                {
                    Customer  customer = new Customer()
                    {
                        CustomerName = customername,
                    };

                    _oMTDataContext.Customer.Add(customer);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Customer created successfully";

                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        //public ResultDTO DeleteCustomer(int customerId)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        Customer  customer = _oMTDataContext.Customer.Where(x => x.CustomerId == customerId).FirstOrDefault();

        //        if (customer != null)
        //        {
        //            customer.IsActive = false;
        //            _oMTDataContext.Customer.Update(customer);
        //            _oMTDataContext.SaveChanges();
        //            resultDTO.IsSuccess = true;
        //            resultDTO.Message = "Customer has been deleted successfully";
        //        }
        //        else
        //        {
        //            resultDTO.StatusCode = "404";
        //            resultDTO.IsSuccess = false;
        //            resultDTO.Message = "Customer is not found";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resultDTO.IsSuccess = false;
        //        resultDTO.StatusCode = "500";
        //        resultDTO.Message = ex.Message;
        //    }
        //    return resultDTO;
        //}

        public ResultDTO GetCustomerList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<CustomerDTO> customers = _oMTDataContext.Customer.Where(x=>x.IsActive).Select(_=> new CustomerDTO() 
                {
                    CustomerId = _.CustomerId,
                    CustomerName = _.CustomerName,  
                }).ToList();

                if (customers.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Customer";
                    resultDTO.Data = customers;
                }

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO UpdateCustomer(CustomerDTO customerDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                Customer customer = _oMTDataContext.Customer.Where(x => x.CustomerId == customerDTO.CustomerId).FirstOrDefault();

                if (customer != null)
                {
                    customer.CustomerName = customerDTO.CustomerName;
                    _oMTDataContext.Customer.Update(customer);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Customer updated successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Customer not found";
                    resultDTO.StatusCode = "404";
                }
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
