using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class SciExceptionService : ISciExceptionService
    {

        private readonly OMTDataContext _oMTDataContext;
        public SciExceptionService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }
        public ResultDTO GetSciExceptionReport(GetSciExceptionReportDTO getSciExceptionReportDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO UploadSciExceptionReport(UploadSciExceptionReportDTO uploadSciExceptionReportDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string tabelname = "SciException";

                string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                using SqlConnection connection = new(connectionstring);
                using SqlCommand command = new()
                {
                    Connection = connection,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "InsertSciExceptionReport"
                };

                command.Parameters.AddWithValue("@Tablename", tabelname);
                command.Parameters.AddWithValue("@jsonData", uploadSciExceptionReportDTO.JsonData);

                SqlParameter returnValue = new()
                {
                    ParameterName = "@RETURN_VALUE",
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnValue);

                connection.Open();
                command.ExecuteNonQuery();

                int returnCode = (int)command.Parameters["@RETURN_VALUE"].Value;

                if (returnCode != 1)
                {
                    throw new InvalidOperationException("Something went wrong while uploading the orders,please check the order details.");
                }

                resultDTO.IsSuccess = true;
                resultDTO.Message = "SCI-Trailing Docs-Exception report uploaded successfully";
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
