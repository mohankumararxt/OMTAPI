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
                                                                              ShiftTime = sd.ShiftStartTime.ToString(@"hh\:mm") + "-" + sd.ShiftEndTime.ToString(@"hh\:mm"),
                                                                              ShiftDays = sd.ShiftDays,

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

                if(shiftDetails != null)
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
    }
}
