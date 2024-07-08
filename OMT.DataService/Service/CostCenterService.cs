using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;

namespace OMT.DataService.Service
{
    public class CostCenterService : ICostCenterService
    {
        private readonly OMTDataContext _oMTDataContext;
        public CostCenterService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateCostCenter(string CostcenterAmount)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existing_costcenter = _oMTDataContext.CostCenter.Where(x => x.CostCenterAmount == CostcenterAmount).FirstOrDefault();

                if (existing_costcenter != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Cost Center already exists. Please try to add different Cost Center";
                }
                else
                {
                    CostCenter costCenter = new CostCenter()
                    {
                         CostCenterAmount = CostcenterAmount,
                    };

                    _oMTDataContext.CostCenter.Add(costCenter);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Cost Center created successfully";

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

        public ResultDTO DeleteCostCenter(int costCenterId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                CostCenter costCenter = _oMTDataContext.CostCenter.Where(x => x.CostCenterId == costCenterId).FirstOrDefault();

                if (costCenter != null)
                {
                    _oMTDataContext.CostCenter.Remove(costCenter);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Cost Center has been deleted successfully";
                }
                else
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Cost Center is not found";
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

        public ResultDTO GetCostCenterList()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<CostCenter>  costCenters = _oMTDataContext.CostCenter.ToList();

                if (costCenters.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Cost Center";
                    resultDTO.Data = costCenters;
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

        public ResultDTO UpdateCostCenter(UpdateCostCenterDTO updateCostCenterDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                CostCenter costCenter = _oMTDataContext.CostCenter.Where(x => x.CostCenterId == updateCostCenterDTO.CostCenterId).FirstOrDefault();

                if (costCenter != null)
                {
                    costCenter.CostCenterAmount = updateCostCenterDTO.CostCenterAmount;
                    _oMTDataContext.CostCenter.Update(costCenter);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Cost Center updated successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Cost Center not found";
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
