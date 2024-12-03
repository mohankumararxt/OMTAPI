using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DataService.Settings;
using OMT.DTO;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OMT.DataService.Service
{
    public class SkillSetService : ISkillSetService
    {
        private readonly OMTDataContext _oMTDataContext;

        private readonly IOptions<EmailDetailsSettings> _emailDetailsSettings;
        private readonly IConfiguration _configuration;
        public SkillSetService(OMTDataContext oMTDataContext, IOptions<EmailDetailsSettings> emailDetailsSettings, IConfiguration configuration)
        {
            _oMTDataContext = oMTDataContext;
            _emailDetailsSettings = emailDetailsSettings;
            _configuration = configuration;
        }
        public ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                string existingSkillSetName = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillSetCreateDTO.SkillSetName && x.SystemofRecordId == skillSetCreateDTO.SystemofRecordId && x.IsActive).Select(_ => _.SkillSetName).FirstOrDefault();
                if (existingSkillSetName != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The SkillSet already exists. Please try to add different SkillSet.";
                }
                else
                {
                    bool isHardState = skillSetCreateDTO.HardstateNames != null && skillSetCreateDTO.HardstateNames.Any();
                    SkillSet skillSet = new SkillSet()
                    {
                        SystemofRecordId = skillSetCreateDTO.SystemofRecordId,
                        SkillSetName = skillSetCreateDTO.SkillSetName,
                        Threshold = skillSetCreateDTO.Threshold,
                        IsHardState = skillSetCreateDTO.IsHardState,
                        IsActive = true
                    };
                    _oMTDataContext.SkillSet.Add(skillSet);
                    _oMTDataContext.SaveChanges();

                    // add harstatenames for skillset
                    var skillsetid = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillSetCreateDTO.SkillSetName && x.IsActive).Select(_ => _.SkillSetId).FirstOrDefault();

                    if (isHardState)
                    {
                        foreach (var item in skillSetCreateDTO.HardstateNames)
                        {
                            SkillSetHardStates skillSetHardStates = new SkillSetHardStates()
                            {
                                SkillSetId = skillsetid,
                                StateName = item,
                                IsActive = true,
                                CreatedDate = DateTime.Now,
                            };
                            _oMTDataContext.SkillSetHardStates.Add(skillSetHardStates);
                            _oMTDataContext.SaveChanges();
                        }
                    }

                }
                // send details via mail
                var url = _emailDetailsSettings.Value.SendEmailURL;

                DateTime uploadeddate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                string skillsetname = _oMTDataContext.SkillSet.Where(x => x.SkillSetName == skillSetCreateDTO.SkillSetName).Select(x => x.SkillSetName).FirstOrDefault();
                string username = _oMTDataContext.UserProfile.Where(x => x.UserId == userid).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();


                IConfigurationSection toEmailId = _configuration.GetSection("EmailConfig:UploadorderAPIdetails:ToEmailId");

                List<string> toEmailIds1 = toEmailId.AsEnumerable()
                                                          .Where(c => !string.IsNullOrEmpty(c.Value))
                                                          .Select(c => c.Value)
                                                          .ToList();

                var skillsetdetails = $"New Skillset named '{skillsetname}' created by {username} at {uploadeddate}.Please Configure the required Invoice details in OMT as soon as possible before processing begins.";

                SendEmailDTO sendEmailDTO1 = new SendEmailDTO
                {
                    ToEmailIds = toEmailIds1,
                    Subject = $"Update Invoice details in OMT for newly added skillset - {skillsetname}",
                    Body = skillsetdetails,
                };
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailDTO1);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var webApiUrl = new Uri(url);
                        var response = client.PostAsync(webApiUrl, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = response.Content.ReadAsStringAsync().Result;

                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                resultDTO.Message = "SkillSet created successfully";
                resultDTO.StatusCode = "200";
                resultDTO.IsSuccess = true;

            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO DeleteSkillSet(int skillsetId, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                SkillSet? skillSet = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == skillsetId && x.IsActive).FirstOrDefault();
                if (skillSet == null)
                {
                    resultDTO.StatusCode = "404";
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill Set is not found";
                }
                else
                {
                    skillSet.IsActive = false;
                    _oMTDataContext.SkillSet.Update(skillSet);
                    _oMTDataContext.SaveChanges();

                    // delete userskillsets associated with this skillset

                    var us = _oMTDataContext.UserSkillSet.Where(x => x.SkillSetId == skillsetId && x.IsActive).ToList();

                    foreach (var u in us)
                    {
                        u.IsActive = false;
                        _oMTDataContext.UserSkillSet.Update(u);
                    }

                    _oMTDataContext.SaveChanges();

                    //delete template

                    List<TemplateColumns> columns = _oMTDataContext.TemplateColumns.Where(x => x.SkillSetId == skillSet.SkillSetId).ToList();

                    _oMTDataContext.TemplateColumns.RemoveRange(columns);
                    _oMTDataContext.SaveChanges();

                    string? connectionstring = _oMTDataContext.Database.GetConnectionString();

                    using SqlConnection connection = new(connectionstring);
                    using SqlCommand command = new()
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = @"
                                       IF OBJECT_ID(@TableName, 'U') IS NOT NULL
                                       BEGIN
                                           DROP TABLE [" + skillSet.SkillSetName + @"]
                                       END"
                    };
                    command.Parameters.AddWithValue("@TableName", skillSet.SkillSetName);

                    connection.Open();
                    command.ExecuteNonQuery();

                    // delete timeline associated with the skillset

                    var tl = _oMTDataContext.Timeline.Where(x => x.SkillSetId == skillSet.SkillSetId && x.IsActive).ToList();

                    foreach (var t in tl)
                    {
                        t.IsActive = false;
                        _oMTDataContext.Timeline.Update(t);
                    }

                    _oMTDataContext.SaveChanges();

                    // delete SkillSetHardStates of this skillset

                    var sshs = _oMTDataContext.SkillSetHardStates.Where(x => x.IsActive && x.SkillSetId == skillSet.SkillSetId).ToList();

                    foreach (var ss in sshs)
                    {
                        ss.IsActive = false;
                        _oMTDataContext.SkillSetHardStates.Update(ss);
                    }
                    _oMTDataContext.SaveChanges();

                    // delete details from joint tables

                    var ijr = _oMTDataContext.InvoiceJointResware.Where(x => x.SkillSetId == skillSet.SkillSetId).FirstOrDefault();

                    if (ijr != null)
                    {
                        _oMTDataContext.InvoiceJointResware.Remove(ijr);
                        _oMTDataContext.SaveChanges();
                    }

                    var ijs = _oMTDataContext.InvoiceJointSci.Where(x => x.SkillSetId == skillSet.SkillSetId).FirstOrDefault();

                    if (ijs != null)
                    {
                        _oMTDataContext.InvoiceJointSci.Remove(ijs);
                        _oMTDataContext.SaveChanges();
                    }

                    // send details via mail
                    var url = _emailDetailsSettings.Value.SendEmailURL;

                    DateTime uploadeddate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                    string skillsetname = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == skillsetId).Select(x => x.SkillSetName).FirstOrDefault();
                    string username = _oMTDataContext.UserProfile.Where(x => x.UserId == userid).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();


                    IConfigurationSection toEmailId = _configuration.GetSection("EmailConfig:UploadorderAPIdetails:ToEmailId");

                    List<string> toEmailIds1 = toEmailId.AsEnumerable()
                                                              .Where(c => !string.IsNullOrEmpty(c.Value))
                                                              .Select(c => c.Value)
                                                              .ToList();

                    var message = $"{username} has deleted the skillset named '{skillsetname}' along with all its related details at {uploadeddate}.";

                    SendEmailDTO sendEmailDTO1 = new SendEmailDTO
                    {
                        ToEmailIds = toEmailIds1,
                        Subject = "Skillset has been Deleted",
                        Body = message,
                    };
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailDTO1);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var webApiUrl = new Uri(url);
                            var response = client.PostAsync(webApiUrl, content).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = response.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                    var rpm = _oMTDataContext.ResWareProductDescriptionMap.Where(x => x.SkillSetId == skillSet.SkillSetId).ToList();

                    if (rpm.Any())
                    {
                        _oMTDataContext.ResWareProductDescriptionMap.RemoveRange(rpm);
                        _oMTDataContext.SaveChanges();
                    }

                    //delete from reportcolumns table
                    var rc = _oMTDataContext.ReportColumns.Where(x => x.SkillSetId == skillSet.SkillSetId).ToList();

                    foreach (var u in rc)
                    {
                        u.IsActive = false;
                        _oMTDataContext.ReportColumns.Update(u);
                    }

                    _oMTDataContext.SaveChanges();

                    resultDTO.Message = "Skill Set has been deleted successfully";
                    resultDTO.StatusCode = "200";
                    resultDTO.IsSuccess = true;
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

        public ResultDTO GetSkillSetList(int? skillsetid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                if (skillsetid == null)
                {
                    var skillSetGroups = (from ss in _oMTDataContext.SkillSet
                                          join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                          join hs in _oMTDataContext.SkillSetHardStates on ss.SkillSetId equals hs.SkillSetId into hsGroup
                                          from hs in hsGroup.DefaultIfEmpty()  // Left join to include all skill sets
                                          where sor.IsActive && ss.IsActive
                                          group new { ss, sor, hs } by new  //Group the data's
                                          {
                                              ss.SkillSetId,
                                              ss.SkillSetName,
                                              ss.Threshold,
                                              sor.SystemofRecordName,
                                              ss.SystemofRecordId,
                                              ss.IsHardState
                                          } into grp  //here grp is the grouping key
                                          select new SkillSetResponseDTO
                                          {
                                              SkillSetId = grp.Key.SkillSetId,
                                              SkillSetName = grp.Key.SkillSetName,
                                              Threshold = grp.Key.Threshold,
                                              SystemofRecordName = grp.Key.SystemofRecordName,
                                              SystemofRecordId = grp.Key.SystemofRecordId,
                                              IsHardState = grp.Any(x => x.hs != null && x.hs.IsActive), //Isactive only
                                              StateName = grp
                                                            .Where(x => x.hs != null && x.hs.IsActive)
                                                            .Select(x => x.hs.StateName)
                                                            .ToArray()  //Isactive only
                                          })
                                          .OrderBy(x => x.SystemofRecordName)
                                          .ThenBy(x => x.SkillSetName)//ordering here Bcoz we have used Grouping key
                                          .ToList();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of SkillSets";
                    resultDTO.Data = skillSetGroups;
                }

                else
                {
                    var skillSetGroups = (from ss in _oMTDataContext.SkillSet
                                          join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                          join hs in _oMTDataContext.SkillSetHardStates on ss.SkillSetId equals hs.SkillSetId into hsGroup
                                          from hs in hsGroup.DefaultIfEmpty()
                                          where sor.IsActive && ss.IsActive && ss.SkillSetId == skillsetid
                                          group new { ss, sor, hs } by new
                                          {
                                              ss.SkillSetId,
                                              ss.SkillSetName,
                                              ss.Threshold,
                                              sor.SystemofRecordName,
                                              ss.SystemofRecordId,
                                              ss.IsHardState
                                          } into grp
                                          select new SkillSetResponseDTO
                                          {
                                              SkillSetId = grp.Key.SkillSetId,
                                              SkillSetName = grp.Key.SkillSetName,
                                              Threshold = grp.Key.Threshold,
                                              SystemofRecordName = grp.Key.SystemofRecordName,
                                              SystemofRecordId = grp.Key.SystemofRecordId,
                                              IsHardState = grp.Any(x => x.hs != null && x.hs.IsActive), //Isactive only
                                              StateName = grp
                                                            .Where(x => x.hs != null && x.hs.IsActive)
                                                            .Select(x => x.hs.StateName)
                                                            .ToArray()  //Isactive only
                                          })
                                         .OrderBy(x => x.SkillSetId)
                                         .ThenBy(x => x.SkillSetName)
                                         .ToList();

                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "SkillSet Details Fetched Successfully";
                    resultDTO.Data = skillSetGroups;

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

        public ResultDTO GetSkillSetListBySORId(int sorid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var skillSetGroups = (from ss in _oMTDataContext.SkillSet
                                      join sor in _oMTDataContext.SystemofRecord on ss.SystemofRecordId equals sor.SystemofRecordId
                                      join hs in _oMTDataContext.SkillSetHardStates on ss.SkillSetId equals hs.SkillSetId into hsGroup
                                      from hs in hsGroup.DefaultIfEmpty()
                                      where sor.IsActive && ss.IsActive && sor.SystemofRecordId == sorid // Filter by the specific SOR ID only Actives
                                      group new { ss, sor, hs } by new
                                      {
                                          ss.SkillSetId,
                                          ss.SkillSetName,
                                          ss.Threshold,
                                          sor.SystemofRecordName,
                                          ss.SystemofRecordId,
                                          ss.IsHardState
                                      } into grp
                                      select new SkillSetResponseDTO
                                      {
                                          SkillSetId = grp.Key.SkillSetId,
                                          SkillSetName = grp.Key.SkillSetName,
                                          Threshold = grp.Key.Threshold,
                                          SystemofRecordName = grp.Key.SystemofRecordName,
                                          SystemofRecordId = grp.Key.SystemofRecordId,
                                          IsHardState = grp.Any(x => x.hs != null && x.hs.IsActive), //null  Isactive 
                                          StateName = grp
                                                            .Where(x => x.hs != null && x.hs.IsActive)
                                                            .Select(x => x.hs.StateName)
                                                            .ToArray()  //Isactive only
                                      })
                                      .OrderBy(x => x.SystemofRecordId)
                                      .ThenBy(x => x.SkillSetName)
                                      .ToList();

                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of SkillSets";
                resultDTO.Data = skillSetGroups;
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }

        public ResultDTO GetStatenameList(int skillsetid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                List<HardStatenameDTO> hardStatename = (from shs in _oMTDataContext.SkillSetHardStates
                                                        join ss in _oMTDataContext.SkillSet on shs.SkillSetId equals ss.SkillSetId
                                                        where shs.SkillSetId == skillsetid && shs.IsActive && ss.IsActive
                                                        select new HardStatenameDTO
                                                        {
                                                            //SkillSetId = ss.SkillSetId,
                                                            //SkillSetName = ss.SkillSetName,
                                                            HardstateName = shs.StateName,
                                                        }).ToList();

                if (hardStatename.Count > 0)
                {
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Statenames";
                    resultDTO.Data = hardStatename;
                }
                else
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Hardstate Names not found for the given SystemofRecordId";
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

        public ResultDTO UpdateSkillSet(SkillSetUpdateDTO skillSetUpdateDTO, int userid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                // Find the existing skillset by SkillSetId
                SkillSet? skillset = _oMTDataContext.SkillSet.Find(skillSetUpdateDTO.SkillSetId);
                if (skillset == null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "Skill set not found.";
                    resultDTO.StatusCode = "404";
                    return resultDTO;
                }
                else
                {
                    bool IsThresholdChanged = false;

                    if (skillset.Threshold != skillSetUpdateDTO.Threshold)
                    {
                        IsThresholdChanged = true;
                    }

                    //Old Details
                    var Olddetails = new
                    {
                        Threshold = skillset.Threshold,
                        IsHardState = skillset.IsHardState,
                        HardStates = _oMTDataContext.SkillSetHardStates.Where(hs => hs.SkillSetId == skillSetUpdateDTO.SkillSetId && hs.IsActive)
                                     .Select(hs => hs.StateName).ToList()
                    };

                    // Updating Skillset Details
                    skillset.Threshold = skillSetUpdateDTO.Threshold;
                    skillset.IsHardState = skillSetUpdateDTO.IsHardState;
                    skillset.IsActive = true;
                    _oMTDataContext.SkillSet.Update(skillset);
                    _oMTDataContext.SaveChanges();

                    if (IsThresholdChanged)
                    {
                        // call UpdateGocTable to update getordercaltable
                        UpdateGocTable(resultDTO, skillSetUpdateDTO.SkillSetId, skillSetUpdateDTO.Threshold);
                    }

                    if (skillSetUpdateDTO.IsHardState == true && (skillSetUpdateDTO.StateName != null || skillSetUpdateDTO.StateName.Any()))
                    {
                        // Fetch all hard states (active or inactive) 
                        var existingHardStates = _oMTDataContext.SkillSetHardStates.Where(h => h.SkillSetId == skillSetUpdateDTO.SkillSetId).ToList();

                        //Disable Hardstates
                        foreach (var hardState in existingHardStates)
                        {
                            hardState.IsActive = false;
                            _oMTDataContext.SkillSetHardStates.Update(hardState);
                            _oMTDataContext.SaveChanges();
                        }

                        //Activate  Hardstates
                        foreach (var stateName in skillSetUpdateDTO.StateName)
                        {
                            var existingHardState = existingHardStates.FirstOrDefault(h => h.StateName == stateName);

                            if (existingHardState != null) // Reactivate 
                            {
                                existingHardState.IsActive = true;
                                _oMTDataContext.SkillSetHardStates.Update(existingHardState);
                                _oMTDataContext.SaveChanges();
                            }
                            else // Add New Hardstate
                            {
                                SkillSetHardStates newHardState = new SkillSetHardStates()
                                {
                                    SkillSetId = skillSetUpdateDTO.SkillSetId, // Use the Same SkillSetId used above
                                    StateName = stateName,
                                    IsActive = true,
                                    CreatedDate = DateTime.Now
                                };
                                _oMTDataContext.SkillSetHardStates.Add(newHardState);
                                _oMTDataContext.SaveChanges();
                            }
                        }
                    }
                    else if (skillSetUpdateDTO.IsHardState == false && (skillSetUpdateDTO.StateName == null || !skillSetUpdateDTO.StateName.Any()))
                    {
                        // Disable all hard states if IsHardState is false
                        var existingHardStates = _oMTDataContext.SkillSetHardStates.Where(h => h.SkillSetId == skillSetUpdateDTO.SkillSetId && h.IsActive).ToList();

                        foreach (var hardState in existingHardStates)
                        {
                            hardState.IsActive = false;
                            _oMTDataContext.SkillSetHardStates.Update(hardState);
                            _oMTDataContext.SaveChanges();
                        }
                    }

                    //send details via mail
                    var url = _emailDetailsSettings.Value.SendEmailURL;

                    DateTime uploadeddate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                    string skillsetname = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == skillSetUpdateDTO.SkillSetId).Select(x => x.SkillSetName).FirstOrDefault();
                    string username = _oMTDataContext.UserProfile.Where(x => x.UserId == userid).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                    IConfigurationSection toEmailId = _configuration.GetSection("EmailConfig:UploadorderAPIdetails:toEmailId");

                    List<string> toEmailIds = toEmailId.AsEnumerable()
                                            .Where(c => !string.IsNullOrEmpty(c.Value))
                                            .Select(c => c.Value)
                                            .ToList();

                    //New Details
                    var Newdetails = new
                    {
                        Threshold = skillSetUpdateDTO.Threshold,
                        IsHardState = skillSetUpdateDTO.IsHardState,
                        HardStates = skillSetUpdateDTO.StateName ?? new List<string>()
                    };

                    var changes = new List<string>();

                    //Compare Changes
                    if (Olddetails.Threshold != Newdetails.Threshold)
                    {
                        changes.Add($"Threshold Updated from {Olddetails.Threshold} to {Newdetails.Threshold}");
                    }
                    if (Olddetails.IsHardState != Newdetails.IsHardState)
                    {
                        changes.Add($"IsHardState Updated from {Olddetails.IsHardState} to {Newdetails.IsHardState}");
                    }
                    if (Olddetails.HardStates != Newdetails.HardStates)
                    {
                        var oldHS = string.Join(", ", Olddetails.HardStates);
                        var newHS = string.Join(", ", Newdetails.HardStates);
                        changes.Add($"HardStates Updated from {oldHS} to {newHS}");
                    }

                    //changes message
                    var changesmessage = changes.Any() ? $"{string.Join(", ", changes)}" : "No Changes Were Made.";

                    //Email body
                    var message = $"{username} has Updated the Skillset named '{skillsetname}' at {uploadeddate}. " + $"{changesmessage}";

                    //Send Email
                    SendEmailDTO sendEmailDTO = new SendEmailDTO
                    {
                        ToEmailIds = toEmailIds,
                        Subject = "SkillsetUpdated Successfully",
                        Body = message,
                    };

                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(sendEmailDTO);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var webApiUrl = new Uri(url);
                            var response = client.PostAsync(webApiUrl, content).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = response.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    resultDTO.IsSuccess = true;
                    resultDTO.StatusCode = "200";
                    resultDTO.Message = "SkillSet Updated Successfully";
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

        private void UpdateGocTable(ResultDTO resultDTO, int skillsetid, int threshold)
        {
            try
            {
                var UserWithSkillSet = _oMTDataContext.GetOrderCalculation.Where(x => x.SkillSetId == skillsetid && x.IsActive && x.IsCycle1).ToList();

                if (UserWithSkillSet.Count > 0)
                {
                    foreach (var uss in UserWithSkillSet)
                    {
                        // var skillset = _oMTDataContext.SkillSet.Where(x => x.SkillSetId == uss.SkillSetId && x.IsActive).FirstOrDefault();

                        double totalorders = ((double)uss.Weightage / 100) * threshold;

                        int roundedtotalorders = totalorders == 0 ? 0 : (totalorders > 0 && totalorders < 1) ? 1 : (int)Math.Round(totalorders, MidpointRounding.AwayFromZero);

                        if (uss.OrdersCompleted != roundedtotalorders && uss.OrdersCompleted < roundedtotalorders && uss.IsCycle1)
                        {
                            uss.Utilized = false;
                        }
                        else if (uss.OrdersCompleted >= roundedtotalorders && uss.IsCycle1)
                        {
                            uss.Utilized = true;
                        }

                        uss.TotalOrderstoComplete = roundedtotalorders;

                        _oMTDataContext.GetOrderCalculation.Update(uss);
                        _oMTDataContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
        }
        public ResultDTO CreateTimeLine(SkillSetTimeLineDTO skillSetTimeLineDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                var existingSkillSet = _oMTDataContext.Timeline.Where(x => x.SkillSetId == skillSetTimeLineDTO.SkillSetId && x.IsActive).FirstOrDefault();
                if (existingSkillSet != null)
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "The SkillSet already has Timeline Details.";
                }
                else
                {
                    foreach (var detail in skillSetTimeLineDTO.HardStateTimelineDetails)
                    {
                        Timeline timeline = new Timeline
                        {
                            SkillSetId = skillSetTimeLineDTO.SkillSetId,
                            Hardstatename = detail.HardStateName,
                            ExceedTime = detail.ExceedTime,
                            IsHardState = true,
                            IsActive = true
                        };
                        _oMTDataContext.Timeline.Add(timeline);
                        _oMTDataContext.SaveChanges();
                    }
                    foreach (var details in skillSetTimeLineDTO.NormalStateTimelineDetails)
                    {
                        Timeline timeline2 = new Timeline
                        {
                            SkillSetId = skillSetTimeLineDTO.SkillSetId,
                            Hardstatename = details.HardStateName,
                            ExceedTime = details.ExceedTime,
                            IsHardState = false,
                            IsActive = true
                        };
                        _oMTDataContext.Timeline.Add(timeline2);
                        _oMTDataContext.SaveChanges();
                    }
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "Timeline Added Successfully";
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

        public ResultDTO UpdateTimeLine(SkillSetUpdateTimeLineDTO skillSetUpdateTimeLineDTO)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "201" };
            try
            {
                var existingTimelines = _oMTDataContext.Timeline.Where(t => t.SkillSetId == skillSetUpdateTimeLineDTO.SkillSetId).ToList();

                // Disable 
                var existingHardStates = existingTimelines.Where(t => t.IsHardState).ToList();
                foreach (var hardState in existingHardStates)
                {
                    hardState.IsActive = false;
                    _oMTDataContext.Timeline.Update(hardState);
                    _oMTDataContext.SaveChanges();
                }
                foreach (var hardStateDetail in skillSetUpdateTimeLineDTO.HardStateTimelineDetails)
                {
                    var existingTimeline = existingHardStates.FirstOrDefault(t => t.Hardstatename == hardStateDetail.HardStateName);

                    if (existingTimeline != null)
                    {
                        // Activate 
                        existingTimeline.ExceedTime = hardStateDetail.ExceedTime;
                        existingTimeline.IsActive = true;
                        _oMTDataContext.Timeline.Update(existingTimeline);
                        _oMTDataContext.SaveChanges();
                    }
                    else
                    {
                        // Add  
                        Timeline newTimeline = new Timeline()
                        {
                            SkillSetId = skillSetUpdateTimeLineDTO.SkillSetId,
                            Hardstatename = hardStateDetail.HardStateName,
                            ExceedTime = hardStateDetail.ExceedTime,
                            IsHardState = true,
                            IsActive = true
                        };
                        _oMTDataContext.Timeline.Add(newTimeline);
                        _oMTDataContext.SaveChanges();
                    }
                }
                //Normalstate  
                var existingNormalState = _oMTDataContext.Timeline.Where(t => t.SkillSetId == skillSetUpdateTimeLineDTO.SkillSetId && t.IsHardState == false && t.IsActive).FirstOrDefault();

                foreach (var normalStateDetail in skillSetUpdateTimeLineDTO.NormalStateTimelineDetails)
                {
                    if (existingNormalState != null)
                    {

                        existingNormalState.ExceedTime = normalStateDetail.ExceedTime;
                        _oMTDataContext.Timeline.Update(existingNormalState);
                        _oMTDataContext.SaveChanges();
                    }
                }
                resultDTO.IsSuccess = true;
                resultDTO.Message = "Timeline Details Updated Successfully";
            }
            catch (Exception ex)
            {
                resultDTO.IsSuccess = false;
                resultDTO.StatusCode = "500";
                resultDTO.Message = ex.Message;
            }
            return resultDTO;
        }
        public ResultDTO GetSkillSetTimelineList(int? skillsetid)
        {
            ResultDTO resultDTO = new ResultDTO() { IsSuccess = true, StatusCode = "200" };
            try
            {
                var skillsetTimeline = (from ss in _oMTDataContext.SkillSet
                                        join tl in _oMTDataContext.Timeline on ss.SkillSetId equals tl.SkillSetId
                                        where ss.IsActive && tl.IsActive
                                        select new
                                        {
                                            ss.SkillSetId,
                                            ss.SkillSetName,
                                            tl.TimelineId,
                                            tl.Hardstatename,
                                            tl.ExceedTime,
                                            tl.IsHardState,
                                        }).ToList();

                if (skillsetid != null)
                {
                    skillsetTimeline = skillsetTimeline.Where(t => t.SkillSetId == skillsetid.Value).ToList();
                }

                //group by Skillsetid
                var groupedTimelines = skillsetTimeline
                                      .GroupBy(t => new { t.SkillSetId, t.SkillSetName })
                                      .Select(g => new SkillSetTimelineResponseDTO
                                      {
                                          SkillSetId = g.Key.SkillSetId,
                                          SkillSetName = g.Key.SkillSetName,
                                          HardStateTimelineDetails = g.Where(t => t.IsHardState)
                                                                    .Select(t => new ResponseTimelineDetailDTO
                                                                    {
                                                                        HardStateName = t.Hardstatename,
                                                                        ExceedTime = t.ExceedTime,
                                                                        IsHardstate = t.IsHardState,
                                                                    }).ToList(),
                                          NormalStateTimelineDetails = g.Where(t => !t.IsHardState)
                                                                      .Select(t => new ResponseTimelineDetailDTO
                                                                      {
                                                                          HardStateName = t.Hardstatename,
                                                                          ExceedTime = t.ExceedTime,
                                                                          IsHardstate = t.IsHardState,
                                                                      }).ToList()
                                      }).ToList();

                if (!groupedTimelines.Any())
                {
                    resultDTO.IsSuccess = false;
                    resultDTO.Message = "No timeline details found for this Skillsetid";
                    return resultDTO;
                }

                resultDTO.Data = groupedTimelines;
                resultDTO.IsSuccess = true;
                resultDTO.Message = "List of Timeline Details";
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