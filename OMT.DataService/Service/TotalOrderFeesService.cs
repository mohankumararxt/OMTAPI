using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class TotalOrderFeesService: ITotalOrderFeesService
    {
        private readonly OMTDataContext _oMTDataContext;
        public TotalOrderFeesService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateTotalOrderFee(string TotalOrderfeeAmount)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_OrderFees = _oMTDataContext.TotalOrderFees.Where(x => x.TotalOrderFeesAmount == TotalOrderfeeAmount).FirstOrDefault();

                if (existing_OrderFees != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Order Fee already exists. Please try to add different Order Fee";
                }
                else
                {
                    TotalOrderFees totalOrderFees = new TotalOrderFees()
                    {
                        TotalOrderFeesAmount = TotalOrderfeeAmount,
                    };

                    _oMTDataContext.TotalOrderFees.Add(totalOrderFees);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Order fee created successfully";

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

        //public ResultDTO DeleteTotalOrderFee(int orderFeeId)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        TotalOrderFees totalOrderFees = _oMTDataContext.TotalOrderFees.Where(x => x.TotalOrderFeesId == orderFeeId).FirstOrDefault();

        //        if (totalOrderFees != null)
        //        {
        //            _oMTDataContext.TotalOrderFees.Remove(totalOrderFees);
        //            _oMTDataContext.SaveChanges();
        //            resultDTO.IsSuccess = true;
        //            resultDTO.Message = "Order fee has been deleted successfully";
        //        }
        //        else
        //        {
        //            resultDTO.StatusCode = "404";
        //            resultDTO.IsSuccess = false;
        //            resultDTO.Message = "Order fee is not found";
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

        public ResultDTO GetTotalOrderFeeList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<TotalOrderFees> totalOrderFees = _oMTDataContext.TotalOrderFees.ToList();

                if (totalOrderFees.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Total Order Fee";
                    resultDTO.Data = totalOrderFees;
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

        public ResultDTO UpdateTotalOrderFee(UpdateTotalOrderFeeDTO updateTotalOrderFeeDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                TotalOrderFees totalOrderFees = _oMTDataContext.TotalOrderFees.Where(x => x.TotalOrderFeesId == updateTotalOrderFeeDTO.TotalOrderFeeId).FirstOrDefault();

                if (totalOrderFees != null)
                {
                    totalOrderFees.TotalOrderFeesAmount = updateTotalOrderFeeDTO.TotalOrderFee;
                    _oMTDataContext.TotalOrderFees.Update(totalOrderFees);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Total Order Fee updated successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Total Order Fee not found";
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
