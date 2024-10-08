using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Context;
using OMT.DataAccess.Entities;
using OMT.DataService.Interface;
using OMT.DTO;
using System.Data;

namespace OMT.DataService.Service
{
    public class SkillSetService : ISkillSetService
    {
        private readonly OMTDataContext _oMTDataContext;
        public SkillSetService(OMTDataContext oMTDataContext)
        {
            _oMTDataContext = oMTDataContext;
        }

        public ResultDTO CreateSkillSet(SkillSetCreateDTO skillSetCreateDTO)
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
                    resultDTO.Message = "SkillSet created successfully";
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

        public ResultDTO DeleteSkillSet(int skillsetId)
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

                    resultDTO.Message = "Skill Set has been deleted successfully";
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
                                              StateName = string.Join(", ", grp.Where(x => x.hs != null && x.hs.IsActive).Select(x => x.hs.StateName))  //Isactive only
                                          })
                                          .OrderBy(x => x.SkillSetId) //ordering here Bcoz we have used Grouping key
                                          .ThenBy(x => x.SkillSetName)
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
                                              StateName = string.Join(", ", grp.Where(x => x.hs != null && x.hs.IsActive).Select(x => x.hs.StateName))  
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
                                          StateName = string.Join(", ", grp.Where(x => x.hs != null && x.hs.IsActive).Select(x => x.hs.StateName))
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

        public ResultDTO UpdateSkillSet(SkillSetUpdateDTO skillSetUpdateDTO)
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
                    // Updating Skillset Details
                    skillset.Threshold = skillSetUpdateDTO.Threshold;
                    skillset.IsHardState = skillSetUpdateDTO.IsHardState;
                    skillset.IsActive = true;
                    _oMTDataContext.SkillSet.Update(skillset);
                    _oMTDataContext.SaveChanges();

                    if (IsThresholdChanged)
                    {
                        // call method which will update getordercaltable,create private method here
                    }

                    if (skillSetUpdateDTO.StateName != null && skillSetUpdateDTO.IsHardState == true)
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
                    else if (skillSetUpdateDTO.IsHardState == false && skillSetUpdateDTO.StateName == null)
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
                    resultDTO.IsSuccess = true;
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
                            IsHardState = detail.IsHardstate,
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
                            IsHardState = details.IsHardstate,
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
                List<SkillSetTimelineResponseDTO> allSkillSetTimelines = new List<SkillSetTimelineResponseDTO>();

                var SSid = _oMTDataContext.SkillSet.Where(ss => ss.IsActive && _oMTDataContext.Timeline.Any(tl => tl.SkillSetId == ss.SkillSetId && tl.IsActive));

                var skillSetIds = (skillsetid == null) ? SSid.Select(ss => ss.SkillSetId).ToList()
                                  : new List<int> { skillsetid.Value };

                
                    foreach (var id in skillSetIds)
                    {
                        // HardState 
                        List<ResponseTimelineDetailDTO> Listof_HS_TimeLineDetails = (from tl in _oMTDataContext.Timeline
                                                                                     where tl.IsActive && tl.IsHardState && tl.SkillSetId == id
                                                                                     orderby tl.TimelineId
                                                                                     select new ResponseTimelineDetailDTO()
                                                                                     {
                                                                                         HardStateName = tl.Hardstatename,
                                                                                         ExceedTime = tl.ExceedTime,
                                                                                         IsHardstate = tl.IsHardState
                                                                                     }).ToList();

                        // NormalState 
                        List<ResponseTimelineDetailDTO> Listof_NS_TimeLineDetails = (from tl in _oMTDataContext.Timeline
                                                                                     where tl.IsActive && !tl.IsHardState && tl.SkillSetId == id
                                                                                     orderby tl.TimelineId
                                                                                     select new ResponseTimelineDetailDTO()
                                                                                     {
                                                                                         HardStateName = tl.Hardstatename,
                                                                                         ExceedTime = tl.ExceedTime,
                                                                                         IsHardstate = tl.IsHardState
                                                                                     }).ToList();

                        if(Listof_HS_TimeLineDetails.Count==0 && Listof_NS_TimeLineDetails.Count==0) //new
                        {
                            resultDTO.IsSuccess = false;
                            resultDTO.Message = "No timeline details found for this Skillsetid";
                            return resultDTO;
                        }

                        //combine timeline details 
                        SkillSetTimelineResponseDTO skillSetTimelineResponseDTO = new SkillSetTimelineResponseDTO()
                        {
                            SkillSetId = id,
                            HardStateTimelineDetails = Listof_HS_TimeLineDetails,
                            NormalStateTimelineDetails = Listof_NS_TimeLineDetails
                        };

                        allSkillSetTimelines.Add(skillSetTimelineResponseDTO); // Add list of all skillset timelines
                    }

                    resultDTO.Data = allSkillSetTimelines;
                    resultDTO.IsSuccess = true;
                    resultDTO.Message = "List of Timeline Details Successfully Fetched";

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