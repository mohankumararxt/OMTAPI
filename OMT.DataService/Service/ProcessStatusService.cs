﻿using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Service
{
    public class ProcessStatusService : IProcessStatusService
    {
        private readonly OMTDataContext _oMTDataContext;

        public ProcessStatusService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO GetStatusList(int systemofrecordid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<ProcessStatusResponseDTO> listofstatus = (from sor in _oMTDataContext.SystemofRecord
                                                               join ps in _oMTDataContext.ProcessStatus on sor.SystemofRecordId equals ps.SystemOfRecordId
                                                               where ps.IsActive == true && ps.SystemOfRecordId == systemofrecordid
                                                               select new ProcessStatusResponseDTO
                                                               {
                                                                    SystemofRecordName = sor.SystemofRecordName,
                                                                    Status = ps.Status,
                                                                    SystemofRecordId = ps.SystemOfRecordId,
                                                               }).ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Status";
                resultDTO.Data = listofstatus;
                
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
