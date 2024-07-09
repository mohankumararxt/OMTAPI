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
    public class SourceTypeService : ISourceTypeService
    {
        private readonly OMTDataContext _oMTDataContext;

        public SourceTypeService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateSourceType(string SourceTypeName)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                SourceType st = _oMTDataContext.SourceType.Where(x => x.IsActive && x.SourceTypeName == SourceTypeName).FirstOrDefault();

                if (st != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Source Type already exists. Please try to add different Source Type";
                }
                else
                {
                    SourceType sty = new SourceType()
                    {
                        SourceTypeName = SourceTypeName,
                        IsActive = true,
                    };

                    _oMTDataContext.SourceType.Add(sty);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Source Type created successfully";
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

        //public ResultDTO DeleteSourceType(int stid)
        //{
        //    ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
        //    try
        //    {
        //        SourceType st = _oMTDataContext.SourceType.Where(x => x.IsActive && x.SourceTypeId == stid).FirstOrDefault();

        //        if (st != null)
        //        {
        //            st.IsActive = false;
        //            _oMTDataContext.SourceType.Update(st);
        //            _oMTDataContext.SaveChanges();
        //            resultDTO.IsSuccess = true;
        //            resultDTO.Message = "Source Type has been deleted successfully";
        //        }
        //        else
        //        {
        //            resultDTO.StatusCode = "404";
        //            resultDTO.IsSuccess = false;
        //            resultDTO.Message = "Source Type is not found";
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

        public ResultDTO GetSourceType()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<SourceType> st = _oMTDataContext.SourceType.Where(x => x.IsActive).OrderBy(x => x.SourceTypeName).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Source Types";
                resultDTO.Data = st;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO UpdateSourceType(SourceTypeDTO sourceTypeDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                SourceType st = _oMTDataContext.SourceType.Find(sourceTypeDTO.SourceTypeId);

                if (st != null)
                {
                    st.SourceTypeName = sourceTypeDTO.SourceTypeName;
                    _oMTDataContext.SourceType.Update(st);
                    _oMTDataContext.SaveChanges();
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Source Type updated successfully";

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Source Type not found";
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
