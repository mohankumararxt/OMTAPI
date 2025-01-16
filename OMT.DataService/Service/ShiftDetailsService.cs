﻿using Microsoft.EntityFrameworkCore;
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
    public class ShiftDetailsService : IShiftDetailsService
    {
        public readonly OMTDataContext _oMTDataContext;

        public ShiftDetailsService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateShiftDetails(ShiftDetailsDTO shiftDetailsDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var existingShiftDetails = _oMTDataContext.ShiftDetails.Where(x => x.ShiftCode == shiftDetailsDTO.ShiftCode && x.IsActive).FirstOrDefault();

                if (existingShiftDetails != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "ShiftCode already exists. Please try to add different Shift details.";
                }
                else
                {

                    ShiftDetails shiftDetails = new ShiftDetails()
                    {
                        ShiftCode = shiftDetailsDTO.ShiftCode,
                        ShiftStartTime = TimeSpan.Parse(shiftDetailsDTO.ShiftStartTime),
                        ShiftEndTime = TimeSpan.Parse(shiftDetailsDTO.ShiftEndTime),
                        ShiftDays = shiftDetailsDTO.ShiftDays,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CreatedBy = userid,
                        ModifiedBy = userid
                    };

                    _oMTDataContext.ShiftDetails.Add(shiftDetails);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Shift details created successfully";
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

        public ResultDTO DeleteShiftDetails(int ShiftCodeId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                ShiftDetails shiftDetails = _oMTDataContext.ShiftDetails.Where(x => x.ShiftCodeId == ShiftCodeId && x.IsActive).FirstOrDefault();

                if (shiftDetails != null)
                {
                    shiftDetails.IsActive = false;

                    _oMTDataContext.ShiftDetails.Update(shiftDetails);
                    _oMTDataContext.SaveChanges();
                    resultDTO.Message = "Shift Details Deleted Successfully.";
                    resultDTO.IsSuccess = true;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Shift Details not found";
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

        public ResultDTO GetShiftDetails()
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                List<ShiftDetailsResponseDTO> shiftDetailsResponseDTOs = (from sd in _oMTDataContext.ShiftDetails
                                                                          join up in _oMTDataContext.UserProfile on sd.CreatedBy equals up.UserId
                                                                          where sd.IsActive
                                                                          orderby sd.ShiftCodeId
                                                                          select new ShiftDetailsResponseDTO()
                                                                          {
                                                                              ShiftCodeId = sd.ShiftCodeId,
                                                                              ShiftCode = sd.ShiftCode,
                                                                              ShiftDays = sd.ShiftDays,
                                                                              ShiftStartTime = sd.ShiftStartTime.ToString(@"hh\:mm"),
                                                                              ShiftEndTime = sd.ShiftEndTime.ToString(@"hh\:mm")

                                                                          }).ToList();

                if (shiftDetailsResponseDTOs.Count > 0)
                {
                    resultDTO.Data = shiftDetailsResponseDTOs;
                    resultDTO.IsSuccess = true;
                    resultDTO.StatusCode = "200";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "List of Shift Details not found";
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

        public ResultDTO UpdateShiftDetails(EditShiftDetailsDTO editShiftDetailsDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                ShiftDetails? shiftDetails = _oMTDataContext.ShiftDetails.Where(x => x.ShiftCodeId == editShiftDetailsDTO.ShiftCodeId && x.IsActive).FirstOrDefault();

                if (shiftDetails != null)
                {
                    shiftDetails.ShiftCode = editShiftDetailsDTO.ShiftCode;
                    shiftDetails.ShiftDays = editShiftDetailsDTO.ShiftDays;
                    shiftDetails.ShiftStartTime = TimeSpan.Parse(editShiftDetailsDTO.ShiftStartTime);
                    shiftDetails.ShiftEndTime = TimeSpan.Parse(editShiftDetailsDTO.ShiftEndTime);
                    shiftDetails.ModifiedBy = userid;
                    shiftDetails.ModifiedDate = DateTime.UtcNow;


                    _oMTDataContext.ShiftDetails.Update(shiftDetails);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Shift Details updated successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Shift Details not found";
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

        public ResultDTO CreateShiftAssociation(CreateShiftAssociationDTO createShiftAssociationDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var existingshiftass = _oMTDataContext.ShiftAssociation.Where(x => x.AgentEmployeeId == createShiftAssociationDTO.AgentEmployeeId).FirstOrDefault();

                if (existingshiftass != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Shift association already exists for this agent. Please edit the details";
                    resultDTO.StatusCode = "404";
                }
                else
                {
                    ShiftAssociation shiftAssociation = new ShiftAssociation()
                    {
                        AgentEmployeeId = createShiftAssociationDTO.AgentEmployeeId,
                        TlEmployeeId = createShiftAssociationDTO.TlEmployeeId,
                        PrimarySystemOfRecordId = createShiftAssociationDTO.PrimarySystemOfRecordId,
                        ShiftCode = createShiftAssociationDTO.ShiftCode,
                        ShiftDate = createShiftAssociationDTO.ShiftDate,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userid,
                        ModifiedDate = DateTime.Now,
                        ModifiedBy = userid,

                    };

                    _oMTDataContext.ShiftAssociation.Add(shiftAssociation);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "ShiftAssociation details created successfully";
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

        public ResultDTO GetShiftAssociation(GetShiftAssociationDTO getShiftAssociation)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                var pagination = getShiftAssociation.Pagination;
                List<ShiftAssociationResponseDTO> shiftAssociationResponseDTOs = new List<ShiftAssociationResponseDTO>();

                if (getShiftAssociation.AgentEmployeeId == null && getShiftAssociation.TlEmployeeId == null)
                {
                    shiftAssociationResponseDTOs = (from sd in _oMTDataContext.ShiftAssociation
                                                    join up1 in _oMTDataContext.UserProfile on sd.AgentEmployeeId equals up1.EmployeeId // Join for Agent Name
                                                    join up2 in _oMTDataContext.UserProfile on sd.TlEmployeeId equals up2.EmployeeId    // Join for Team Lead Name
                                                    where sd.IsActive
                                                          && up1.IsActive
                                                          && up2.IsActive
                                                          && EF.Functions.DateDiffDay(sd.ShiftDate, getShiftAssociation.FromDate) >= 0
                                                          && EF.Functions.DateDiffDay(sd.ShiftDate, getShiftAssociation.ToDate) <= 0
                                                    orderby sd.ShiftDate
                                                    select new ShiftAssociationResponseDTO()
                                                    {
                                                        ShiftAssociationId = sd.ShiftAssociationId,
                                                        AgentName = up1.FirstName + " " + up1.LastName + " (" + up1.EmployeeId + ")",
                                                        TlName = up2.FirstName + " " + up2.LastName + " (" + up2.EmployeeId + ")",
                                                        ShiftDate = sd.ShiftDate,
                                                        ShiftCode = sd.ShiftCode,
                                                    }).ToList();
                }

                else if (getShiftAssociation.AgentEmployeeId != null && getShiftAssociation.TlEmployeeId != null)
                {

                }

                else if (getShiftAssociation.AgentEmployeeId != null && getShiftAssociation.TlEmployeeId == null)
                {

                }
                else if (getShiftAssociation.AgentEmployeeId == null && getShiftAssociation.TlEmployeeId != null)
                {

                }

                if (shiftAssociationResponseDTOs.Count > 0)
                {
                    if (pagination.IsPagination)
                    {
                        var skip = (pagination.PageNo - 1) * pagination.NoOfRecords;
                        var paginatedData = shiftAssociationResponseDTOs.Skip(skip).Take(pagination.NoOfRecords).ToList();
                        var totalRecords = shiftAssociationResponseDTOs.Count;
                        var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.NoOfRecords);

                        var paginationOutput = new PaginationOutputDTO
                        {
                            Records = paginatedData.Cast<object>().ToList(),
                            PageNo = pagination.PageNo,
                            NoOfPages = totalPages,
                            TotalCount = totalRecords,

                        };

                        resultDTO.Data = paginationOutput;
                        resultDTO.IsSuccess = true;
                        resultDTO.Message = "Shift association response has been fetched successfully";
                    }
                    else
                    {
                        resultDTO.IsSuccess = true;
                        resultDTO.Data = shiftAssociationResponseDTOs;
                        resultDTO.Message = "Shift association response has been fetched successfully";
                    }

                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.StatusCode = "404";
                    resultDTO.Message = "List of Shift association Details not found";

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

        public ResultDTO UpdateShiftAssociation(UpdateShiftAssociationDTO updateShiftAssociationDTO,int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                ShiftAssociation? shiftDetails = _oMTDataContext.ShiftAssociation.Where(x => x.ShiftAssociationId == updateShiftAssociationDTO.ShiftAssociationId && x.IsActive).FirstOrDefault();

                if (shiftDetails != null)
                {
                    shiftDetails.TlEmployeeId = updateShiftAssociationDTO.TlEmployeeId;
                    shiftDetails.PrimarySystemOfRecordId = (int)updateShiftAssociationDTO.PrimarySystemOfRecordId;
                    shiftDetails.ShiftCode = updateShiftAssociationDTO.ShiftCode;
                    shiftDetails.ModifiedBy = userid;
                    shiftDetails.ModifiedDate = DateTime.UtcNow;

                    _oMTDataContext.ShiftAssociation.Update(shiftDetails);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Shift association Details updated successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Shift association Details not found";
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

        public ResultDTO DeleteShiftAssociation(int ShiftAssociationId)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                ShiftAssociation? shiftDetails = _oMTDataContext.ShiftAssociation.Where(x => x.ShiftAssociationId == ShiftAssociationId && x.IsActive).FirstOrDefault();

                if (shiftDetails != null)
                {
                    shiftDetails.IsActive = false;

                    _oMTDataContext.ShiftAssociation.Update(shiftDetails);
                    _oMTDataContext.SaveChanges();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Shift association Details deleted successfully";
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Shift association Details not found";
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
