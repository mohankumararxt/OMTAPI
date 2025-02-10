using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Settings;
using OMT.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                        TLEmployeeId = createShiftAssociationDTO.TlEmployeeId,
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
                                                    join up1 in _oMTDataContext.UserProfile on sd.AgentEmployeeId equals up1.EmployeeId
                                                    join up2 in _oMTDataContext.UserProfile on sd.TLEmployeeId equals up2.EmployeeId
                                                    join ss in _oMTDataContext.SystemofRecord on sd.PrimarySystemOfRecordId equals ss.SystemofRecordId
                                                    where sd.IsActive
                                                          && up1.IsActive
                                                          && up2.IsActive
                                                          && sd.ShiftDate.Date >= getShiftAssociation.FromDate.Date
                                                          && sd.ShiftDate.Date <= getShiftAssociation.ToDate.Date
                                                    orderby up1.EmployeeId, sd.ShiftDate
                                                    group new { sd, up2, ss } by new
                                                    {
                                                        AgentName = up1.FirstName + " " + up1.LastName + " (" + up1.EmployeeId + ")"
                                                    } into groupedData
                                                    select new ShiftAssociationResponseDTO
                                                    {
                                                        AgentName = groupedData.Key.AgentName,
                                                        ShiftDetails = groupedData.Select(g => new ShiftDTO
                                                        {
                                                            PrimarySystemOfRecordId = g.sd.PrimarySystemOfRecordId,
                                                            PrimarySystemOfRecordName = g.ss.SystemofRecordName,
                                                            ShiftAssociationId = g.sd.ShiftAssociationId,
                                                            ShiftCode = g.sd.ShiftCode,
                                                            ShiftDate = g.sd.ShiftDate.ToString("MM/dd/yyyy"),
                                                            TlName = g.up2.FirstName + " " + g.up2.LastName + " (" + g.up2.EmployeeId + ")"
                                                        }).ToList()
                                                    }).ToList();
                }
                else if (getShiftAssociation.AgentEmployeeId != null && getShiftAssociation.TlEmployeeId != null)
                {
                    shiftAssociationResponseDTOs = (from sd in _oMTDataContext.ShiftAssociation
                                                    join up1 in _oMTDataContext.UserProfile on sd.AgentEmployeeId equals up1.EmployeeId
                                                    join up2 in _oMTDataContext.UserProfile on sd.TLEmployeeId equals up2.EmployeeId
                                                    join ss in _oMTDataContext.SystemofRecord on sd.PrimarySystemOfRecordId equals ss.SystemofRecordId
                                                    where sd.IsActive
                                                          && up1.IsActive && up1.EmployeeId == getShiftAssociation.AgentEmployeeId
                                                          && up2.IsActive && up2.EmployeeId == getShiftAssociation.TlEmployeeId
                                                          && sd.ShiftDate.Date >= getShiftAssociation.FromDate.Date
                                                          && sd.ShiftDate.Date <= getShiftAssociation.ToDate.Date
                                                    orderby up1.EmployeeId, sd.ShiftDate
                                                    group new { sd, up2, ss } by new
                                                    {
                                                        AgentName = up1.FirstName + " " + up1.LastName + " (" + up1.EmployeeId + ")"
                                                    } into groupedData
                                                    select new ShiftAssociationResponseDTO
                                                    {
                                                        AgentName = groupedData.Key.AgentName,
                                                        ShiftDetails = groupedData.Select(g => new ShiftDTO
                                                        {
                                                            PrimarySystemOfRecordId = g.sd.PrimarySystemOfRecordId,
                                                            PrimarySystemOfRecordName = g.ss.SystemofRecordName,
                                                            ShiftAssociationId = g.sd.ShiftAssociationId,
                                                            ShiftCode = g.sd.ShiftCode,
                                                            ShiftDate = g.sd.ShiftDate.ToString("MM/dd/yyyy"),
                                                            TlName = g.up2.FirstName + " " + g.up2.LastName + " (" + g.up2.EmployeeId + ")"
                                                        }).ToList()
                                                    }).ToList();
                }

                else if (getShiftAssociation.AgentEmployeeId != null && getShiftAssociation.TlEmployeeId == null)
                {
                    shiftAssociationResponseDTOs = (from sd in _oMTDataContext.ShiftAssociation
                                                    join up1 in _oMTDataContext.UserProfile on sd.AgentEmployeeId equals up1.EmployeeId
                                                    join up2 in _oMTDataContext.UserProfile on sd.TLEmployeeId equals up2.EmployeeId
                                                    join ss in _oMTDataContext.SystemofRecord on sd.PrimarySystemOfRecordId equals ss.SystemofRecordId
                                                    where sd.IsActive
                                                          && up1.IsActive && up1.EmployeeId == getShiftAssociation.AgentEmployeeId
                                                          && up2.IsActive
                                                          && sd.ShiftDate.Date >= getShiftAssociation.FromDate.Date
                                                          && sd.ShiftDate.Date <= getShiftAssociation.ToDate.Date
                                                    orderby up1.EmployeeId, sd.ShiftDate
                                                    group new { sd, up2, ss } by new
                                                    {
                                                        AgentName = up1.FirstName + " " + up1.LastName + " (" + up1.EmployeeId + ")"
                                                    } into groupedData
                                                    select new ShiftAssociationResponseDTO
                                                    {
                                                        AgentName = groupedData.Key.AgentName,
                                                        ShiftDetails = groupedData.Select(g => new ShiftDTO
                                                        {
                                                            PrimarySystemOfRecordId = g.sd.PrimarySystemOfRecordId,
                                                            PrimarySystemOfRecordName = g.ss.SystemofRecordName,
                                                            ShiftAssociationId = g.sd.ShiftAssociationId,
                                                            ShiftCode = g.sd.ShiftCode,
                                                            ShiftDate = g.sd.ShiftDate.ToString("MM/dd/yyyy"),
                                                            TlName = g.up2.FirstName + " " + g.up2.LastName + " (" + g.up2.EmployeeId + ")"
                                                        }).ToList()
                                                    }).ToList();
                }
                else if (getShiftAssociation.AgentEmployeeId == null && getShiftAssociation.TlEmployeeId != null)
                {
                    shiftAssociationResponseDTOs = (from sd in _oMTDataContext.ShiftAssociation
                                                    join up1 in _oMTDataContext.UserProfile on sd.AgentEmployeeId equals up1.EmployeeId
                                                    join up2 in _oMTDataContext.UserProfile on sd.TLEmployeeId equals up2.EmployeeId
                                                    join ss in _oMTDataContext.SystemofRecord on sd.PrimarySystemOfRecordId equals ss.SystemofRecordId
                                                    where sd.IsActive
                                                          && up1.IsActive
                                                          && up2.IsActive && up2.EmployeeId == getShiftAssociation.TlEmployeeId
                                                          && sd.ShiftDate.Date >= getShiftAssociation.FromDate.Date
                                                          && sd.ShiftDate.Date <= getShiftAssociation.ToDate.Date
                                                    orderby up1.EmployeeId, sd.ShiftDate
                                                    group new { sd, up2, ss } by new
                                                    {
                                                        AgentName = up1.FirstName + " " + up1.LastName + " (" + up1.EmployeeId + ")"
                                                    } into groupedData
                                                    select new ShiftAssociationResponseDTO
                                                    {
                                                        AgentName = groupedData.Key.AgentName,
                                                        ShiftDetails = groupedData.Select(g => new ShiftDTO
                                                        {
                                                            PrimarySystemOfRecordId = g.sd.PrimarySystemOfRecordId,
                                                            PrimarySystemOfRecordName = g.ss.SystemofRecordName,
                                                            ShiftAssociationId = g.sd.ShiftAssociationId,
                                                            ShiftCode = g.sd.ShiftCode,
                                                            ShiftDate = g.sd.ShiftDate.ToString("MM/dd/yyyy"),
                                                            TlName = g.up2.FirstName + " " + g.up2.LastName + " (" + g.up2.EmployeeId + ")"
                                                        }).ToList()
                                                    }).ToList();
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

        public ResultDTO UpdateShiftAssociation(UpdateShiftAssociationDTO updateShiftAssociationDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };

            try
            {
                ShiftAssociation? shiftDetails = _oMTDataContext.ShiftAssociation.Where(x => x.ShiftAssociationId == updateShiftAssociationDTO.ShiftAssociationId && x.IsActive).FirstOrDefault();

                if (shiftDetails != null)
                {
                    shiftDetails.TLEmployeeId = updateShiftAssociationDTO.TlEmployeeId;
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

        public ResultDTO UploadShiftAssociationDetails(UploadShiftAssociationDetailsDTO uploadShiftAssociationDetailsDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);

                using SqlCommand command = new()
                {
                    Connection = connection,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "UploadShiftDetails"
                };

                command.Parameters.AddWithValue("@jsonData", uploadShiftAssociationDetailsDTO.JsonData);
                command.Parameters.AddWithValue("@UserId", userid);

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
                    throw new InvalidOperationException("Something went wrong while uploading the shift association details,please check the shift details.");
                }

                resultDTO.IsSuccess = true;
                resultDTO.Message = "Shift association details uploaded successfully";
                resultDTO.StatusCode = "200";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO DownloadShiftDetailsTemplate(DownloadShiftDetailsTemplateDTO downloadShiftDetailsTemplateDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string? connectionstring = _oMTDataContext.Database.GetConnectionString();
                using SqlConnection connection = new(connectionstring);

                List<ShiftAssociation> templateList = new List<ShiftAssociation>();
                var tableName = "ShiftAssociation";
                var dynamiclist = string.Empty;

                var ExcludedColumns = new List<string>()
                {
                    "ShiftAssociationId","IsActive","CreatedDate","ModifiedDate","CreatedBy","ModifiedBy","ShiftCode","ShiftDate"
                };

                string GetDynamicColumnssql = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@Tablename";
                using (SqlCommand Colcommand = new SqlCommand(GetDynamicColumnssql, connection))
                {
                    Colcommand.Parameters.AddWithValue("@Tablename", tableName);
                    var Columns = new List<string>();

                    using SqlDataAdapter dataAdapter = new(Colcommand);
                    DataSet dataset = new DataSet();
                    dataAdapter.Fill(dataset);

                    if (dataset != null && dataset.Tables.Count > 0 && dataset.Tables[0].Rows.Count > 0)
                    {
                        DataTable datatable = dataset.Tables[0];

                        dynamiclist = string.Join(",", datatable.AsEnumerable()
                                       .Select(row => row["COLUMN_NAME"].ToString())
                                       .Where(columnName => !ExcludedColumns.Contains(columnName)));

                        resultDTO.Message = "List of Order Details";
                        resultDTO.IsSuccess = true;
                    }

                }

                // generate range of dates to be included in excel
                List<string> dates = new List<string>();

                DateTime fromDate = downloadShiftDetailsTemplateDTO.FromDate;
                DateTime toDate = downloadShiftDetailsTemplateDTO.ToDate;

                dates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                                       .Select(i => fromDate.AddDays(i).ToString("yyyy-MM-dd"))
                                       .ToList();

                dynamiclist = dynamiclist + "," + string.Join(",", dates.Select(d => d.ToString()));

                resultDTO.Data = dynamiclist;
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
