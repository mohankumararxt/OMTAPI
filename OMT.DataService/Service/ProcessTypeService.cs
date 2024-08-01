using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class ProcessTypeService : IProcessTypeService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ProcessTypeService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateProcessType(string ProcessType)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                ProcessType Pt = _oMTDataContext.ProcessType.Where(x => x.IsActive && x.ProcessTypeName == ProcessType).FirstOrDefault();

                if (Pt != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Process Type already exists. Please try to add different Process Type";
                }
                else
                {
                    ProcessType pt = new ProcessType()
                    {
                        ProcessTypeName = ProcessType,
                        IsActive = true,
                    };

                    _oMTDataContext.ProcessType.Add(pt);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Process Type created successfully";
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

        //public ResultDTO DeleteProcessType(int id)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        ProcessType Pt = _oMTDataContext.ProcessType.Where(x => x.IsActive && x.ProcessTypeId == id).FirstOrDefault();

        //        if (Pt != null)
        //        {
        //            Pt.IsActive = false;
        //            _oMTDataContext.ProcessType.Update(Pt);
        //            _oMTDataContext.SaveChanges();
        //            resultDTO.IsSuccess = true;
        //            resultDTO.Message = "Process Type has been deleted successfully";
        //        }
        //        else
        //        {
        //            resultDTO.StatusCode = "404";
        //            resultDTO.IsSuccess = false;
        //            resultDTO.Message = "Process Type is not found";
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

        public ResultDTO GetProcessType()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<ProcessType> pt = _oMTDataContext.ProcessType.Where(x => x.IsActive).OrderBy(x => x.ProcessTypeName).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Process Types";
                resultDTO.Data = pt;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO UpdateProcessType(ProcessTypeDTO processTypeDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                ProcessType Pt = _oMTDataContext.ProcessType.Find(processTypeDTO.ProcessTypeId);

                if (Pt != null)
                {
                    Pt.ProcessTypeName = processTypeDTO.ProcessTypeName;
                    _oMTDataContext.ProcessType.Update(Pt);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Process Type updated successfully";

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Process Type not found";
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
